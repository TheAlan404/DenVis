using GameOverlay.Drawing;
using GameOverlay.Windows;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace DenVis
{
	public static class Renderer
	{
		public static GraphicsWindow graphicsWindow;
		public static int screenW = Utils.GetDisplay().Item1;
		public static int screenH = Utils.GetDisplay().Item2;
		public static float pointDistance = ((int)Program.fftSize) / screenW;

		public static float TaskbarHeight = Program.IsWin8 ? 38 : 42;
		public static float ScreenOffset = Program.IsWin8 ? 220 : 0;
		public static float ScreenBottom = screenH - ScreenOffset;
		public static float BassRange = 8;
		public static bool IsFullscreen = Utils.IsForegroundWindowFullScreen();

		public static List<float> dataHistory = new List<float>(30);

		// Graphics
		public static SolidBrush Brush;
		public static Font Font;
		public static double lastHue = 0;

		// Other
		public static Timer FullscreenTimer;

		public static void Setup()
		{
			GameOverlay.TimerService.EnableHighPrecisionTimers();

			var gfx = new Graphics()
			{
				MeasureFPS = true,
				PerPrimitiveAntiAliasing = true,
				TextAntiAliasing = true
			};

			graphicsWindow = new GraphicsWindow(0, 0, screenW, screenH, gfx)
			{
				FPS = 30,
				IsTopmost = true,
				IsVisible = true
			};

			graphicsWindow.SetupGraphics += (object _, SetupGraphicsEventArgs e) =>
			{
				var gfx = e.Graphics;
				if (e.RecreateResources) Brush.Dispose();
				Brush = gfx.CreateSolidBrush(177, 156, 217, 128);
				Font = gfx.CreateFont("Arial", 10);

				SnowRenderer.Setup(gfx);
				TextRenderer.Setup(gfx);

				TextRenderer.Add(new Text()
				{
					String = "DenVis is running",
					Expire = 3000,
					X = 20,
					Y = 20
				});
			};

			graphicsWindow.DestroyGraphics += (object _, DestroyGraphicsEventArgs e) =>
			{
				Brush.Dispose();
				Console.WriteLine($"DestroyGraphics called");
			};

			graphicsWindow.DrawGraphics += Render;

			SetFullscreenTimer();

			Console.WriteLine($"GraphicsSetup done");

			graphicsWindow.Create();
			graphicsWindow.Join();
		}

		public static void SetFullscreenTimer()
		{
			FullscreenTimer = new Timer(2000);
			FullscreenTimer.Elapsed += (sender, args) =>
			{
				IsFullscreen = Utils.IsForegroundWindowFullScreen();
			};
			FullscreenTimer.AutoReset = true;
			FullscreenTimer.Enabled = true;
		}

		public static void Render(object _, DrawGraphicsEventArgs e)
		{
			Graphics gfx = e.Graphics;

			if (Settings.Enabled) RenderVisualizer(gfx);
			if (Settings.Snow.Enabled)
			{
				SnowRenderer.Render(gfx);
				SnowRenderer.MoveSnow(gfx);
			}
			TextRenderer.Render(gfx);
		}

		public static void RenderVisualizer(Graphics gfx)
		{
			gfx.ClearScene();

			float[] data = new float[(int)Program.fftSize];

			Program.fftProvider.GetFftData(data);
			float[] dataPart = new ArraySegment<float>(data, 0, screenW / 2).ToArray();

			float max = dataPart.Max();

			if (max < Settings.Sensitivity) Array.Fill(dataPart, 0f);

			if (dataHistory.Capacity == dataHistory.Count) dataHistory.RemoveAt(0);
			dataHistory.Add(max);



			dataPart = Normalize(dataPart);




			// doesnt work.. TODO?
			float bassSum = 0;
			for (int i = 0; i < BassRange; i++)
			{
				bassSum += dataPart[i];
			}
			bassSum /= BassRange;
			

			if (Settings.CenterVisualizer)
			{
				//float[] yarısı = new ArraySegment<float>(dataPart, 0, (int)(screenW / 4)).ToArray();
				//float[] centered = yarısı.Reverse().Concat(yarısı).ToArray();
				//dataPart = centered;


			}



			float previousValue = 0f;
			float xPosition = 0;

			for (int i = 0; i < dataPart.Length; i++)
			{
				float value = dataPart[i];

				//value += bassSum; 
				gfx.DrawLine(Brush,
					// start x
					xPosition - pointDistance,
					// start y
					ValueToY(previousValue),
					// end x
					xPosition,
					// end y
					ValueToY(value),

					// stroke
					5
				);

				if (lastHue > 1) lastHue = lastHue % 1;
				lastHue += Settings.HueChangeSpeed;
				System.Drawing.Color c = ColorScale.ColorFromHSL(lastHue, 0.5, 0.5);
				Brush.Color = new Color(c.R, c.G, c.B);


				xPosition += pointDistance;
				previousValue = value;
			}


			//if (bassSum > 0.02) lastHue += bassSum;


			float ValueToY(float v)
			{
				return (screenH) - (v * Settings.HeightMultiplier) - (ScreenOffset + Settings.yOffset + (IsFullscreen ? 0 : TaskbarHeight));
			}

			float[] Normalize(float[] values, float by = 1)
			{
				float ratio = (Settings.UseDataHistory ? dataHistory.Max() : values.Max()) / by;
				return values.Select(i => i / ratio).ToArray();
			}
		}
	}
}