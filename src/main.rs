use anyhow::Result;
use app::{Action, State};
use femtovg::{renderer::OpenGl, Canvas, Color, Renderer};
use glutin::{
    context::PossiblyCurrentContext,
    prelude::GlDisplay,
    surface::{GlSurface, Surface, WindowSurface},
};
use winit::{
    dpi::PhysicalPosition,
    event_loop::{ControlFlow, EventLoop},
    window::Window,
};

mod app;
mod audio;
mod init_window;
mod util;

fn main() -> Result<()> {
    let event_loop = EventLoop::<Action>::with_user_event().build()?;
    let (context, display, window, surface) = init_window::create_window(&event_loop)?;

    let renderer =
        unsafe { OpenGl::new_from_function_cstr(|s| display.get_proc_address(s) as *const _)? };

    let monitor = window.current_monitor().unwrap();
    let size = monitor.size();
    window.set_outer_position(PhysicalPosition::new(0, 0));
    let _ = window.request_inner_size(size);

    let mut canvas = Canvas::new(renderer)?;
    canvas.add_font_mem(&resource::resource!("assets/Lexend-VariableFont_wght.ttf"))?;

    canvas.set_size(size.width, size.height, window.scale_factor() as f32);
    util::clear(&mut canvas);
    canvas.flush();
    surface.swap_buffers(&context)?;
    window.set_visible(true);

    event_loop.set_control_flow(ControlFlow::Poll);

    let mut state = State {
        canvas,
        context,
        display,
        surface,
        window,
    };

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
