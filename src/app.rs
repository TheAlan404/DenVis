use std::sync::{Arc, RwLock};

use femtovg::{renderer::OpenGl, Canvas, Color, Paint};
use glutin::{
    context::PossiblyCurrentContext,
    display::Display,
    surface::{GlSurface, Surface, WindowSurface},
};
use winit::{application::ApplicationHandler, event::WindowEvent, window::Window};

pub enum Action {}

pub struct State {
    pub context: PossiblyCurrentContext,
    pub window: Window,
    pub display: Display,
    pub surface: Surface<WindowSurface>,
    pub canvas: Canvas<OpenGl>,
    pub audio_spectrum: Arc<RwLock<Vec<f32>>>,
}

impl ApplicationHandler<Action> for State {
    fn resumed(&mut self, event_loop: &winit::event_loop::ActiveEventLoop) {
        println!("resumed");
    }

    fn about_to_wait(&mut self, event_loop: &winit::event_loop::ActiveEventLoop) {
        self.window.request_redraw();
    }

    fn window_event(
        &mut self,
        event_loop: &winit::event_loop::ActiveEventLoop,
        _window_id: winit::window::WindowId,
        event: WindowEvent,
    ) {
        match event {
            WindowEvent::CloseRequested => {
                println!("CloseRequested");
                event_loop.exit();
            }

            WindowEvent::Resized(size) => {
                self.canvas
                    .set_size(size.width, size.height, self.window.scale_factor() as f32);
            }

            WindowEvent::RedrawRequested => {
                crate::util::clear(&mut self.canvas);

                let paint = Paint::color(Color::black());

                self.canvas
                    .clear_rect(30, 30, 30, 30, Color::rgbf(1., 0., 0.));
                self.canvas.fill_text(20.0, 20.0, "meow", &paint).unwrap();

                self.canvas.flush();
                self.window.pre_present_notify();
                self.surface
                    .swap_buffers(&self.context)
                    .expect("Could not swap buffers");
            }

            _ => {}
        }
    }
}
