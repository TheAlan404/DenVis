using System;
using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DenVis
{
	public static class WebSocketAPI
	{
		public static int Port = 4242;
		public static WebSocketServer Server;

		public static void Setup()
		{
			Server = new WebSocketServer($"ws://0.0.0.0:{Port}");
			Server.RestartAfterListenError = true;

			Server.Start(socket =>
			{
				socket.OnOpen += () =>
				{
					socket.Send(GetBanner());
				};

				socket.OnMessage += (text) =>
				{
					try
					{
						JObject data = JsonConvert.DeserializeObject<JObject>(text);
						HandleCommand(data);
					}
					catch (Exception any)
					{
						Console.WriteLine(any.ToString());
					}
				};
			});
		}

		public static void HandleCommand(JObject command)
		{
			string commandName = command["name"].ToString();
			JToken data = command["data"];

			switch (commandName)
			{
				case "SetYOffset":
					Renderer.yOffset = data.ToObject<float>();
					break;
				case "SetHeightMultiplier":
					Renderer.heightMultiplier = data.ToObject<float>();
					break;
				default:
					return;
			}
		}

		public static string GetBanner()
		{
			JObject root = new JObject();
			root["version"] = Program.DenVisVersion;
			root["screenW"] = Renderer.screenW;
			root["screenH"] = Renderer.screenH;
			root["win8"] = Program.IsWin8;

			return JsonConvert.SerializeObject(root);
		}
	}
}