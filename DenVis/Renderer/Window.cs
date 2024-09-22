using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DenVis.Renderer.WindowsNativeMethods;

namespace DenVis.Renderer
{
    public static class Window
    {
        public static WindowForm _form;
        public static Bitmap Bitmap;

        public static int ScreenWidth = Utils.GetDisplay().Item1;
        public static int ScreenHeight = Utils.GetDisplay().Item2;
        public static Size ScreenSize = new Size(ScreenWidth, ScreenHeight);

        public static void Setup()
        {
            _form = new WindowForm();
            Bitmap = new(ScreenWidth, ScreenHeight);

            //ApplicationConfiguration.Initialize();
            Application.Run(_form);

            while(true)
            {
                RenderTest();
                Render();
            }
        }

        public static void Render()
        {
            _form.Render(Bitmap, Settings.MasterOpacity);
        }

        public static void RenderTest()
        {
            using var g = Graphics.FromImage(Bitmap);
            g.Clear(Color.Transparent);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;
            g.DrawString(
                DateTime.Now.ToString("HH:mm:ss"),
                new Font(_form.Font.FontFamily, 60, FontStyle.Bold),
                Brushes.White,
                0, 0
            );
            
        }
    }

    public class WindowForm : Form
    {
        public WindowForm()
        {
            //FormBorderStyle = FormBorderStyle.FixedSingle;
            ShowInTaskbar = false;
            TopMost = true;
            Size = Window.ScreenSize;
            StartPosition = FormStartPosition.Manual;
            Location = new Point(0, 0);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.ExStyle |= WS_EX_TRANSPARENT;
                //createParams.ExStyle |= WS_EX_LAYERED;
                return createParams;
            }
        }

        public void Render(Bitmap bitmap, int opacity = 255)
        {
            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                throw new Exception(
                    "The bitmap must be 32bpp with alpha-channel.");
            }
            IntPtr screenDc = GetDC(IntPtr.Zero);
            IntPtr memDc = CreateCompatibleDC(screenDc);
            IntPtr hBitmap = IntPtr.Zero;
            IntPtr hOldBitmap = IntPtr.Zero;
            try
            {
                hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                hOldBitmap = SelectObject(memDc, hBitmap);
                SIZE newSize = new SIZE(bitmap.Width, bitmap.Height);
                POINT sourceLocation = new POINT(0, 0);
                POINT newLocation = new POINT(this.Left, this.Top);
                BLENDFUNCTION blend = new BLENDFUNCTION();
                blend.BlendOp = AC_SRC_OVER;
                blend.BlendFlags = 0;
                blend.SourceConstantAlpha = (byte)opacity;
                blend.AlphaFormat = AC_SRC_ALPHA;
                UpdateLayeredWindow(this.Handle, screenDc, ref newLocation,
                    ref newSize, memDc, ref sourceLocation, 0, ref blend, ULW_ALPHA);
            }
            finally
            {
                ReleaseDC(IntPtr.Zero, screenDc);
                if (hBitmap != IntPtr.Zero)
                {
                    SelectObject(memDc, hOldBitmap);
                    DeleteObject(hBitmap);
                }
                DeleteDC(memDc);
            }
        }
    }
}
