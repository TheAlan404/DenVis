using System.Drawing;
using System;
using System.Runtime.InteropServices;

namespace DenVis
{
	public static class Utils
	{
		[DllImport("user32.dll")]
		static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

		[StructLayout(LayoutKind.Sequential)]
		struct DEVMODE
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
			public string dmDeviceName;
			public short dmSpecVersion;
			public short dmDriverVersion;
			public short dmSize;
			public short dmDriverExtra;
			public int dmFields;
			public int dmPositionX;
			public int dmPositionY;
			public int dmDisplayOrientation;
			public int dmDisplayFixedOutput;
			public short dmColor;
			public short dmDuplex;
			public short dmYResolution;
			public short dmTTOption;
			public short dmCollate;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 0x20)]
			public string dmFormName;
			public short dmLogPixels;
			public int dmBitsPerPel;
			public int dmPelsWidth;
			public int dmPelsHeight;
			public int dmDisplayFlags;
			public int dmDisplayFrequency;
			public int dmICMMethod;
			public int dmICMIntent;
			public int dmMediaType;
			public int dmDitherType;
			public int dmReserved1;
			public int dmReserved2;
			public int dmPanningWidth;
			public int dmPanningHeight;
		}

		public static (int, int) GetDisplay()
		{
			const int ENUM_CURRENT_SETTINGS = -1;

			DEVMODE devMode = default;
			devMode.dmSize = (short)Marshal.SizeOf(devMode);
			EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode);

			return (devMode.dmPelsWidth, devMode.dmPelsHeight);
		}




		/*

		public static Color SetHue(Color oldColor)
		{
			var temp = new HSV();
			temp.h = oldColor.GetHue();
			temp.s = oldColor.GetSaturation();
			temp.v = getBrightness(oldColor);
			return ColorFromHSL(temp);
		}

		// color brightness as perceived:
		public static float getBrightness(Color c)
		{ return (c.R * 0.299f + c.G * 0.587f + c.B * 0.114f) / 256f; }

		// A common triple float struct for both HSL & HSV
		// Actually this should be immutable and have a nice constructor!!
		public struct HSV { public float h; public float s; public float v; }

		// the Color Converter
		static public Color ColorFromHSL(HSV hsl)
		{
			if (hsl.s == 0)
			{ int L = (int)hsl.v; return Color.FromArgb(255, L, L, L); }

			double min, max, h;
			h = hsl.h / 360d;

			max = hsl.v < 0.5d ? hsl.v * (1 + hsl.s) : (hsl.v + hsl.s) - (hsl.v * hsl.s);
			min = (hsl.v * 2d) - max;
			
			Color c = Color.FromArgb(255, (int)(255 * RGBChannelFromHue(min, max, h + 1 / 3d)),
										  (int)(255 * RGBChannelFromHue(min, max, h)),
										  (int)(255 * RGBChannelFromHue(min, max, h - 1 / 3d)));
			return c;
		}

		static double RGBChannelFromHue(double m1, double m2, double h)
		{
			h = (h + 1d) % 1d;
			if (h < 0) h += 1;
			if (h * 6 < 1) return m1 + (m2 - m1) * 6 * h;
			else if (h * 2 < 1) return m2;
			else if (h * 3 < 2) return m1 + (m2 - m1) * 6 * (2d / 3d - h);
			else return m1;

		}

		// big brain but i dont want to lmao
		public static float OldHue;
		public static (float, float, float) AddHueToColor(float r, float b, float g)
		{
			Color c = Color.FromArgb(255, (int)r, (int)g, (int)b);

			Color ret = ColorFromHSL(new HSV()
			{
				h = ((c.GetHue()) + 1) % 100, // hue 100% değilmi
				s = c.GetSaturation(),
				v = c.GetBrightness(),
			});

			return ((float)ret.R, (float)ret.G, (float)ret.B);
		}*/
	}



	public static class ColorScale
	{
		public static Color ColorFromHSL(double h, double s, double l)
		{
			double r = 0, g = 0, b = 0;
			if (l != 0)
			{
				if (s == 0)
					r = g = b = l;
				else
				{
					double temp2;
					if (l < 0.5)
						temp2 = l * (1.0 + s);
					else
						temp2 = l + s - (l * s);

					double temp1 = 2.0 * l - temp2;

					r = GetColorComponent(temp1, temp2, h + 1.0 / 3.0);
					g = GetColorComponent(temp1, temp2, h);
					b = GetColorComponent(temp1, temp2, h - 1.0 / 3.0);
				}
			}
			return Color.FromArgb((int)(255 * r), (int)(255 * g), (int)(255 * b));

		}

		private static double GetColorComponent(double temp1, double temp2, double temp3)
		{
			if (temp3 < 0.0)
				temp3 += 1.0;
			else if (temp3 > 1.0)
				temp3 -= 1.0;

			if (temp3 < 1.0 / 6.0)
				return temp1 + (temp2 - temp1) * 6.0 * temp3;
			else if (temp3 < 0.5)
				return temp2;
			else if (temp3 < 2.0 / 3.0)
				return temp1 + ((temp2 - temp1) * ((2.0 / 3.0) - temp3) * 6.0);
			else
				return temp1;
		}
	}
}