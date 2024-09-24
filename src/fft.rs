use std::collections::VecDeque;

use crossbeam::channel::{Receiver, Sender};
use rustfft::{num_complex::Complex, FftPlanner};

pub const FFT_SIZE: usize = 1536;
pub const FFT_NYQUIST: usize = FFT_SIZE / 2;
pub const OVERLAP: f32 = 0.5;

pub fn fft_thread(
    audio_recv: Receiver<(f32, f32)>,
    fft_sender: Sender<Vec<f32>>,
) {
    let mut planner = FftPlanner::new();
    let fft = planner.plan_fft_forward(FFT_SIZE);

    let mut sample_vec = VecDeque::with_capacity(FFT_SIZE);
    sample_vec.resize(FFT_SIZE, 0.0);

    let mut fft_data = vec![Complex::default(); FFT_SIZE];
    let mut f32_scratch = vec![0.0; FFT_SIZE];
    let mut agc_scratch = vec![0.0; FFT_SIZE];

    // Only interested in the first half since the is real data, and because
    // nyquist is a problem.
    let mut fft_energy = vec![0.0; FFT_NYQUIST];

    // Calculate the overlap to faciliate a pseudo-welch's method.
    let overlap = (FFT_SIZE as f32 * OVERLAP) as usize;
    
    loop {
        // Create a running buffer, dropping and consuming `overlap` amounts of data each time, except for initial fill.
        // fill from the audio thread.
        let drain_amount = std::cmp::min(sample_vec.len(), overlap);
        sample_vec.drain(..drain_amount);

        let sample_iterator = audio_recv
            .iter()
            .take(FFT_SIZE - sample_vec.len())
            .map(|(l, r)| (l + r) / 2.0);

        sample_vec.extend(sample_iterator);

        // overwrites f32_scratch
        {
            // Copy into a continous buffer since a dequeue is represented as two slices. 
            let (sample_left, sample_right) = sample_vec.as_slices();
            let left_len = sample_left.len();
            let (scratch_l, scratch_r) = f32_scratch.split_at_mut(left_len);
            scratch_l.copy_from_slice(sample_left);
            scratch_r.copy_from_slice(sample_right);
        }

        sinc_window_inner(&mut f32_scratch, &mut fft_data /* overwrites */);
        fft.process(&mut fft_data /* updates */);

        for (c, a) in fft_data.iter().zip(fft_energy.iter_mut()) {
            // First get the amplitude, normalize by dividing by the nyquist bin.
            let norm = c.to_polar().0 / (FFT_NYQUIST as f32);
            *a = norm;
        }

        fft_sender.try_send(fft_energy.clone()).unwrap();
    }
}

const COEFF: [f32; 4] = [0.3635819, 0.4891775, 0.1365995, 0.0106411];

fn sinc_window_inner(data: &[f32], output: &mut [Complex<f32>]) {
    use std::f32::consts::PI;
    let len = data.len();
    for (n, x) in data.iter().enumerate() {
        output[n] = (x * COEFF.iter().enumerate().map(|(k,&a)| {
            let kf = k as f32;
            let n = n as f32;
            (-1.0f32).powi(k as i32) * a * ((2.0*PI*kf*n)/(len - 1) as f32).cos()
        }).sum::<f32>()).into();
    }
}
