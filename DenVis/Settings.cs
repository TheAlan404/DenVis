using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DenVis
{
	public class Settings
	{
		[Setting("Enable Visualizer", true, "Enables the sound bar", "Features", Shortcut = 'v')]
		public static bool Enabled = true;

		[Setting("Bass Waves", false, "[BETA] Enable sound waves", "Features", Shortcut = 'b')]
		public static bool EnableBassWaves = false;

		[Setting("Enable Snow", false, "Make it rain snow", "Features", Shortcut = 's')]
		public static bool SnowEnabled = false;

		// Category: Position

		[Setting("Height Multiplier", 100, 1, 1000, "Specifies the height of the bar")]
		public static int HeightMultiplier = 100; // Program: im sensitive uwu
		[Setting("Show on top of Taskbar", true, "Shows the bar on top of the Taskbar when it is visible")]
		public static bool IsOnTopOfTaskbar = true;

		[Setting("Y Offset", 0, -1000, 1000, "Specifies the offset of the visualizer")]
		public static float yOffset = 0;

		// Category: Visuals

		[Setting("Enable Rainbow Mode", true, "Makes the visualizer gay")]
		public static bool RainbowMode = true;

		[Setting("HueChangePerCoord", 5, "", Min = 1, Max = 200, Step = 1)]
		public static float HueChangePerCoord = 5;

		[Setting("HueChangeAmount", 0.01f, "", Min = 0, Max = 1, Step = 0.001f)]
		public static float HueChangeAmount = 0.01f;

		private static float _opacity = 0.7f;
		[Setting("Opacity", 0.7f, 0, 1, "Set how visible it should be")]
		public static float Opacity
		{
			get => IsReady() ? Renderer.Brush.Color.A : _opacity;
			set => Renderer.SetColor(-1, -1, -1, value);
		}

		[Setting("Stroke", 5, 1, 20)]
		public static int Stroke = 5;

		[WebSocketIgnoreSetting]
		public static float ColorR
		{
			get => Renderer.Brush.Color.R;
			set => Renderer.SetColor(value, -1, -1, -1);
		}

		[WebSocketIgnoreSetting]
		public static float ColorG
		{
			get => Renderer.Brush.Color.G;
			set => Renderer.SetColor(-1, value, -1, -1);
		}

		[WebSocketIgnoreSetting]
		public static float ColorB
		{
			get => Renderer.Brush.Color.B;
			set => Renderer.SetColor(-1, -1, value, -1);
		}

		[Setting("Check for updates", true, "Checks for updates when DenVis starts", "Maintenance")]
		public static bool CheckForUpdates = true;

		public static string FFMPEGArguments = ""; // Linux

		// Category: Advanced

		[Setting("Use Data History", true, "Use an array of previous floats to calculate maximum value", "Advanced")]
		public static bool UseDataHistory = true;
		[Setting("Data History Length", 30, 1, 900, "30 = 1 second because DenVis is 30 FPS", "Advanced")]
		public static int DataHistoryLength
		{
			get => Renderer.dataHistory.Capacity;
			set
			{
				Renderer.dataHistory = new List<float>(value);
			}
		}

		[Setting("[BETA] Use Bars", false, "", "Advanced")]
		public static bool Bars = false;

		[Setting("SkipValueAmount", 1, 1, 20)]
		public static int SkipValueAmount = 1;

		[Setting("SkipValuesSum", true)]
		public static bool SkipValuesSum = true;

		[Setting("Show FPS", false, "", "Advanced")]
		public static bool ShowFPS = false;

		private static int _fps = 30;
		[Setting("[Experimental] FPS", 30, "[BETA] Unstable", "Advanced", Min = 1, Max = 240)]
		public static int FPS
		{
			get => IsReady() ? Renderer.graphicsWindow.FPS : _fps;
			set
			{
				if (IsReady()) Renderer.graphicsWindow.FPS = value;
				_fps = value;
			}
		}

		// Category: Bass

		[Setting("Range of Bass", 36, 0, 500, "[BETA] If 'Use Bass Range' is on, this will be used for the range to calculate bass", "Bass")]
		public static int BassRange = 36;

		[Setting("Bass sensitivity", 0.6, 0, 1, "[BETA] Percentage of loudness that determines if the current audio is bass depending on the history of bass values", "Bass", Step = 0.01f)]
		public static float BassSensitivity = 0.6f;

		[Setting("Color Bass Range Differiently", false, "[BETA] Colors selected bass range to black. Useful when setting 'Range of Bass'", "Bass")]
		public static bool ColorBassDifferiently = false;

		[Setting("Wave Opacity", 0.5f, 0, 1, "[BETA] Starting opacity for a wave", "Bass-Animation")]
		public static float WaveOpacity = 0.5f;

		[Setting("_WavePosYSubtract", 5, 0, 30, "[BETA] ", "Bass-Animation", Step = 1)]
		public static float _WavePosYSubtract = 5;

		[Setting("Wave Opacity Subtract", 0.03f, 0, 1, "[BETA] Amount to subtract every frame from wave opacity", "Bass-Animation", Step = 0.01f)]
		public static float _WaveOpacitySubtract = 0.03f;

		[Setting("Bass Check Start", true, "[BETA] Dont allow trails by checking if the previus frame was bass", "Bass-Animation")]
		public static bool _BassCheckStart = true;

		[Setting("Use Bass Range", true, "[BETA] true = use bass range, false = calculate based on full spectrum", "Bass")]
		public static bool _BassUseBassRange = true;

		[Setting("_bassIntensityHistory", 100, 1, 900, "[BETA] 30 = 1 second because DenVis is 30 FPS", "Bass")]
		public static int _bassIntensityHistory
		{
			get => Renderer.bassIntensityHistory.Capacity;
			set
			{
				Renderer.bassIntensityHistory = new List<float>(value);
			}
		}

		// Category: Sound

		//[Setting("Sensitivity", 0.0001f, 0, 1, "Threshold to render sound", "Sound", Step = 0.00001f)]
		[WebSocketIgnoreSetting]
		public static float Sensitivity = 0.0001f;

		// Category: Side Flash

		[Setting("Side Flash Speed", 60, 0, 255, "How fast should the side flash fade out?", "SideFlash")]
		public static int SideFlashSpeed = 60;

		[Setting("Side Flash Width", 0.5, 0, 1, "Width of the side flashes", "SideFlash", Step = 0.01f)]
		public static float SideFlashWidthRatio = 0.5f;

		// Category: Snow

		[Setting("Max Font Size", 30, 1, 100, "", "Snow")]
		public static int SnowMaxFontSize = 30;

		[Setting("Min Font Size", 8, 1, 100, "", "Snow")]
		public static int SnowMinFontSize = 8;

		private static int _snowAmount = 100;
		[Setting("Amount", 100, 1, 10000, "Amount of visible snow", "Snow")]
		public static int SnowAmount
		{
			get => _snowAmount;
			set
			{
				_snowAmount = value;
				SnowRenderer.UpdateCount();
			}
		}

		[Setting("Base Sink Speed", 0.6f, 0, 2, "", "Snow", Step = 0.1f)]
		public static float SnowBaseSinkSpeed = 0.6f;

		[WebSocketIgnoreSetting]
		public static float SnowFadeAmount = 0.01f;

		private static int _snowOpacity = 128;
		[Setting("Opacity", 128, 0, 255, "Set the opacity of the snow", "Snow")]
		public static int SnowOpacity
		{
			get => _snowOpacity;
			set
			{
				_snowOpacity = value;
				SnowRenderer.Colors = SnowRenderer.Colors.Select(color =>
				{
					color.A = value / 255;
					return color;
				}).ToArray();
			}
		}





		// --- Saving / Loading ---

		public static bool IsReady() =>
			Renderer.TCSReady.Task.IsCompleted;


		public static JObject ToJSON()
		{
			void SerializeFields(Type t, JObject ret)
			{
				foreach (FieldInfo fi in t.GetFields())
				{
					var o = fi.GetValue(null);
					var type = o.GetType();
					if (type == typeof(bool))
					{
						ret[fi.Name] = (bool)o;
					}
					else if (type == typeof(int))
					{
						ret[fi.Name] = (int)o;
					}
					else if (type == typeof(float))
					{
						ret[fi.Name] = (float)o;
					}
					else if (type == typeof(string))
					{
						ret[fi.Name] = (string)o;
					}
				}
			}

			void SerializeProperties(Type t, JObject ret)
			{
				foreach (PropertyInfo pi in t.GetProperties())
				{
					Console.WriteLine(pi);
					var o = pi.GetValue(null);
					var type = o.GetType();
					if (type == typeof(bool))
					{
						ret[pi.Name] = (bool)o;
					}
					else if (type == typeof(int))
					{
						ret[pi.Name] = (int)o;
					}
					else if (type == typeof(float))
					{
						ret[pi.Name] = (float)o;
					}
					else if (type == typeof(string))
					{
						ret[pi.Name] = (string)o;
					}
				}
			}

			JObject root = new JObject();
			SerializeFields(typeof(Settings), root);
			SerializeProperties(typeof(Settings), root);

			return root;
		}

		public static void FromJSON(JObject sets)
		{
			foreach (var kvp in sets)
			{
				try
				{
					TrySetValue<Settings>(kvp.Key, kvp.Value);
				}
				catch (Exception)
				{
					continue;
				}
			}

			void TrySetValue<T>(string key, JToken value)
			{
				PropertyInfo pi = typeof(T).GetProperty(key);
				if (pi == null)
				{
					FieldInfo fi = typeof(T).GetField(key);
					if (fi == null)
					{
						return;
					}
					fi.SetValue(null, Convert.ChangeType(value, fi.FieldType));
				}
				else
				{
					pi.SetValue(null, Convert.ChangeType(value, pi.PropertyType));
				}
			}

			WebSocketAPI.Broadcast("SettingsChange", ToJSONWithTypes());
		}

		public static JObject ToJSONWithTypes()
		{
			JObject SerializeSetting(Type type, SettingAttribute attr, object objValue)
			{
				var value = Convert.ChangeType(objValue, type);
				JObject obj = attr == null ? new JObject() : JObject.FromObject(attr);
				obj["Type"] = type.Name;
				obj["Value"] = JToken.FromObject(value);
				return obj;
			}

			void SerializeFields(Type t, JObject ret)
			{
				foreach (FieldInfo fi in t.GetFields())
				{
					if (fi.GetCustomAttribute<WebSocketIgnoreSettingAttribute>() != null) continue;
					var o = fi.GetValue(null);
					var type = o.GetType();
					ret[fi.Name] = SerializeSetting(type, fi.GetCustomAttribute<SettingAttribute>(), fi.GetValue(null));
				}
			}

			void SerializeProperties(Type t, JObject ret)
			{
				foreach (PropertyInfo pi in t.GetProperties())
				{
					if (pi.GetCustomAttribute<WebSocketIgnoreSettingAttribute>() != null) continue;
					var o = pi.GetValue(null);
					var type = o.GetType();
					ret[pi.Name] = SerializeSetting(type, pi.GetCustomAttribute<SettingAttribute>(), pi.GetValue(null));
				}
			}

			JObject root = new JObject();
			SerializeFields(typeof(Settings), root);
			SerializeProperties(typeof(Settings), root);

			return root;
		}

		public static void ResetToDefaults()
		{
			void ResetFields(Type t)
			{
				foreach (FieldInfo fi in t.GetFields())
				{
					var o = fi.GetValue(null);
					var type = o.GetType();
					if (fi.GetCustomAttribute<WebSocketIgnoreSettingAttribute>() != null) continue;
					if (fi.GetCustomAttribute<SettingAttribute>() == null) continue;
					fi.SetValue(null, Convert.ChangeType(fi.GetCustomAttribute<SettingAttribute>().Default, fi.FieldType));
				}
			}

			void ResetProperties(Type t)
			{
				foreach (PropertyInfo pi in t.GetProperties())
				{
					var o = pi.GetValue(null);
					var type = o.GetType();
					if (pi.GetCustomAttribute<WebSocketIgnoreSettingAttribute>() != null) continue;
					if (pi.GetCustomAttribute<SettingAttribute>() == null) continue;
					pi.SetValue(null, Convert.ChangeType(pi.GetCustomAttribute<SettingAttribute>().Default, pi.PropertyType));
				}
			}

			ResetFields(typeof(Settings));
			ResetProperties(typeof(Settings));

			WebSocketAPI.Broadcast("SettingsChange", ToJSONWithTypes());
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public class SettingAttribute : Attribute
	{
		public string Name;
		public object Default;
		public string Description;
		public string Category;
		public float Min;
		public float Max;
		public float Step;
		public char Shortcut;

		public SettingAttribute(string name, object _default, string description = "", string category = "Visualizer")
		{
			Name = name;
			Default = _default;
			Description = description;
			Category = category;
		}

		public SettingAttribute(string name, object _default, float min, float max, string description = "", string category = "Visualizer")
		{
			Name = name;
			Default = _default;
			Min = min;
			Max = max;
			Description = description;
			Category = category;
		}
	}

	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = false, AllowMultiple = true)]
	public class WebSocketIgnoreSettingAttribute : Attribute
	{
		public WebSocketIgnoreSettingAttribute()
		{
			
		}
	}
}