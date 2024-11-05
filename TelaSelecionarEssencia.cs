using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using NpcGen_Editor.Properties;
using NpcGen_Editor.sELedit;
using System;
using System.Collections;
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
    public partial class TelaSelecionarEssencia : DevExpress.XtraEditors.XtraForm
    {
        private int idx_lista;
        private int idx_name;
        private int idx_icone;
        private int idx_id;
        public int retId;
        class Item
        {
            public int id { get; set; }
            public string nome { get; set; }

            public Item(int id, string nome)
            {
                this.id = id;
                this.nome = nome;
            }
        }

        public TelaSelecionarEssencia(string nomeLista = "_ESSENCE")
        {
            InitializeComponent();
            idx_lista = -1;
            idx_icone = -1;
            idx_id = -1;
            idx_name = -1;
            PopularLista(nomeLista);
        }

        void PopularLista(string nomeLista)
        {
            if (TelaInicio.elc == null) return;
            for (int j = 0; j < TelaInicio.elc.Lists.Length; j++)
            {
                if (TelaInicio.elc.Lists[j].listName.Contains(nomeLista))
                {
                    int index = Convert.ToInt32(TelaInicio.elc.Lists[j].listName.Trim().Split('-')[0]) - 1;
                    idx_lista = index;
                    for (int i = 0; i < TelaInicio.elc.Lists[index].elementFields.Length; i++)
                    {
                        if (TelaInicio.elc.Lists[index].elementFields[i] == "ID")
                        {
                            idx_id = i;
                        }
                        if (TelaInicio.elc.Lists[index].elementFields[i] == "Name")
                        {
                            idx_name = i;
                        }
                        if (TelaInicio.elc.Lists[index].elementFields[i] == "file_icon" || TelaInicio.elc.Lists[index].elementFields[i] == "file_icon1")
                        {
                            idx_icone = i;
                        }
                        if (idx_id != -1 && idx_name != -1 && idx_icone != -1) { break; }
                    }

                    comboBoxEdit1.Properties.Items.Add(TelaInicio.elc.Lists[j]);
                    break;
                }
            }
            comboBoxEdit1.SelectedIndex = 0;
        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEdit1.SelectedIndex == -1) return;
            eList lista = (comboBoxEdit1.SelectedItem as eList);
            List<Item> listaItem = new List<Item>();
            for (int i = 0; i < lista.elementValues.Length; i++)
                listaItem.Add(new Item((int)lista.elementValues[i][idx_id], Encoding.Unicode.GetString((byte[])lista.elementValues[i][idx_name])));
            gridControl1.DataSource = listaItem;
        }

        private void comboBoxEdit1_Properties_CustomItemDisplayText(object sender, CustomItemDisplayTextEventArgs e)
        {
            if (comboBoxEdit1.SelectedIndex == -1) return;
            eList item = e.Item as eList;
            if (item != null)
            {
                e.DisplayText = item.listName;
            }
        }

        private void comboBoxEdit1_Properties_CustomDisplayText(object sender, DevExpress.XtraEditors.Controls.CustomDisplayTextEventArgs e)
        {
            eList item = comboBoxEdit1.SelectedItem as eList;
            if (item != null)
            {
                e.DisplayText = item.listName;
            }
        }

        private void gridView1_CustomDrawCell(object sender, DevExpress.XtraGrid.Views.Base.RowCellCustomDrawEventArgs e)
        {
            /*Item lista = gridView1.GetRow(e.RowHandle) as Item;
            if (e.Column.VisibleIndex == 0)
                e.DisplayText = lista.id.ToString();
            if (e.Column.VisibleIndex == 1)
                e.DisplayText = lista.nome;*/
        }

        private void searchControl1_QueryIsSearchColumn(object sender, QueryIsSearchColumnEventArgs args)
        {
        }

        private void searchControl1_QuerySearchParameters(object sender, DevExpress.Utils.SearchControlQueryParamsEventArgs e)
        {            
            
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            this.Close();
            DialogResult = DialogResult.None;
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            retId = (gridView1.FocusedRowObject as Item).id;
            this.Close();
            DialogResult = DialogResult.OK;
        }
    }
}