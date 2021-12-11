using System;
using System.Threading.Tasks;
using CSCore.SoundIn;
using GameOverlay.Windows;
using GameOverlay.Drawing;
using CSCore.Streams;
using CSCore;
using CSCore.Streams.Effects;
using System.Linq;

namespace DenVis
{
	public static class Program
	{
		// Overlay
		public static GraphicsWindow graphicsWindow;
		public static int screenW = Utils.GetDisplay().Item1;
		public static int screenH = Utils.GetDisplay().Item2;
		public static int heightMultiplier = 1;
		public static int pointDistance;

		// Graphics
		public static SolidBrush Brush;

		// Capture
		public static WasapiLoopbackCapture soundIn;

		public static float[] soundData;

		public static void Main()
		{
			Console.WriteLine($"Screen is ({screenW}w, {screenH}h)");

			GameOverlay.TimerService.EnableHighPrecisionTimers();
			SetupGraphics();
			SetupCapture();

			graphicsWindow.Create();
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
				FPS = 10,
				IsTopmost = true,
				IsVisible = true
			};

			graphicsWindow.SetupGraphics += (object _, SetupGraphicsEventArgs e) =>
			{
				var gfx = e.Graphics;
				if (e.RecreateResources) Brush.Dispose();
				Brush = gfx.CreateSolidBrush(0, 0, 255, 128);
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
			var gfx = e.Graphics;

			gfx.ClearScene();

			gfx.DrawBox2D(Brush, Brush, new Rectangle(0, 0, 100, 100), 1);

			if (soundData == null || soundData.Length == 0) return;

			float posY = 0;
			float posX = -pointDistance;
			foreach(float b in soundData)
			{
				float height = b * heightMultiplier;
				float currentYPos = screenH - height;
				gfx.DrawLine(Brush, posX, posY, posX + pointDistance, currentYPos, 5);
				posX = posX + pointDistance;
				posY = currentYPos;
			}
			Console.WriteLine(pointDistance);
			//Console.WriteLine($"Render called");
		}

		public static void SetupCapture()
		{
			soundIn = new WasapiLoopbackCapture();
			soundIn.Initialize();

			//var soundInSource = new SoundInSource(soundIn);

			var soundInSource = new SoundInSource(soundIn);
			ISampleSource source = soundInSource.ToSampleSource().AppendSource(x => new PitchShifter(x), out var _pitchShifter);

			/*
			//the SingleBlockNotificationStream is used to intercept the played samples
			var notificationSource = new SingleBlockNotificationStream(aSampleSource);
			//pass the intercepted samples as input data to the spectrumprovider (which will calculate a fft based on them)
			notificationSource.SingleBlockRead += (s, a) =>
			{
			
			};*/

			//var _source = notificationSource.ToWaveSource(16);
			/*
			// We need to read from our source otherwise SingleBlockRead is never called and our spectrum provider is not populated
			byte[] buffer = new byte[_source.WaveFormat.BytesPerSecond / 2];
			soundInSource.DataAvailable += (s, aEvent) =>
			{
				int read;
				while ((read = _source.Read(buffer, 0, buffer.Length)) > 0) ;
			};*/

			soundIn.DataAvailable += dataAvailableEvent;

			soundIn.Start();

			Console.WriteLine($"SetupCapture done");
		}

		private static void dataAvailableEvent(object sender, DataAvailableEventArgs e)
		{
			if (e.ByteCount == 0) return;
			//Console.WriteLine($"Got data with length {e.ByteCount}");
			soundData = ConvertByteArrayToFloat(e.Data);
			pointDistance = e.ByteCount / screenW;
		}

		public static float[] ConvertByteArrayToFloat(byte[] bytes)
		{
			if (bytes == null) throw new ArgumentNullException("bytes");
			if (bytes.Length % 4 != 0) throw new ArgumentException("bytes does not represent a sequence of floats");
			return Enumerable.Range(0, bytes.Length / 4)
							 .Select(i => BitConverter.ToSingle(bytes, i * 4))
							 .ToArray();
		}
	}
}
