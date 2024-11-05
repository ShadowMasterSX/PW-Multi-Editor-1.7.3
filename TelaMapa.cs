using Cyotek.Windows.Forms;
using DevExpress.Utils.Taskbar;
using DevExpress.XtraEditors;
using NpcGen_Editor.MapTools.Precinct_Editor;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using static DevExpress.Utils.Drawing.Helpers.NativeMethods;

namespace NpcGen_Editor
{
    public partial class TelaMapa : DevExpress.XtraEditors.XtraForm
    {
        int tipo;

        public PointF location;
        public TelaInicio telaInicio;

        List<PointF> points;
        private PointF? selectedPoint = null;
        private const int PointRadius = 5;
        int index = -1;

        public TelaMapa(TelaInicio telaInicio, int tipo)
        {
            this.location = new PointF(0, 0);
            this.telaInicio = telaInicio;
            points = new List<PointF>();
            InitializeComponent();
            GC.Collect();
            if (tipo == 1)
            {
                popupMenu1.Manager = null;
            }
        }

        private void path_tooltip(object sender, MouseEventArgs e)
        {
            int z = 1;
            if (pictureBox1.Image.Size != new Size(8192, 11264))
                z = 2;
            string text = null;
            if (e.X > -1 && e.X < pictureBox1.Image.Width && e.Y > -1 && e.Y < pictureBox1.Image.Height)
            {
                Point ponto = pictureBox1.AutoScrollPosition;
                int mouseX = e.X - ponto.X;
                int mouseY = e.Y - ponto.Y;
                double zoom = Convert.ToDouble(pictureBox1.Zoom / 100.0f);
                Rectangle viewPort = pictureBox1.GetImageViewPort();
                int imageX = (int)((mouseX - viewPort.X) / zoom);
                int imageY = (int)((mouseY - viewPort.Y) / zoom);                
                double x = ((imageX - pictureBox1.Image.Width / 2) * z) + 0.5;
                double y = ((pictureBox1.Image.Height / 2 - imageY) * z) - 0.5;
                text = string.Concat(string.Concat("X: " + x.ToString("F1"), "\nZ: "), y.ToString("F1"));
                if (text != toolTip.GetToolTip(pictureBox1))
                {
                    toolTip.SetToolTip(pictureBox1, text);
                }

                location = new PointF(Convert.ToSingle(x), Convert.ToSingle(y));
                GC.Collect();

                if (selectedPoint.HasValue)
                {
                    Point newLocation = new Point((int)(e.X / zoom), (int)(e.Y / zoom));
                    if (IsPointValid(newLocation))
                    {
                        index = points.IndexOf(selectedPoint.Value);
                        points[index] = newLocation;
                        selectedPoint = newLocation;
                        pictureBox1.Invalidate();
                        UpdateGrid();
                    }
                    else
                    {
                        MessageBox.Show("Ponto em localização inválida.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        public void AddPoint(PointF loc, Bitmap img)
        {
            Graphics g = Graphics.FromImage(pictureBox1.Image);
            g.DrawImage(Extensions.ResizeImage(img, 7, 7), loc.X - 5, loc.Y - 5);
        }

        public void GetCoordinates(List<PointF> ls, Bitmap img)
        {
            pictureBox1.Zoom = 100;
            int z = 1;
            if (pictureBox1.Image.Size != new Size(8192, 11264))
                z = 2;
            foreach (var item in ls)
            {
                float X = ((item.X / z) + (pictureBox1.Image.Width / 2));
                float Y = (Math.Abs((item.Y / z) - (pictureBox1.Image.Height / 2)));
                if (X <= (float)pictureBox1.Image.Width && Y <= (float)pictureBox1.Image.Height)
                {
                    AddPoint(new PointF(X, Y), img);
                }
                int scrollX = Math.Max(0, (int)(X - pictureBox1.ClientSize.Width / 2));
                int scrollY = Math.Max(0, (int)(Y - pictureBox1.ClientSize.Height / 2));
                pictureBox1.ScrollTo(scrollX, scrollY);
                pictureBox1.Refresh();
            }

            pictureBox1.Refresh();
            GC.Collect();
        }

        private void barButtonItem1_ItemClick_1(object sender, DevExpress.XtraBars.ItemClickEventArgs e)
        {
            TelaMapaAdicionar telaMapaAdicionar = new TelaMapaAdicionar(this);
            telaMapaAdicionar.ShowDialog();
        }

        public void DesenhaPontos(List<PointF> listPos)
        {
            popupMenu1.Manager = null;
            points.Clear();
            pictureBox1.Zoom = 100;
            pictureBox1.AllowZoom = false;
            pictureBox1.Refresh();
            int z = 1;
            if (pictureBox1.Image.Size != new Size(8192, 11264))
                z = 2;
            foreach (var item in listPos)
            {
                float X = ((item.X / z) + (pictureBox1.Image.Width / 2));
                float Y = (Math.Abs((item.Y / z) - (pictureBox1.Image.Height / 2)));
                if (X <= (float)pictureBox1.Image.Width && Y <= (float)pictureBox1.Image.Height)
                {
                    points.Add(new PointF(X, Y));
                }
            }
            pictureBox1.Invalidate();
        }

        private bool IsPointValid(PointF point)
        {
            return point.X >= 0 && point.Y >= 0 && point.X < pictureBox1.ClientSize.Width && point.Y < pictureBox1.ClientSize.Height;
        }
        private bool IsPointInRadius(PointF p1, PointF p2, float radius)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y)) <= radius;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                float zoom = Convert.ToSingle(pictureBox1.Zoom / 100.0f);
                Graphics g = e.Graphics;
                g.ScaleTransform(zoom, zoom);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                if (points.Count > 2)
                {
                    foreach (var point in points)
                    {
                        if (!IsPointValid(point))
                        {
                            throw new InvalidOperationException("Ponto inválido detectado.");
                        }
                    }

                    GraphicsPath path = new GraphicsPath();
                    path.AddPolygon(points.ToArray());

                    using (Brush brush = new SolidBrush(Color.FromArgb(128, 65, 140, 240)))
                    {
                        g.FillPath(brush, path);
                    }
                    using (Pen pen = new Pen(Color.Blue, 2 / zoom))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                float pr = PointRadius / zoom;
                foreach (var point in points)
                {
                    g.FillEllipse(Brushes.Red, point.X - pr, point.Y - pr, pr * 2, pr * 2);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            float zoom = Convert.ToSingle(pictureBox1.Zoom / 100.0f);
            PointF newPoint = new PointF(e.X / zoom, e.Y / zoom);
            if (e.Button == MouseButtons.Left && Control.ModifierKeys == Keys.Control)
            {
                if (IsPointValid(newPoint))
                {
                    points.Add(newPoint);
                    pictureBox1.Refresh();
                    UpdateGrid();
                }
                else
                {
                    MessageBox.Show("Ponto em localização inválida.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                foreach (var point in points)
                {
                    if (IsPointInRadius(e.Location, point, PointRadius / zoom))
                    {
                        selectedPoint = point;
                        break;
                    }
                }
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            selectedPoint = null;
        }

        void UpdateGrid()
        {
            int z = 1;
            if (pictureBox1.Image.Size != new Size(8192, 11264))
                z = 2;
            var pointData = new List<CELPrecinct.VECTOR3>();
            foreach (var e in points)
            {
                if (e.X > -1 && e.X < pictureBox1.Image.Width && e.Y > -1 && e.Y < pictureBox1.Image.Height)
                {
                    Point ponto = pictureBox1.AutoScrollPosition;
                    int mouseX = (int)e.X - ponto.X;
                    int mouseY = (int)e.Y - ponto.Y;
                    double zoom = Convert.ToDouble(pictureBox1.Zoom / 100.0f);
                    Rectangle viewPort = pictureBox1.GetImageViewPort();
                    int imageX = (int)((mouseX - viewPort.X) / zoom);
                    int imageY = (int)((mouseY - viewPort.Y) / zoom);
                    double x = ((imageX - pictureBox1.Image.Width / 2) * z) + 0.5;
                    double y = ((pictureBox1.Image.Height / 2 - imageY) * z) - 0.5;

                    pointData.Add(new CELPrecinct.VECTOR3 { X = x, Y = 0.0, Z = y });
                }
            }
            CELPrecinct val = (CELPrecinct)telaInicio.gvPrecinct.FocusedRowObject;
            if (val != null)
            {
                val.m_aPoints = pointData;                
            }
            
        }
    }
}