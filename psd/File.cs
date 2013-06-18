using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace psd
{
    public class File : IDisposable
    {
        public class HeaderInfo
        {
            public int ChannelCount    { get { return this.channelCount; } }
            public int Width           { get { return this.width; } }
            public int Height          { get { return this.height; } }
            public int BitsPerPixel    { get { return this.bitsPerPixel; } }
            public ColorMode ColorMode { get { return this.colorMode; } }
            public int LayerCount      { get { return this.layerCount; } }

            internal int channelCount;
            internal int width, height;
            internal int bitsPerPixel;
            internal ColorMode colorMode;
            internal int layerCount;
        }

        public class LayerMask
        {
            public bool Enabled { get { return this.enabled; } }
            public bool Inverted { get { return this.inverted; } }
            public int Top          { get { return this.top; } }
            public int Left         { get { return this.left; } }
            public int Bottom       { get { return this.bottom; } }
            public int Right        { get { return this.right; } }
            public DefaultColor DefaultColor { get { return this.defaultColor; } }

            internal bool enabled;
            internal bool inverted;
            internal int top, left, bottom, right;
            internal DefaultColor defaultColor;
        }

        public class LayerHeaderInfo
        {
            public string Name      { get { return this.name; } }
            public int Top          { get { return this.top; } }
            public int Left         { get { return this.left; } }
            public int Bottom       { get { return this.bottom; } }
            public int Right        { get { return this.right; } }
            public int ChannelCount { get { return this.channelCount; } }
            public BlendMode Blend  { get { return this.blendMode; } }
            public int Opacity      { get { return this.opacity; } }
            public LayerMask Mask   { get { return this.layerMask; } }

            internal string name;
            internal int top, left, bottom, right;
            internal int channelCount;
            internal BlendMode blendMode;
            internal int opacity;
            internal bool visible;
            internal LayerMask layerMask;
        }


        public HeaderInfo Header { get { return this.header; } }
        public List<LayerHeaderInfo> LayerHeaderList { get { return this.layerHeaderList; } }


        public File() { }

        public File(string filename)
        {
            this.Open(filename);
        }

        public void Dispose()
        {
            this.Close();
        }

        public bool Open(string filename)
        {
            if (this.reader != null)
            {
                this.Close();
            }
            this.reader = new BinaryReader(new FileStream(filename, FileMode.Open), Encoding.ASCII);
            return (this.reader != null);
        }

        public void Parse()
        {
            this.header = this.ReadHeader();

            this.layerHeaderList = new List<LayerHeaderInfo>();
            for (int i = 0, layerCount = this.header.layerCount; i < layerCount; ++i)
            {
                LayerHeaderInfo info = this.ReadLayerHeader();
                this.layerHeaderList.Add(info);
            }
        }

        public void Close()
        {
            if (this.reader != null)
            {
                this.reader.Close();
                this.reader = null;
            }
        }


        private BinaryReader reader = null;
        private HeaderInfo header = null;
        private List<LayerHeaderInfo> layerHeaderList = null;

        private short ReadNextInt16()
        {
            byte[] data = this.reader.ReadBytes(2);
            Array.Reverse(data);
            return BitConverter.ToInt16(data, 0);
        }
        private int ReadNextInt32()
        {
            byte[] data = this.reader.ReadBytes(4);
            Array.Reverse(data);
            return BitConverter.ToInt32(data, 0);
        }
        private string ReadNextString(int length)
        {
            byte[] data = this.reader.ReadBytes(length);
            return Encoding.GetEncoding(932).GetString(data);
        }
        private static int Char4ToInt32(char[] key)
        {
            // http://stackoverflow.com/questions/3858908/convert-a-4-char-string-into-int32
            return (key[3] << 24) + (key[2] << 16) + (key[1] << 8) + key[0];
        }
        private static int Str4ToInt32(string keyStr)
        {
            char[] key = keyStr.ToCharArray(0, 4);
            return Char4ToInt32(key);
        }
        private Dictionary<int, BlendMode> blendModeDic = new Dictionary<int, BlendMode>()
        {
            { Str4ToInt32("pass"), BlendMode.PassThrough },
	        { Str4ToInt32("norm"), BlendMode.Normal },
	        { Str4ToInt32("diss"), BlendMode.Dissolve },
	        { Str4ToInt32("dark"), BlendMode.Darken },
	        { Str4ToInt32("mul "), BlendMode.Multiply },
	        { Str4ToInt32("idiv"), BlendMode.ColorBurn },
	        { Str4ToInt32("lbrn"), BlendMode.LinearBurn },
	        { Str4ToInt32("dkCl"), BlendMode.DarkerColor },
	        { Str4ToInt32("lite"), BlendMode.Lighten },
	        { Str4ToInt32("scrn"), BlendMode.Screen },
	        { Str4ToInt32("div "), BlendMode.ColorDodge },
	        { Str4ToInt32("lddg"), BlendMode.LinearDodge },
	        { Str4ToInt32("lgCl"), BlendMode.LighterColor },
	        { Str4ToInt32("over"), BlendMode.Overlay },
	        { Str4ToInt32("sLit"), BlendMode.SoftLight },
	        { Str4ToInt32("hLit"), BlendMode.HardLight },
	        { Str4ToInt32("vLit"), BlendMode.VividLight },
	        { Str4ToInt32("lLit"), BlendMode.LinearLight },
	        { Str4ToInt32("pLit"), BlendMode.PinLight },
	        { Str4ToInt32("hMix"), BlendMode.HardMix },
	        { Str4ToInt32("diff"), BlendMode.Difference },
	        { Str4ToInt32("smud"), BlendMode.Exclusion },
	        { Str4ToInt32("fsub"), BlendMode.Subtract },
	        { Str4ToInt32("fdiv"), BlendMode.Divide },
	        { Str4ToInt32("hue "), BlendMode.Hue },
	        { Str4ToInt32("sat "), BlendMode.Saturation },
	        { Str4ToInt32("colr"), BlendMode.Color },
	        { Str4ToInt32("lum "), BlendMode.Luminosity },
        };

        private HeaderInfo ReadHeader()
        {
            var header = new HeaderInfo();

            reader.ReadChars(4);// '8BPS'
            reader.ReadInt16();// version
            reader.ReadBytes(6);// reserved

            header.channelCount = this.ReadNextInt16();
            header.width = this.ReadNextInt32();
            header.height = this.ReadNextInt32();
            header.bitsPerPixel = this.ReadNextInt16();
            switch(this.ReadNextInt16())
            {
                case 0: header.colorMode = ColorMode.Mono; break;
                case 1: header.colorMode = ColorMode.GrayScale; break;
                case 3: header.colorMode = ColorMode.RGB; break;
                case 4: header.colorMode = ColorMode.CMYK; break;
                case 9: header.colorMode = ColorMode.Lab; break;
                default: header.colorMode = ColorMode.Unknown; break;
            }

            // Color Mode Data Section
            int blockSize = this.ReadNextInt32();
            reader.ReadBytes(blockSize);

            // Image Resources Section
            blockSize = this.ReadNextInt32();
            reader.ReadBytes(blockSize);

            // Layer and Mask Information Section
            reader.ReadBytes(4);// Whole Block Size
            reader.ReadBytes(4);// Layer Information Block Size

            header.layerCount = Math.Abs(this.ReadNextInt16());

            return header;
        }

        private LayerHeaderInfo ReadLayerHeader()
        {
            var header = new LayerHeaderInfo();

            header.top = this.ReadNextInt32();
            header.left = this.ReadNextInt32();
            header.bottom = this.ReadNextInt32();
            header.right = this.ReadNextInt32();
            header.channelCount = this.ReadNextInt16();

            // Channel information
            reader.ReadBytes(6 * header.channelCount);

            // Blend mode signature: "8BIM"
            char[] c = reader.ReadChars(4);

            int blendKey = Char4ToInt32(reader.ReadChars(4));
            this.blendModeDic.TryGetValue(blendKey, out header.blendMode);

            // Opacity. 0 = transparent ... 255 = opaque
            header.opacity = reader.ReadByte();

            // Clipping: 0 = base, 1 = non-base
            reader.ReadByte();

            //Flags:
            //bit 0 = transparency protected; bit 1 = visible; bit 2 = obsolete;
            //bit 3 = 1 for Photoshop 5.0 and later, tells if bit 4 has useful information;
            //bit 4 = pixel data irrelevant to appearance of document
            header.visible = ((reader.ReadByte() & 0x02) != 1);

            // Filler (zero)
            reader.ReadByte();

            // Length of the extra data field ( = the total length of the next five fields).
            int extraDataSize = this.ReadNextInt32();
            // あとで正しく読み飛ばすために位置を覚えとく
            long extraDataStartPos = reader.BaseStream.Position;

            int layerMaskDataSize = this.ReadNextInt32();
            if (layerMaskDataSize == 0)
            {
            }
            else if (layerMaskDataSize == 20)
            {
                reader.ReadBytes(20);
            }
            else if (layerMaskDataSize == 36)
            {
                reader.ReadBytes(36);
            }

            // Layer blending ranges data
            int blockSize = this.ReadNextInt32();
            reader.ReadBytes(blockSize);

            // Layer name: Pascal string, padded to a multiple of 4 bytes.
            int nameLength = reader.ReadByte();
            header.name = this.ReadNextString(nameLength);

            // Extra Data のところまで戻ってから読み飛ばす
            reader.BaseStream.Position = extraDataStartPos;
            reader.ReadBytes(extraDataSize);

            return header;
        }

    }
}
