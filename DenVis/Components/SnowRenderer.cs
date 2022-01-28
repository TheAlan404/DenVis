using GameOverlay.Drawing;
using System;
using System.Collections.Generic;

namespace DenVis
{
	public static class SnowRenderer
	{
		public static Random Random = new Random();
		public static List<SolidBrush> Brushes;
		public static Font[] Fonts;

		public static List<Snowflake> Snowflakes;

		public static Color[] Colors = new Color[]
		{
			new Color(170, 170, 204, Settings.SnowOpacity),
			new Color(221, 221, 255, Settings.SnowOpacity),
			new Color(204, 204, 221, Settings.SnowOpacity),
			new Color(243, 243, 243, Settings.SnowOpacity),
			new Color(240, 255, 255, Settings.SnowOpacity),
		};

		public static void Setup(Graphics gfx)
		{
			Brushes = new List<SolidBrush>();
			Snowflakes = new List<Snowflake>();
			Fonts = new Font[]
			{
					gfx.CreateFont("Times", 16),
					gfx.CreateFont("Arial", 16),
					gfx.CreateFont("Verdana", 16)
			};

			foreach (Color color in Colors)
			{
				Brushes.Add(gfx.CreateSolidBrush(color));
			}

			for (int i = 0; i < Settings.SnowAmount; i++)
			{
				Snowflakes.Add(new Snowflake(gfx));
			}
		}

		public static void UpdateCount()
		{
			Graphics gfx = Renderer.graphicsWindow.Graphics;
		}

		public static void Render(Graphics gfx)
		{
			foreach (Snowflake flake in Snowflakes)
			{
				gfx.DrawText(Fonts[flake.FontIndex], flake.Size, Brushes[flake.BrushIndex], flake.X, flake.Y, "*");
			}
		}

		public static void MoveSnow(Graphics gfx, float extra = 0)
		{
			for (int i = 0; i < Snowflakes.Count; i++)
			{
				Snowflake flake = Snowflakes[i];
				if (flake.OnGround)
				{
					// todo: fade away before regen

					// cant set it in one line because { get; set; } bruh.
					//Color tmp = flake.Brush.Color;
					//tmp.A -= Settings.Snow.FadeAmount;
					//flake.Brush.Color = tmp;
					//if (tmp.A <= 0)
					//{
					flake.Regenerate(gfx);
					continue;
					//}
					//continue;
				}
				flake.coord += flake.x_mv;
				flake.Y += flake.SinkSpeed + extra;
				flake.X = flake.InitialX + flake.lftrght * (float)Math.Sin(flake.coord);
				if (flake.Y >= (Renderer.ScreenBottom - flake.Size))
				{
					flake.OnGround = true;
				}
			}
		}




		public class Snowflake
		{
			public int FontIndex;
			// TODO: i just realized we can override font size in Graphics#DrawText,
			// so remove this and get preinitialized font etc instead of initializing one for every snowflake
			public float InitialX;
			public float X;
			public float Y;
			public int Size;
			public int BrushIndex;
			public float SinkSpeed;
			public bool OnGround = false;

			// idk what these are for...
			public float coord;
			public float x_mv;
			public float lftrght;

			public Snowflake(Graphics gfx)
			{
				Regenerate(gfx); // lol
			}

			public void Regenerate(Graphics gfx)
			{
				try
				{
					Size = Random.Next(Settings.SnowMaxFontSize - Settings.SnowMinFontSize) + Settings.SnowMinFontSize;
				}
				catch (Exception)
				{
					Size = Settings.SnowMaxFontSize;
				}

				FontIndex = Random.Next(Fonts.Length);
				BrushIndex = Random.Next(Brushes.Count);
				SinkSpeed = Settings.SnowBaseSinkSpeed * Size / 5;
				InitialX = Random.Next(Renderer.screenW);
				X = InitialX;
				Y = -Size;

				OnGround = false;

				// ???
				coord = 0;
				x_mv = 0.03f + (float)Random.NextDouble() / 10;
				lftrght = (float)Random.NextDouble() * 15;
			}
		}
	}
}
