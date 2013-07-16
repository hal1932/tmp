using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace psdview
{
    public partial class MainForm : Form
    {
        public delegate void OnSetupCompleteCallback(bool result);

        private LayerController layerController = new LayerController();
        private Image image = null;

        private LayerForm layerForm = null;

        private string filename = null;


        public MainForm(string filename)
        {
            this.filename = filename;
            InitializeComponent();
            this.layerForm = new LayerForm(this);

            if (!Blender.Setup())
            {
                MessageBox.Show("レイヤ合成機能の初期化に失敗しました");
            }
        }


        public void SetImage(Image image)
        {
            if (this.pictureBox.InvokeRequired)
            {
                this.pictureBox.Invoke((MethodInvoker)delegate
                {
                    this.pictureBox.Image = image;
                    if (image != null)
                    {
                        this.pictureBox.Dock = DockStyle.None;
                    }
                    else
                    {
                        this.pictureBox.Dock = DockStyle.Fill;
                    }
                });
            }
            else
            {
                this.pictureBox.Image = image;
                if (image != null)
                {
                    this.pictureBox.Dock = DockStyle.None;
                }
                else
                {
                    this.pictureBox.Dock = DockStyle.Fill;
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // レイヤ結合済み画像を表示してお茶を濁してるあいだに、
            // 必要なレイヤ情報をひとつずつ全部読み込む
            this.layerController.LoadCombinedImage(this.filename, (image) =>
            {
                if (image == null)
                {
                    MessageBox.Show(string.Format("画像の読み込みに失敗しました: {0}", filename));
                }
                else
                {
                    var size = image.Size;// Size は struct だからそのままコピー
                    size.Height += this.statusStrip1.Height;
                    this.ClientSize = size;
                    this.Invoke((MethodInvoker)delegate
                    {
                        this.pictureBox.Image = image;
                        this.Text = Path.GetFileName(filename);
                        this.toolStripStatusLabel_Size.Text = string.Format("{0}x{1}", image.Width, image.Height);
                        this.toolStripStatusLabel_Format.Text = image.PixelFormat.ToString();
                    });

                    this.layerController.LoadAllLayer(filename, (layerInfo, layerImage) =>
                    {
                        this.layerForm.AddLayer(layerInfo, layerImage);
                    });
                }
            });
        }

        private void View_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.image != null)
            {
                this.image.Dispose();
                this.pictureBox.Image = null;
            }
        }

        private void toolStripMenuItem_OpenLayerWindow_Click(object sender, EventArgs e)
        {
            this.layerForm.Show();
        }

    }
}
