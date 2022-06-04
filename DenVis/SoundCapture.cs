using CSCore;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.Streams;
using CSCore.Streams.Effects;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace DenVis
{
	public static class SoundCapture
	{
		public static void Setup()
		{
			ISoundIn soundIn;

			if (OperatingSystem.IsWindows())
			{
				soundIn = new WasapiLoopbackCapture();
			} else if (OperatingSystem.IsLinux())
			{
				soundIn = new PulseSoundCapture();
			} else
			{
				// idk
				return;
			}

			soundIn.Initialize();
			var soundInSource = new SoundInSource(soundIn);
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

			soundIn.Start();
		}

		public class PulseSoundCapture : ISoundIn
		{
			public WaveFormat _waveFormat = new WaveFormat();
			public WaveFormat WaveFormat => _waveFormat;

			public RecordingState RecordingState => throw new NotImplementedException();

#pragma warning disable CS0067
			public event EventHandler<DataAvailableEventArgs> DataAvailable;
			public event EventHandler<RecordingStoppedEventArgs> Stopped;
#pragma warning restore CS0067

			public Process _ffmpeg;
			public bool ShouldStop = false;

			public void Dispose()
			{
				ShouldStop = true;
				_ffmpeg.Dispose();
			}

			public void Initialize()
			{
				//
			}

			public void Start()
			{
				_ffmpeg = Process.Start(new ProcessStartInfo("ffmpeg", Settings.FFMPEGArguments)
				{
					UseShellExecute = true,
					RedirectStandardError = true,
					RedirectStandardOutput = true,
					StandardOutputEncoding = Encoding.ASCII,
				});

				Task.Run(() =>
				{
					while (!ShouldStop)
					{
						byte[] bytes = Encoding.ASCII.GetBytes(_ffmpeg.StandardOutput.ReadToEnd());
						DataAvailable.Invoke(this, new(bytes, 0, bytes.Length, _waveFormat));
					}
				});
			}

			public void Stop()
			{
				Stopped.Invoke(this, new());
				Dispose();
			}
		}
	}
}
