using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace psdview
{
    class Blender
    {
        public delegate void OnBlendCompleteCallback(Image image);


        private static BlenderImpl Impl { get; set; }

        public static bool Setup()
        {
            BlenderImpl impl = new BlenderImpl();
            if (!impl.Setup())
            {
                return false;
            }
            Blender.Impl = impl;
            return true;
        }

        public static void Blend(List<psd.File.LayerHeaderInfo> infoList, List<Image> imageList, OnBlendCompleteCallback callback)
        {
            // レイヤの下から順に、ブレンドするレイヤを拾う
            var targetList = new List<psd.File.LayerHeaderInfo>();
            infoList.Reverse();
            foreach (psd.File.LayerHeaderInfo layer in infoList)
            {
                // imageList の中身はレイヤ順に並んでるから、layer.Index でそのまま取り出す
                targetList.Add(layer);
            }

            if (targetList.Count == 0)
            {
                callback(null);
                return;
            }

            // 実際にブレンド
            var bitmap = new Bitmap(imageList[targetList[0].Index - 1].Clone() as Image);
            for (int i = 1, count = targetList.Count; i < count; ++i)
            {
                var info = targetList[i];
                var image = imageList[info.Index - 1];
                Blender.Blend_(ref bitmap, image, info.Blend);
            }

            callback(bitmap);
        }

        private static void Blend_(ref Bitmap src, Image dst, psd.BlendMode mode)
        {
            using (var g = Graphics.FromImage(src))
            {
                g.DrawImage(dst, 0, 0);
            }
        }
    }
}
