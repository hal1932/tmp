using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace psdview
{
    class LayerController
    {
        public Image MainImage { get; set; }
        public List<Image> LayerImageList { get; set; }
        bool LoadLayerComplete { get; set; }

        public delegate void OnLoadImageComplete(Image image);
        public delegate void OnLoadLayerComplete(psd.File.LayerHeaderInfo layerInfo, Image layerImage);

        public LayerController()
        {
            this.MainImage = null;
            this.LayerImageList = new List<Image>();
            this.LoadLayerComplete = false;
        }

        public void LoadCombinedImage(string filename, OnLoadImageComplete callback)
        {
            var image = Converter.LoadImage(filename);
            callback(image);
        }

        public void LoadAllLayer(string filename, OnLoadLayerComplete callback)
        {
            psd.File.HeaderInfo header = null;
            List<psd.File.LayerHeaderInfo> layerInfoList = null;

            using (psd.File file = new psd.File(filename))
            {
                file.Parse();
                header = file.Header;
                layerInfoList = file.LayerHeaderList;
            }

            foreach (var layerInfo in layerInfoList)
            {
                var layerImage = Converter.LoadLayer(header, layerInfo);
                callback(layerInfo, layerImage);
            }
        }
    }
}
