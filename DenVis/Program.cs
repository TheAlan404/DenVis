using System;
using System.IO;
using CSCore.DSP;

/*
 
 /// TW: SHITCODE AHEAD
 
im sorry -dennis also thearmagan
 
 */

namespace DenVis
{
	public static class Program
	{
		public const string DenVisVersion = "a.0.2";

		public static bool IsWin8 = Environment.OSVersion.Version.Major == 6;

		// Shared
		public const FftSize fftSize = FftSize.Fft4096;
		public static FftProvider fftProvider;

		public static void Main()
		{
			Console.WriteLine($"Screen is ({Renderer.screenW}w, {Renderer.screenH}h)");

			SoundCapture.Setup();
			WebSocketAPI.Setup();
			Renderer.Setup();
		}
	}
}
