use std::num::NonZeroU32;

use anyhow::Result;

use glutin::{
    config::ConfigTemplateBuilder,
    context::{ContextAttributesBuilder, PossiblyCurrentContext},
    display::{Display, GetGlDisplay},
    prelude::{GlDisplay, NotCurrentGlContext},
    surface::{Surface, SurfaceAttributesBuilder, WindowSurface},
};
use glutin_winit::DisplayBuilder;
use winit::{
    event_loop::EventLoop, monitor::{self, VideoModeHandle}, platform::windows::{WindowAttributesExtWindows, WindowExtWindows}, raw_window_handle::HasWindowHandle, window::{Fullscreen, Window, WindowAttributes, WindowButtons, WindowLevel}
};

use crate::app::Action;

pub fn create_window(
    event_loop: &EventLoop<Action>,
) -> Result<(
    PossiblyCurrentContext,
    Display,
    Window,
    Surface<WindowSurface>,
)> {
    

    let mut window_attributes = WindowAttributes::default()
        .with_title(String::from("DenVis"))
        .with_decorations(false)
        .with_fullscreen(None)
        .with_transparent(true)
        .with_window_level(WindowLevel::AlwaysOnTop)
        .with_enabled_buttons(WindowButtons::empty());

    #[cfg(windows)]
    {
        window_attributes = window_attributes.with_skip_taskbar(true);
    }

    let display_builder = DisplayBuilder::new().with_window_attributes(Some(window_attributes));

    let (window, gl_config) = display_builder
        .build(
            event_loop,
            ConfigTemplateBuilder::new()
                .with_alpha_size(8)
                .with_transparency(true),
            |mut configs| configs.next().unwrap(),
        )
        .unwrap();

    let window = window.unwrap();
    window.set_cursor_hittest(false)?;

    #[cfg(windows)]
    {
        window.set_skip_taskbar(true);
    }

    let gl_display = gl_config.display();

    let context_attributes =
        ContextAttributesBuilder::new().build(Some(window.window_handle()?.as_raw()));

    let mut not_current_gl_context = Some(unsafe {
        gl_display
            .create_context(&gl_config, &context_attributes)
            .unwrap()
    });

    let size = window.inner_size();
    let attrs = SurfaceAttributesBuilder::<WindowSurface>::new().build(
        window.window_handle()?.as_raw(),
        NonZeroU32::new(size.width).unwrap(),
        NonZeroU32::new(size.height).unwrap(),
    );

    let surface = unsafe {
        gl_config
            .display()
            .create_window_surface(&gl_config, &attrs)
            .unwrap()
    };

    let _context = not_current_gl_context
        .take()
        .unwrap()
        .make_current(&surface)?;

    Ok((_context, gl_display, window, surface))
}
