using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace psdview
{
    class Env
    {
        public static Env Instance { get { return instance; } }

        public string TempDirectory { get { return this.tempDirectory; } }
        public string CurrentDirectory { get { return this.currentDirectory; } }

        public static void Setup()
        {
            if (Env.instance == null)
            {
                Env.instance = new Env();

                string tmpdir = Env.Instance.TempDirectory;
                if (!Directory.Exists(tmpdir))
                {
                    Directory.CreateDirectory(tmpdir);
                }
            }
        }

        public static void Shutdown()
        {
            string tmpdir = Env.Instance.TempDirectory;
            if (Directory.Exists(tmpdir))
            {
                // TODO: 完成したらコメントアウトはずす
                //Directory.Delete(tmpdir, true);
            }
        }


        private static Env instance = null;
        private string tempDirectory = null;
        private string currentDirectory = null;

        private Env()
        {
            this.tempDirectory = string.Format(@"{0}\tmp", Environment.CurrentDirectory);
            this.currentDirectory = Environment.CurrentDirectory;
        }
    }
}
