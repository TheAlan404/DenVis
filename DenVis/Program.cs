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
using System.Collections.Generic;

/*
 
 /// TW: SHITCODE AHEAD
 
im sorry -dennis also thearmagan
 
 */

namespace DenVis
{
	public static class Program
	{
		public const string DenVisVersion = "a.0.1";

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
