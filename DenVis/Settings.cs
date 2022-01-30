using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DenVis
{
	public class Settings
	{
		[Setting("Enable Visualizer", true, "Enables the sound bar")]
		public static bool Enabled = true;

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

		[Setting("Hue Change Speed", 0.001f, "Fine-tuning might be needed")]
		public static float HueChangeSpeed = 0.001f;

		private static float _opacity = 0.5f;
		[Setting("Opacity", 0.5f, 0, 1, "Set how visible it should be")]
		public static float Opacity
		{
			get => Renderer.TCSReady.Task.IsCompleted ? Renderer.Brush.Color.A : _opacity;
			set => Renderer.SetColor(-1, -1, -1, value);
		}

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

		[Setting("(unused) Range of Bass", 100, 0, 4096, "FFT", "Sound")]
		public static int BassRange = 100;

		[Setting("Sensitivity", 0.0001f, 0, 1, "Threshold to render sound", "Sound", Step = 0.00001f)]
		public static float Sensitivity = 0.0001f;

		// Category: Snow

		[Setting("Enable Snow", false, "Make it rain snow", "Snow")]
		public static bool SnowEnabled = false;

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