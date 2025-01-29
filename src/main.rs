#![feature(slice_as_chunks)]
#![feature(iter_array_chunks)]

use std::sync::{Arc, RwLock};

use anyhow::Result;
use app::{Action, State};
use femtovg::{renderer::OpenGl, Canvas};
use glutin::{prelude::GlDisplay, surface::GlSurface};
use winit::event_loop::{ControlFlow, EventLoop};

mod app;
mod audio;
mod fft;
mod init_window;
mod spectrum;
mod util;

fn main() -> Result<()> {
    let event_loop = EventLoop::<Action>::with_user_event().build()?;
    let (context, display, window, surface) = init_window::create_window(&event_loop)?;

    let renderer =
        unsafe { OpenGl::new_from_function_cstr(|s| display.get_proc_address(s) as *const _)? };

    println!("Initializing canvas...");

    let mut canvas = Canvas::new(renderer)?;
    canvas.add_font_mem(&resource::resource!("assets/Lexend-VariableFont_wght.ttf"))?;

    let monitor = window.current_monitor().unwrap();
    let mut size = monitor.size();
    size.height += 1;
    let _ = window.request_inner_size(size);

    canvas.set_size(size.width, size.height, window.scale_factor() as f32);
    util::clear(&mut canvas);
    canvas.flush();
    surface.swap_buffers(&context)?;

    //

    println!("Initializing threads...");

    let (audio_sender, audio_recv) = crossbeam::channel::unbounded::<(f32, f32)>();
    let (fft_sender, fft_recv) = crossbeam::channel::unbounded::<Vec<f32>>();
    let audio_spectrum = Arc::new(RwLock::new(vec![]));

    let dev = audio::get_output_device()?;
    // let _ = std::thread::spawn(|| audio::audio_thread(dev, audio_sender));
    let _stream = audio::audio_thread(dev, audio_sender)?;
    let _ = std::thread::spawn(|| fft::fft_thread(audio_recv, fft_sender));
    let arc_spectrum = audio_spectrum.clone();
    let _ = std::thread::spawn(move || loop {
        let vec = fft_recv.recv().unwrap();
        let mut wg = arc_spectrum.write().unwrap();
        *wg = vec;
    });

    //

    event_loop.set_control_flow(ControlFlow::Poll);

    let mut state = State {
        canvas,
        context,
        display,
        surface,
        window,
        audio_spectrum,
        drawables: vec![],
    };

    println!("Running event loop...");

    event_loop.run_app(&mut state)?;

    Ok(())
}

/* println!("Available monitors:");
println!("{:#?}", window.available_monitors().collect::<Vec<_>>());
audio::print_devices()?;

let (tx, rx) = mpsc::channel::<AudioBuffer>();
let dev = audio::get_output_device()
    .context("get_output_device")?;
audio::capture_audio(dev, tx)
    .context("capture_audio")?; */
