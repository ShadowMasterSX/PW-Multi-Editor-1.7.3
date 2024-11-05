using NpcGen_Editor.Classes;
using NpcGen_Editor.sELedit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NpcGen_Editor
{
    public partial class Main : Form
    {
        public static NpcGen npcgen;
        public static eListCollection elc;

        public Main() { InitializeComponent(); }

        private void buttonEdit1_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        { OpenFileDialog("npcgen.data"); }

        private void npcgenData_EditValueChanged(object sender, EventArgs e)
        {
        }

        void OpenFileDialog(string text)
        {
            ofd.FileName = text;
            ofd.Title = string.Format("Procurar por {0}", text);
            var result = ofd.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                txtNpcgen.Text = ofd.FileName;
            }
        }

        private async void simpleButton1_Click(object sender, EventArgs e)
        {
            // Chama LoadELEMENTS e espera a conclusão
            await LoadELEMENTS(txtElements.Text);

            // Chama LoadNPCGEN depois que LoadELEMENTS termina
            await LoadNPCGEN(txtNpcgen.Text);

            ConfigureNPCGEN();
        }

        public static async Task LoadELEMENTS(string path)
        {
            await Task.Run(() =>
            {
                Console.WriteLine("Carregando elements.data");
                elc = new eListCollection(path);
            });

            Console.WriteLine("elements.data carregado com sucesso!");
        }

        #region NPCGEN

        public static async Task LoadNPCGEN(string path)
        {
            await Task.Run(() =>
            {
                Console.WriteLine("Carregando npcgen.data");
                npcgen = new NpcGen();
                npcgen.ReadNpcgen(path);
            });

            Console.WriteLine("npcgen.data carregado com sucesso!");
        }

        public void ConfigureNPCGEN()
        {
            List<NpcMonster> lista = new List<NpcMonster>();
            for(int i = 0; i < npcgen.NpcMobsAmount; i++)
            {
                DefaultMonsters mob = npcgen.NpcMobList[i];
                string id = "";
                string name = "";
                for(int o = 0; o < mob.MobDops.Count; o++)
                {
                    if (o > 0)
                    {
                        id += ",";
                        name += ",";
                    }
                    id += mob.MobDops[o].Id.ToString();
                    name += elc.GetItemName(mob.MobDops[o].Id);
                }
                lista.Add(new NpcMonster(int.Parse(id), name));
            }
            tableNpcgenMobNpc.DataSource = lista;
        }

        #endregion

        private void gridView1_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            if (e.RowHandle < 0) { return; }
            var mob = npcgen.NpcMobList[e.RowHandle];
            List<NpcMonster> lista = new List<NpcMonster>();
            foreach (var m in mob.MobDops)
            {
                string name = elc.GetItemName(m.Id);
                lista.Add(new NpcMonster(m.Id, name));
            }
            tableMobNpc.DataSource = lista;
        }

        private void dropDownButton1_Click(object sender, EventArgs e)
        {

        }

        private void checkEdit1_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void checkEdit3_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void gridView2_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            if (e.RowHandle < 0) { return; }
            var mob = npcgen.NpcMobList[gridView1.FocusedRowHandle].MobDops[e.RowHandle];
            Console.WriteLine(mob.Amount.ToString());
        }
    }
}
