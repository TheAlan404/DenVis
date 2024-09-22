use std::{thread, sync::mpsc::Sender};
use std::sync::mpsc::{channel, Receiver};

use cpal::StreamConfig;
use winit::event_loop::ControlFlow;
use winit::{
    event::{Event, WindowEvent},
    event_loop::EventLoop,
    window::WindowBuilder,
};
use cpal::{traits::{HostTrait, DeviceTrait, StreamTrait}, InputCallbackInfo};

fn main() {
    let (tx, rx) = channel();

    start_input_stream(tx);

    let event_loop = EventLoop::new();

    pollster::block_on(run(event_loop, rx));
}

async fn run(event_loop: EventLoop<()>, rx: Receiver<[f32; BUFFER_SIZE]>) {
    let instance = wgpu::Instance::default();

    let adapter = instance
        .request_adapter(&Default::default())
        .await
        .expect("Failed to find an appropriate adapter");

    // Create the logical device and command queue
    let (device, queue) = adapter
        .request_device(
            &wgpu::DeviceDescriptor {
                label: None,
                features: wgpu::Features::empty(),
                limits: wgpu::Limits::default(),
            },
            None,
        )
        .await
        .expect("Failed to create device");

    
    let window = WindowBuilder::new()
        .with_decorations(false)
        .with_transparent(true)
        .build(&event_loop)
        .unwrap();

    window.set_title("denvis rust");
    window.set_window_level(winit::window::WindowLevel::AlwaysOnTop);
    if let Err(e) = window.set_cursor_hittest(false) {
        println!("cursor hittest false failed, err: {e}");
    }

    let surface = unsafe {
        instance.create_surface(&window)
    }.unwrap();

    let size = window.inner_size();

    let caps = surface.get_capabilities(&adapter);

    println!("Surface Capabilities Formats:");
    for f in &caps.formats {
        println!(" - {f:#?}");
    }

    let config = wgpu::SurfaceConfiguration {
        usage: wgpu::TextureUsages::RENDER_ATTACHMENT,
        format: caps.formats[0],
        width: size.width,
        height: size.height,
        present_mode: wgpu::PresentMode::Fifo,
        alpha_mode: caps.alpha_modes[0],
        view_formats: vec![],
    };

    surface.configure(&device, &config);

    event_loop.run(move |event, _, control_flow| {
        //println!("{event:?}");
        
        // Have the closure take ownership of the resources.
        // `event_loop.run` never returns, therefore we must do this to ensure
        // the resources are properly cleaned up.
        let _ = (&instance, &adapter);
        
        *control_flow = ControlFlow::Wait;
        match event {
            Event::WindowEvent {
                event: WindowEvent::CloseRequested,
                ..
            } => *control_flow = ControlFlow::Exit,
            Event::RedrawRequested(_) => {
                let frame = surface.get_current_texture().expect("Failed to acquire next swap chain texture");
                let view = frame
                    .texture
                    .create_view(&wgpu::TextureViewDescriptor::default());
                let mut encoder = device
                    .create_command_encoder(&wgpu::CommandEncoderDescriptor { label: None });
                {
                    let _rpass = encoder.begin_render_pass(&wgpu::RenderPassDescriptor {
                        label: None,
                        color_attachments: &[Some(wgpu::RenderPassColorAttachment {
                            view: &view,
                            resolve_target: None,
                            ops: wgpu::Operations {
                                load: wgpu::LoadOp::Clear(wgpu::Color { r: 0.5f64, g: 0.5f64, b: 0.5f64, a: 1f64 }),
                                store: true,
                            },
                        })],
                        depth_stencil_attachment: None,
                    });
                }

                queue.submit(Some(encoder.finish()));
                frame.present();
            }
            _ => (),
        }
    })
}

const BUFFER_SIZE: usize = 48000;

fn start_input_stream(tx: Sender<[f32; BUFFER_SIZE]>) {
    for host_id in cpal::available_hosts() {
        println!("Host name: {}", host_id.name());

        let host = cpal::host_from_id(host_id).expect("host to be available");

        for device in host.devices().expect("devices to be avail") {
            println!("- Device name: {}", device.name().unwrap_or("<unknown>".to_owned()));
        }
    }

    let out = cpal::default_host().default_output_device().expect("def out");

    println!("Will use {}", out.name().unwrap_or("<unknown>".to_owned()));

    let def_cfg = out.default_output_config().expect("def out cfg");

    let stream = out.build_input_stream(&StreamConfig {
            buffer_size: cpal::BufferSize::Fixed(BUFFER_SIZE as u32),
            ..def_cfg.config()
        },
        move |data: &[f32], _: &InputCallbackInfo| {
            let data = data.try_into().expect("buf size");
            if let Err(e) = tx.send(data) {
                println!("send error: {e}");
            }
        }, 
        move |e| {
            println!("ERROR:");
            println!("  {e}");
        }, 
        None
    ).expect("inp stream");

    stream.play().unwrap();
    println!("stream playing...");
}
