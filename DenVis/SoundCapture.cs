using CSCore;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.Streams.Effects;
using System;

namespace DenVis
{
	public static class SoundCapture
	{
		public static WasapiLoopbackCapture SoundIn;

		public static void Setup()
		{
			SoundIn = new WasapiLoopbackCapture();
			SoundIn.Initialize();

			var soundInSource = new SoundInSource(SoundIn);
			ISampleSource source = soundInSource.ToSampleSource().AppendSource(x => new PitchShifter(x), out var _pitchShifter);


			// this thing divides the data into blocks of audio data i think
			var notificationSource = new SingleBlockNotificationStream(source);
			// we pass every block to the fft thingy and magic
			notificationSource.SingleBlockRead += (s, a) =>
			{
				Program.fftProvider.Add(a.Left, a.Right);
			};

			// no idea what this is but its neccesary...
			float[] buffer = new float[source.WaveFormat.BytesPerSecond / 2];
			soundInSource.DataAvailable += (s, aEvent) =>
			{
				int read;
				while ((read = notificationSource.Read(buffer, 0, buffer.Length)) > 0) ; // it just reads idk
			};

			Program.fftProvider = new FftProvider(soundInSource.WaveFormat.Channels, Program.fftSize);

			SoundIn.Start();

			Console.WriteLine($"SoundCapture Initialized");
		}
	}
}
