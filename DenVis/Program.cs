using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CSCore.DSP;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;


/*
 
 /// TW: SHITCODE AHEAD
 
im sorry -dennis also thearmagan
 
 */

namespace DenVis
{
	public static class Program
	{
		public const string DenVisVersion = "a.0.4";

		public static bool IsWin8 = Environment.OSVersion.Version.Major == 6;

		public static string ConfigFilepath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
		public static string ConfigFilename = ".DenVisConfiguration.json";

		public const FftSize fftSize = FftSize.Fft4096;
		public static FftProvider fftProvider;

		public static NotifyIcon trayIcon;

		public static void Main()
		{
#if DEBUG
			Utils.AllocConsole();
#endif
			Utils.EnsureSingleton();
			Console.WriteLine($"Screen is ({Renderer.screenW}w, {Renderer.screenH}h)");

			SetupTrayIcon();

			SoundCapture.Setup();
			WebSocketAPI.Setup();
			Renderer.Setup();

			Task.Delay(-1).Wait();
		}

		public static void SetupTrayIcon()
		{
			trayIcon = new NotifyIcon();
			trayIcon.Icon = new("DenVis.ico");
			trayIcon.Text = $"DenVis {DenVisVersion}";
			trayIcon.ContextMenuStrip = new();
			trayIcon.ContextMenuStrip.Items.Add("Settings", null, (_sender, _args) =>
			{
				OpenURL("");
			});
			trayIcon.ContextMenuStrip.Items.Add("Exit", null, (_sender, _args) =>
			{
				Renderer.graphicsWindow.Dispose();
				trayIcon.Dispose();
				Environment.Exit(0);
			});
			EventHandler handler = (_sender, _args) => OpenURL("");
			trayIcon.Click += handler;
			trayIcon.DoubleClick += handler;
			trayIcon.Visible = true;
		}

		public static async Task CheckUpdates()
		{
			try
			{
				Console.WriteLine($"Checking for updates...");
				HttpClient client = new HttpClient();
				client.DefaultRequestHeaders.Add("User-Agent", "DenVis-App");
				var response = await client.GetAsync("https://api.github.com/repos/TheAlan404/DenVis/releases");
				var body = await response.Content.ReadAsStringAsync();
				Console.WriteLine(body);
				JArray releases = JArray.Parse(body);

				string latestVersion = (string)releases[0]["tag_name"];
				latestVersion = latestVersion.Replace("alpha", "a");

				Console.WriteLine($"Fetched Latest Version = {latestVersion}");
				if (latestVersion != DenVisVersion)
				{
					OpenURL("update.html");
					//trayIcon.BalloonTipClicked += (_sender, _args) =>
					//{
					//	OpenURL("update.html");
					//};
					//trayIcon.ShowBalloonTip(3000, "DenVis Update", $"New update available: {latestVersion}\nClick to update!", ToolTipIcon.Info);
				}
			} catch(Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		public static void OpenURL(string url)
		{
			Process.Start(new ProcessStartInfo(url.StartsWith("http") ? url : $"https://denvis.glitch.me/{url}")
			{
				UseShellExecute = true,
			});
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
