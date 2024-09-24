use femtovg::{renderer::OpenGl, Canvas, Color};

pub fn clear(canvas: &mut Canvas<OpenGl>) {
    canvas.clear_rect(
        0,
        0,
        canvas.width(),
        canvas.height(),
        Color::rgbaf(0.0, 0.0, 0.0, 0.0),
    );
}
