using System;
using System.Threading.Tasks;
using CSCore.SoundIn;
using GameOverlay.Windows;
using GameOverlay.Drawing;
using CSCore.Streams;
using CSCore;
using CSCore.Streams.Effects;
using System.Linq;
using CSCore.DSP;
using CSCore.Utils;
using System.Text;
using System.Threading;

/*
 
 /// TW: SHITCODE AHEAD
 
im sorry -dennis
 
 */

namespace DenVis
{
	public static class Program
	{
		// Overlay
		public static GraphicsWindow graphicsWindow;
		public static int screenW = Utils.GetDisplay().Item1;
		public static int screenH = Utils.GetDisplay().Item2;
		public static float heightMultiplier = 100; // Program: im sensitive uwu
		public static bool IsOnTopOfTaskbar = true;
		public static float yOffset = 220 + (IsOnTopOfTaskbar ? 38 : 0);
		public static float pointDistance = ((int)fftSize) / screenW;
		public static int maxHeight = 500;

		

		// Graphics
		public static SolidBrush Brush;
		public static Font Font;
		
		// Sound
		public static WasapiLoopbackCapture soundIn;
		public const FftSize fftSize = FftSize.Fft4096;
		public static FftProvider fftProvider;

		// Shared
		public static float[] soundData;
		public static bool _initialized = false;

		public static void Main()
		{
			Console.WriteLine($"Screen is ({screenW}w, {screenH}h)");

			GameOverlay.TimerService.EnableHighPrecisionTimers();
			SetupGraphics();

			graphicsWindow.Create();
			SetupCapture();
			Console.WriteLine("Started");
			graphicsWindow.Join();
		}

		public static void SetupGraphics()
		{
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
				Console.WriteLine($"SetupGraphics called");
			};

			graphicsWindow.DestroyGraphics += (object _, DestroyGraphicsEventArgs e) =>
			{
				Brush.Dispose();
				Console.WriteLine($"DestroyGraphics called");
			};

			graphicsWindow.DrawGraphics += Render;

			Console.WriteLine($"GraphicsSetup done");
		}

		public static void Render(object _, DrawGraphicsEventArgs e)
		{
			if (!_initialized) return;

			var gfx = e.Graphics;

			gfx.ClearScene();

			float[] data = new float[(int)fftSize];

			fftProvider.GetFftData(data);

			float maxValue = data.Max();
			if (maxValue < 0.0001) Array.Fill(data, 0f);

			data = Normalize(data);

			StringBuilder sb = new StringBuilder();

			

			float previousValue = 0f;
			float xPosition = pointDistance;
			foreach(float value in data)
			{
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

				// sorry for my shitcode
				//(float r, float g, float b) nextColor = Utils.AddHueToColor(Brush.Color.R, Brush.Color.G, Brush.Color.B);
				//Brush.Color = new Color(nextColor.r, nextColor.g, nextColor.b);
				//var c = System.Drawing.Color.FromArgb(255, ConvColor(Brush.Color.R), ConvColor(Brush.Color.G), ConvColor(Brush.Color.B));
				//var nextColor = ColorScale.ColorFromHSL((c.GetHue() + 1) % 360, c.GetSaturation(), c.GetBrightness());
				//Brush.Color = new Color(nextColor.R, nextColor.G, nextColor.B);
				//Console.WriteLine(nextColor);

				//sb.AppendLine(value.ToString("N2"));

				xPosition += pointDistance;
				previousValue = value;
			}

			// why stop ._.

			float ValueToY(float v) // bizi sevmiyo sanırım
			{ // paint
				float r = (screenH - yOffset) - (v * heightMultiplier); // bidaha de ._.
				//float r = (screenH - v * heightMultiplier) - 300;

				//Console.WriteLine($"DBG = {r}");
				
				return r;

				//return (((v * heightMultiplier) - yOffset) + screenH) * -1; // ters
			}

			float[] Normalize(float[] values, float by = 1)
			{
				float ratio = values.Max() / by;
				return values.Select(i => i / ratio).ToArray();
			}

			int ConvColor(float f)
			{
				return (int)(f * 255);
			}

			// ah yes console log death
			/*
			int i = 0;
			float prevX = 0;
			float prevY = screenH;
			foreach(float value in data)
			{
				float pointYPosition = value; //screenH - value;
				gfx.DrawLine(Brush, prevY, prevX, prevX + pointDistance, pointYPosition, 5);
				prevX += pointDistance;
				prevY = pointYPosition;

				sb.AppendLine(value.ToString("N2"));

				//Console.WriteLine($"DBG = {i}");
				i++;
				//Thread.Sleep(1);

				/*
				float height = b * heightMultiplier;
				float currentYPos = screenH - height;
				gfx.DrawLine(Brush, posX, posY, posX + pointDistance, currentYPos, 5);
				posX += pointDistance;
				posY = currentYPos;
			}*/
			//Console.WriteLine(pointDistance);
			//Console.WriteLine($"Render called");





			//gfx.DrawText(Font, Brush, 0, 0, sb.ToString());
		}

		public static void SetupCapture()
		{
			soundIn = new WasapiLoopbackCapture();
			soundIn.Initialize();

			//var soundInSource = new SoundInSource(soundIn);

			var soundInSource = new SoundInSource(soundIn);
			ISampleSource source = soundInSource.ToSampleSource().AppendSource(x => new PitchShifter(x), out var _pitchShifter);

			
			// this thing divides the data into blocks of audio data i think
			var notificationSource = new SingleBlockNotificationStream(source);
			// we pass every block to the fft thingy and magic
			notificationSource.SingleBlockRead += (s, a) =>
			{
				fftProvider.Add(a.Left, a.Right);
			};
			
			// no idea what this is but its neccesary...
			float[] buffer = new float[source.WaveFormat.BytesPerSecond / 2];
			soundInSource.DataAvailable += (s, aEvent) =>
			{
				int read;
				while ((read = notificationSource.Read(buffer, 0, buffer.Length)) > 0) ; // it just reads idk
			};

			fftProvider = new FftProvider(soundInSource.WaveFormat.Channels, fftSize);

			soundIn.Start();

			Console.WriteLine($"SetupCapture done");
			_initialized = true;
		}
	}
}
