using DevExpress.XtraEditors;
using NpcGen_Editor.Classes;
using NpcGen_Editor.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace NpcGen_Editor
{
    public partial class TelaMapaAdicionar : DevExpress.XtraEditors.XtraForm
    {
        TelaMapa telaMapa;

        public TelaMapaAdicionar(TelaMapa telaMapa)
        {
            InitializeComponent();
            this.telaMapa = telaMapa;
            var p = telaMapa.location;
            mobPosX.Value = Convert.ToDecimal(p.X);
            mobPosZ.Value = Convert.ToDecimal(p.Y);
            resPosX.Value = Convert.ToDecimal(p.X);
            resPosZ.Value = Convert.ToDecimal(p.Y);
            dynPosX.Value = Convert.ToDecimal(p.X);
            dynPosZ.Value = Convert.ToDecimal(p.Y);
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            DefaultMonsters mob = new DefaultMonsters
            {
                Amount_in_group = 1,
                BInitGen = 0,
                BValicOnce = 0,
                bAutoRevive = 0,
                dwGenId = 0,
                iGroupType = 0,
                Life_time = 0,
                Location = mobLocal.SelectedIndex,
                MaxRespawnTime = 0,
                Trigger_id = Convert.ToInt32(mobTriggerId.Value),
                Type = mobTipo.SelectedIndex,
                X_direction = Convert.ToSingle(mobRotX.Value),
                X_position = Convert.ToSingle(mobPosX.Value),
                X_random = Convert.ToSingle(mobScatterX.Value),
                Y_direction = Convert.ToSingle(mobRotY.Value),
                Y_position = Convert.ToSingle(mobPosY.Value),
                Y_random = Convert.ToSingle(mobScatterY.Value),
                Z_direction = Convert.ToSingle(mobRotZ.Value),
                Z_position = Convert.ToSingle(mobPosZ.Value),
                Z_random = Convert.ToSingle(mobScatterZ.Value),
                MobDops =
                    new List<ExtraMonsters>
                    {
                        new ExtraMonsters
                        {
                            Agression = mobAgressive.SelectedIndex,
                            Amount = Convert.ToInt32(mobQt.Value),
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
                            Id = Convert.ToInt32(mobId.Value),
                            Path = 0,
                            Path_type = 0,
                            RefreshLower = 0,
                            Respawn = Convert.ToInt32(mobRespawn.Value),
                            Speed = 0,
                        }
                    }
            };

            telaMapa.GetCoordinates(new List<PointF> { telaMapa.location }, mob.Type == 1 ? Resources.radar_npc : Resources.pet);
            var mobs = telaMapa.telaInicio.gridView1.DataSource as List<DefaultMonsters>;
            mobs.Add(mob);
            telaMapa.telaInicio.xtraTabControl1.SelectedTabPageIndex = 0;
            telaMapa.telaInicio.gridView1.RefreshData();
            telaMapa.telaInicio.gridView1.MoveLast();
            Close();
        }

        private void simpleButton4_Click(object sender, EventArgs e)
        {
            var res = new DefaultResources
            {
                Amount_in_group = 1,
                bAutoRevive = 0,
                bValidOnce = 0,
                bInitGen = 0,
                dwGenID = 0,
                IMaxNum = 1,
                InCline1 = Convert.ToByte(resIncl1.Value),
                InCline2 = Convert.ToByte(resIncl2.Value),
                Rotation = Convert.ToByte(resRot.Value),
                Trigger_id = Convert.ToInt32(resTriggerId.Value),
                X_position = Convert.ToSingle(resPosX.Value),
                X_Random = Convert.ToSingle(resScatterX.Value),
                Y_position = Convert.ToSingle(resPosY.Value),
                Z_position = Convert.ToSingle(resPosZ.Value),
                Z_Random = Convert.ToSingle(resScatterZ.Value),
                ResExtra =
                            new List<ExtraResources>
                                {
                                    new ExtraResources
                                    {
                                        Amount = Convert.ToInt32(resNum.Value),
                                        fHeiOff = Convert.ToInt32(resHeig.Value),
                                        Id = Convert.ToInt32(resId.Value),
                                        ResourceType = Convert.ToInt32(resType.Value),
                                        Respawntime = Convert.ToInt32(resRespawn.Value)
                                    }
                                }

            };
            telaMapa.GetCoordinates(new List<PointF> { telaMapa.location }, Resources.radar_ftasknpc);
            var ress = telaMapa.telaInicio.gridView4.DataSource as List<DefaultResources>;
            ress.Add(res);
            telaMapa.telaInicio.xtraTabControl1.SelectedTabPageIndex = 1;
            telaMapa.telaInicio.gridView4.RefreshData();
            telaMapa.telaInicio.gridView4.MoveLast();
            Close();
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void simpleButton6_Click(object sender, EventArgs e)
        {
            var od = new DynamicObject
            {
                Id = Convert.ToInt32(dynId.Value),
                InCline1 = Convert.ToByte(dynIncl1.Value),
                InCline2 = Convert.ToByte(dynIncl2.Value),
                Rotation = Convert.ToByte(dynRot.Value),
                Scale = Convert.ToByte(dynScale.Value),
                TriggerId = Convert.ToInt32(dynTriggerID.Value),
                X_position = Convert.ToSingle(dynPosX.Value),
                Y_position = Convert.ToSingle(dynPosY.Value),
                Z_position = Convert.ToSingle(dynPosZ.Value),
            };
            telaMapa.GetCoordinates(new List<PointF> { telaMapa.location }, Resources.radar_tasknpc);
            var dyn = telaMapa.telaInicio.gridView5.DataSource as List<DynamicObject>;
            dyn.Add(od);
            telaMapa.telaInicio.xtraTabControl1.SelectedTabPageIndex = 2;
            telaMapa.telaInicio.gridView5.RefreshData();
            telaMapa.telaInicio.gridView5.MoveLast();
            Close();
        }

        private void mobId_DoubleClick(object sender, EventArgs e)
        {
            string tipo = mobTipo.SelectedIndex == 0 ? "MONSTER_ESSENCE" : "NPC_ESSENCE";
            TelaSelecionarEssencia ess = new TelaSelecionarEssencia(tipo);
            if (ess.ShowDialog() == DialogResult.OK)
            {
                (sender as SpinEdit).Value = ess.retId;
            }
        }

        private void resId_DoubleClick(object sender, EventArgs e)
        {
            TelaSelecionarEssencia ess = new TelaSelecionarEssencia("MINE_ESSENCE");
            if (ess.ShowDialog() == DialogResult.OK)
            {
                (sender as SpinEdit).Value = ess.retId;
            }
        }

        private void dynId_DoubleClick(object sender, EventArgs e)
        {
            TelaObjetosDinamicos ess = new TelaObjetosDinamicos(telaMapa.telaInicio.DynamicsListEn);
            if (ess.ShowDialog() == DialogResult.OK)
            {
                (sender as SpinEdit).Value = ess.retId;
            }
        }
    }
}