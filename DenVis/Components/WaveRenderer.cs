using GameOverlay.Drawing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DenVis
{
	public static class WaveRenderer
	{
		public static SolidBrush Brush;
		public static List<Wave> Waves = new List<Wave>();
		public static int Length = 200;

		public static void Setup(Graphics gfx)
		{
			Brush = gfx.CreateSolidBrush(255, 255, 255, 255);
		}

		public static void Render(Graphics gfx)
		{
			foreach (Wave wave in Waves)
			{
				for(int i = 1; i == 1; i--)
				{
					Brush.SetColor(-1, -1, -1, wave.Opacity);
					gfx.DrawLine(
						//Brush
						Brush,
						// start x
						0,
						// start y
						wave.Y + i,
						// end x
						Renderer.screenW,
						// end y
						wave.Y + i,
						// stroke
						wave.Stroke
					);
				}
				wave.Y -= Settings._WavePosYSubtract;
				wave.Opacity -= Settings._WaveOpacitySubtract;
			}
			Waves = Waves.Where(w => w.Opacity > 0).ToList();
		}

		public static void AddWave()
		{
			Waves.Add(new Wave());
		}

		public static void AddWave(Wave wave)
		{
			Waves.Add(wave);
		}

		public class Wave
		{
			public float Y = Renderer.screenH - (Renderer.ScreenOffset + Settings.yOffset + (Renderer.IsFullscreen ? 0 : Settings.IsOnTopOfTaskbar ? Renderer.TaskbarHeight : 0));
			public float Opacity = Settings.WaveOpacity;
			public float Stroke = 5;
		}
	}
}
