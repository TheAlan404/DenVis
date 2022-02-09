using System;
using GameOverlay.Drawing;

namespace DenVis
{
	public class SideFlash
	{
		public static LinearGradientBrush GradientBrush;

		public static void Setup(Graphics gfx)
		{
			GradientBrush = new LinearGradientBrush(gfx,
				new Color(255, 255, 255, 128),
				new Color(255, 255, 255, 0)
			);
		}

		public static int RightOpacity = 128;
		public static int LeftOpacity = 128;

		public static void Render(Graphics gfx)
		{

		}
	}
}