use std::sync::{Arc, RwLock};

use femtovg::{renderer::OpenGl, Canvas, Color, Paint, Path};
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
    pub drawables: Vec<Box<dyn Drawable>>,
}

pub trait Drawable {
    fn draw(&self, canvas: Canvas<OpenGl>);
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
                self.redraw();
            }

            _ => {}
        }
    }
}

impl State {
    pub fn redraw(&mut self) {
        crate::util::clear(&mut self.canvas);

        self.render();

        self.canvas.flush();
        self.window.pre_present_notify();
        self.surface
            .swap_buffers(&self.context)
            .expect("Could not swap buffers");
    }

    pub fn render(&mut self) {
        self.render_spectrum();
    }

    pub fn render_spectrum(&mut self) {
        let paint = Paint::color(Color::hex("#0000ff"));

        let li = <RwLock<Vec<f32>>>::read(&self.audio_spectrum).unwrap();
        let mut path = Path::new();
        path.move_to(0.0, self.canvas.height() as f32);

        let h = self.canvas.height() as f32 - 5. - 36.;

        let max = li.clone().into_iter()
            .reduce(f32::max)
            .unwrap_or(0.);

        if max < 0.00001 {
            return;
        }

        let space =
            self.canvas.width() as f32 / if li.len() > 0 { li.len() as f32 } else { 1. };

        for (i, value) in li.iter().enumerate() {
            let x = i as f32 * space;
            let v = *value / max;
            let y = h - v * 100.0;
            path.line_to(x, y);
        }

        self.canvas.stroke_path(&path, &paint);
    }
}
