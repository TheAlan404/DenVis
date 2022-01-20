using System;
using System.Collections.Generic;
using Fleck;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DenVis
{
	public static class WebSocketAPI
	{
		public static int Port = 4242;
		public static WebSocketServer Server;
		public static List<IWebSocketConnection> Clients = new List<IWebSocketConnection>();

		public static void Setup()
		{
			Server = new WebSocketServer($"ws://0.0.0.0:{Port}");
			Server.RestartAfterListenError = true;

			Server.Start(socket =>
			{
				socket.OnOpen += () =>
				{
					Clients.Add(socket);
					socket.Send(GetBanner());
				};

				socket.OnMessage += (text) =>
				{
					try
					{
						JObject data = JsonConvert.DeserializeObject<JObject>(text);
						try
						{
							HandleCommand(data);
						}
						catch (Exception any)
						{
							JObject root = new JObject();
							root["error"] = true; // maybe
							root["name"] = "error";
							root["data"] = JsonConvert.SerializeObject(any);

							socket.Send(JsonConvert.SerializeObject(data));
						}
					}
					catch (Exception any)
					{
						Console.WriteLine(any.ToString());
					}
				};

				socket.OnClose += () =>
				{
					Clients.Remove(socket);
				};
			});
		}

		public static void HandleCommand(JObject command)
		{
			string commandName = command["name"].ToString();
			JToken data = command["data"];

			// TODO: switch to attributes etc idk
			switch (commandName)
			{
				case "SetYOffset":
					Settings.yOffset = data.ToObject<float>();
					break;
				case "SetHeightMultiplier":
					Settings.HeightMultiplier = data.ToObject<float>();
					break;
				case "EnableSnow":
					Settings.Snow.Enabled = true;
					break;
				case "DisableSnow":
					Settings.Snow.Enabled = false;
					break;
				case "SetSnowAmount":
					Settings.Snow.Amount  = data.ToObject<int>();
					break;
				case "SetSnowBaseSinkSpeed":
					Settings.Snow.BaseSinkSpeed = data.ToObject<float>();
					break;
				case "SetSnowOpacity":
					Settings.Snow.Opacity = data.ToObject<int>();
					break;
				case "AddText":
					TextRenderer.Add(data.ToObject<Text>());
					break;
				case "AddMultipleTexts":
					JArray texts = (JArray)data;
					foreach(JValue value in texts)
					{
						TextRenderer.Add(value.ToObject<Text>());
					}
					break;
				default:
					return;
			}
		}

		public static void Broadcast(JToken token) => Broadcast(JsonConvert.SerializeObject(token));
		public static void Broadcast(string msg)
		{
			foreach(IWebSocketConnection client in Clients)
			{
				client.Send(msg);
			}
		}

		public static string GetBanner()
		{
			JObject root = new JObject();

			JObject data = new JObject();
			data["version"] = Program.DenVisVersion;
			data["screenW"] = Renderer.screenW;
			data["screenH"] = Renderer.screenH;
			data["win8"] = Program.IsWin8;

			root["data"] = data;
			root["name"] = "welcome";

			return JsonConvert.SerializeObject(data);
		}
	}
}