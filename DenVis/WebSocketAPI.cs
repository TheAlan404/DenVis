using System;
using System.Linq;
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

			Server.Start((socket) =>
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
							HandleCommand(socket, data);
						}
						catch (Exception any)
						{
							JObject root = new JObject();
							root["error"] = true; // maybe
							root["name"] = "error";
							root["errortype"] = "backend";
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

		public static void HandleCommand(IWebSocketConnection socket, JObject command)
		{
			string commandName = command["name"].ToString();
			JToken data = command["data"];

			// TODO: switch to attributes etc idk
			switch (commandName)
			{
				case "SetSettings":
					JObject sets = (JObject)data;
					Settings.FromJSON(sets);
					Program.SaveConfiguration();
					break;
				case "ResetSettings":
					Settings.ResetToDefaults();
					Program.SaveConfiguration();
					WaveRenderer.AddWave(new WaveRenderer.Wave()
					{
						Stroke = 10,
						Opacity = 1
					});
					socket.Send(GetBanner()); // Refresh the settings (known bug)
					break;
				case "AddText":
					TextRenderer.Add(data.ToObject<Text>());
					break;
				case "AddMultipleTexts":
					JArray texts = (JArray)data;
					foreach (JValue value in texts)
					{
						TextRenderer.Add(value.ToObject<Text>());
					}
					break;
				case "RemoveText":
					string id = (string)data;
					var text = TextRenderer.Texts.Where(t => t.Id == id).First();
					if (text == null) return;
					TextRenderer.Texts.Remove(text);
					break;
				case "RemoveMultipleTexts":
					JArray idlist = (JArray)data;
					foreach (JValue value in idlist)
					{
						var atext = TextRenderer.Texts.Where(t => t.Id == (string)value).First();
						if (atext == null) continue;
						TextRenderer.Texts.Remove(atext);
					}
					break;
				case "TriggerWave":
					WaveRenderer.AddWave();
					break;
				case "SetColor":
					JArray clr = (JArray)data;
					Settings.ColorR = (float)clr[0];
					Settings.ColorG = (float)clr[1];
					Settings.ColorB = (float)clr[2];
					break;
				case "ResetCache":
					Renderer.bassIntensityHistory = new List<float>(Renderer.bassIntensityHistory.Capacity);
					Renderer.dataHistory = new List<float>(Renderer.dataHistory.Capacity);
					break;
				default:
					return;
			}
		}

		public static void Broadcast(string eventName, JToken data = null)
		{
			JObject obj = new JObject();
			obj["name"] = eventName;
			obj["data"] = data;
			Broadcast(obj);
		}
		public static void Broadcast(JToken token) => Broadcast(JsonConvert.SerializeObject(token));
		public static void Broadcast(string msg)
		{
			foreach(var client in Clients)
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
			data["settings"] = Settings.ToJSONWithTypes();

			root["data"] = data;
			root["name"] = "welcome";

			return JsonConvert.SerializeObject(root);
		}
	}
}