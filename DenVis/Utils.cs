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

		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		static extern int GetSystemMetrics(int smIndex);

		public const int SM_CXSCREEN = 0;
		public const int SM_CYSCREEN = 1;

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool GetWindowRect(IntPtr hWnd, out W32RECT lpRect);

		[StructLayout(LayoutKind.Sequential)]
		public struct W32RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;
		}

		[DllImport("user32.dll")]
		static extern IntPtr GetDesktopWindow();

		public static bool IsForegroundWindowFullScreen()
		{
			int scrX = GetSystemMetrics(SM_CXSCREEN),
				scrY = GetSystemMetrics(SM_CYSCREEN);

			IntPtr handle = GetForegroundWindow();
			if (handle == IntPtr.Zero) return false;
			if (GetDesktopWindow() == handle) return false;

			W32RECT wRect;
			if (!GetWindowRect(handle, out wRect)) return false;

			return scrX == (wRect.Right - wRect.Left) && scrY == (wRect.Bottom - wRect.Top);
		}
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