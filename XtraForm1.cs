using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid;
using DevExpress.XtraWaitForm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace NpcGen_Editor
{
    public partial class XtraForm1 : DevExpress.XtraEditors.XtraForm
    {
        private List<Point> points;
        private Point? selectedPoint = null;
        private const int PointRadius = 5;
        private GridControl gridControl;
        private GridView gridView;

        public XtraForm1()
        {
            this.Text = "DevExpress GridControl with Editable Points";
            this.Size = new Size(1000, 600);
            this.points = new List<Point>(); // Lista inicial vazia

            InitializeComponent();
            this.gridControl = new GridControl();
            this.gridView = new GridView(gridControl);
            this.gridControl.MainView = this.gridView;
            this.gridControl.Dock = DockStyle.Right;
            this.gridControl.Width = 300;
            this.Controls.Add(this.gridControl);

            this.gridView.OptionsBehavior.Editable = true;
            this.gridView.CellValueChanged += GridView_CellValueChanged;
            BindGrid();

            this.Paint += new PaintEventHandler(MainForm_Paint);
            this.MouseDown += new MouseEventHandler(MainForm_MouseDown);
            this.MouseMove += new MouseEventHandler(MainForm_MouseMove);
            this.MouseUp += new MouseEventHandler(MainForm_MouseUp);
        }

        private void BindGrid()
        {
            var pointData = new List<PointData>();
            foreach (var point in points)
            {
                pointData.Add(new PointData { X = point.X, Y = point.Y });
            }
            this.gridControl.DataSource = pointData;
        }

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Graphics g = e.Graphics;
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

                    using (Pen pen = new Pen(Color.Blue, 2))
                    {
                        g.DrawPath(pen, path);
                    }
                }

                foreach (var point in points)
                {
                    g.FillEllipse(Brushes.Red, point.X - PointRadius, point.Y - PointRadius, PointRadius * 2, PointRadius * 2);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Control.ModifierKeys == Keys.Control)
            {
                Point newPoint = e.Location;
                if (IsPointValid(newPoint))
                {
                    points.Add(newPoint);
                    this.Invalidate();
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
                    if (IsPointInRadius(e.Location, point, PointRadius))
                    {
                        selectedPoint = point;
                        break;
                    }
                }
            }
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (selectedPoint.HasValue)
            {
                Point newLocation = e.Location;
                if (IsPointValid(newLocation))
                {
                    int index = points.IndexOf(selectedPoint.Value);
                    points[index] = newLocation;
                    selectedPoint = newLocation;
                    this.Invalidate();
                    UpdateGrid();
                }
                else
                {
                    MessageBox.Show("Ponto em localização inválida.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            selectedPoint = null;
        }

        private void GridView_CellValueChanged(object sender, DevExpress.XtraGrid.Views.Base.CellValueChangedEventArgs e)
        {
            try
            {
                int rowHandle = e.RowHandle;
                if (rowHandle >= 0 && rowHandle < points.Count)
                {
                    int x = Convert.ToInt32(gridView.GetRowCellValue(rowHandle, "X"));
                    int y = Convert.ToInt32(gridView.GetRowCellValue(rowHandle, "Y"));
                    Point newPoint = new Point(x, y);
                    if (IsPointValid(newPoint))
                    {
                        points[rowHandle] = newPoint;
                        this.Invalidate();
                    }
                    else
                    {
                        MessageBox.Show("Ponto em localização inválida.", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro: {ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateGrid()
        {
            var pointData = new List<PointData>();
            foreach (var point in points)
            {
                pointData.Add(new PointData { X = point.X, Y = point.Y });
            }
            this.gridControl.DataSource = pointData;
        }

        private bool IsPointInRadius(Point p1, Point p2, int radius)
        {
            return Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y)) <= radius;
        }

        private bool IsPointValid(Point point)
        {
            // Verifique se as coordenadas do ponto são válidas (por exemplo, dentro dos limites da tela)
            return point.X >= 0 && point.Y >= 0 && point.X < this.ClientSize.Width && point.Y < this.ClientSize.Height;
        }
    }

    public class PointData
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}
