using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace CaffeineForWindows
{
    /// <summary>
    /// Draws the coffee cup icon in code, so the tray icon stays sharp at any DPI.
    /// build.ps1 also uses this class to write the .ico embedded in the exe.
    /// </summary>
    public static class IconFactory
    {
        [DllImport("user32.dll")]
        static extern bool DestroyIcon(IntPtr handle);

        public static Bitmap Draw(int size, bool active)
        {
            var bmp = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.Clear(Color.Transparent);

                // Artwork is drawn on a 32x32 grid and scaled to the requested size.
                g.ScaleTransform(size / 32f, size / 32f);
                g.TranslateTransform(1.75f, 0f);

                Color body = active ? Color.FromArgb(242, 169, 59) : Color.FromArgb(138, 143, 152);
                Color surface = active ? Color.FromArgb(107, 63, 29) : Color.FromArgb(200, 204, 211);
                Color saucer = active ? Color.FromArgb(201, 130, 43) : Color.FromArgb(110, 115, 124);

                using (var path = new GraphicsPath())
                {
                    path.AddLine(4.5f, 13f, 21.5f, 13f);
                    path.AddLine(21.5f, 13f, 19.5f, 24f);
                    path.AddBezier(19.5f, 24f, 19f, 26.5f, 17.5f, 27f, 16f, 27f);
                    path.AddLine(16f, 27f, 10f, 27f);
                    path.AddBezier(10f, 27f, 8.5f, 27f, 7f, 26.5f, 6.5f, 24f);
                    path.CloseFigure();
                    using (var brush = new SolidBrush(body))
                        g.FillPath(brush, path);
                }

                using (var pen = new Pen(body, 2.6f))
                    g.DrawArc(pen, 17.5f, 15f, 8f, 8f, -80f, 180f);

                using (var brush = new SolidBrush(surface))
                    g.FillEllipse(brush, 5.5f, 11.5f, 15f, 3.2f);

                using (var pen = new Pen(saucer, 2.4f))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawLine(pen, 3f, 29f, 23f, 29f);
                }

                if (active)
                {
                    using (var pen = new Pen(body, 1.8f))
                    {
                        pen.StartCap = LineCap.Round;
                        pen.EndCap = LineCap.Round;
                        foreach (float x in new[] { 9f, 13f, 17f })
                            g.DrawBezier(pen, x, 10f, x - 2.2f, 7.5f, x + 2.2f, 5.5f, x, 2.5f);
                    }
                }
            }
            return bmp;
        }

        public static Icon CreateIcon(int size, bool active)
        {
            using (var bmp = Draw(size, active))
            {
                IntPtr handle = bmp.GetHicon();
                try
                {
                    using (var temp = Icon.FromHandle(handle))
                        return (Icon)temp.Clone();
                }
                finally
                {
                    DestroyIcon(handle);
                }
            }
        }

        /// <summary>Writes a multi-size .ico file with PNG-compressed images.</summary>
        public static void WriteIco(string path, bool active)
        {
            int[] sizes = { 16, 20, 24, 32, 40, 48, 64, 128, 256 };
            var images = new byte[sizes.Length][];
            for (int i = 0; i < sizes.Length; i++)
            {
                using (var bmp = Draw(sizes[i], active))
                using (var ms = new MemoryStream())
                {
                    bmp.Save(ms, ImageFormat.Png);
                    images[i] = ms.ToArray();
                }
            }

            using (var w = new BinaryWriter(File.Create(path)))
            {
                w.Write((ushort)0);
                w.Write((ushort)1);
                w.Write((ushort)sizes.Length);

                int offset = 6 + 16 * sizes.Length;
                for (int i = 0; i < sizes.Length; i++)
                {
                    w.Write((byte)(sizes[i] >= 256 ? 0 : sizes[i]));
                    w.Write((byte)(sizes[i] >= 256 ? 0 : sizes[i]));
                    w.Write((byte)0);
                    w.Write((byte)0);
                    w.Write((ushort)1);
                    w.Write((ushort)32);
                    w.Write(images[i].Length);
                    w.Write(offset);
                    offset += images[i].Length;
                }
                foreach (var image in images)
                    w.Write(image);
            }
        }

        public static void WritePng(string path, int size, bool active)
        {
            using (var bmp = Draw(size, active))
                bmp.Save(path, ImageFormat.Png);
        }
    }
}
