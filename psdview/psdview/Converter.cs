using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace psdview
{
    class Converter
    {
        public static Image CreateThumbnail(int width, int height, Image image)
        {
            var result = new Bitmap(width, height);
            using (var g = Graphics.FromImage(result))
            {
                g.DrawImage(Properties.Resources.minigrid, 0, 0);
                g.DrawImage(image, 0, 0, width, height);
            }
            return result;
        }

        public static Image LoadImage(string filename, int width = 0, int height = 0)
        {
            var infile = string.Format(@"{0}[0]", filename);
            var outfile = string.Format(@"{0}\{1}.png", Env.Instance.TempDirectory, Path.GetFileName(filename));
            if (Converter.Convert_(infile, outfile, width, height))
            {
                return Image.FromFile(outfile);
            }
            return null;
        }

        public delegate void OnLoadLayerComplete(psd.File.LayerHeaderInfo layerInfo, Image image);

        public static Image LoadLayer(psd.File.HeaderInfo header, psd.File.LayerHeaderInfo layerInfo)
        {
            var infile = string.Format(@"{0}[{1}]", header.FileName, layerInfo.Index);
            var outfile = string.Format(@"{0}\{1}_{2}.png", Env.Instance.TempDirectory, Path.GetFileName(header.FileName), layerInfo.Index);
            Converter.Convert_(infile, outfile);

            var bitmap = new Bitmap(header.Width, header.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                var rect = new Rectangle(
                    layerInfo.Left, layerInfo.Top,
                    layerInfo.Right - layerInfo.Left, layerInfo.Bottom - layerInfo.Top
                    );
                g.DrawImage(Image.FromFile(outfile), rect);
            }
            //bitmap.Save(Path.GetDirectoryName(outfile) + "\\_" + Path.GetFileName(outfile));

            return bitmap;
        }


        private static bool Convert_(
            string infile, string outfile,
            int width = 0, int height = 0,
            int ofsX = 0, int ofsY = 0,
            string background = "",
            string transparent = "",
            bool repage = false,
            bool flatten = false
            )
        {
            var command = string.Format(@"{0}\{1}", Env.Instance.CurrentDirectory, @"external\convert.exe");

            var args = new List<string>();
            if(background.Length > 0)
            {
                args.Add("-background");
                args.Add(background);
            }
            if (transparent.Length > 0)
            {
                args.Add("-transparent");
                args.Add(transparent);
            }
            if (width > 0 && height > 0 && ofsX > 0 && ofsY > 0)
            {
                args.Add("-page");
                args.Add(string.Format("{0}x{1}+{2}+{3}", width, height, ofsX, ofsY));
            }
            if(flatten)
            {
                args.Add("-flatten");
            }
            if (width > 0 && height > 0)
            {
                args.Add("-geometry");
                args.Add(string.Format("{0}x{1}", width, height));
            }
            args.Add(string.Format(@"""{0}""", infile));
            if(repage)
            {
                args.Add("+repage");
            }
            args.Add(string.Format(@"""{0}""", outfile));

            using (Process p = new Process())
            {
                p.StartInfo.FileName = command;
                p.StartInfo.Arguments = string.Join(" ", args);
                
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = true;

                if (!p.Start())
                {
                    return false;
                }
                p.WaitForExit();
            }

            return true;
        }
    }
}
