using DevExpress.Utils.About;
using DevExpress.XtraEditors;
using NpcGen_Editor.Classes;
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

namespace NpcGen_Editor
{
    public partial class TelaObjetosDinamicos : DevExpress.XtraEditors.XtraForm
    {
        public int retId;
        public TelaObjetosDinamicos(List<DefaultInformation> lista)
        {
            InitializeComponent();
            gridControl1.DataSource = lista;
            gridControl1.RefreshDataSource();
        }

        private void gridView1_SelectionChanged(object sender, DevExpress.Data.SelectionChangedEventArgs e)
        {

        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            if (gridView1.SelectedRowsCount > 0)
            {
                retId = (gridView1.FocusedRowObject as DefaultInformation).Id;
                this.Close();
                DialogResult = DialogResult.OK;
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            Close();
            DialogResult = DialogResult.None;
        }

        private void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e)
        {
            List<DefaultInformation> info = gridView1.DataSource as List<DefaultInformation>;
            string DynScreenPath = string.Format("{0}\\DynamicObjects\\d{1}.jpg", Application.StartupPath, info[e.FocusedRowHandle].Id);
            if (File.Exists(DynScreenPath))
            {
                pictureEdit1.Image = Bitmap.FromFile(DynScreenPath);
            }
        }

        private void gridView1_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            DefaultInformation info = gridView1.FocusedRowObject as DefaultInformation;
            string DynScreenPath = string.Format("{0}\\DynamicObjects\\d{1}.jpg", Application.StartupPath, info.Id);
            if (File.Exists(DynScreenPath))
            {
                pictureEdit1.Image = Bitmap.FromFile(DynScreenPath);
            }

        }
    }
}