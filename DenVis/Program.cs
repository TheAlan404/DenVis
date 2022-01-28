using System;
using System.IO;
using System.Threading.Tasks;
using CSCore.DSP;
using Newtonsoft.Json.Linq;


/*
 
 /// TW: SHITCODE AHEAD
 
im sorry -dennis also thearmagan
 
 */

namespace DenVis
{
	public static class Program
	{
		public const string DenVisVersion = "a.0.3";

		public static bool IsWin8 = Environment.OSVersion.Version.Major == 6;

		public static string ConfigFilepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		public static string ConfigFilename = ".DenVisConfiguration.json";

		// Shared
		public const FftSize fftSize = FftSize.Fft4096;
		public static FftProvider fftProvider;

		public static void Main()
		{
			Console.WriteLine($"Screen is ({Renderer.screenW}w, {Renderer.screenH}h)");

			// TODO: command line args
			//LoadConfiguration(); // moved to Renderer.cs because of how Settings.Opacity works (maybe a TODO?)

			SoundCapture.Setup();
			WebSocketAPI.Setup();
			Renderer.Setup();

			Task.Delay(-1).Wait();
		}

		public static void LoadConfiguration()
		{
			string fn = ConfigFilepath + "/" + ConfigFilename;
			if (!File.Exists(fn)) return;
			try
			{
				string content = File.ReadAllText(fn);
				JObject config = JObject.Parse(content);
				Settings.FromJSON(config);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
			}
		}

		public static void SaveConfiguration()
		{
			string fn = ConfigFilepath + "/" + ConfigFilename;
			string content = Settings.ToJSON().ToString();
			File.WriteAllText(fn, content);
		}
	}
}
