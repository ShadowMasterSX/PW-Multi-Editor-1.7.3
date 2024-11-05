using DevExpress.LookAndFeel;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using ELFSharp.ELF;
using ELFSharp.UImage;
using NpcGen_Editor.Classes;
using NpcGen_Editor.GSEditor;
using NpcGen_Editor.MapTools.Precinct_Editor;
using NpcGen_Editor.Properties;
using NpcGen_Editor.sELedit;
using PCKEngine;
using pwAdminLocal.DDSReader;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static DevExpress.Utils.Frames.FrameHelper;
using static DevExpress.XtraEditors.Mask.Design.MaskSettingsForm.DesignInfo.MaskManagerInfo;
using static NpcGen_Editor.Classes.NpcGen;
using static NpcGen_Editor.GSEditor.Signatures;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace NpcGen_Editor
{
    public partial class TelaInicio : DevExpress.XtraEditors.XtraForm
    {
        // NPCGen Editor
        public static eListCollection elc;
        public static NpcGen npcgen;
        List<MapLoadedInformation> LoadedMapConfigs;
        PCKStream streamsurfaces;
        TelaMapa telaMapa;
        public List<DefaultInformation> DynamicsListEn;
        public List<GameMapInfo> Maps;

        // Precinct Editor
        CELPrecinctSet precinct;

        // GS Editor
        private Data_Handling data;
        private byte[] dataAllFile;

        // ElementsEditor
        public struct ImagePosition
        {
            public string name;
            public Point pos;

            public ImagePosition(string name, Point pos)
            {
                this.name = name;
                this.pos = pos;
            }
        }
        public eListConversation conversationList;
        private bool loadedElementsData = false;
        private bool loadedSurfaces = false;
        private bool loadedConfigs = false;
        public static SortedList<int, string> imagesx_m;
        public static SortedList<int, string> imagesx_f;
        public static SortedList<int, string> imagesx_skill;
        public static SortedList<int, ImagePosition> imageposition_m;
        public static SortedList<int, ImagePosition> imageposition_f;
        public static SortedList<int, ImagePosition> imageposition_skill;
        public static SortedDictionary<string, Bitmap> imagesCache_m = new SortedDictionary<string, Bitmap>();
        public static SortedDictionary<string, Bitmap> imagesCache_f = new SortedDictionary<string, Bitmap>();
        public static SortedDictionary<string, Bitmap> imagesCache_skill = new SortedDictionary<string, Bitmap>();
        public static Bitmap iconlist_ivtrm { get; set; }
        public static Bitmap iconlist_ivtrf { get; set; }
        public static Bitmap iconlist_skill { get; set; }
        public static Dictionary<int, int> ItemColor = new Dictionary<int, int>();
        public static SortedList<int, string> ItemDescription = new SortedList<int, string>();
        public static SortedList<int, string> item_desc = new SortedList<int, string>();

        public TelaInicio()
        {
            InitializeComponent();
            DynamicsListEn = new List<DefaultInformation>();
            if (File.Exists(Application.StartupPath + "\\DynObjectInfo.EN"))
            {
                StreamReader sr = new StreamReader(Application.StartupPath + "\\DynObjectInfo.EN");
                while (true)
                {
                    var k = sr.ReadLine().Split(new string[] { "->" }, StringSplitOptions.None);
                    DefaultInformation di = new DefaultInformation() { Id = Convert.ToInt32(k[0]), Name = k[1] };
                    DynamicsListEn.Add(di);
                    if (sr.EndOfStream == true)
                        break;
                }
            }
            LoadedMapConfigs = new List<MapLoadedInformation>();
            if (File.Exists(Application.StartupPath + "\\Maps.conf"))
            {
                StreamReader sr = new StreamReader(Application.StartupPath + "\\Maps.conf");
                while (true)
                {
                    var k = sr.ReadLine().Split(new string[] { "->" }, StringSplitOptions.None);
                    MapLoadedInformation di = new MapLoadedInformation() { Name = k[0] };
                    string[] Sizes = k[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    int.TryParse(Sizes[0], out di.Width);
                    int.TryParse(Sizes[1], out di.Height);
                    LoadedMapConfigs.Add(di);
                    if (sr.EndOfStream == true)
                        break;
                }
            }
        }


        private async void carregarDatas_Click(object sender, EventArgs e)
        {
            // Chama LoadELEMENTS e espera a conclusão
            if (File.Exists(txtElements.Text))
            {
                await LoadELEMENTS(txtElements.Text);
            }
            else
                XtraMessageBox.Show("Não foi possível encontrar o arquivo elements.data");

            // Chama LoadNPCGEN depois que LoadELEMENTS termina
            if (File.Exists(txtNpcgen.Text))
            {
                await LoadNPCGEN(txtNpcgen.Text);

                ConfigureNPCGEN();
            }
            else
                XtraMessageBox.Show("Não foi possível encontrar o arquivo npcgen.data");
        }

        private void DynamicObjectDefault_EnterPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                DynamicObjectDefaultLeave(sender, null);
        }

        private void DynamicObjectDefaultLeave(object sender, EventArgs e)
        {
            var dyn = gridView5.FocusedRowObject as DynamicObject;
            if (dyn == null)
                return;
            Control c = sender as Control;
            if (c == null)
                return;

            switch (c.Name)
            {
                case "dynPosX":
                    {
                        dyn.X_position = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "dynPosY":
                    {
                        dyn.Y_position = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "dynPosZ":
                    {
                        dyn.Z_position = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "dynRot":
                    {
                        dyn.Rotation = Convert.ToByte((c as SpinEdit).Value);
                        break;
                    }
                case "dynIncl1":
                    {
                        dyn.InCline1 = Convert.ToByte((c as SpinEdit).Value);
                        break;
                    }
                case "dynIncl2":
                    {
                        dyn.InCline2 = Convert.ToByte((c as SpinEdit).Value);
                        break;
                    }
                case "dynId":
                    {
                        dyn.Id = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "dynTriggerID":
                    {
                        dyn.TriggerId = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "dynScale":
                    {
                        dyn.Scale = Convert.ToByte((c as SpinEdit).Value);
                        break;
                    }
            }
        }

        private void gridView1_CustomColumnDisplayText(
            object sender,
            DevExpress.XtraGrid.Views.Base.CustomColumnDisplayTextEventArgs e)
        {
        }

        private void gridView1_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            ColumnView view = (ColumnView)sender;
            DefaultMonsters mob = view.GetRow(e.RowHandle) as DefaultMonsters;
            string id = string.Empty;
            string name = string.Empty;
            for (int o = 0; o < mob.MobDops.Count; o++)
            {
                if (o > 0)
                {
                    id += ",";
                    name += ",";
                }
                id += mob.MobDops[o].Id.ToString();
                name += mob.Type == 0 ? elc.GetItemName(mob.MobDops[o].Id, "MONSTER_ESSENCE") : elc.GetItemName(mob.MobDops[o].Id, "NPC_ESSENCE");
            }
            if (e.Column.VisibleIndex == 0)
            {
                e.DisplayText = id;
            }
            if (e.Column.VisibleIndex == 1)
            {
                if (mob.Type == 1)
                    e.Appearance.ForeColor = Color.FromArgb(181, 48, 0);
                e.DisplayText = name;
            }
        }

        private void gridView1_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0)
            {
                return;
            }
            if (gridView1.FocusedRowObject == null)
                return;
            var mob = gridView1.FocusedRowObject as DefaultMonsters;
            tableMobNpc.DataSource = mob.MobDops;
            if (mob.MobDops.Count > 0)
            {
                gridView2.FocusedRowHandle = 0;
                selectMobInSubList(mob.MobDops.First());
            }
            selectMobInMainList(mob);
        }

        private void gridView1_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            if (e.RowHandle < 0)
            {
                return;
            }
            var mob = gridView1.FocusedRowObject as DefaultMonsters;
            tableMobNpc.DataSource = mob.MobDops;
            if (mob.MobDops.Count > 0)
            {
                gridView2.FocusedRowHandle = 0;
                selectMobInSubList(mob.MobDops.First());
            }
            selectMobInMainList(mob);
        }

        private void gridView2_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            ColumnView view = (ColumnView)sender;
            ExtraMonsters mob = view.GetRow(e.RowHandle) as ExtraMonsters;
            string name = string.Empty;
            name = cbMobTipo.SelectedIndex == 0 ? elc.GetItemName(mob.Id, "MONSTER_ESSENCE") : elc.GetItemName(mob.Id, "NPC_ESSENCE");
            if (e.Column.VisibleIndex == 0)
            {
                e.DisplayText = mob.Id.ToString();
            }
            if (e.Column.VisibleIndex == 1)
            {
                e.DisplayText = name;
            }
        }

        private void gridView2_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0)
            {
                return;
            }
            if (gridView2.FocusedRowObject == null)
                return;
            var mob = gridView2.FocusedRowObject as ExtraMonsters;
            selectMobInSubList(mob);
        }

        private void gridView2_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            if (e.RowHandle < 0)
            {
                return;
            }
            var mob = gridView2.FocusedRowObject as ExtraMonsters;
            selectMobInSubList(mob);
        }

        private void gridView3_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            ColumnView view = (ColumnView)sender;
            ExtraResources res = view.GetRow(e.RowHandle) as ExtraResources;
            string name = string.Empty;
            name += elc.GetItemName(res.Id, "MINE_ESSENCE");
            if (e.Column.VisibleIndex == 0)
            {
                e.DisplayText = res.Id.ToString();
            }
            if (e.Column.VisibleIndex == 1)
            {
                e.DisplayText = name;
            }
        }

        private void gridView3_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0)
            {
                return;
            }
            if (gridView3.FocusedRowObject == null)
                return;
            var res = gridView3.FocusedRowObject as ExtraResources;
            selectResourceInSubList(res);
        }

        private void gridView3_RowClick(object sender, RowClickEventArgs e)
        {
            if (e.RowHandle < 0)
            {
                return;
            }
            var res = gridView3.FocusedRowObject as ExtraResources;
            selectResourceInSubList(res);
        }

        private void gridView4_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            ColumnView view = (ColumnView)sender;
            DefaultResources res = view.GetRow(e.RowHandle) as DefaultResources;
            string id = string.Empty;
            string name = string.Empty;
            for (int o = 0; o < res.ResExtra.Count; o++)
            {
                if (o > 0)
                {
                    id += ",";
                    name += ",";
                }
                id += res.ResExtra[o].Id.ToString();
                name += elc.GetItemName(res.ResExtra[o].Id, "MINE_ESSENCE");
            }
            if (e.Column.VisibleIndex == 0)
            {
                e.DisplayText = id;
            }
            if (e.Column.VisibleIndex == 1)
            {
                e.DisplayText = name;
            }
        }

        private void gridView4_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0)
            {
                return;
            }
            if (gridView4.FocusedRowObject == null)
                return;
            var res = gridView4.FocusedRowObject as DefaultResources;
            tableResources.DataSource = res.ResExtra;
            if (res.ResExtra.Count > 0)
            {
                gridView3.FocusedRowHandle = 0;
                selectResourceInSubList(res.ResExtra.First());
            }
            selectResourceInMainList(res);
        }

        private void gridView4_RowClick(object sender, DevExpress.XtraGrid.Views.Grid.RowClickEventArgs e)
        {
            if (e.RowHandle < 0)
            {
                return;
            }
            var res = gridView4.FocusedRowObject as DefaultResources;
            tableResources.DataSource = res.ResExtra;
            if (res.ResExtra.Count > 0)
            {
                gridView3.FocusedRowHandle = 0;
                selectResourceInSubList(res.ResExtra.First());
            }
            selectResourceInMainList(res);
        }

        private void gridView5_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            ColumnView view = (ColumnView)sender;
            DynamicObject res = view.GetRow(e.RowHandle) as DynamicObject;
            string name = "?";
            int j = DynamicsListEn.FindIndex(z => z.Id == res.Id);
            if (j != -1)
                name = DynamicsListEn[j].Name;
            if (e.Column.VisibleIndex == 0)
            {
                e.DisplayText = res.Id.ToString();
            }
            if (e.Column.VisibleIndex == 1)
            {
                e.DisplayText = name;
            }
        }

        private void gridView5_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0)
            {
                return;
            }
            if (gridView5.FocusedRowObject == null)
                return;
            var res = gridView5.FocusedRowObject as DynamicObject;
            selectDynamicObjectInMainList(res);
        }

        private void gridView5_RowClick(object sender, RowClickEventArgs e)
        {
            if (e.RowHandle < 0)
            {
                return;
            }
            var dyn = gridView5.FocusedRowObject as DynamicObject;
            selectDynamicObjectInMainList(dyn);
        }

        private void gridView6_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            ColumnView view = (ColumnView)sender;
            Trigger trigger = view.GetRow(e.RowHandle) as Trigger;
            if (e.Column.VisibleIndex == 0)
            {
                e.DisplayText = trigger.Id.ToString();
            }
            if (e.Column.VisibleIndex == 1)
            {
                e.DisplayText = trigger.GmID.ToString();
            }
            if (e.Column.VisibleIndex == 2)
            {
                e.DisplayText = trigger.TriggerName;
            }
        }

        private void gridView6_RowClick(object sender, RowClickEventArgs e)
        {
            if (e.RowHandle < 0)
            {
                return;
            }
            var trigger = gridView6.FocusedRowObject as Trigger;
            selectTriggerInMainList(trigger);
        }

        private void gridView7_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            ColumnView view = (ColumnView)sender;
            DefaultMonsters mob = view.GetRow(e.RowHandle) as DefaultMonsters;
            string id = string.Empty;
            string name = string.Empty;
            for (int o = 0; o < mob.MobDops.Count; o++)
            {
                if (o > 0)
                {
                    id += ",";
                    name += ",";
                }
                id += mob.MobDops[o].Id.ToString();
                name += mob.Type == 0 ? elc.GetItemName(mob.MobDops[o].Id, "MONSTER_ESSENCE") : elc.GetItemName(mob.MobDops[o].Id, "NPC_ESSENCE");
            }
            if (e.Column.VisibleIndex == 0)
            {
                e.DisplayText = id;
            }
            if (e.Column.VisibleIndex == 1)
            {
                if (mob.Type == 1)
                    e.Appearance.ForeColor = Color.FromArgb(181, 48, 0);
                e.DisplayText = name;
            }
        }

        private void gridView7_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (gridView7.FocusedRowObject != null)
                {
                    DefaultMonsters mob = gridView7.FocusedRowObject as DefaultMonsters;
                    List<DefaultMonsters> defaultMonsters = gridView1.DataSource as List<DefaultMonsters>;
                    int index = defaultMonsters.IndexOf(mob);
                    if (index != -1)
                    {
                        xtraTabControl1.SelectedTabPageIndex = 0;
                        gridView1.FocusedRowHandle = index;
                    }
                }
            }
            catch
            {
            }
        }

        private void gridView8_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            ColumnView view = (ColumnView)sender;
            DefaultResources res = view.GetRow(e.RowHandle) as DefaultResources;
            string id = string.Empty;
            string name = string.Empty;
            for (int o = 0; o < res.ResExtra.Count; o++)
            {
                if (o > 0)
                {
                    id += ",";
                    name += ",";
                }
                id += res.ResExtra[o].Id.ToString();
                name += elc.GetItemName(res.ResExtra[o].Id, "MINE_ESSENCE");
            }
            if (e.Column.VisibleIndex == 0)
            {
                e.DisplayText = id;
            }
            if (e.Column.VisibleIndex == 1)
            {
                e.DisplayText = name;
            }
        }

        private void gridView8_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (gridView8.FocusedRowObject != null)
                {
                    DefaultResources res = gridView8.FocusedRowObject as DefaultResources;
                    List<DefaultResources> defaultResources = gridView4.DataSource as List<DefaultResources>;
                    int index = defaultResources.IndexOf(res);
                    if (index != -1)
                    {
                        xtraTabControl1.SelectedTabPageIndex = 1;
                        gridView4.FocusedRowHandle = index;
                    }
                }
            }
            catch
            {
            }
        }

        private void gridView9_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            ColumnView view = (ColumnView)sender;
            DynamicObject res = view.GetRow(e.RowHandle) as DynamicObject;
            string name = "?";
            int j = DynamicsListEn.FindIndex(z => z.Id == res.Id);
            if (j != -1)
                name = DynamicsListEn[j].Name;
            if (e.Column.VisibleIndex == 0)
            {
                e.DisplayText = res.Id.ToString();
            }
            if (e.Column.VisibleIndex == 1)
            {
                e.DisplayText = name;
            }
        }

        private void gridView9_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                if (gridView9.FocusedRowObject != null)
                {
                    DynamicObject dyn = gridView9.FocusedRowObject as DynamicObject;
                    List<DynamicObject> defaultDynObj = gridView5.DataSource as List<DynamicObject>;
                    int index = defaultDynObj.IndexOf(dyn);
                    if (index != -1)
                    {
                        xtraTabControl1.SelectedTabPageIndex = 2;
                        gridView5.FocusedRowHandle = index;
                    }
                }
            }
            catch
            {
            }
        }

        void LinkMaps(List<PCKFileEntry> l, string MapName, int index = 0)
        {
            int val = 256;
            Bitmap bm = null;
            int MapIndex = LoadedMapConfigs.FindIndex(z => z.Name == MapName);
            if (MapIndex != -1)
            {
                if (l.Count == 88)
                    val = 1024;
                bm = new Bitmap(LoadedMapConfigs[MapIndex].Width, LoadedMapConfigs[MapIndex].Height);
                int x = 0;
                int y = 0;
                Graphics gr = Graphics.FromImage(bm);
                if (index == 0)
                {
                    pbmapa.BeginInvoke(
                        new MethodInvoker(
                            delegate
                            {
                                pbmapa.Properties.Maximum = l.Count;
                                pbmapa.Position = 0;
                            }));
                }
                if (index == 1)
                {
                    pbprecinct.BeginInvoke(
                        new MethodInvoker(
                            delegate
                            {
                                pbprecinct.Properties.Maximum = l.Count;
                                pbprecinct.Position = 0;
                            }));
                }
                l = l.OrderBy(t => t.Path).ToList();
                for (int i = 0; i < l.Count; i++)
                {
                    DDSImage img = new DDSImage(ReadFile(streamsurfaces, l[i]));
                    gr.DrawImage(img.BitmapImage, new Point(x, y));
                    x += val;
                    if (x == bm.Width)
                    {
                        y += val;
                        x = 0;
                    }
                    if (index == 0)
                    {
                        pbmapa.BeginInvoke(
                            new MethodInvoker(
                                delegate
                                {
                                    pbmapa.Position++;
                                }));
                    }
                    if (index == 1)
                    {
                        pbprecinct.BeginInvoke(
                            new MethodInvoker(
                                delegate
                                {
                                    pbprecinct.Position++;
                                }));
                    }
                }
                this.BeginInvoke(
                    new MethodInvoker(
                        delegate
                        {
                            if (index == 0)
                                pbmapa.Position = 0;
                            if (index == 1)
                                pbprecinct.Position = 0;
                            if (telaMapa == null)
                            {
                                telaMapa = new TelaMapa(this, index);
                                //telaMapa.pictureBox1.BackgroundImage = bm;
                                telaMapa.pictureBox1.Image = bm;
                                telaMapa.pictureBox1.Width = bm.Width;
                                telaMapa.pictureBox1.Height = bm.Height;
                                telaMapa.Show(this);
                            }
                            else if (telaMapa.Visible == false)
                            {
                                telaMapa = new TelaMapa(this, index);
                                // telaMapa.pictureBox1.BackgroundImage = bm;
                                telaMapa.pictureBox1.Image = bm;
                                telaMapa.pictureBox1.Width = telaMapa.pictureBox1.Image.Width;
                                telaMapa.pictureBox1.Height = telaMapa.pictureBox1.Image.Height;
                                telaMapa.Show(this);
                            }
                            else
                            {
                                //telaMapa.pictureBox1.BackgroundImage = bm;
                                telaMapa.pictureBox1.Image = bm;
                                telaMapa.pictureBox1.Width = telaMapa.pictureBox1.Image.Width;
                                telaMapa.pictureBox1.Height = telaMapa.pictureBox1.Image.Height;
                            }
                        }));
            }
        }

        private void NpcAndMobsDefault_EnterPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                NpcAndMobsDefaultLeave(sender, null);
        }

        private void NpcAndMobsDefaultLeave(object sender, EventArgs e)
        {
            var mob = gridView1.FocusedRowObject as DefaultMonsters;
            if (mob == null)
                return;
            Control c = sender as Control;
            if (c == null)
                return;

            switch (c.Name)
            {
                case "cbMobLocal":
                    {
                        mob.Location = (c as ComboBoxEdit).SelectedIndex;
                        break;
                    }
                case "cbMobTipo":
                    {
                        mob.Type = (c as ComboBoxEdit).SelectedIndex;
                        break;
                    }
                case "mobPosX":
                    {
                        mob.X_position = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "mobPosY":
                    {
                        mob.Y_position = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "mobPosZ":
                    {
                        mob.Z_position = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "mobRotX":
                    {
                        mob.X_direction = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "mobRotY":
                    {
                        mob.Y_direction = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "mobRotZ":
                    {
                        mob.Z_direction = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "mobScatterX":
                    {
                        mob.X_random = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "mobScatterY":
                    {
                        mob.Y_random = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "mobScatterZ":
                    {
                        mob.Z_random = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "mobTriggerID":
                    {
                        mob.Trigger_id = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobDwGenId":
                    {
                        mob.dwGenId = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobMaxAmount":
                    {
                        mob.MaxRespawnTime = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobGroupType":
                    {
                        mob.iGroupType = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobLifeTime":
                    {
                        mob.Life_time = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobAmountInGroup":
                    {
                        mob.Amount_in_group = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobcbAutoInit":
                    {
                        mob.BInitGen = Convert.ToByte((c as CheckEdit).Checked);
                        break;
                    }
                case "mobcbValidOnce":
                    {
                        mob.BValicOnce = Convert.ToByte((c as CheckEdit).Checked);
                        break;
                    }
                case "mobcbAutoRespawn":
                    {
                        mob.bAutoRevive = Convert.ToByte((c as CheckEdit).Checked);
                        break;
                    }
            }

            var mobdop = gridView2.FocusedRowObject as ExtraMonsters;
            if (mobdop == null)
                return;
            switch (c.Name)
            {
                case "mobId":
                    {
                        mobdop.Id = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobAmount":
                    {
                        mobdop.Amount = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobRefresh":
                    {
                        mobdop.Respawn = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobRefreshLow":
                    {
                        mobdop.RefreshLower = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobDeathCount":
                    {
                        mobdop.Dead_amount = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobWatterOffset":
                    {
                        mobdop.fOffsetWater = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "mobTurnOffset":
                    {
                        mobdop.fOffsetTrn = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "mobPathID":
                    {
                        mobdop.Path = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobcbLoopType":
                    {
                        mobdop.Path_type = Convert.ToInt32((c as ComboBoxEdit).SelectedIndex);
                        break;
                    }
                case "mobcbSpeed":
                    {
                        mobdop.Speed = Convert.ToInt32((c as ComboBoxEdit).SelectedIndex);
                        break;
                    }
                case "mobDeathTime":
                    {
                        mobdop.Dead_time = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobcbAgressive":
                    {
                        mobdop.Agression = Convert.ToInt32((c as ComboBoxEdit).SelectedIndex);
                        break;
                    }
                case "mobFaction":
                    {
                        mobdop.Group = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobFactionAsk":
                    {
                        mobdop.Group_help_Needer = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobFactionReceive":
                    {
                        mobdop.Group_help_sender = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "mobcbDefFaction":
                    {
                        mobdop.bFaction = Convert.ToByte((c as CheckEdit).Checked);
                        break;
                    }
                case "mobcbDefFactionAsk":
                    {
                        mobdop.bFac_Helper = Convert.ToByte((c as CheckEdit).Checked);
                        break;
                    }
                case "mobcbDefFactionReceive":
                    {
                        mobdop.bFac_Accept = Convert.ToByte((c as CheckEdit).Checked);
                        break;
                    }
                case "mobcbNeedHelp":
                    {
                        mobdop.bNeedHelp = Convert.ToByte((c as CheckEdit).Checked);
                        break;
                    }
            }
            gridView1.RefreshData();
            gridView2.RefreshData();
        }

        private void ResourcesDefault_EnterPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                ResourcesDefaultLeave(sender, null);
        }

        private void ResourcesDefaultLeave(object sender, EventArgs e)
        {
            var mob = gridView4.FocusedRowObject as DefaultResources;
            if (mob == null)
                return;
            Control c = sender as Control;
            if (c == null)
                return;

            switch (c.Name)
            {
                case "resPosX":
                    {
                        mob.X_position = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "resPosY":
                    {
                        mob.Y_position = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "resPosZ":
                    {
                        mob.Z_position = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "resRot":
                    {
                        mob.Rotation = Convert.ToByte((c as SpinEdit).Value);
                        break;
                    }
                case "resIncl1":
                    {
                        mob.InCline1 = Convert.ToByte((c as SpinEdit).Value);
                        break;
                    }
                case "resIncl2":
                    {
                        mob.InCline2 = Convert.ToByte((c as SpinEdit).Value);
                        break;
                    }
                case "resScatterX":
                    {
                        mob.X_Random = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "resScatterZ":
                    {
                        mob.Z_Random = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
                case "resTriggerID":
                    {
                        mob.Trigger_id = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "resDwGenId":
                    {
                        mob.dwGenID = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "resMaxNum":
                    {
                        mob.IMaxNum = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "resInGroup":
                    {
                        mob.Amount_in_group = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "rescbInitAuto":
                    {
                        mob.bInitGen = Convert.ToByte((c as CheckEdit).Checked);
                        break;
                    }
                case "rescbRespawnInsta":
                    {
                        mob.bAutoRevive = Convert.ToByte((c as CheckEdit).Checked);
                        break;
                    }
                case "rescbValidOnce":
                    {
                        mob.bValidOnce = Convert.ToByte((c as CheckEdit).Checked);
                        break;
                    }
            }
            var resextra = gridView3.FocusedRowObject as ExtraResources;
            if (resextra == null)
                return;
            switch (c.Name)
            {
                case "resId":
                    {
                        resextra.Id = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "resNum":
                    {
                        resextra.Amount = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "resRespawn":
                    {
                        resextra.Respawntime = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "resType":
                    {
                        resextra.ResourceType = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "resHeig":
                    {
                        resextra.fHeiOff = Convert.ToSingle((c as SpinEdit).Value);
                        break;
                    }
            }
            gridView3.RefreshData();
            gridView4.RefreshData();
        }

        private void selectDynamicObjectInMainList(DynamicObject dyn)
        {
            dynId.Value = dyn.Id;
            dynPosX.Value = Convert.ToDecimal(dyn.X_position);
            dynPosY.Value = Convert.ToDecimal(dyn.Y_position);
            dynPosZ.Value = Convert.ToDecimal(dyn.Z_position);
            dynIncl1.Value = dyn.InCline1;
            dynIncl2.Value = dyn.InCline2;
            dynRot.Value = dyn.Rotation;
            dynTriggerID.Value = dyn.TriggerId;
            dynScale.Value = dyn.Scale;
            string DynScreenPath = string.Format("{0}\\DynamicObjects\\d{1}.jpg", Application.StartupPath, dyn.Id);
            if (File.Exists(DynScreenPath))
            {
                dynPicture.Image = Bitmap.FromFile(DynScreenPath);
            }
            if (telaMapa != null)
            {
                telaMapa.GetCoordinates(
                    new List<PointF> { new PointF(dyn.X_position, dyn.Z_position) },
                    Properties.Resources.radar_ftasknpc);
            }
        }

        void selectMobInMainList(DefaultMonsters mob)
        {
            cbMobLocal.SelectedIndex = mob.Location;
            cbMobTipo.SelectedIndex = mob.Type;
            mobPosX.Value = Convert.ToDecimal(mob.X_position);
            mobPosY.Value = Convert.ToDecimal(mob.Y_position);
            mobPosZ.Value = Convert.ToDecimal(mob.Z_position);
            mobRotX.Value = Convert.ToDecimal(mob.X_direction);
            mobRotY.Value = Convert.ToDecimal(mob.Y_direction);
            mobRotZ.Value = Convert.ToDecimal(mob.Z_direction);
            mobScatterX.Value = Convert.ToDecimal(mob.X_random);
            mobScatterY.Value = Convert.ToDecimal(mob.Y_random);
            mobScatterZ.Value = Convert.ToDecimal(mob.Z_random);
            mobTriggerID.Value = mob.Trigger_id;
            mobDwGenId.Value = mob.dwGenId;
            mobMaxAmount.Value = mob.MaxRespawnTime;
            mobGroupType.Value = mob.iGroupType;
            mobAmountInGroup.Value = mob.Amount_in_group;
            mobLifeTime.Value = mob.Life_time;
            mobcbAutoInit.Checked = mob.BInitGen == 1 ? true : false;
            mobcbValidOnce.Checked = mob.BValicOnce == 1 ? true : false;
            mobcbAutoRespawn.Checked = mob.bAutoRevive == 1 ? true : false;
            if (telaMapa != null)
            {
                Bitmap img = mob.Type == 0 ? Properties.Resources.pet : Properties.Resources.radar_npc;
                telaMapa.GetCoordinates(new List<PointF> { new PointF(mob.X_position, mob.Z_position) }, img);
            }
        }

        void selectMobInSubList(ExtraMonsters mob)
        {
            mobId.Value = mob.Id;
            mobAmount.Value = mob.Amount;
            mobRefresh.Value = mob.Respawn;
            mobRefreshLow.Value = mob.RefreshLower;
            mobDeathCount.Value = mob.Dead_time;
            mobWatterOffset.Value = Convert.ToDecimal(mob.fOffsetWater);
            mobTurnOffset.Value = Convert.ToDecimal(mob.fOffsetTrn);
            mobPathID.Value = mob.Path;
            mobcbLoopType.SelectedIndex = mob.Path_type;
            mobcbSpeed.SelectedIndex = mob.Speed;
            mobDeathTime.Value = mob.Dead_time;
            mobcbAgressive.SelectedIndex = mob.Agression;
            mobFaction.Value = mob.Group;
            mobFactionAsk.Value = mob.Group_help_sender;
            mobFactionReceive.Value = mob.Group_help_Needer;
            mobcbDefFaction.Checked = mob.bFaction == 1 ? true : false;
            mobcbDefFactionAsk.Checked = mob.bFac_Helper == 1 ? true : false;
            mobcbDefFactionReceive.Checked = mob.bFac_Accept == 1 ? true : false;
            mobcbNeedHelp.Checked = mob.bNeedHelp == 1 ? true : false;
        }

        private void selectResourceInMainList(DefaultResources res)
        {
            resPosX.Value = Convert.ToDecimal(res.X_position);
            resPosY.Value = Convert.ToDecimal(res.Y_position);
            resPosZ.Value = Convert.ToDecimal(res.Z_position);
            resScatterX.Value = Convert.ToDecimal(res.X_Random);
            resScatterZ.Value = Convert.ToDecimal(res.Z_Random);
            resRot.Value = res.Rotation;
            resIncl1.Value = res.InCline1;
            resIncl2.Value = res.InCline2;
            resInGroup.Value = res.Amount_in_group;
            resDwGenId.Value = res.dwGenID;
            resTriggerID.Value = res.Trigger_id;
            resMaxNum.Value = res.IMaxNum;
            rescbInitAuto.Checked = res.bInitGen == 1 ? true : false;
            rescbRespawnInsta.Checked = res.bAutoRevive == 1 ? true : false;
            rescbValidOnce.Checked = res.bValidOnce == 1 ? true : false;
            if (telaMapa != null)
            {
                telaMapa.GetCoordinates(
                    new List<PointF> { new PointF(res.X_position, res.Z_position) },
                    Properties.Resources.radar_tasknpc);
            }
        }

        private void selectResourceInSubList(ExtraResources extraResources)
        {
            resId.Value = extraResources.Id;
            resNum.Value = extraResources.Amount;
            resRespawn.Value = extraResources.Respawntime;
            resType.Value = extraResources.ResourceType;
            resHeig.Value = Convert.ToDecimal(extraResources.fHeiOff);
        }

        private void selectTriggerInMainList(Trigger trigger)
        {
            triggerId.Value = trigger.Id;
            triggerGMID.Value = trigger.GmID;
            triggerDescricao.Text = trigger.TriggerName;
            triggerStartDelay.Value = trigger.WaitWhileStart;
            triggerStopDelay.Value = trigger.WaitWhileStop;
            triggerDuration.Value = trigger.Duration;
            triggerbAutoInit.Checked = trigger.AutoStart == 1 ? true : false;
            triggerbStartSch.Checked = trigger.DontStartOnSchedule == 1 ? true : false;
            triggerStopSch.Checked = trigger.DontStopOnSchedule == 1 ? true : false;
            triggerStartDay.Value = trigger.StartDay;
            triggerStartMonth.Value = trigger.StartMonth;
            triggerStartYear.Value = trigger.StartYear;
            triggerStartHour.Value = trigger.StartHour;
            triggerStartMinute.Value = trigger.StartMinute;
            triggercbStartDayWeek.SelectedIndex = trigger.StartWeekDay;
            triggerStopDay.Value = trigger.StopDay;
            triggerStopMonth.Value = trigger.StopMonth;
            triggerStopYear.Value = trigger.StopYear;
            triggerStopHour.Value = trigger.StopHour;
            triggerStopMinute.Value = trigger.StopMinute;
            triggerStopDayWeek.SelectedIndex = trigger.StopWeekDay;
            List<DefaultMonsters> MonstersContact = (gridView1.DataSource as List<DefaultMonsters>).Where(
                z => z.Trigger_id == trigger.Id)
                .ToList();
            List<DefaultResources> ResourcesContact = (gridView4.DataSource as List<DefaultResources>).Where(
                z => z.Trigger_id == trigger.Id)
                .ToList();
            List<DynamicObject> DynamicObjectsContact = (gridView5.DataSource as List<DynamicObject>).Where(
                z => z.TriggerId == trigger.Id)
                .ToList();
            triggerRefMobNpc.DataSource = MonstersContact;
            triggerRefMat.DataSource = ResourcesContact;
            triggerRefObjDyn.DataSource = DynamicObjectsContact;
        }

        private void simpleButton10_Click(object sender, EventArgs e)
        {
            if (gridView2.FocusedRowHandle < 0)
                return;
            var bs = gridView2.DataSource as List<ExtraMonsters>;
            if (bs != null)
            {
                bs.Add(gridView2.FocusedRowObject as ExtraMonsters);
                gridView2.RefreshData();
                gridView2.MoveLast();
                mobAmountInGroup.Value = gridView2.RowCount;
            }
        }

        private void simpleButton12_Click(object sender, EventArgs e)
        {
            if (gridView4.FocusedRowHandle < 0)
                return;
            var bs = gridView4.DataSource as List<DefaultResources>;
            if (bs != null)
            {
                bs.Remove(gridView4.FocusedRowObject as DefaultResources);
                gridView4.RefreshData();
                gridView4.MoveLast();
            }
        }

        private void simpleButton13_Click(object sender, EventArgs e)
        {
            var bs = gridView4.DataSource as List<DefaultResources>;
            if (bs != null)
            {
                bs.Add(
                    new DefaultResources
                    {
                        Amount_in_group = 1,
                        bAutoRevive = 0,
                        bValidOnce = 0,
                        bInitGen = 0,
                        dwGenID = 0,
                        IMaxNum = 1,
                        InCline1 = 0,
                        InCline2 = 0,
                        Rotation = 0,
                        Trigger_id = 0,
                        X_position = 0,
                        X_Random = 0,
                        Y_position = 0,
                        Z_position = 0,
                        Z_Random = 0,
                        ResExtra =
                            new List<ExtraResources>
                                {
                                    new ExtraResources
                                    {
                                        Amount = 1,
                                        fHeiOff = 0,
                                        Id = 16,
                                        ResourceType = 80,
                                        Respawntime = 60
                                    }
                                }
                    });

                gridView4.RefreshData();
                gridView4.MoveLast();
            }
        }

        private void simpleButton14_Click(object sender, EventArgs e)
        {
            if (gridView4.FocusedRowHandle < 0)
                return;
            var bs = gridView4.DataSource as List<DefaultResources>;
            if (bs != null)
            {
                bs.Add(gridView4.FocusedRowObject as DefaultResources);
                gridView4.RefreshData();
                gridView4.MoveLast();
            }
        }

        private void simpleButton16_Click(object sender, EventArgs e)
        {
            if (gridView3.FocusedRowHandle < 0)
                return;
            var bs = gridView3.DataSource as List<ExtraResources>;
            if (bs != null)
            {
                bs.Remove(gridView3.FocusedRowObject as ExtraResources);
                gridView3.RefreshData();
                gridView3.MoveLast();
                resInGroup.Value = gridView3.RowCount;
            }
        }

        private void simpleButton17_Click(object sender, EventArgs e)
        {
            var bs = gridView3.DataSource as List<ExtraResources>;
            if (bs != null)
            {
                bs.Add(
                    new ExtraResources { Amount = 1, fHeiOff = 0, Id = 16, ResourceType = 80, Respawntime = 60 }

            );
                gridView3.RefreshData();
                gridView3.MoveLast();
                resInGroup.Value = gridView3.RowCount;
            }
        }

        private void simpleButton18_Click(object sender, EventArgs e)
        {
            if (gridView3.FocusedRowHandle < 0)
                return;
            var bs = gridView3.DataSource as List<ExtraResources>;
            if (bs != null)
            {
                bs.Add(gridView3.FocusedRowObject as ExtraResources);
                gridView3.RefreshData();
                gridView3.MoveLast();
                resInGroup.Value = gridView3.RowCount;
            }
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            if (File.Exists(txtSurfaces.Text))
            {
                try
                {
                    streamsurfaces = new PCKStream(txtSurfaces.Text);
                    var surfaces = ReadFileTable(streamsurfaces);
                    Maps = new List<GameMapInfo>();
                    List<PCKFileEntry> AllFiles = surfaces.Where(
                        z => z.Path.Contains("minimaps") && !z.Path.Contains("surfaces\\minimaps\\world"))
                        .ToList();
                    List<string> AlreadyExistion = new List<string>();
                    foreach (PCKFileEntry fs in AllFiles)
                    {
                        GameMapInfo map = new GameMapInfo();
                        List<string> st = fs.Path.Split('\\').ToList();
                        map.MapName = st.ElementAt(st.Count - 2);
                        st.RemoveAt(st.Count - 1);
                        map.MapPath = string.Join("\\", st);
                        if (AlreadyExistion.FindIndex(v => v == string.Join("\\", st)) == -1)
                        {
                            map.MapFragments = AllFiles.Where(z => z.Path.Contains(map.MapPath))
                                .ToList()
                                .OrderBy(z => z.Path)
                                .ToList();
                            Maps.Add(map);
                            AlreadyExistion.Add(map.MapPath);
                        }
                    }
                    if (Maps.Count != 0)
                    {
                        GameMapInfo map1 = new GameMapInfo() { MapName = "World", MapPath = "surfaces\\maps\\" };
                        map1.MapFragments = surfaces.Where(z => z.Path.Contains(map1.MapPath)).ToList();
                        Maps.Add(map1);
                        cbMapas.Properties.Items.Clear();
                        for (int i = 0; i < Maps.Count; i++)
                        {
                            cbMapas.Properties.Items.Add(Maps[i].MapName);
                        }
                        cbMapas.SelectedIndex = cbMapas.Properties.Items.Count - 1;
                    }
                }
                catch
                {
                    XtraMessageBox.Show("Não foi possível carregar o surfaces.pck, possivelmente o arquivo esteja protegido ou corrompido.");
                    cbMapas.Properties.Items.Clear();
                    Maps = null;
                }
            }
            else
            {
                XtraMessageBox.Show("Arquivo surfaces.pck não encontrado");
            }
        }

        private void simpleButton20_Click(object sender, EventArgs e)
        {
            if (gridView5.FocusedRowHandle < 0)
                return;
            var bs = gridView5.DataSource as List<DynamicObject>;
            if (bs != null)
            {
                bs.Remove(gridView5.FocusedRowObject as DynamicObject);
                gridView5.RefreshData();
                gridView5.MoveLast();
            }
        }

        private void simpleButton21_Click(object sender, EventArgs e)
        {
            var bs = gridView5.DataSource as List<DynamicObject>;
            if (bs != null)
            {
                bs.Add(
                    new DynamicObject
                    {
                        Id = 0,
                        InCline1 = 0,
                        InCline2 = 0,
                        Rotation = 0,
                        Scale = 0,
                        TriggerId = 0,
                        X_position = 0,
                        Y_position = 0,
                        Z_position = 0
                    });
                gridView5.RefreshData();
                gridView5.MoveLast();
            }
        }

        private void simpleButton22_Click(object sender, EventArgs e)
        {
            if (gridView5.FocusedRowHandle < 0)
                return;
            var bs = gridView5.DataSource as List<DynamicObject>;
            if (bs != null)
            {
                bs.Add(gridView5.FocusedRowObject as DynamicObject);
                gridView5.RefreshData();
                gridView5.MoveLast();
            }
        }

        private void simpleButton23_Click(object sender, EventArgs e)
        {
            if (Maps != null)
            {
                int SelectedIndex = cbMapas.SelectedIndex;
                System.Threading.Thread th = new System.Threading.Thread(
                    () => LinkMaps(Maps[SelectedIndex].MapFragments, Maps[SelectedIndex].MapName));
                th.Start();
            }
        }

        private void simpleButton25_Click(object sender, EventArgs e)
        {
            if (gridView6.FocusedRowHandle < 0)
                return;
            var bs = gridView6.DataSource as List<Trigger>;
            if (bs != null)
            {
                bs.Remove(gridView6.FocusedRowObject as Trigger);
                gridView6.RefreshData();
                gridView6.MoveLast();
            }
        }

        private void simpleButton26_Click(object sender, EventArgs e)
        {
            var bs = gridView6.DataSource as List<Trigger>;
            if (bs != null)
            {
                var last = bs.LastOrDefault();
                bs.Add(
                    new Trigger
                    {
                        AutoStart = 0,
                        DontStartOnSchedule = 0,
                        DontStopOnSchedule = 0,
                        Duration = 0,
                        GmID = last.GmID + 1,
                        Id = last.Id + 1,
                        StartDay = 0,
                        StartHour = 0,
                        StartMinute = 0,
                        StartMonth = 0,
                        StartWeekDay = 0,
                        StartYear = 0,
                        StopDay = 0,
                        StopHour = 0,
                        StopMinute = 0,
                        StopMonth = 0,
                        StopWeekDay = 0,
                        StopYear = 0,
                        TriggerName = string.Empty,
                        WaitWhileStart = 0,
                        WaitWhileStop = 0
                    });
                gridView6.RefreshData();
                gridView6.MoveLast();
            }
        }

        private void simpleButton27_Click(object sender, EventArgs e)
        {
            if (gridView6.FocusedRowHandle < 0)
                return;
            var bs = gridView6.DataSource as List<Trigger>;
            if (bs != null)
            {
                bs.Add(gridView6.FocusedRowObject as Trigger);
                gridView6.RefreshData();
                gridView6.MoveLast();
            }
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            if (gridView1.FocusedRowHandle < 0)
                return;
            var bs = gridView1.DataSource as List<DefaultMonsters>;
            if (bs != null)
            {
                bs.Remove(gridView1.FocusedRowObject as DefaultMonsters);
                gridView1.RefreshData();
                gridView1.MoveLast();
            }
        }

        private void simpleButton5_Click(object sender, EventArgs e)
        {
            var bs = gridView1.DataSource as List<DefaultMonsters>;
            if (bs != null)
            {
                bs.Add(
                    new DefaultMonsters
                    {
                        Amount_in_group = 1,
                        BInitGen = 0,
                        BValicOnce = 0,
                        bAutoRevive = 0,
                        dwGenId = 0,
                        iGroupType = 0,
                        Life_time = 0,
                        Location = 0,
                        MaxRespawnTime = 0,
                        Trigger_id = 0,
                        Type = 0,
                        X_direction = 0,
                        X_position = 0,
                        X_random = 0,
                        Y_direction = 0,
                        Y_position = 0,
                        Y_random = 0,
                        Z_direction = 0,
                        Z_position = 0,
                        Z_random = 0,
                        MobDops =
                            new List<ExtraMonsters>
                                {
                                    new ExtraMonsters
                                    {
                                        Agression = 0,
                                        Amount = 1,
                                        bFaction = 0,
                                        bFac_Accept = 0,
                                        bFac_Helper = 0,
                                        bNeedHelp = 0,
                                        Dead_amount = 0,
                                        Dead_time = 0,
                                        fOffsetTrn = 0,
                                        fOffsetWater = 0,
                                        Group = 0,
                                        Group_help_Needer = 0,
                                        Group_help_sender = 0,
                                        Id = 16,
                                        Path = 0,
                                        Path_type = 0,
                                        RefreshLower = 0,
                                        Respawn = 0,
                                        Speed = 0,
                                    }
                                }
                    });
                gridView1.RefreshData();
                gridView1.MoveLast();
            }
        }

        private void simpleButton6_Click(object sender, EventArgs e)
        {
            if (gridView1.FocusedRowHandle < 0)
                return;
            var bs = gridView1.DataSource as List<DefaultMonsters>;
            if (bs != null)
            {
                bs.Add(gridView1.FocusedRowObject as DefaultMonsters);
                gridView1.RefreshData();
                gridView1.MoveLast();
            }
        }

        private void simpleButton8_Click(object sender, EventArgs e)
        {
            if (gridView2.FocusedRowHandle < 0)
                return;
            var bs = gridView2.DataSource as List<ExtraMonsters>;
            if (bs != null)
            {
                bs.Remove(gridView2.FocusedRowObject as ExtraMonsters);
                gridView2.RefreshData();
                gridView2.MoveLast();
                mobAmountInGroup.Value = gridView2.RowCount;
            }
        }

        private void simpleButton9_Click(object sender, EventArgs e)
        {
            var bs = gridView2.DataSource as List<ExtraMonsters>;
            if (bs != null)
            {
                bs.Add(
                    new ExtraMonsters
                    {
                        Agression = 0,
                        Amount = 1,
                        bFaction = 0,
                        bFac_Accept = 0,
                        bFac_Helper = 0,
                        bNeedHelp = 0,
                        Dead_amount = 0,
                        Dead_time = 0,
                        fOffsetTrn = 0,
                        fOffsetWater = 0,
                        Group = 0,
                        Group_help_Needer = 0,
                        Group_help_sender = 0,
                        Id = 16,
                        Path = 0,
                        Path_type = 0,
                        RefreshLower = 0,
                        Respawn = 0,
                        Speed = 0,
                    });
                gridView2.RefreshData();
                gridView2.MoveLast();
                mobAmountInGroup.Value = gridView2.RowCount;
            }
        }

        private void TriggerDefault_EnterPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                TriggerDefaultLeave(sender, null);
        }

        private void TriggerDefaultLeave(object sender, EventArgs e)
        {
            var tg = gridView6.FocusedRowObject as Trigger;
            if (tg == null)
                return;
            Control c = sender as Control;
            if (c == null)
                return;

            switch (c.Name)
            {
                case "triggerId":
                    {
                        tg.Id = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggerGMID":
                    {
                        tg.GmID = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggerDescricao":
                    {
                        tg.TriggerName = (c as TextEdit).Text;
                        break;
                    }
                case "triggerStartDelay":
                    {
                        tg.WaitWhileStart = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggerStopDelay":
                    {
                        tg.WaitWhileStop = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggerDuration":
                    {
                        tg.Duration = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggerbAutoInit":
                    {
                        tg.AutoStart = Convert.ToByte((c as CheckEdit).Checked);
                        break;
                    }
                case "triggerbStartSch":
                    {
                        tg.DontStartOnSchedule = Convert.ToByte((c as CheckEdit).Checked);
                        break;
                    }
                case "triggerStopSch":
                    {
                        tg.DontStopOnSchedule = Convert.ToByte((c as CheckEdit).Checked);
                        break;
                    }
                case "triggerStartDay":
                    {
                        tg.StartDay = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggerStartMonth":
                    {
                        tg.StartMonth = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggerStartYear":
                    {
                        tg.StartYear = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggerStartHour":
                    {
                        tg.StartHour = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggerStartMinute":
                    {
                        tg.StartMinute = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggercbStartDayWeek":
                    {
                        tg.StartWeekDay = (c as ComboBoxEdit).SelectedIndex;
                        break;
                    }
                case "triggerStopDay":
                    {
                        tg.StopDay = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggerStopMonth":
                    {
                        tg.StopMonth = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggerStopYear":
                    {
                        tg.StopYear = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggerStopHour":
                    {
                        tg.StopHour = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggerStopMinute":
                    {
                        tg.StopMinute = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "triggerStopDayWeek":
                    {
                        tg.StopWeekDay = (c as ComboBoxEdit).SelectedIndex;
                        break;
                    }
            }
            gridView6.RefreshData();
        }

        private void triggerDescricao_KeyUp(object sender, KeyEventArgs e) { TriggerDefaultLeave(sender, null); }

        private void txtElements_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            using (XtraOpenFileDialog openFileDialog = new XtraOpenFileDialog())
            {
                openFileDialog.Title = "Selecionar um arquivo";
                openFileDialog.Filter = "elements.data (elements.data)|elements.data|Arquivos data (*.data)|*.data|Todos os Arquivos (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    txtElements.Text = selectedFilePath;
                }
            }
        }

        private void txtNpcgen_Properties_ButtonClick(
            object sender,
            DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            using (XtraOpenFileDialog openFileDialog = new XtraOpenFileDialog())
            {
                openFileDialog.Title = "Selecionar um arquivo";
                openFileDialog.Filter = "npcgen.data (npcgen.data)|npcgen.data|Arquivos data (*.data)|*.data|Todos os Arquivos (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    txtNpcgen.Text = selectedFilePath;
                }
            }
        }

        private void txtSurfaces_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            using (XtraOpenFileDialog openFileDialog = new XtraOpenFileDialog())
            {
                openFileDialog.Title = "Selecionar um arquivo";
                openFileDialog.Filter = "surfaces.pck (surfaces.pck)|surfaces.pck|Arquivos pck (*.pck)|*.pck|Todos os Arquivos (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    txtSurfaces.Text = selectedFilePath;
                }
            }
        }

        public void ConfigureNPCGEN()
        {
            tableNpcgenMobNpc.DataSource = npcgen.NpcMobList;
            tableNpcgenResources.DataSource = npcgen.ResourcesList;
            tableDynamicObject.DataSource = npcgen.DynamicsList;
            tableTriggers.DataSource = npcgen.TriggersList;
            switch (npcgen.File_version)
            {
                case 6:
                    {
                        cbVersion.SelectedIndex = 0;
                        break;
                    }
                case 7:
                    {
                        cbVersion.SelectedIndex = 1;
                        break;
                    }
                case 10:
                    {
                        cbVersion.SelectedIndex = 2;
                        break;
                    }
                case 11:
                    {
                        cbVersion.SelectedIndex = 3;
                        break;
                    }
            }
        }

        public static async Task LoadELEMENTS(string path)
        {
            await Task.Run(
                () =>
                {
                    Console.WriteLine("Carregando elements.data");
                    elc = new eListCollection(path);
                    //elc = new Elementsdata(path);
                });

            Console.WriteLine("elements.data carregado com sucesso!");
        }


        public static async Task LoadNPCGEN(string path)
        {
            await Task.Run(
                () =>
                {
                    Console.WriteLine("Carregando npcgen.data");
                    npcgen = new NpcGen();
                    npcgen.ReadNpcgen(path);
                });

            Console.WriteLine("npcgen.data carregado com sucesso!");
        }

        public static byte[] ReadFile(PCKStream stream, PCKFileEntry file)
        {
            stream.Seek(file.Offset, SeekOrigin.Begin);
            byte[] bytes = stream.ReadBytes(file.CompressedSize);
            return file.CompressedSize < file.Size ? PCKZlib.Decompress(bytes, file.Size) : bytes;
        }

        public static PCKFileEntry[] ReadFileTable(PCKStream stream)
        {
            stream.Seek(-8, SeekOrigin.End);
            int FilesCount = stream.ReadInt32();
            stream.Seek(-272, SeekOrigin.End);
            long FileTableOffset = (long)((ulong)((uint)(stream.ReadUInt32() ^ (ulong)stream.key.KEY_1)));
            PCKFileEntry[] entrys = new PCKFileEntry[FilesCount];
            stream.Seek(FileTableOffset, SeekOrigin.Begin);
            for (int i = 0; i < entrys.Length; ++i)
            {
                int EntrySize = stream.ReadInt32() ^ stream.key.KEY_1;
                stream.ReadInt32();
                entrys[i] = new PCKFileEntry(stream.ReadBytes(EntrySize));
            }
            return entrys;
        }

        private void simpleButton28_Click(object sender, EventArgs e)
        {
            if (File.Exists(txtNpcgen.Text))
            {
                npcgen.NpcMobList = gridView1.DataSource as List<DefaultMonsters>;
                npcgen.NpcMobsAmount = npcgen.NpcMobList.Count;
                npcgen.ResourcesList = gridView4.DataSource as List<DefaultResources>;
                npcgen.ResourcesAmount = npcgen.ResourcesList.Count;
                npcgen.DynamicsList = gridView5.DataSource as List<DynamicObject>;
                npcgen.DynobjectAmount = npcgen.DynamicsList.Count;
                npcgen.TriggersList = gridView6.DataSource as List<Trigger>;
                npcgen.TriggersAmount = npcgen.TriggersList.Count;
                BinaryWriter bw = new BinaryWriter(File.Create(txtNpcgen.Text));
                npcgen.WriteNpcgen(bw, Convert.ToInt32(cbVersion.SelectedItem.ToString()));
                bw.Close();
                XtraMessageBox.Show("Arquivo salvo com sucesso!", "NPCGen editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                XtraMessageBox.Show("Arquivo npcgen.data não foi encontrado.", "NPCGen editor", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void mobId_DoubleClick(object sender, EventArgs e)
        {
            string tipo = cbMobTipo.SelectedIndex == 0 ? "MONSTER_ESSENCE" : "NPC_ESSENCE";
            TelaSelecionarEssencia ess = new TelaSelecionarEssencia(tipo);
            if (ess.ShowDialog() == DialogResult.OK)
            {
                (sender as SpinEdit).Value = ess.retId;
                var mobdop = gridView2.FocusedRowObject as ExtraMonsters;
                mobdop.Id = ess.retId;
                gridView2.RefreshData();
            }
        }

        private void resId_DoubleClick(object sender, EventArgs e)
        {
            TelaSelecionarEssencia ess = new TelaSelecionarEssencia("MINE_ESSENCE");
            if (ess.ShowDialog() == DialogResult.OK)
            {
                (sender as SpinEdit).Value = ess.retId;
                var mobdop = gridView3.FocusedRowObject as ExtraResources;
                mobdop.Id = ess.retId;
                gridView3.RefreshData();
            }
        }

        private void xtraTabControl1_Click(object sender, EventArgs e)
        {

        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            WindowsFormsSettings.DefaultLookAndFeel.SetSkinStyle(comboBoxEdit1.SelectedItem.ToString());
        }

        private void simpleButton30_Click(object sender, EventArgs e)
        {
            precinct = new CELPrecinctSet();
            if (txtPrecinct.Text.EndsWith("clt"))
            {
                if (precinct.LoadClt(txtPrecinct.Text))
                {
                    gcPrecinct.DataSource = precinct.m_aPrecincts;
                    cbPrecinctVersion.SelectedIndex = precinct.version - 1;
                }
            }
            else if (txtPrecinct.Text.EndsWith("sev"))
            {
                if (precinct.LoadSev(txtPrecinct.Text))
                {
                    gcPrecinct.DataSource = precinct.m_aPrecincts;
                    cbPrecinctVersion.SelectedIndex = precinct.version - 1;
                }
            }
            if (File.Exists(txtSurfaces.Text))
            {
                try
                {
                    streamsurfaces = new PCKStream(txtSurfaces.Text);
                    var surfaces = ReadFileTable(streamsurfaces);
                    Maps = new List<GameMapInfo>();
                    List<PCKFileEntry> AllFiles = surfaces.Where(
                        z => z.Path.Contains("minimaps") && !z.Path.Contains("surfaces\\minimaps\\world"))
                        .ToList();
                    List<string> AlreadyExistion = new List<string>();
                    foreach (PCKFileEntry fs in AllFiles)
                    {
                        GameMapInfo map = new GameMapInfo();
                        List<string> st = fs.Path.Split('\\').ToList();
                        map.MapName = st.ElementAt(st.Count - 2);
                        st.RemoveAt(st.Count - 1);
                        map.MapPath = string.Join("\\", st);
                        if (AlreadyExistion.FindIndex(v => v == string.Join("\\", st)) == -1)
                        {
                            map.MapFragments = AllFiles.Where(z => z.Path.Contains(map.MapPath))
                                .ToList()
                                .OrderBy(z => z.Path)
                                .ToList();
                            Maps.Add(map);
                            AlreadyExistion.Add(map.MapPath);
                        }
                    }
                    if (Maps.Count != 0)
                    {
                        GameMapInfo map1 = new GameMapInfo() { MapName = "World", MapPath = "surfaces\\maps\\" };
                        map1.MapFragments = surfaces.Where(z => z.Path.Contains(map1.MapPath)).ToList();
                        Maps.Add(map1);
                        cbPrecinctMaps.Properties.Items.Clear();
                        for (int i = 0; i < Maps.Count; i++)
                        {
                            cbPrecinctMaps.Properties.Items.Add(Maps[i].MapName);
                        }
                        cbPrecinctMaps.SelectedIndex = cbPrecinctMaps.Properties.Items.Count - 1;
                    }
                }
                catch
                {
                    XtraMessageBox.Show("Não foi possível carregar o surfaces.pck, possivelmente o arquivo esteja protegido ou corrompido.");
                    cbPrecinctMaps.Properties.Items.Clear();
                    Maps = null;
                }
            }
            else
            {
                XtraMessageBox.Show("Arquivo surfaces.pck não encontrado");
            }
        }

        private void gridView10_RowClick(object sender, RowClickEventArgs e)
        {
            CELPrecinct val = gvPrecinct.FocusedRowObject as CELPrecinct;
            if (val != null)
            {
                txtm_strName.Text = val.m_strName;
                txtm_strSound.Text = val.m_strSound;
                txtm_strSound_n.Text = val.m_strSound_n;
                ckm_bNightSFX.Checked = val.m_bNightSFX;
                txtm_dwID.Value = val.m_dwID;
                txtm_iMusicInter.Value = val.m_iMusicInter;
                cbm_iMusicLoop.SelectedIndex = val.m_iMusicLoop;
                gcPrecinctListaMarcacoes.DataSource = val.m_aMarks;
                gcPrecinctListaMusicas.DataSource = val.m_aMusicFiles;
                gcPrecinctListaPosicoes.DataSource = val.m_aPoints;
                txtm_idDstInst.Value = val.m_idDstInst;
                txtm_idSrcInst.Value = val.m_idSrcInst;
                txtm_iPriority.Value = val.m_iPriority;
                txtm_idDomain.Value = val.m_idDomain;
                ckm_bPKProtect.Checked = val.m_bPKProtect;
                m_vCityPosX.Value = Convert.ToDecimal(val.m_vCityPos.X.ToString("F6"));
                m_vCityPosY.Value = Convert.ToDecimal(val.m_vCityPos.Y.ToString("F6"));
                m_vCityPosZ.Value = Convert.ToDecimal(val.m_vCityPos.Z.ToString("F6"));
                m_fLeft.Value = Convert.ToDecimal(val.m_fLeft.ToString("F6"));
                m_fRight.Value = Convert.ToDecimal(val.m_fRight.ToString("F6"));
                m_fTop.Value = Convert.ToDecimal(val.m_fTop.ToString("F6"));
                m_fBottom.Value = Convert.ToDecimal(val.m_fBottom.ToString("F6"));
                /*if (val.m_aPoints.Count > 0)
                {
                    txtPrecinctPointsX.Value = Convert.ToDecimal(val.m_aPoints[0].X.ToString("F6"));
                    txtPrecinctPointsY.Value = Convert.ToDecimal(val.m_aPoints[0].Y.ToString("F6"));
                    txtPrecinctPointsZ.Value = Convert.ToDecimal(val.m_aPoints[0].Z.ToString("F6"));
                } else
                {
                    txtPrecinctPointsX.Value = Convert.ToDecimal(0.ToString("F6"));
                    txtPrecinctPointsY.Value = Convert.ToDecimal(0.ToString("F6"));
                    txtPrecinctPointsZ.Value = Convert.ToDecimal(0.ToString("F6"));
                }*/
                simpleButton32_Click(sender, e);
            }
        }

        private void groupControl24_Paint(object sender, PaintEventArgs e)
        {

        }

        private void m_vCityPosY_EditValueChanged(object sender, EventArgs e)
        {

        }

        private void simpleButton29_Click(object sender, EventArgs e)
        {
            precinct.version = cbPrecinctVersion.SelectedIndex + 1;
            precinct.m_aPrecincts = gvPrecinct.DataSource as List<CELPrecinct>;
            if (precinct != null)
            {
                if (precinct.SaveClt(txtPrecinct.Text, precinct.version))
                    XtraMessageBox.Show("Precinct (cliente) salvo com sucesso!");
            }
        }

        private void gvPrecinctListaPosicoes_CustomDrawCell(object sender, RowCellCustomDrawEventArgs e)
        {
            CELPrecinct.VECTOR3 v = (CELPrecinct.VECTOR3)gvPrecinctListaPosicoes.GetRow(e.RowHandle);
            if (e.Column.VisibleIndex == 0)
                e.DisplayText = $"Ponto {e.RowHandle + 1}";
            if (e.Column.VisibleIndex == 1)
                e.DisplayText = v.X.ToString("F6");
            if (e.Column.VisibleIndex == 2)
                e.DisplayText = v.Y.ToString("F6");
            if (e.Column.VisibleIndex == 3)
                e.DisplayText = v.Z.ToString("F6");

        }

        private void PrecinctDefault_EnterPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                PrecinctDefaultLeave(sender, null);
        }

        private void PrecinctDefaultLeave(object sender, EventArgs e)
        {
            var p = gvPrecinct.FocusedRowObject as CELPrecinct;
            if (p == null)
                return;
            Control c = sender as Control;
            if (c == null)
                return;

            switch (c.Name)
            {
                case "txtm_strName":
                    {
                        p.m_strName = (c as TextEdit).Text;
                        break;
                    }
                case "txtm_strSound":
                    {
                        p.m_strSound = (c as TextEdit).Text;
                        break;
                    }
                case "txtm_strSound_n":
                    {
                        p.m_strSound_n = (c as TextEdit).Text;
                        break;
                    }
                case "ckm_bNightSFX":
                    {
                        p.m_bNightSFX = (c as CheckEdit).Checked;
                        break;
                    }
                case "ckm_bPKProtect":
                    {
                        p.m_bPKProtect = (c as CheckEdit).Checked;
                        break;
                    }
                case "txtm_dwID":
                    {
                        p.m_dwID = Convert.ToUInt32((c as SpinEdit).Value);
                        break;
                    }
                case "txtm_iMusicInter":
                    {
                        p.m_iMusicInter = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "txtm_idDstInst":
                    {
                        p.m_idDstInst = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "txtm_idSrcInst":
                    {
                        p.m_idSrcInst = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "txtm_iPriority":
                    {
                        p.m_iPriority = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "txtm_idDomain":
                    {
                        p.m_idDomain = Convert.ToInt32((c as SpinEdit).Value);
                        break;
                    }
                case "m_vCityPosX":
                    {
                        p.m_vCityPos.X = Convert.ToDouble((c as SpinEdit).Value);
                        break;
                    }
                case "m_vCityPosY":
                    {
                        p.m_vCityPos.Y = Convert.ToDouble((c as SpinEdit).Value);
                        break;
                    }
                case "m_vCityPosZ":
                    {
                        p.m_vCityPos.Z = Convert.ToDouble((c as SpinEdit).Value);
                        break;
                    }
                case "m_fLeft":
                    {
                        p.m_fLeft = Convert.ToDouble((c as SpinEdit).Value);
                        break;
                    }
                case "m_fRight":
                    {
                        p.m_fRight = Convert.ToDouble((c as SpinEdit).Value);
                        break;
                    }
                case "m_fTop":
                    {
                        p.m_fTop = Convert.ToDouble((c as SpinEdit).Value);
                        break;
                    }
                case "m_fBottom":
                    {
                        p.m_fBottom = Convert.ToDouble((c as SpinEdit).Value);
                        break;
                    }
                case "cbm_iMusicLoop":
                    {
                        p.m_iMusicLoop = (c as ComboBoxEdit).SelectedIndex;
                        break;
                    }
            }
            if (p.m_aMarks.Count > 0)
                if (gvPrecinctListaMarcacoes.SelectedRowsCount > 0)
                {
                    var o = (CELPrecinct.MARK)gvPrecinctListaMarcacoes.FocusedRowObject;
                    switch (c.Name)
                    {
                        case "m_aMarksName":
                            {
                                o.strName = (c as TextEdit).Text;
                                break;
                            }
                        case "m_aMarksX":
                            {
                                o.vPos.X = Convert.ToDouble((c as SpinEdit).Value);
                                break;
                            }
                        case "m_aMarksY":
                            {
                                o.vPos.Y = Convert.ToDouble((c as SpinEdit).Value);
                                break;
                            }
                        case "m_aMarksZ":
                            {
                                o.vPos.Z = Convert.ToDouble((c as SpinEdit).Value);
                                break;
                            }
                    }
                    p.m_aMarks[gvPrecinctListaMarcacoes.FocusedRowHandle] = o;
                }
            if (p.m_aMusicFiles.Count > 0)
                if (gvPrecinctListaMusicas.SelectedRowsCount > 0)
                {
                    var o = (CELPrecinct.MUSICFILES)gvPrecinctListaMusicas.FocusedRowObject;
                    switch (c.Name)
                    {
                        case "m_aMarksName":
                            {
                                o.strName = (c as TextEdit).Text;
                                break;
                            }
                    }
                    p.m_aMusicFiles[gvPrecinctListaMusicas.FocusedRowHandle] = o;
                }
            /*if (p.m_aPoints.Count > 0)
                if (gvPrecinctListaPosicoes.SelectedRowsCount > 0)
                {
                    var o = (CELPrecinct.VECTOR3)gvPrecinctListaPosicoes.FocusedRowObject;
                    switch (c.Name)
                    {
                        case "txtPrecinctPointsX":
                            {
                                o.X = Convert.ToDouble((c as SpinEdit).Value);
                                break;
                            }
                        case "txtPrecinctPointsY":
                            {
                                o.Y = Convert.ToDouble((c as SpinEdit).Value);
                                break;
                            }
                        case "txtPrecinctPointsZ":
                            {
                                o.Z = Convert.ToDouble((c as SpinEdit).Value);
                                break;
                            }
                    }
                    p.m_aPoints[gvPrecinctListaPosicoes.FocusedRowHandle] = o;
                    gvPrecinctListaPosicoes.RefreshData();
                }*/
        }

        private void PrecintDefault_KeyUp(object sender, KeyEventArgs e)
        {
            PrecinctDefaultLeave(sender, null);
        }

        private async void simpleButton31_Click(object sender, EventArgs e)
        {
            if (Maps != null)
            {
                int SelectedIndex = cbPrecinctMaps.SelectedIndex;
                System.Threading.Thread th = new System.Threading.Thread(
                    () => LinkMaps(Maps[SelectedIndex].MapFragments, Maps[SelectedIndex].MapName, 1));
                th.Start();
            }
        }

        private void gvPrecinct_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0)
            {
                return;
            }
            if (gvPrecinct.FocusedRowObject == null)
                return;
            PrecinctDefaultLeave(sender, null);
        }

        private void gvPrecinctListaMarcacoes_RowClick(object sender, RowClickEventArgs e)
        {
            if (e.RowHandle == -1) return;
            CELPrecinct.MARK val = (CELPrecinct.MARK)gvPrecinctListaMarcacoes.FocusedRowObject;
            m_aMarksName.Text = val.strName;
            m_aMarksX.Value = Convert.ToDecimal(val.vPos.X.ToString("F6"));
            m_aMarksY.Value = Convert.ToDecimal(val.vPos.Y.ToString("F6"));
            m_aMarksZ.Value = Convert.ToDecimal(val.vPos.Z.ToString("F6"));
        }

        private void gvPrecinctListaMarcacoes_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0)
            {
                return;
            }
            if (gvPrecinctListaMarcacoes.FocusedRowObject == null)
                return;
            gvPrecinctListaMarcacoes_RowClick(sender, null);
        }

        private void gvPrecinctListaMusicas_RowClick(object sender, RowClickEventArgs e)
        {
            if (e.RowHandle == -1) return;
            CELPrecinct.MUSICFILES val = (CELPrecinct.MUSICFILES)gvPrecinctListaMusicas.FocusedRowObject;
            m_aMusicName.Text = val.strName;
        }

        private void gvPrecinctListaMusicas_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
        }

        private void gvPrecinctListaPosicoes_RowClick(object sender, RowClickEventArgs e)
        {
            if (gvPrecinctListaPosicoes.FocusedRowObject == null) return;
            CELPrecinct.VECTOR3 val = (CELPrecinct.VECTOR3)gvPrecinctListaPosicoes.FocusedRowObject;
            txtPrecinctPointsX.Value = Convert.ToDecimal(val.X.ToString("F6"));
            txtPrecinctPointsY.Value = Convert.ToDecimal(val.Y.ToString("F6"));
            txtPrecinctPointsZ.Value = Convert.ToDecimal(val.Z.ToString("F6"));
        }

        private void gvPrecinctListaPosicoes_FocusedRowChanged(object sender, FocusedRowChangedEventArgs e)
        {
            if (e.FocusedRowHandle < 0)
            {
                return;
            }
            if (gvPrecinctListaPosicoes.FocusedRowObject == null)
                return;
            gvPrecinctListaPosicoes_RowClick(sender, null);
        }

        private void simpleButton32_Click(object sender, EventArgs e)
        {
            if (gvPrecinct.FocusedRowObject == null) return;
            if (telaMapa == null) return;
            List<PointF> points = new List<PointF>();
            CELPrecinct val = (CELPrecinct)gvPrecinct.FocusedRowObject;
            foreach (var point in val.m_aPoints)
            {
                points.Add(new PointF((float)point.X, (float)point.Z));
            }
            telaMapa.DesenhaPontos(points);
        }

        private void simpleButton33_Click(object sender, EventArgs e)
        {
            precinct.version = cbPrecinctVersion.SelectedIndex + 1;
            precinct.m_aPrecincts = gvPrecinct.DataSource as List<CELPrecinct>;
            if (precinct != null)
            {
                if (txtPrecinct.Text.EndsWith("sev"))
                {
                    if (precinct.SaveSev(txtPrecinct.Text, precinct.version))
                        XtraMessageBox.Show("Precinct (servidor) salvo com sucesso!");
                }
                else if (txtPrecinct.Text.EndsWith("clt"))
                {
                    var txt = txtPrecinct.Text.Replace(".clt", ".sev");
                    if (precinct.SaveSev(txt, precinct.version))
                        XtraMessageBox.Show("Precinct (servidor) salvo com sucesso!");
                }
            }
        }

        private void groupControl30_CustomButtonClick(object sender, DevExpress.XtraBars.Docking2010.BaseButtonEventArgs e)
        {
            List<CELPrecinct> lista = gvPrecinct.DataSource as List<CELPrecinct>;
            Console.WriteLine("Botao " + e.Button.Properties.VisibleIndex);
            if (e.Button.Properties.VisibleIndex == 0)
            {
                lista.Add(new CELPrecinct
                {
                    m_aMarks = new List<CELPrecinct.MARK>(),
                    m_aMusicFiles = new List<CELPrecinct.MUSICFILES>(),
                    m_aPoints = new List<CELPrecinct.VECTOR3>(),
                    m_bNightSFX = false,
                    m_bPKProtect = false,
                    m_dwID = lista.Count > 0 ? (lista[lista.Count - 1].m_dwID + 1) : 1,
                    m_fBottom = 0.0,
                    m_fLeft = 0.0,
                    m_fRight = 0.0,
                    m_fTop = 0.0,
                    m_idDomain = 0,
                    m_idDstInst = 0,
                    m_idSrcInst = 0,
                    m_iMusicInter = 60,
                    m_iMusicLoop = 0,
                    m_iPriority = 0,
                    m_strName = $"Nova Área #{(lista.Count).ToString("D4")}",
                    m_strSound = "",
                    m_strSound_n = "",
                    m_vCityPos = new CELPrecinct.VECTOR3()
                });
            }
            if (e.Button.Properties.VisibleIndex == 1)
            {
                int[] rows = gvPrecinct.GetSelectedRows();
                foreach (int rowHandle in rows.OrderByDescending(r => r))
                {
                    lista.RemoveAt(rowHandle);
                }
            }
            gcPrecinct.RefreshDataSource();
            gvPrecinct.RefreshData();
        }

        private void groupControl31_CustomButtonClick(object sender, DevExpress.XtraBars.Docking2010.BaseButtonEventArgs e)
        {
            switch (e.Button.Properties.VisibleIndex)
            {
                case 0:
                    {
                        var bs = gridView1.DataSource as List<DefaultMonsters>;
                        if (bs != null)
                        {
                            bs.Add(
                                new DefaultMonsters
                                {
                                    Amount_in_group = 1,
                                    BInitGen = 0,
                                    BValicOnce = 0,
                                    bAutoRevive = 0,
                                    dwGenId = 0,
                                    iGroupType = 0,
                                    Life_time = 0,
                                    Location = 0,
                                    MaxRespawnTime = 0,
                                    Trigger_id = 0,
                                    Type = 0,
                                    X_direction = 0,
                                    X_position = 0,
                                    X_random = 0,
                                    Y_direction = 0,
                                    Y_position = 0,
                                    Y_random = 0,
                                    Z_direction = 0,
                                    Z_position = 0,
                                    Z_random = 0,
                                    MobDops =
                                        new List<ExtraMonsters>
                                            {
                                    new ExtraMonsters
                                    {
                                        Agression = 0,
                                        Amount = 1,
                                        bFaction = 0,
                                        bFac_Accept = 0,
                                        bFac_Helper = 0,
                                        bNeedHelp = 0,
                                        Dead_amount = 0,
                                        Dead_time = 0,
                                        fOffsetTrn = 0,
                                        fOffsetWater = 0,
                                        Group = 0,
                                        Group_help_Needer = 0,
                                        Group_help_sender = 0,
                                        Id = 16,
                                        Path = 0,
                                        Path_type = 0,
                                        RefreshLower = 0,
                                        Respawn = 0,
                                        Speed = 0,
                                    }
                                            }
                                });
                            gridView1.RefreshData();
                            gridView1.MoveLast();
                        }
                        break;
                    }
                case 1:
                    {
                        if (gridView1.FocusedRowHandle < 0)
                            return;
                        var bs = gridView1.DataSource as List<DefaultMonsters>;
                        if (bs != null)
                        {
                            bs.Remove(gridView1.FocusedRowObject as DefaultMonsters);
                            gridView1.RefreshData();
                            gridView1.MoveLast();
                        }
                        break;
                    }
                case 2:
                    {
                        if (gridView1.FocusedRowHandle < 0)
                            return;
                        var bs = gridView1.DataSource as List<DefaultMonsters>;
                        if (bs != null)
                        {
                            bs.Add(gridView1.FocusedRowObject as DefaultMonsters);
                            gridView1.RefreshData();
                            gridView1.MoveLast();
                        }
                        break;
                    }
                case 3:
                    {
                        if (npcgen == null) break;
                        XtraOpenFileDialog ofd = new XtraOpenFileDialog();
                        ofd.FileName = "NPCGen Monstros_NPCs";
                        ofd.Filter = "NPCGen Monstros/NPCs | *.npcmn";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            BinaryReader br = new BinaryReader(File.Open(ofd.FileName, FileMode.Open));
                            if (Encoding.Default.GetString(br.ReadBytes(41)).Split(new string[] { "-" }, StringSplitOptions.None).ElementAt(1).Trim() == "Monstros/NPCs")
                            {
                                int Amount = br.ReadInt32();
                                var NpcMobList = gridView1.DataSource as List<DefaultMonsters>;
                                int vers = int.Parse(cbVersion.SelectedItem.ToString());
                                for (int i = 0; i < Amount; i++)
                                {
                                    DefaultMonsters mo = npcgen.ReadExistence(br, vers);
                                    NpcMobList.Add(mo);
                                }
                            }
                            else
                            {
                                break;
                            }
                            br.Close();
                            gridView1.RefreshData();
                        }
                    }
                    break;

                case 4:
                    {
                        if (npcgen == null) break;
                        XtraSaveFileDialog ofd = new XtraSaveFileDialog();
                        if (gridView1.RowCount != 0)
                        {
                            int[] rows = gridView1.GetSelectedRows();
                            ofd.FileName = string.Format("NPCGen Monstros_NPCs [{0}]", rows.Count());
                            ofd.Filter = "NPCGen Monstros/NPCs | *.npcmn";
                            if (ofd.ShowDialog() == DialogResult.OK)
                            {
                                int vers = int.Parse(cbVersion.SelectedItem.ToString());
                                BinaryWriter bw = new BinaryWriter(File.Create(ofd.FileName));
                                bw.Write("NPCGen editado por Alien - Monstros/NPCs");
                                bw.Write(rows.Count());
                                var NpcMobList = gridView1.DataSource as List<DefaultMonsters>;
                                foreach (int i in rows)
                                {
                                    bw.Write(NpcMobList[i].Location);
                                    bw.Write(NpcMobList[i].Amount_in_group);
                                    bw.Write(NpcMobList[i].X_position);
                                    bw.Write(NpcMobList[i].Y_position);
                                    bw.Write(NpcMobList[i].Z_position);
                                    bw.Write(NpcMobList[i].X_direction);
                                    bw.Write(NpcMobList[i].Y_direction);
                                    bw.Write(NpcMobList[i].Z_direction);
                                    bw.Write(NpcMobList[i].X_random);
                                    bw.Write(NpcMobList[i].Y_random);
                                    bw.Write(NpcMobList[i].Z_random);
                                    bw.Write(NpcMobList[i].Type);
                                    bw.Write(NpcMobList[i].iGroupType);
                                    bw.Write(NpcMobList[i].BInitGen);
                                    bw.Write(NpcMobList[i].bAutoRevive);
                                    bw.Write(NpcMobList[i].BValicOnce);
                                    bw.Write(NpcMobList[i].dwGenId);
                                    if (vers > 6)
                                    {
                                        bw.Write(NpcMobList[i].Trigger_id);
                                        bw.Write(NpcMobList[i].Life_time);
                                        bw.Write(NpcMobList[i].MaxRespawnTime);
                                    }
                                    for (int k = 0; k < NpcMobList[i].Amount_in_group; k++)
                                    {
                                        bw.Write(NpcMobList[i].MobDops[k].Id);
                                        bw.Write(NpcMobList[i].MobDops[k].Amount);
                                        bw.Write(NpcMobList[i].MobDops[k].Respawn);
                                        bw.Write(NpcMobList[i].MobDops[k].Dead_amount);
                                        bw.Write(NpcMobList[i].MobDops[k].Agression);
                                        bw.Write(NpcMobList[i].MobDops[k].fOffsetWater);
                                        bw.Write(NpcMobList[i].MobDops[k].fOffsetTrn);
                                        bw.Write(NpcMobList[i].MobDops[k].Group);
                                        bw.Write(NpcMobList[i].MobDops[k].Group_help_sender);
                                        bw.Write(NpcMobList[i].MobDops[k].Group_help_Needer);
                                        bw.Write(NpcMobList[i].MobDops[k].bNeedHelp);
                                        bw.Write(NpcMobList[i].MobDops[k].bFaction);
                                        bw.Write(NpcMobList[i].MobDops[k].bFac_Helper);
                                        bw.Write(NpcMobList[i].MobDops[k].bFac_Accept);
                                        bw.Write(NpcMobList[i].MobDops[k].Path);
                                        bw.Write(NpcMobList[i].MobDops[k].Path_type);
                                        bw.Write(NpcMobList[i].MobDops[k].Speed);
                                        bw.Write(NpcMobList[i].MobDops[k].Dead_time);
                                        if (vers >= 11)
                                        {
                                            bw.Write(NpcMobList[i].MobDops[k].RefreshLower);
                                        }
                                    }
                                }
                                bw.Close();
                                gridView1.RefreshData();
                            }
                        }
                        break;
                    }
            }
        }

        private void groupControl32_CustomButtonClick(object sender, DevExpress.XtraBars.Docking2010.BaseButtonEventArgs e)
        {
            switch (e.Button.Properties.VisibleIndex)
            {
                case 0:
                    {
                        var bs = gridView4.DataSource as List<DefaultResources>;
                        if (bs != null)
                        {
                            bs.Add(
                                new DefaultResources
                                {
                                    Amount_in_group = 1,
                                    bAutoRevive = 0,
                                    bValidOnce = 0,
                                    bInitGen = 0,
                                    dwGenID = 0,
                                    IMaxNum = 1,
                                    InCline1 = 0,
                                    InCline2 = 0,
                                    Rotation = 0,
                                    Trigger_id = 0,
                                    X_position = 0,
                                    X_Random = 0,
                                    Y_position = 0,
                                    Z_position = 0,
                                    Z_Random = 0,
                                    ResExtra =
                                        new List<ExtraResources>
                                            {
                                    new ExtraResources
                                    {
                                        Amount = 1,
                                        fHeiOff = 0,
                                        Id = 16,
                                        ResourceType = 80,
                                        Respawntime = 60
                                    }
                                            }
                                });

                            gridView4.RefreshData();
                            gridView4.MoveLast();
                        }
                        break;
                    }
                case 1:
                    {
                        if (gridView4.FocusedRowHandle < 0)
                            return;
                        var bs = gridView4.DataSource as List<DefaultResources>;
                        if (bs != null)
                        {
                            bs.Remove(gridView4.FocusedRowObject as DefaultResources);
                            gridView4.RefreshData();
                            gridView4.MoveLast();
                        }
                        break;
                    }
                case 2:
                    {
                        if (gridView4.FocusedRowHandle < 0)
                            return;
                        var bs = gridView4.DataSource as List<DefaultResources>;
                        if (bs != null)
                        {
                            bs.Add(gridView4.FocusedRowObject as DefaultResources);
                            gridView4.RefreshData();
                            gridView4.MoveLast();
                        }
                        break;
                    }
                case 3:
                    {
                        if (npcgen == null) break;
                        XtraOpenFileDialog ofd = new XtraOpenFileDialog();
                        ofd.FileName = "NPCGen Materiais_Minerais";
                        ofd.Filter = "NPCGen Materiais/Minerais | *.npcmm";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            BinaryReader br = new BinaryReader(File.Open(ofd.FileName, FileMode.Open));
                            if (Encoding.Default.GetString(br.ReadBytes(41)).Split(new string[] { "-" }, StringSplitOptions.None).ElementAt(1).Trim() == "Materiais/Minerais")
                            {
                                int Amount = br.ReadInt32();
                                var ResourcesList = gridView4.DataSource as List<DefaultResources>;
                                int vers = int.Parse(cbVersion.SelectedItem.ToString());
                                for (int i = 0; i < Amount; i++)
                                {
                                    DefaultResources mm = npcgen.ReadResource(br, vers);
                                    ResourcesList.Add(mm);
                                }
                            }
                            else
                            {
                                break;
                            }
                            br.Close();
                            gridView4.RefreshData();
                        }
                        break;
                    }
                case 4:
                    {
                        if (npcgen == null) break;
                        XtraSaveFileDialog ofd = new XtraSaveFileDialog();
                        if (gridView4.RowCount != 0)
                        {
                            int[] rows = gridView4.GetSelectedRows();
                            ofd.FileName = string.Format("NPCGen Materiais_Minerais [{0}]", rows.Count());
                            ofd.Filter = "NPCGen Materiais/Minerais | *.npcmm";
                            if (ofd.ShowDialog() == DialogResult.OK)
                            {
                                int vers = int.Parse(cbVersion.SelectedItem.ToString());
                                BinaryWriter bw = new BinaryWriter(File.Create(ofd.FileName));
                                bw.Write("NPCGen editado por Alien - Materiais/Minerais");
                                bw.Write(rows.Count());
                                var ResourcesList = gridView4.DataSource as List<DefaultResources>;
                                foreach (int i in rows)
                                {
                                    bw.Write(ResourcesList[i].X_position);
                                    bw.Write(ResourcesList[i].Y_position);
                                    bw.Write(ResourcesList[i].Z_position);
                                    bw.Write(ResourcesList[i].X_Random);
                                    bw.Write(ResourcesList[i].Z_Random);
                                    bw.Write(ResourcesList[i].Amount_in_group);
                                    bw.Write(ResourcesList[i].bInitGen);
                                    bw.Write(ResourcesList[i].bAutoRevive);
                                    bw.Write(ResourcesList[i].bValidOnce);
                                    bw.Write(ResourcesList[i].dwGenID);
                                    bw.Write(ResourcesList[i].InCline1);
                                    bw.Write(ResourcesList[i].InCline2);
                                    bw.Write(ResourcesList[i].Rotation);
                                    bw.Write(ResourcesList[i].Trigger_id);
                                    bw.Write(ResourcesList[i].IMaxNum);
                                    for (int z = 0; z < ResourcesList[i].Amount_in_group; z++)
                                    {
                                        bw.Write(ResourcesList[i].ResExtra[z].ResourceType);
                                        bw.Write(ResourcesList[i].ResExtra[z].Id);
                                        bw.Write(ResourcesList[i].ResExtra[z].Respawntime);
                                        bw.Write(ResourcesList[i].ResExtra[z].Amount);
                                        bw.Write(ResourcesList[i].ResExtra[z].fHeiOff);
                                    }
                                }
                                bw.Close();
                                gridView4.RefreshData();
                            }
                        }
                        break;
                    }
            }
        }

        private void groupControl4_CustomButtonClick(object sender, DevExpress.XtraBars.Docking2010.BaseButtonEventArgs e)
        {
            switch (e.Button.Properties.VisibleIndex)
            {
                case 0:
                    {
                        var bs = gridView2.DataSource as List<ExtraMonsters>;
                        if (bs != null)
                        {
                            bs.Add(
                                new ExtraMonsters
                                {
                                    Agression = 0,
                                    Amount = 1,
                                    bFaction = 0,
                                    bFac_Accept = 0,
                                    bFac_Helper = 0,
                                    bNeedHelp = 0,
                                    Dead_amount = 0,
                                    Dead_time = 0,
                                    fOffsetTrn = 0,
                                    fOffsetWater = 0,
                                    Group = 0,
                                    Group_help_Needer = 0,
                                    Group_help_sender = 0,
                                    Id = 16,
                                    Path = 0,
                                    Path_type = 0,
                                    RefreshLower = 0,
                                    Respawn = 0,
                                    Speed = 0,
                                });
                            gridView2.RefreshData();
                            gridView2.MoveLast();
                            mobAmountInGroup.Value = gridView2.RowCount;
                        }
                        break;
                    }
                case 1:
                    {
                        if (gridView2.FocusedRowHandle < 0)
                            return;
                        var bs = gridView2.DataSource as List<ExtraMonsters>;
                        if (bs != null)
                        {
                            bs.Remove(gridView2.FocusedRowObject as ExtraMonsters);
                            gridView2.RefreshData();
                            gridView2.MoveLast();
                            mobAmountInGroup.Value = gridView2.RowCount;
                        }
                        break;
                    }
                case 2:
                    {
                        if (gridView2.FocusedRowHandle < 0)
                            return;
                        var bs = gridView2.DataSource as List<ExtraMonsters>;
                        if (bs != null)
                        {
                            bs.Add(gridView2.FocusedRowObject as ExtraMonsters);
                            gridView2.RefreshData();
                            gridView2.MoveLast();
                            mobAmountInGroup.Value = gridView2.RowCount;
                        }
                        break;
                    }
            }
        }

        private void groupControl5_CustomButtonClick(object sender, DevExpress.XtraBars.Docking2010.BaseButtonEventArgs e)
        {
            switch (e.Button.Properties.VisibleIndex)
            {
                case 0:
                    {
                        var bs = gridView3.DataSource as List<ExtraResources>;
                        if (bs != null)
                        {
                            bs.Add(
                                new ExtraResources { Amount = 1, fHeiOff = 0, Id = 16, ResourceType = 80, Respawntime = 60 }

                        );
                            gridView3.RefreshData();
                            gridView3.MoveLast();
                            resInGroup.Value = gridView3.RowCount;
                        }
                        break;
                    }
                case 1:
                    {
                        if (gridView3.FocusedRowHandle < 0)
                            return;
                        var bs = gridView3.DataSource as List<ExtraResources>;
                        if (bs != null)
                        {
                            bs.Remove(gridView3.FocusedRowObject as ExtraResources);
                            gridView3.RefreshData();
                            gridView3.MoveLast();
                            resInGroup.Value = gridView3.RowCount;
                        }
                        break;
                    }
                case 2:
                    {
                        if (gridView3.FocusedRowHandle < 0)
                            return;
                        var bs = gridView3.DataSource as List<ExtraResources>;
                        if (bs != null)
                        {
                            bs.Add(gridView3.FocusedRowObject as ExtraResources);
                            gridView3.RefreshData();
                            gridView3.MoveLast();
                            resInGroup.Value = gridView3.RowCount;
                        }
                        break;
                    }
            }
        }

        private void groupControl33_CustomButtonClick(object sender, DevExpress.XtraBars.Docking2010.BaseButtonEventArgs e)
        {
            switch (e.Button.Properties.VisibleIndex)
            {
                case 0:
                    {
                        var bs = gridView5.DataSource as List<DynamicObject>;
                        if (bs != null)
                        {
                            bs.Add(
                                new DynamicObject
                                {
                                    Id = 0,
                                    InCline1 = 0,
                                    InCline2 = 0,
                                    Rotation = 0,
                                    Scale = 0,
                                    TriggerId = 0,
                                    X_position = 0,
                                    Y_position = 0,
                                    Z_position = 0
                                });
                            gridView5.RefreshData();
                            gridView5.MoveLast();
                        }
                        break;
                    }
                case 1:
                    {
                        if (gridView5.FocusedRowHandle < 0)
                            return;
                        var bs = gridView5.DataSource as List<DynamicObject>;
                        if (bs != null)
                        {
                            bs.Remove(gridView5.FocusedRowObject as DynamicObject);
                            gridView5.RefreshData();
                            gridView5.MoveLast();
                        }
                        break;
                    }
                case 2:
                    {
                        if (gridView5.FocusedRowHandle < 0)
                            return;
                        var bs = gridView5.DataSource as List<DynamicObject>;
                        if (bs != null)
                        {
                            bs.Add(gridView5.FocusedRowObject as DynamicObject);
                            gridView5.RefreshData();
                            gridView5.MoveLast();
                        }
                        break;
                    }
                case 3:
                    {
                        if (npcgen == null) break;
                        XtraOpenFileDialog ofd = new XtraOpenFileDialog();
                        ofd.FileName = "NPCGen Objetos Dinamicos";
                        ofd.Filter = "NPCGen Objetos Dinamicos | *.npcod";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            BinaryReader br = new BinaryReader(File.Open(ofd.FileName, FileMode.Open));
                            if (Encoding.Default.GetString(br.ReadBytes(41)).Split(new string[] { "-" }, StringSplitOptions.None).ElementAt(1).Trim() == "Objetos Dinamicos")
                            {
                                int Amount = br.ReadInt32();
                                var dynobj = gridView5.DataSource as List<DynamicObject>;
                                int vers = int.Parse(cbVersion.SelectedItem.ToString());
                                for (int i = 0; i < Amount; i++)
                                {
                                    DynamicObject mm = npcgen.ReadDynObjects(br, vers);
                                    dynobj.Add(mm);
                                }
                            }
                            else
                            {
                                break;
                            }
                            br.Close();
                            gridView5.RefreshData();
                        }
                        break;
                    }
                case 4:
                    {
                        if (npcgen == null) break;
                        XtraSaveFileDialog ofd = new XtraSaveFileDialog();
                        if (gridView5.RowCount != 0)
                        {
                            int[] rows = gridView5.GetSelectedRows();
                            ofd.FileName = string.Format("NPCGen Objetos Dinamicos [{0}]", rows.Count());
                            ofd.Filter = "NPCGen Objetos Dinamicos | *.npcod";
                            if (ofd.ShowDialog() == DialogResult.OK)
                            {
                                int vers = int.Parse(cbVersion.SelectedItem.ToString());
                                BinaryWriter bw = new BinaryWriter(File.Create(ofd.FileName));
                                bw.Write("NPCGen editado por Alien - Objetos Dinamicos");
                                bw.Write(rows.Count());
                                var DynamicsList = gridView5.DataSource as List<DynamicObject>;
                                foreach (int i in rows)
                                {
                                    bw.Write(DynamicsList[i].Id);
                                    bw.Write(DynamicsList[i].X_position);
                                    bw.Write(DynamicsList[i].Y_position);
                                    bw.Write(DynamicsList[i].Z_position);
                                    bw.Write(DynamicsList[i].InCline1);
                                    bw.Write(DynamicsList[i].InCline2);
                                    bw.Write(DynamicsList[i].Rotation);
                                    bw.Write(DynamicsList[i].TriggerId);
                                    bw.Write(DynamicsList[i].Scale);
                                }
                                bw.Close();
                                gridView5.RefreshData();
                            }
                        }
                        break;
                    }
            }
        }

        private void groupControl34_CustomButtonClick(object sender, DevExpress.XtraBars.Docking2010.BaseButtonEventArgs e)
        {
            switch (e.Button.Properties.VisibleIndex)
            {
                case 0:
                    {
                        var bs = gridView6.DataSource as List<Trigger>;
                        if (bs != null)
                        {
                            var last = bs.LastOrDefault();
                            bs.Add(
                                new Trigger
                                {
                                    AutoStart = 0,
                                    DontStartOnSchedule = 0,
                                    DontStopOnSchedule = 0,
                                    Duration = 0,
                                    GmID = last.GmID + 1,
                                    Id = last.Id + 1,
                                    StartDay = 0,
                                    StartHour = 0,
                                    StartMinute = 0,
                                    StartMonth = 0,
                                    StartWeekDay = 0,
                                    StartYear = 0,
                                    StopDay = 0,
                                    StopHour = 0,
                                    StopMinute = 0,
                                    StopMonth = 0,
                                    StopWeekDay = 0,
                                    StopYear = 0,
                                    TriggerName = string.Empty,
                                    WaitWhileStart = 0,
                                    WaitWhileStop = 0
                                });
                            gridView6.RefreshData();
                            gridView6.MoveLast();
                        }
                        break;
                    }
                case 1:
                    {
                        if (gridView6.FocusedRowHandle < 0)
                            return;
                        var bs = gridView6.DataSource as List<Trigger>;
                        if (bs != null)
                        {
                            bs.Remove(gridView6.FocusedRowObject as Trigger);
                            gridView6.RefreshData();
                            gridView6.MoveLast();
                        }
                        break;
                    }
                case 2:
                    {
                        if (gridView6.FocusedRowHandle < 0)
                            return;
                        var bs = gridView6.DataSource as List<Trigger>;
                        if (bs != null)
                        {
                            bs.Add(gridView6.FocusedRowObject as Trigger);
                            gridView6.RefreshData();
                            gridView6.MoveLast();
                        }
                        break;
                    }
                case 3:
                    {
                        if (npcgen == null) break;
                        XtraOpenFileDialog ofd = new XtraOpenFileDialog();
                        ofd.FileName = "NPCGen Controladores";
                        ofd.Filter = "NPCGen Controladores | *.npcct";
                        if (ofd.ShowDialog() == DialogResult.OK)
                        {
                            BinaryReader br = new BinaryReader(File.Open(ofd.FileName, FileMode.Open));
                            if (Encoding.Default.GetString(br.ReadBytes(41)).Split(new string[] { "-" }, StringSplitOptions.None).ElementAt(1).Trim() == "Controladores")
                            {
                                int Amount = br.ReadInt32();
                                var trigger = gridView6.DataSource as List<Trigger>;
                                int vers = int.Parse(cbVersion.SelectedItem.ToString());
                                for (int i = 0; i < Amount; i++)
                                {
                                    Trigger tg = npcgen.ReadTrigger(br, vers);
                                    trigger.Add(tg);
                                }
                            }
                            else
                            {
                                break;
                            }
                            br.Close();
                            gridView6.RefreshData();
                        }
                        break;
                    }
                case 4:
                    {
                        if (npcgen == null) break;
                        XtraSaveFileDialog ofd = new XtraSaveFileDialog();
                        if (gridView6.RowCount != 0)
                        {
                            int[] rows = gridView6.GetSelectedRows();
                            ofd.FileName = string.Format("NPCGen Controladores [{0}]", rows.Count());
                            ofd.Filter = "NPCGen Controladores | *.npcct";
                            if (ofd.ShowDialog() == DialogResult.OK)
                            {
                                int vers = int.Parse(cbVersion.SelectedItem.ToString());
                                BinaryWriter bw = new BinaryWriter(File.Create(ofd.FileName));
                                bw.Write("NPCGen editado por Alien - Controladores");
                                bw.Write(rows.Count());
                                var TriggersList = gridView6.DataSource as List<Trigger>;
                                foreach (int i in rows)
                                {
                                    bw.Write(TriggersList[i].Id);
                                    bw.Write(TriggersList[i].GmID);
                                    bw.Write(GetBytes(TriggersList[i].TriggerName, 128, Encoding.GetEncoding(936)));
                                    bw.Write(TriggersList[i].AutoStart);
                                    bw.Write(TriggersList[i].WaitWhileStart);
                                    bw.Write(TriggersList[i].WaitWhileStop);
                                    if (TriggersList[i].DontStartOnSchedule == 1) bw.Write((byte)0);
                                    else bw.Write((byte)1);
                                    if (TriggersList[i].DontStopOnSchedule == 1) bw.Write((byte)0);
                                    else bw.Write((byte)1);
                                    bw.Write(TriggersList[i].StartYear);
                                    bw.Write(TriggersList[i].StartMonth);
                                    bw.Write(TriggersList[i].StartWeekDay);
                                    bw.Write(TriggersList[i].StartDay);
                                    bw.Write(TriggersList[i].StartHour);
                                    bw.Write(TriggersList[i].StartMinute);
                                    bw.Write(TriggersList[i].StopYear);
                                    bw.Write(TriggersList[i].StopMonth);
                                    bw.Write(TriggersList[i].StopWeekDay);
                                    bw.Write(TriggersList[i].StopDay);
                                    bw.Write(TriggersList[i].StopHour);
                                    bw.Write(TriggersList[i].StopMinute);
                                    if (vers > 7)
                                    {
                                        bw.Write(TriggersList[i].Duration);
                                    }
                                }
                                bw.Close();
                                gridView6.RefreshData();
                            }
                        }
                        break;
                    }
            }
        }

        public byte[] GetBytes(string Name, int NameLength, Encoding e)
        {
            Name = Name.Split('\0')[0];
            byte[] data = new byte[NameLength];
            if (e.GetByteCount(Name) > NameLength)
            {
                Array.Copy(e.GetBytes(Name), 0, data, 0, NameLength);
            }
            else
            {
                Array.Copy(e.GetBytes(Name), data, e.GetByteCount(Name));
            }
            return data;
        }

        private void simpleButton7_Click(object sender, EventArgs e)
        {
            CarregaGS();
        }

        private void CarregaGS()
        {
            dataAllFile = null;
            Signatures.CurrentDataInPos.Clear();
            Signatures.DataPosToRepl.Clear();
            Signatures.SizeFunctions.Clear();
            if (File.Exists(txtPathGs.Text))
            {
                using (FileStream fileStream = new FileStream(txtPathGs.Text, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    dataAllFile = new byte[fileStream.Length];
                    fileStream.Read(dataAllFile, 0, (int)fileStream.Length);
                    data = new Data_Handling(this, dataAllFile);
                }
                if (data.getSizeFunctions(txtPathGs.Text) == -1 || data.SearchBytesToFile(Signatures.FunctionSignature.GS, -1, Signatures.Bytes.BYTE, true) < 0)
                {
                    data = null;
                    dataAllFile = null;
                    txtPathGs.Text = "";
                    Signatures.SizeFunctions.Clear();
                    XtraMessageBox.Show("Erro!");
                }
                else
                {
                    XtraMessageBox.Show($"Carregado! V: {data.Version}");
                    int num = data.SearchBytesToFile(Signatures.FunctionSignature.REFINE_ADDON_1, 9, Signatures.Bytes.INTEGER);
                    Signatures.DataPosToRepl.Add(num);
                    byte[] array = new byte[4];
                    Buffer.BlockCopy(dataAllFile, num, array, 0, 4);
                    Signatures.DataPosToRepl.Add(data.SearchBytesToFile(Signatures.FunctionSignature.REFINE_ADDON_2, 6, Signatures.Bytes.INTEGER));
                    Signatures.CurrentDataInPos.Add(array);
                    Signatures.CurrentDataInPos.Add(array);
                    txtRefino.Value = Signatures.BytesToDecimal(Signatures.Bytes.INTEGER, array);
                    AddListsPosAndData(Signatures.FunctionSignature.TOWNSCROLL2, 15, Signatures.Bytes.INTEGER, txtTeleporte);
                    AddListsPosAndData(Signatures.FunctionSignature.COSMETIC_SUCCESS, 4, Signatures.Bytes.INTEGER, txtRestauracao);
                    AddListsPosAndData(Signatures.FunctionSignature.RECURRECT_SCROLL, 4, Signatures.Bytes.INTEGER, txtRessurreicao);
                    AddListsPosAndData(Signatures.FunctionSignature.CHANGE_ELF_SECURE_STATUS, 5, Signatures.Bytes.INTEGER, txtDaimon);
                    AddListsPosAndData(Signatures.FunctionSignature.CONVERT_PET_TO_EGG, 8, Signatures.Bytes.INTEGER, txtPet);
                    AddListsPosAndData(Signatures.FunctionSignature.FIRST_RUN, 10, Signatures.Bytes.BYTE, txtConjMax);
                    AddListsPosAndData(Signatures.FunctionSignature.FIRST_RUN, 16, Signatures.Bytes.BYTE, txtConjSobr);
                    AddListsPosAndData(Signatures.FunctionSignature.CHANGE_INVENTORY_SIZE, -6, Signatures.Bytes.BYTE, txtInv);
                    AddListsPosAndData(Signatures.FunctionSignature.CHANGE_INVENTORY_SIZE, 0, Signatures.Bytes.INTEGER, txtInv);
                    AddListsPosAndData(Signatures.FunctionSignature.TEST_PK_PROTECTED, 16, Signatures.Bytes.BYTE, txtPK);
                    AddListsPosAndData(Signatures.FunctionSignature.PLAYER_ENABLE_PVP_STATE, 13, Signatures.Bytes.BYTE, txtPK);
                    AddListsPosAndData(Signatures.FunctionSignature.TRASH_BOX, -4, Signatures.Bytes.INTEGER, txtBanq);
                    AddListsPosAndData(Signatures.FunctionSignature.TRASH_BOX2, 12, Signatures.Bytes.INTEGER, txtBanq);
                    AddListsPosAndData(Signatures.FunctionSignature.SHARED_STORAGE, -4, Signatures.Bytes.INTEGER, txtConta);
                    AddListsPosAndData(Signatures.FunctionSignature.MONEY_CAPACITY1, 25, Signatures.Bytes.INTEGER, txtDinheiroMax);
                    AddListsPosAndData(Signatures.FunctionSignature.MONEY_CAPACITY2, 8, Signatures.Bytes.INTEGER, txtDinheiroMax);
                    AddListsPosAndData(Signatures.FunctionSignature.MONEY_CAPACITY3, 25, Signatures.Bytes.INTEGER, txtDinheiroMax);
                }
            }
        }

        private void AddListsPosAndData(Signatures.FunctionSignature signature, int offset, Signatures.Bytes bytes, SpinEdit textBox)
        {
            textBox.Value = -1;
            textBox.Enabled = true;
            int num = data.SearchBytesToFile(signature, offset, bytes);
            Signatures.DataPosToRepl.Add(num);
            if (num == -1)
            {
                textBox.Value = -1;
                textBox.Enabled = false;
                Signatures.CurrentDataInPos.Add(new byte[1]);
            }
            else
            {
                byte[] array = new byte[(int)bytes];
                Buffer.BlockCopy(dataAllFile, num, array, 0, (int)bytes);
                Signatures.CurrentDataInPos.Add(array);
                textBox.Text = Signatures.BytesToDecimal(bytes, array).ToString();
            }
        }

        private void txtPathGs_ButtonClick(object sender, DevExpress.XtraEditors.Controls.ButtonPressedEventArgs e)
        {
            using (XtraOpenFileDialog openFileDialog = new XtraOpenFileDialog())
            {
                openFileDialog.Title = "Selecionar GS";
                openFileDialog.Filter = "gs (gs)|gs";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    txtPathGs.Text = selectedFilePath;
                }
            }
        }

        private void txtRefino_Leave(object sender, EventArgs e)
        {
            SpinEdit spinEdit = sender as SpinEdit;
            if (spinEdit != null)
            {
                if (spinEdit.Value >= 0)
                {

                }
            }
        }

        private void simpleButton3_Click(object sender, EventArgs e)
        {

        }

        private void ReplDataInBuffer(SpinEdit spin, int index, Signatures.Bytes bytes)
        {
            if (spin.Enabled && Signatures.DataPosToRepl[index] >= 0)
            {
                byte[] array = Signatures.DecimalToBytes(bytes, spin.Value.ToString());
                if (!Signatures.CurrentDataInPos[index].SequenceEqual(array))
                {
                    data.ReplaceBytes(Signatures.DataPosToRepl[index], bytes, array);
                    dataAllFile = data.DataAllFile;
                }
            }
        }

        private void ReplDataInBufferTwoPlaces(int index, Signatures.Bytes bytes, byte[] newData)
        {
            if ((Signatures.DataPosToRepl[index] >= 0 || Signatures.DataPosToRepl[index + 1] >= 0) && !Signatures.CurrentDataInPos[index].SequenceEqual(newData))
            {
                data.ReplaceBytes(Signatures.DataPosToRepl[index], bytes, newData);
                data.ReplaceBytes(Signatures.DataPosToRepl[index + 1], bytes, newData);
                dataAllFile = data.DataAllFile;
            }
        }

        private void simpleButton5_Click_1(object sender, EventArgs e)
        {
            try
            {
                ReplDataInBufferTwoPlaces(0, Signatures.Bytes.INTEGER, new byte[4] { Convert.ToByte(txtRefino.Value), 0, 0, 0 });
                ReplDataInBuffer(txtTeleporte, 2, Signatures.Bytes.INTEGER);
                ReplDataInBuffer(txtRestauracao, 3, Signatures.Bytes.INTEGER);
                ReplDataInBuffer(txtRessurreicao, 4, Signatures.Bytes.INTEGER);
                ReplDataInBuffer(txtDaimon, 5, Signatures.Bytes.INTEGER);
                ReplDataInBuffer(txtPet, 6, Signatures.Bytes.INTEGER);
                ReplDataInBuffer(txtConjMax, 7, Signatures.Bytes.BYTE);
                ReplDataInBuffer(txtConjSobr, 8, Signatures.Bytes.BYTE);
                ReplDataInBufferTwoPlaces(9, Signatures.Bytes.BYTE, Signatures.DecimalToBytes(Signatures.Bytes.BYTE, txtInv.Value.ToString()));
                ReplDataInBuffer(txtPK, 11, Signatures.Bytes.BYTE);
                ReplDataInBuffer(txtPK, 12, Signatures.Bytes.BYTE);
                ReplDataInBuffer(txtBanq, 13, Signatures.Bytes.INTEGER);
                ReplDataInBuffer(txtBanq, 14, Signatures.Bytes.INTEGER);
                ReplDataInBuffer(txtConta, 15, Signatures.Bytes.INTEGER);
                ReplDataInBuffer(txtDinheiroMax, 16, Signatures.Bytes.INTEGER);
                ReplDataInBuffer(txtDinheiroMax, 17, Signatures.Bytes.INTEGER);
                ReplDataInBuffer(txtDinheiroMax, 18, Signatures.Bytes.INTEGER);
                File.WriteAllBytes(txtPathGs.Text, dataAllFile);
                XtraMessageBox.Show("Salvo com sucesso!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public static long FindPatternInFile(string filePath, byte[] pattern)
        {
            // Abre o arquivo binário para leitura
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, buffer.Length);

                // Procurando pelo padrão de bytes no arquivo
                for (long i = 0; i <= buffer.Length - pattern.Length; i++)
                {
                    bool found = true;
                    for (int j = 0; j < pattern.Length; j++)
                    {
                        // Verifica se o byte do padrão não é '?'
                        if (pattern[j] != '?' && pattern[j] != buffer[i + j])
                        {
                            found = false;
                            break;
                        }
                    }

                    if (found)
                    {
                        // Retorna a posição onde o padrão foi encontrado
                        return i;
                    }
                }
            }

            // Retorna -1 se o padrão não for encontrado no arquivo
            return -1;
        }

        private void simpleButton3_Click_1(object sender, EventArgs e)
        {
            Console.WriteLine(FindPatternInFile(txtPathGs.Text, Signatures.beginFuncSignatures[0]));
        }

        public bool ReadElements()
        {
            if (File.Exists(txtEEelements.Text))
            {
                try
                {
                    elc = new eListCollection(txtEEelements.Text);
                }
                catch (Exception ex)
                {
                    loadedElementsData = false;
                    XtraMessageBox.Show("Erro ao ler elements:\n" + ex.Message);
                }
                finally
                {
                    if (elc != null)
                        loadedElementsData = true;
                }
            }
            else return false;
            return loadedElementsData;
        }

        public bool ReadSurfaces()
        {
            if (File.Exists(txtEEelements.Text))
            {
                try
                {
                    var streamSurfaces = new PCKStream(txtEEsurfaces.Text);
                    var rftSurfaces = ReadFileTable(streamSurfaces);
                    for (int i = 0; i < rftSurfaces.Length; i++)
                    {
                        if (rftSurfaces[i].Path == "surfaces\\iconset\\iconlist_ivtrm.dds")
                        {
                            var read = ReadFile(streamSurfaces, rftSurfaces[i]);
                            iconlist_ivtrm = new DDSImage(read).BitmapImage;
                        }
                        else if (rftSurfaces[i].Path == "surfaces\\iconset\\iconlist_ivtrf.dds")
                        {
                            var read = ReadFile(streamSurfaces, rftSurfaces[i]);
                            iconlist_ivtrf = new DDSImage(read).BitmapImage;

                        }
                        else if (rftSurfaces[i].Path == "surfaces\\iconset\\iconlist_skill.dds")
                        {
                            var read = ReadFile(streamSurfaces, rftSurfaces[i]);
                            iconlist_skill = new DDSImage(read).BitmapImage;
                        }
                        else if (rftSurfaces[i].Path == "surfaces\\iconset\\iconlist_ivtrm.txt")
                        {
                            var read = ReadFile(streamSurfaces, rftSurfaces[i]);
                            StreamReader file = new StreamReader(new MemoryStream(read), Encoding.GetEncoding("GBK"));
                            string line;
                            List<string> fileNames = new List<string>();
                            imagesx_m = new SortedList<int, string>();
                            int w = 0;
                            int h = 0;
                            int rows = 0;
                            int cols = 0;
                            int counter = 0;
                            while ((line = file.ReadLine()) != null)
                            {
                                switch (counter)
                                {
                                    case 0:
                                        w = int.Parse(line);
                                        break;
                                    case 1:
                                        h = int.Parse(line);
                                        break;
                                    case 2:
                                        rows = int.Parse(line);
                                        break;
                                    case 3:
                                        cols = int.Parse(line);
                                        break;
                                    default:
                                        fileNames.Add(line);
                                        break;
                                }
                                counter++;
                            }
                            file.Close();
                            imageposition_m = new SortedList<int, ImagePosition>();
                            int x, y = 0;
                            for (int a = 0; a < fileNames.Count; a++)
                            {
                                Application.DoEvents();
                                y = a / cols;
                                x = a - y * cols;
                                x = x * w;
                                y = y * h;
                                imagesx_m.Add(a, fileNames[a]);
                                ImagePosition newpos = new ImagePosition();
                                newpos.name = fileNames[a];
                                newpos.pos = new Point(x, y);
                                imageposition_m.Add(a, newpos);
                            }
                        }
                        else if (rftSurfaces[i].Path == "surfaces\\iconset\\iconlist_ivtrf.txt")
                        {
                            var read = ReadFile(streamSurfaces, rftSurfaces[i]);
                            StreamReader file = new StreamReader(new MemoryStream(read), Encoding.GetEncoding("GBK"));
                            string line;
                            List<string> fileNames = new List<string>();
                            imagesx_f = new SortedList<int, string>();
                            int w = 0;
                            int h = 0;
                            int rows = 0;
                            int cols = 0;
                            int counter = 0;
                            while ((line = file.ReadLine()) != null)
                            {
                                switch (counter)
                                {
                                    case 0:
                                        w = int.Parse(line);
                                        break;
                                    case 1:
                                        h = int.Parse(line);
                                        break;
                                    case 2:
                                        rows = int.Parse(line);
                                        break;
                                    case 3:
                                        cols = int.Parse(line);
                                        break;
                                    default:
                                        fileNames.Add(line);
                                        break;
                                }
                                counter++;
                            }
                            file.Close();
                            imageposition_f = new SortedList<int, ImagePosition>();
                            int x, y = 0;
                            for (int a = 0; a < fileNames.Count; a++)
                            {
                                Application.DoEvents();
                                y = a / cols;
                                x = a - y * cols;
                                x = x * w;
                                y = y * h;
                                imagesx_f.Add(a, fileNames[a]);
                                ImagePosition newpos = new ImagePosition();
                                newpos.name = fileNames[a];
                                newpos.pos = new Point(x, y);
                                imageposition_f.Add(a, newpos);
                            }
                        }
                        else if (rftSurfaces[i].Path == "surfaces\\iconset\\iconlist_skill.txt")
                        {
                            var read = ReadFile(streamSurfaces, rftSurfaces[i]);
                            StreamReader file = new StreamReader(new MemoryStream(read), Encoding.GetEncoding("GBK"));
                            string line;
                            List<string> fileNames = new List<string>();
                            imagesx_skill = new SortedList<int, string>();
                            int w = 0;
                            int h = 0;
                            int rows = 0;
                            int cols = 0;
                            int counter = 0;
                            while ((line = file.ReadLine()) != null)
                            {
                                switch (counter)
                                {
                                    case 0:
                                        w = int.Parse(line);
                                        break;
                                    case 1:
                                        h = int.Parse(line);
                                        break;
                                    case 2:
                                        rows = int.Parse(line);
                                        break;
                                    case 3:
                                        cols = int.Parse(line);
                                        break;
                                    default:
                                        fileNames.Add(line);
                                        break;
                                }
                                counter++;
                            }
                            file.Close();
                            imageposition_skill = new SortedList<int, ImagePosition>();
                            int x, y = 0;
                            for (int a = 0; a < fileNames.Count; a++)
                            {
                                Application.DoEvents();
                                y = a / cols;
                                x = a - y * cols;
                                x = x * w;
                                y = y * h;
                                imagesx_skill.Add(a, fileNames[a]);
                                ImagePosition newpos = new ImagePosition();
                                newpos.name = fileNames[a];
                                newpos.pos = new Point(x, y);
                                imageposition_skill.Add(a, newpos);
                            }
                        }
                        else continue;
                    }
                    streamSurfaces.Dispose();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    loadedSurfaces = false;
                }
                finally
                {
                    loadedSurfaces = true;
                    GC.Collect();
                }
            }
            else return false;
            return loadedSurfaces;
        }


        public bool ReadConfigs()
        {
            if (File.Exists(txtEEconfigs.Text))
            {
                try
                {
                    var stream = new PCKStream(txtEEconfigs.Text);
                    var rftConfigs = ReadFileTable(stream);
                    for (int i = 0; i < rftConfigs.Length; i++)
                    {
                        if (rftConfigs[i].Path == "configs\\item_color.txt")
                        {
                            try
                            {
                                var read = ReadFile(stream, rftConfigs[i]);
                                StreamReader sr = new StreamReader(new MemoryStream(read));
                                while (sr.Peek() >= 0)
                                {
                                    var lerTudo = sr.ReadLine();
                                    var separa = lerTudo.Split('\t');
                                    var v1 = separa[0].ToString();
                                    var v2 = separa[1].ToString();
                                    if (v1.Length > 0 && v2.Length > 0)
                                    {
                                        ItemColor.Add(int.Parse(v1), int.Parse(v2));
                                    }
                                    else
                                    {
                                        if (v1.Length > 0)
                                        {
                                            ItemColor.Add(int.Parse(v1), 0);
                                        }
                                        if (v2.Length > 0)
                                        {
                                            ItemColor.Add(0, int.Parse(v2));
                                        }
                                    }
                                }
                                sr.Close();
                            }
                            catch
                            {
                                Console.WriteLine("Erro ao ler \"item_color.txt\"");
                            }
                        }
                        if (rftConfigs[i].Path == "configs\\item_desc.txt")
                        {
                            string line;
                            item_desc = new SortedList<int, string>();
                            var read = ReadFile(stream, rftConfigs[i]);
                            StreamReader file = new StreamReader(new MemoryStream(read), Encoding.GetEncoding("GBK"));
                            int count = 0;
                            while ((line = file.ReadLine()) != null)
                            {
                                if (line != null && line.Length > 0 && !line.StartsWith("#") && !line.StartsWith("/"))
                                {
                                    string[] data = line.Split('"');
                                    data = data.Where(a => a != "").ToArray();
                                    try
                                    {
                                        Application.DoEvents();
                                        item_desc.Add(count, data[0]);
                                        count++;
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine("Erro ao ler \"item_desc.txt\"");
                                    }
                                }
                            }
                            file.Close();
                        }

                        if (rftConfigs[i].Path == "configs\\item_ext_desc.txt")
                        {
                            string line;
                            ItemDescription = new SortedList<int, string>();
                            var read = ReadFile(stream, rftConfigs[i]);
                            StreamReader file = new StreamReader(new MemoryStream(read), Encoding.GetEncoding("GBK"));
                            int count = 0;
                            try
                            {
                                while ((line = file.ReadLine()) != null)
                                {
                                    if (line != null && line.Length > 0 && !line.StartsWith("#") && !line.StartsWith("/"))
                                    {
                                        string[] data = line.Split('"');
                                        try
                                        {
                                            int index = 0;
                                            bool ddd = int.TryParse(data[0], out index);
                                            if (ddd)
                                                ItemDescription[index] = data[1].ToString().Length > 0 ? data[1].ToString().Replace('"', ' ') : "";
                                        }
                                        catch (Exception) { }
                                    }
                                    count++;
                                }
                            }
                            catch
                            {
                                Console.WriteLine("Erro ao ler \"item_ext_desc.txt\"");
                            }
                            file.Close();
                        }
                    }
                    stream.Dispose();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    loadedConfigs = false;
                }
                finally
                {
                    loadedConfigs = true;
                    GC.Collect();
                }
            }
            else return false;
            return loadedConfigs;
        }

        private void simpleButton6_Click_1(object sender, EventArgs e)
        {
            splashScreenManager1.ShowWaitForm();
            splashScreenManager1.SetWaitFormDescription("Carregando elements.data");
            Task t = Task.Factory.StartNew(() => ReadElements());
            t.Wait();
            splashScreenManager1.SetWaitFormDescription("Carregando arquivos pck");
            t = Task.Factory.StartNew(() => ReadSurfaces());
            t.Wait();
            t = Task.Factory.StartNew(() => ReadConfigs());
            t.Wait();
            splashScreenManager1.CloseWaitForm();
            PopularElementsEditor();
        }

        private void PopularElementsEditor()
        {
            cbEElista.Properties.Items.Clear();
            listEElista.DataSource = null;
            gcEEvalores.DataSource = null;
            gvEEvalores.RefreshData();
            for (int l = 0; l < elc.Lists.Length; l++)
            {
                cbEElista.Properties.Items.Add("[" + l + "] " + Definicoes.ListDefNames(elc.Lists[l].listName.Split(new string[] { " - " }, StringSplitOptions.None)[1]) + " (" + elc.Lists[l].elementValues.Length + ")");
            }
            string timestamp = "";
            if (elc.Lists[0].listOffset.Length > 0)
                timestamp = ", Timestamp: " + Extensions.TimestampToDateHour(BitConverter.ToUInt32(elc.Lists[0].listOffset, 0));
            this.Text = " sELedit++ (" + txtEEelements.Text + " [Version: " + elc.Version.ToString() + timestamp + "])";
            cbEElista.SelectedIndex = 0;
            if (elc.ConversationListIndex > -1 && elc.Lists.Length > elc.ConversationListIndex)
            {
                conversationList = new eListConversation((byte[])elc.Lists[elc.ConversationListIndex].elementValues[0][0]);
            }
        }

        public static Bitmap images(string name, int gender = 0)
        {
            if ((gender == 0 ? iconlist_ivtrm : iconlist_ivtrf) != null)
            {
                if ((gender == 0 ? imagesCache_m : imagesCache_f) != null && (gender == 0 ? imagesCache_m : imagesCache_f).ContainsKey(name))
                {
                    return gender == 0 ? imagesCache_m[name] : imagesCache_f[name];
                }
                else
                {
                    int w = 32;
                    int h = 32;
                    Point d = (gender == 0 ? imageposition_m : imageposition_f).FirstOrDefault(x => x.Value.name.Equals(name)).Value.pos;
                    Bitmap pageBitmap = new Bitmap(w, h, PixelFormat.Format32bppArgb);
                    using (Graphics graphics = Graphics.FromImage(pageBitmap))
                    {
                        graphics.DrawImage((gender == 0 ? iconlist_ivtrm : iconlist_ivtrf), new Rectangle(0, 0, w, h), new Rectangle(d.X, d.Y, w, h), GraphicsUnit.Pixel);
                    }
                    if ((gender == 0 ? imagesCache_m : imagesCache_f) == null || (gender == 0 ? imagesCache_m : imagesCache_f) != null && !(gender == 0 ? imagesCache_m : imagesCache_f).ContainsKey(name))
                    {
                        (gender == 0 ? imagesCache_m : imagesCache_f)[name] = pageBitmap;
                    }
                    return pageBitmap;
                }
            }
            return Resources.blank;
        }

        private void cbEElista_SelectedIndexChanged(object sender, EventArgs ew)
        {
            List<ElementsField> list = new List<ElementsField>();
            if (cbEElista.SelectedIndex > -1)
            {
                int l = cbEElista.SelectedIndex;
                listEElista.DataSource = null;
                listEElista.Refresh();

                if (l != elc.ConversationListIndex)
                {
                    int pos = -1;
                    int pos2 = -1;
                    for (int i = 0; i < elc.Lists[l].elementFields.Length; i++)
                    {
                        if (elc.Lists[l].elementFields[i] == "Name")
                        {
                            pos = i;
                        }
                        if (elc.Lists[l].elementFields[i] == "file_icon" || elc.Lists[l].elementFields[i] == "file_icon1")
                        {
                            pos2 = i;
                        }
                        if (pos != -1 && pos2 != -1) { break; }
                    }

                    for (int e = 0; e < elc.Lists[l].elementValues.Length; e++)
                    {
                        ElementsField ef;
                        if (elc.Lists[l].elementFields[0] == "ID")
                        {
                            string path = Path.GetFileName(elc.GetValue(l, e, pos2));
                            Bitmap img = images(path, 0);
                            ef = new ElementsField
                            {
                                Icon = img,
                                ID = int.Parse(elc.GetValue(l, e, 0)),
                                Name = elc.GetValue(l, e, pos)
                            };
                        }
                        else
                        {
                            ef = new ElementsField
                            {
                                Icon = Resources.blank,
                                ID = 0,
                                Name = elc.GetValue(l, e, pos)
                            };
                        }
                        list.Add(ef);
                    }
                    listEElista.DataSource = list;
                }
            }
        }

        private void listEElista_CustomItemDisplayText(object sender, CustomItemDisplayTextEventArgs e)
        {


            /*for (int i = 0; i < obj.Length; i++)
            {
                if (obj[i] == "file_icon" || obj[i] == "file_icon1")
                {
                    posIcon = i;
                }
                if (obj[i] == "Name")
                {
                    posName = i;
                }
                if (posIcon != -1 && posName != -1) { break; }
            }

            if (obj == "ID")
            {
                if (MainWindow.elc.GetValue(l, e, 0) == id.ToString())
                {
                    Bitmap img = Resources.blank1;
                    string path = Path.GetFileName(MainWindow.elc.GetValue(l, e, pos2));
                    img = MainWindow.images(path, 0);
                    return img;
                }
            }
            else
            {
                return Properties.Resources._0;
            }*/



        }

        private void listEElista_CustomItemTemplate(object sender, CustomItemTemplateEventArgs e)
        {
            ElementsField item = e.Item as ElementsField;
            if (ItemColor.ContainsKey(item.ID))
            {
                e.Template.Elements[1].Appearance.Normal.ForeColor = Extensions.getColorByItemID(ItemColor[item.ID]);
            }
        }

        private void listEElista_SelectedIndexChanged(object sender, EventArgs ew)
        {
            int l = cbEElista.SelectedIndex;
            int e = listEElista.SelectedIndex;
            List<ElementsValues> listaComum = new List<ElementsValues>();
            List<ElementsValues> listaAddons = new List<ElementsValues>();
            try
            {
                if (l != elc.ConversationListIndex)
                {

                    for (int f = 0; f < elc.Lists[l].elementValues[e].Length; f++)
                    {
                        if (elc.Lists[l].elementFields[f].Contains("addon"))
                        {
                            listaAddons.Add(new ElementsValues
                            {
                                Type = elc.Lists[l].elementFields[f],
                                ObjectType = elc.Lists[l].elementTypes[f],
                                Value = elc.GetValue(l, e, f)
                            });
                        }
                        else
                        {
                            listaComum.Add(new ElementsValues
                            {
                                Type = elc.Lists[l].elementFields[f],
                                ObjectType = elc.Lists[l].elementTypes[f],
                                Value = elc.GetValue(l, e, f)
                            });
                        }
                    }

                }
                else
                {
                    if (e > -1)
                    {
                        listaComum.Add(new ElementsValues { Type = "id_talk", ObjectType = "int32", Value = conversationList.talk_procs[e].id_talk.ToString() });
                        listaComum.Add(new ElementsValues { Type = "text", ObjectType = "wstring:128", Value = conversationList.talk_procs[e].GetText() });

                        for (int q = 0; q < conversationList.talk_procs[e].num_window; q++)
                        {
                            listaComum.Add(new ElementsValues { Type = $"window_{q}_id", ObjectType = "int32", Value = conversationList.talk_procs[e].windows[q].id.ToString() });
                            listaComum.Add(new ElementsValues { Type = $"window_{q}_id_parent", ObjectType = "int32", Value = conversationList.talk_procs[e].windows[q].id_parent.ToString() });
                            listaComum.Add(new ElementsValues { Type = $"window_{q}_talk_text", ObjectType = $"wstring:{conversationList.talk_procs[e].windows[q].talk_text_len}", Value = conversationList.talk_procs[e].windows[q].GetText() });
                            for (int c = 0; c < conversationList.talk_procs[e].windows[q].num_option; c++)
                            {
                                listaComum.Add(new ElementsValues { Type = $"window_{q}_option_{c}_param", ObjectType = "int32", Value = conversationList.talk_procs[e].windows[q].options[c].param.ToString() });
                                listaComum.Add(new ElementsValues { Type = $"window_{q}_option_{c}_text", ObjectType = "wstring:128", Value = conversationList.talk_procs[e].windows[q].options[c].GetText() });
                                listaComum.Add(new ElementsValues { Type = $"window_{q}_option_{c}_id", ObjectType = $"int32", Value = conversationList.talk_procs[e].windows[q].options[c].id.ToString() });

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                gcEEvalores.DataSource = listaComum;
                gcEEaddons.DataSource = listaAddons;
            }
        }

        private void gvEEvalores_CustomDrawRowIndicator(object sender, RowIndicatorCustomDrawEventArgs e)
        {
            if (e.RowHandle >= 0)
                e.Info.DisplayText = e.RowHandle.ToString();
        }

        private void gvEEvalores_RowUpdated(object sender, RowObjectEventArgs e)
        {
        }

        private void gvEEvalores_CellValueChanged(object sender, CellValueChangedEventArgs ew)
        {
            try
            {
                var valor = gvEEvalores.GetFocusedRow() as ElementsValues;
                if (elc != null && ew.Column.VisibleIndex == 2)
                {
                    int l = cbEElista.SelectedIndex;
                    int r = listEElista.SelectedIndex;
                    int f = ew.RowHandle;
                    if (l != elc.ConversationListIndex)
                    {
                        int[] selIndices = listEElista.SelectedIndices.ToArray();
                        for (int e = 0; e < selIndices.Length; e++)
                        {
                            elc.SetValue(l, selIndices[e], f, Convert.ToString(ew.Value));
                            valor.Value = ew.Value.ToString();
                            var itens = listEElista.GetItem(e) as ElementsField;
                            if (valor.Type == "ID")
                            {
                                itens.ID = Convert.ToInt32(ew.Value);
                            }
                            else if (valor.Type == "Name")
                            {
                                itens.Name = ew.Value.ToString();
                            }
                            else if (valor.Type == "file_icon" || valor.Type == "file_icon1")
                            {
                                string path = Path.GetFileName(ew.Value.ToString());
                                itens.Icon = images(path, 0);
                            }
                        }
                        listEElista.Refresh();
                        gvEEvalores.RefreshData();
                    }
                    else
                    {

                        string fieldName = valor.Type;

                        if (fieldName == "id_talk")
                        {
                            conversationList.talk_procs[r].id_talk = Convert.ToInt32(ew.Value);
                            valor.Value = ew.Value.ToString();
                            return;
                        }
                        if (fieldName == "text")
                        {
                            conversationList.talk_procs[r].SetText(ew.Value.ToString());
                            valor.Value = ew.Value.ToString();
                            return;
                        }
                        if (fieldName.StartsWith("window_") && fieldName.EndsWith("_id"))
                        {
                            int q = Convert.ToInt32(fieldName.Replace("window_", "").Replace("_id", ""));
                            conversationList.talk_procs[r].windows[q].id = Convert.ToInt32(ew.Value);
                            valor.Value = ew.Value.ToString();
                            return;
                        }
                        if (fieldName.StartsWith("window_") && fieldName.Contains("option_") && fieldName.EndsWith("_param"))
                        {
                            string[] s = fieldName.Replace("window_", "").Replace("_option_", ";").Replace("_param", "").Split(new char[] { ';' });
                            int q = Convert.ToInt32(s[0]);
                            int c = Convert.ToInt32(s[1]);
                            conversationList.talk_procs[r].windows[q].options[c].param = Convert.ToInt32(ew.Value);
                            valor.Value = ew.Value.ToString();
                            return;
                        }
                        if (fieldName.StartsWith("window_") && fieldName.Contains("option_") && fieldName.EndsWith("_text"))
                        {
                            string[] s = fieldName.Replace("window_", "").Replace("_option_", ";").Replace("_text", "").Split(new char[] { ';' });
                            int q = Convert.ToInt32(s[0]);
                            int c = Convert.ToInt32(s[1]);
                            conversationList.talk_procs[r].windows[q].options[c].SetText(ew.Value.ToString());
                            valor.Value = ew.Value.ToString();
                            return;
                        }
                        if (fieldName.StartsWith("window_") && fieldName.Contains("option_") && fieldName.EndsWith("_id"))
                        {
                            string[] s = fieldName.Replace("window_", "").Replace("_option_", ";").Replace("_id", "").Split(new char[] { ';' });
                            int q = Convert.ToInt32(s[0]);
                            int c = Convert.ToInt32(s[1]);
                            conversationList.talk_procs[r].windows[q].options[c].id = Convert.ToInt32(ew.Value);
                            valor.Value = ew.Value.ToString();
                            return;
                        }
                        if (fieldName.StartsWith("window_") && fieldName.EndsWith("_id_parent"))
                        {
                            int q = Convert.ToInt32(fieldName.Replace("window_", "").Replace("_id_parent", ""));
                            conversationList.talk_procs[r].windows[q].id_parent = Convert.ToInt32(ew.Value);
                            valor.Value = ew.Value.ToString();
                            return;
                        }
                        if (fieldName.StartsWith("window_") && fieldName.EndsWith("_talk_text"))
                        {
                            int q = Convert.ToInt32(fieldName.Replace("window_", "").Replace("_talk_text", ""));
                            conversationList.talk_procs[r].windows[q].SetText(ew.Value.ToString());
                            valor.Value = ew.Value.ToString();
                            valor.ObjectType = "wstring:" + conversationList.talk_procs[r].windows[q].talk_text_len;
                            return;
                        }
                        gvEEvalores.RefreshData();
                    }
                }
            }
            catch
            {
                XtraMessageBox.Show("Erro!\nErro ao alterar o valor.");
            }

        }

        private void simpleButton4_Click_1(object sender, EventArgs e)
        {
            if (txtElements.Text != "" && File.Exists(txtElements.Text))
            {
                try
                {
                    Cursor = Cursors.AppStarting;
                    File.Copy(txtElements.Text, txtElements.Text + ".bak", true);
                    if (elc.ConversationListIndex > -1 && elc.Lists.Length > elc.ConversationListIndex)
                    {
                        elc.Lists[elc.ConversationListIndex].elementValues[0][0] = conversationList.GetBytes();
                    }
                    elc.Save(txtElements.Text);
                    Cursor = Cursors.Default;
                }
                catch
                {
                    MessageBox.Show("SAVING ERROR!\nThis error mostly occurs of configuration and elements.data mismatch");
                    Cursor = Cursors.Default;
                }
            }
        }

        private void dynId_DoubleClick(object sender, EventArgs e)
        {
            TelaObjetosDinamicos ess = new TelaObjetosDinamicos(DynamicsListEn);
            if (ess.ShowDialog() == DialogResult.OK)
            {
                (sender as SpinEdit).Value = ess.retId;
                var mobdop = gridView5.FocusedRowObject as DynamicObject;
                mobdop.Id = ess.retId;
                gridView5.RefreshData();

            }
        }
    }
}