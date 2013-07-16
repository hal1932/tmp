using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace psdview
{
    public partial class LayerForm : Form
    {
        private MainForm parent = null;
        private Blender blender = null;

        class Layer
        {
            public psd.File.LayerHeaderInfo Info { get; set; }
            public Image Image { get; set; }
            public Image Thumb { get; set; }
            public Layer(psd.File.LayerHeaderInfo info, Image image, Image thumb)
            {
                this.Info = info;
                this.Image = image;
                this.Thumb = thumb;
            }
        }
        private Dictionary<int, Layer> layerDic = new Dictionary<int, Layer>();

        public LayerForm(MainForm parent)
        {
            InitializeComponent();
            this.parent = parent;
        }

        public void AddLayer(psd.File.LayerHeaderInfo layerInfo, Image layerImage)
        {
            var thumbnail = Converter.CreateThumbnail(40, 40, layerImage);

            if (this.dataGridView.InvokeRequired)
            {
                this.dataGridView.Invoke((MethodInvoker)delegate
                {
                    this.dataGridView.Rows.Add(layerInfo.Visible, thumbnail, layerInfo.Name);
                });
            }
            else
            {
                this.dataGridView.Rows.Add(layerInfo.Visible, thumbnail, layerInfo.Name);
            }

            this.layerDic[this.dataGridView.Rows.Count - 1] = new Layer(layerInfo, layerImage, thumbnail);
        }

        private void LayerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void dataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // デフォルトでチェックボックスの変更を検知できないから、自前でコミットする
            if (this.dataGridView.CurrentCellAddress.X == 0 && this.dataGridView.IsCurrentCellDirty)
            {
                this.dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            var rowList = this.dataGridView.Rows;
            var value = rowList[e.RowIndex].Cells[e.ColumnIndex].Value;

            switch (e.ColumnIndex)
            {
                case 0:// チェックボックス
                    var imageList = new List<Image>();
                    var infoList = new List<psd.File.LayerHeaderInfo>();
                    foreach (DataGridViewRow row in rowList)
                    {
                        bool visible = (bool)row.Cells[0].Value;
                        if (visible)
                        {
                            imageList.Add(this.layerDic[row.Index].Image);
                            infoList.Add(this.layerDic[row.Index].Info);
                        }
                    }
                    Blender.Blend(infoList, imageList, (image) =>
                    {
                        this.parent.SetImage(image);
                    });
                    break;
                default:
                    break;
            }
        }
    }
}
