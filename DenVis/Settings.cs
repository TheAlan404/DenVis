using GameOverlay.Drawing;
using System;
using System.Linq;

namespace DenVis
{
	public static class Settings
	{
		public static bool Enabled = true;

		public static float HeightMultiplier = 100; // Program: im sensitive uwu
		public static bool IsOnTopOfTaskbar = true;

		public static bool UseDataHistory = true;
		public static int DataHistoryLength
		{
			get => Renderer.dataHistory.Capacity;
			set => Renderer.dataHistory.Capacity = value;
		}

		public static bool CenterVisualizer = true;

		public static float yOffset = 0;
		
		public static float HueChangeSpeed = 0.001f;
		public static float BassRange = 8;

		public static float Sensitivity = 0.0001f;

		public const string SnowflakeText = "*";
		public static class Snow
		{
			public static bool Enabled = false;

			public static int MaxFontSize = 30;
			public static int MinFontSize = 8;

			private static int _amount = 100;
			public static int Amount
			{
				get => _amount;
				set
				{
					_amount = value;
					SnowRenderer.UpdateCount();
				}
			}

			public static float BaseSinkSpeed = 0.6f;
			public static float FadeAmount = 0.01f;

			private static int _opacity = 128;
			public static int Opacity
			{
				get => _opacity;
				set
				{
					_opacity = value;
					Colors = Colors.Select(color =>
					{
						color.A = value / 255;
						return color;
					}).ToArray();
				}
			}

			

			public static Color[] Colors = new Color[]
			{
				new Color(170, 170, 204, Opacity),
				new Color(221, 221, 255, Opacity),
				new Color(204, 204, 221, Opacity),
				new Color(243, 243, 243, Opacity),
				new Color(240, 255, 255, Opacity),
			};
		}
	}
}