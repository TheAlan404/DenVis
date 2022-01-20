using GameOverlay.Drawing;
using System;
using System.Collections.Generic;

namespace DenVis
{
	public static class SnowRenderer
	{
		public static Random Random = new Random();

		public static List<SolidBrush> Brushes = new List<SolidBrush>();
		public static Font[] Fonts;


		public static List<Snowflake> Snowflakes = new List<Snowflake>();

		public static void Setup(Graphics gfx)
		{
			Fonts = new Font[]
			{
					gfx.CreateFont("Times", 16),
					gfx.CreateFont("Arial", 16),
					gfx.CreateFont("Verdana", 16)
			};

			foreach (Color color in Settings.Snow.Colors)
			{
				Brushes.Add(gfx.CreateSolidBrush(color));
			}

			for (int i = 0; i < Settings.Snow.Amount; i++)
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
				gfx.DrawText(Fonts[flake.FontIndex], flake.Size, Brushes[flake.BrushIndex], flake.X, flake.Y, Settings.SnowflakeText);
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
				Size = Random.Next(Settings.Snow.MaxFontSize - Settings.Snow.MinFontSize) + Settings.Snow.MinFontSize;

				FontIndex = Random.Next(Fonts.Length);
				BrushIndex = Random.Next(Brushes.Count);
				SinkSpeed = Settings.Snow.BaseSinkSpeed * Size / 5;
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
