use crossbeam::channel::Sender;

use anyhow::{anyhow, Context, Result};
use cpal::{
    traits::{DeviceTrait, HostTrait, StreamTrait}, Device, SampleRate, Stream, StreamConfig
};

pub const AUDIO_BUFFER_SIZE: usize = 48000;

pub fn print_devices() -> Result<()> {
    for host_id in cpal::available_hosts() {
        println!("Host: `{}`", host_id.name());

        let host = cpal::host_from_id(host_id).expect("host to be available");

        for device in host.devices().expect("devices to be avail") {
            println!(
                "  - Device: `{}`",
                device.name().unwrap_or("<unknown>".to_owned())
            );
            println!(
                "  Inputs: {:#?}",
                device.supported_input_configs()?.collect::<Vec<_>>()
            );
            println!(
                "  Outputs: {:#?}",
                device.supported_output_configs()?.collect::<Vec<_>>()
            );
        }
    }

    Ok(())
}

pub fn get_output_device() -> Result<Device> {
    print_devices()?;

    cpal::default_host()
        .devices()?
        .find(|d| d.name().is_ok_and(|s| s == "Hoparlör (Realtek High Definition Audio)"))
        .ok_or(anyhow!("Device not found"))
}

pub fn audio_thread(dev: Device, audio_sender: Sender<(f32, f32)>) -> Result<Stream> {
    println!("Selected device: {:#?}", dev.name());
    println!("Starting to capture audio");

    let stream = dev
        .build_input_stream(
            &StreamConfig {
                buffer_size: cpal::BufferSize::Fixed(AUDIO_BUFFER_SIZE.try_into().unwrap()),
                channels: 2,
                sample_rate: SampleRate(48000),
            },
            move |data: &[f32], _: &_| {
                // println!("!");
                let lr_pairs = data.chunks_exact(2).map(|x| (x[0], x[1]));
                for pair in lr_pairs {
                    let _ = audio_sender.try_send(pair);
                }
            },
            |e| {
                eprintln!("ERROR");
                eprintln!("{e:#?}");
            },
            None,
        )
        .context("building Stream")?;

    stream.play().context("playing Stream")?;

    println!("Capture started");

    Ok(stream)
}
