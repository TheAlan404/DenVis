using GameOverlay.Drawing;
using GameOverlay.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace DenVis
{
	public static class Renderer
	{
		public static GraphicsWindow graphicsWindow;
		public static int screenW = Utils.GetDisplay().Item1;
		public static int screenH = Utils.GetDisplay().Item2;
		public static float pointDistance = ((int)Program.fftSize) / screenW;

		public static float TaskbarHeight = Program.IsWin8 ? 36 : 42;
		public static float ScreenOffset = Program.IsWin8 ? 220 : 0;
		public static float ScreenBottom = screenH - ScreenOffset;
		public static float BassRange = 8;
		public static bool IsFullscreen = Utils.IsForegroundWindowFullScreen();

		public static List<float> dataHistory = new List<float>(30);

		public static List<float> bassIntensityHistory = new List<float>(100);
		public static bool IsBassFrame = false;

		// Graphics
		public static SolidBrush Brush;
		public static Font Font;
		public static double lastHue = 0;

		// Other
		public static Timer FullscreenTimer;

		public static TaskCompletionSource TCSReady = new TaskCompletionSource();

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
				IsVisible = true,
				Title = "DenVis",
				MenuName = "DenVis",
			};

			graphicsWindow.SetupGraphics += (object _, SetupGraphicsEventArgs e) =>
			{
				var gfx = e.Graphics;
				if (e.RecreateResources) Brush.Dispose();
				Brush = gfx.CreateSolidBrush(177, 156, 217, 255);
				Font = gfx.CreateFont("Arial", 10);

				SnowRenderer.Setup(gfx);
				TextRenderer.Setup(gfx);
				WaveRenderer.Setup(gfx);

				TextRenderer.Add(new Text()
				{
					String = "DenVis is running",
					Expire = 3000,
					X = 20,
					Y = 20
				});

				if(!TCSReady.Task.IsCompleted) TCSReady.SetResult();
				Program.LoadConfiguration();
				if (Settings.CheckForUpdates) _ = Program.CheckUpdates();
				SetColor(-1, -1, -1, Settings.Opacity);
				Console.WriteLine("All ready");
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
			_ = Task.Run(() => graphicsWindow.Join());
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

			gfx.ClearScene();

			if (Settings.Enabled) RenderVisualizer(gfx);
			if (Settings.SnowEnabled)
			{
				SnowRenderer.Render(gfx);
				SnowRenderer.MoveSnow(gfx);
			}
			TextRenderer.Render(gfx);
			WaveRenderer.Render(gfx);
		}

		public static void SetColor(float r, float g, float b, float a)
		{
			Brush.Color = new Color(
				r == -1 ? Brush.Color.R : r,
				g == -1 ? Brush.Color.G : g,
				b == -1 ? Brush.Color.B : b,
				a == -1 ? Brush.Color.A : a
			);
		}

		public static void RenderVisualizer(Graphics gfx)
		{
			float[] data = new float[(int)Program.fftSize];

			Program.fftProvider.GetFftData(data);
			float[] dataPart = new ArraySegment<float>(data, 0, screenW / 2).ToArray();

			float max = dataPart.Max();

			if (max < Settings.Sensitivity) Array.Fill(dataPart, 0f);

			if (dataHistory.Capacity == dataHistory.Count) dataHistory.RemoveAt(0);
			dataHistory.Add(max);



			dataPart = Normalize(dataPart);


			// bass

			if (Settings.EnableBassWaves)
			{
				float bassIntensity = 0;
				//gfx.DrawText(Font, 30, Brush, 20, 20, dataPart.Length.ToString());
				for (var i = 8; i < (Settings._BassUseBassRange ? Settings.BassRange : dataPart.Length - 8); i++) bassIntensity += dataPart[i];

				if (bassIntensityHistory.Capacity == bassIntensityHistory.Count) bassIntensityHistory.RemoveAt(0);
				bassIntensityHistory.Add(bassIntensity);

				//float bassLineY = ValueToY(bassIntensity / 2);
				//gfx.DrawLine(Brush, 0, bassLineY, screenW, bassLineY, 5);

				if ((bassIntensityHistory.Max() * Settings.BassSensitivity) < bassIntensity)
				{
					if (!Settings._BassCheckStart || !IsBassFrame)
					{
						WaveRenderer.AddWave();
					}
					IsBassFrame = true;
					//gfx.DrawText(Font, 30, Brush, 20, 20, "BASS!!!");
				}
				else
				{
					IsBassFrame = false;
				};
			}

			// end bass

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


				if (Settings.RainbowMode)
				{
					if (lastHue > 1) lastHue = lastHue % 1;
					lastHue += Settings.HueChangeSpeed;
					System.Drawing.Color c = ColorScale.ColorFromHSL(lastHue, 0.5, 0.5);
					SetColor(c.R, c.G, c.B, -1);
				}

				if (Settings.ColorBassDifferiently && xPosition < Settings.BassRange) SetColor(0, 0, 0, 1);

				xPosition += pointDistance;
				previousValue = value;
			}


			//if (bassSum > 0.02) lastHue += bassSum;


			float ValueToY(float v)
			{
				return screenH - 
					(v * Settings.HeightMultiplier) - 
					(ScreenOffset + Settings.yOffset + (IsFullscreen ? 0 : Settings.IsOnTopOfTaskbar ? TaskbarHeight : 0));
			}

			float[] Normalize(float[] values, float by = 1)
			{
				float ratio = (Settings.UseDataHistory ? dataHistory.Max() : values.Max()) / by;
				return values.Select(i => i / ratio).ToArray();
			}
		}
	}
}