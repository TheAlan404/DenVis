using GameOverlay.Drawing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DenVis
{
	public class Text
	{
		[JsonProperty("Text")]
		public string String = "DenVis";
		public string Id = Guid.NewGuid().ToString();
		public float X = 0;
		public float Y = 0;
		public int Size = 16;
		public bool HasBackground = true;
		public int Expire = -1;
	}

	public static class TextRenderer
	{
		public static Font Font;
		public static SolidBrush Brush;
		public static SolidBrush BackgroundBrush;

		public static List<Text> Texts = new List<Text>();

		public static void Setup(Graphics gfx)
		{
			Font = gfx.CreateFont("Arial", 16);
			Brush = gfx.CreateSolidBrush(255, 255, 255);
			BackgroundBrush = gfx.CreateSolidBrush(0, 0, 0, 128);
		}

		public static void Render(Graphics gfx)
		{
			foreach(Text text in Texts)
			{
				if (text.HasBackground)
				{
					gfx.DrawTextWithBackground(Font, text.Size, Brush, BackgroundBrush, text.X, text.Y, text.String);
				}
				else
				{
					gfx.DrawText(Font, text.Size, Brush, text.X, text.Y, text.String);
				}
			}
		}

		public static void Add(Text text)
		{
			Texts.Add(text);
			if(text.Expire > 0)
			{
				Task.Run(async () =>
				{
					await Task.Delay(text.Expire);
					Texts.Remove(text);
				});
			}
		}
	}
}
