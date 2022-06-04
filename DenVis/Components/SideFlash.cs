using System;
using GameOverlay.Drawing;

namespace DenVis
{
	public class SideFlash
	{
		public static LinearGradientBrush GradientBrushLeft;
		public static LinearGradientBrush GradientBrushRight;
		public static SolidBrush AlphaBrush;

		public static float FlashWidth = Renderer.screenW / 2;

		public static void Setup(Graphics gfx)
		{
			GradientBrushLeft = new LinearGradientBrush(gfx,
				new Color(255, 255, 255, 255), // opaque
				new Color(0, 0, 0, 0) // trans
			);

			GradientBrushRight = new LinearGradientBrush(gfx,
				new Color(255, 255, 255, 255), // opaque
				new Color(255, 255, 255, 0) // trans
			);
			
			AlphaBrush = gfx.CreateSolidBrush(255, 255, 255, 1);
		}

		public static int RightOpacity = 0;
		public static bool RightEasingIn = true;
		public static float RightX = 0;
		public static int LeftOpacity = 0;
		public static float LeftX = 0;

		public static void Render(Graphics gfx)
		{
			float startY = 0;
			float endY = Renderer.ScreenBottom;
			if(LeftX > 0)
			{
				GradientBrushLeft.SetRange(
					LeftX - FlashWidth,	
					0,
					FlashWidth, // end x
					0 // end y
				);
				gfx.DrawBox2D(GradientBrushLeft, GradientBrushLeft,
					0, // start x
					startY,
					FlashWidth, // end x
					endY, // end y
					0);
				LeftX -= Settings.SideFlashSpeed;
			}

			if (RightX > 0)
			{
				GradientBrushRight.SetRange(
					Renderer.screenW,
					0,
					Renderer.screenW - RightX, // end x
					0 // end y
				);
				gfx.DrawBox2D(GradientBrushRight, GradientBrushRight,
					Renderer.screenW - FlashWidth, // start x
					startY,
					Renderer.screenW, // end x
					endY, // end y
					0);
				RightX -= Settings.SideFlashSpeed;
			}
		}

		public static void FlashRight()
		{
			RightX = FlashWidth * Settings.SideFlashWidthRatio;
		}

		public static void FlashLeft()
		{
			LeftX = FlashWidth * Settings.SideFlashWidthRatio;
		}
	}
}