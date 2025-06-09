using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using MatrixCore;

namespace _25_09_30_MatrixUI
{
    public partial class Form1 : Form
    {
        private CancellationTokenSource _cts;

        public Form1()
        {
            InitializeComponent();
            Load += Form1_Load;
            cmbOp.SelectedIndexChanged += cmbOp_SelectedIndexChanged;
            btnGenerate.Click += btnGenerate_Click;
            btnStart.Click += btnStart_Click;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cmbOp.Items.AddRange(new[] { "Multiply", "Determinant", "Inverse" });
            cmbOp.SelectedIndex = 0;
            cmbOp_SelectedIndexChanged(null, null);
        }

        private void cmbOp_SelectedIndexChanged(object sender, EventArgs e)
        {
            string op = cmbOp.SelectedItem?.ToString();
            bool needB = op == "Multiply";
            gridB.Visible = needB;
            gridB.Enabled = needB;
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            int rows = (int)numRows.Value;
            int cols = (int)numCols.Value;

            ConfigureGrid(gridA, rows, cols);
            ConfigureGrid(gridB, rows, cols);
            ConfigureGrid(gridResult, 0, 0);
        }

        private void ConfigureGrid(DataGridView grid, int rows, int cols)
        {
            grid.Columns.Clear();
            for (int c = 0; c < cols; c++)
            {
                grid.Columns.Add($"c{c}", "");
                grid.Columns[c].Width = 50;
            }
            if (rows <= 0 || cols <= 0)
                return;
            grid.RowCount = rows;
        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            ToggleUi(false);
            prog.Style = ProgressBarStyle.Marquee;
            _cts = new CancellationTokenSource();

            try
            {
                Matrix a = ReadMatrix(gridA);
                Matrix b = null;
                if (gridB.Visible) b = ReadMatrix(gridB);

                var op = MatrixOperationFactory.Create(cmbOp.SelectedItem.ToString());
                Matrix result = await op.ExecuteAsync(a, b, _cts.Token);
                ShowResult(result);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("Operation cancelled.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                prog.Style = ProgressBarStyle.Blocks;
                ToggleUi(true);
            }
        }

        private Matrix ReadMatrix(DataGridView grid)
        {
            int r = grid.RowCount;
            int c = grid.ColumnCount;
            var m = new Matrix(r, c);

            for (int i = 0; i < r; i++)
                for (int j = 0; j < c; j++)
                    m[i, j] = double.TryParse(grid[j, i].Value?.ToString(), out var v) ? v : 0;

            return m;
        }

        private void ShowResult(Matrix m)
        {
            if (m.Rows == 1 && m.Cols == 1)
            {
                ConfigureGrid(gridResult, 1, 1);
                gridResult[0, 0].Value = m[0, 0];
                return;
            }

            ConfigureGrid(gridResult, m.Rows, m.Cols);
            for (int i = 0; i < m.Rows; i++)
                for (int j = 0; j < m.Cols; j++)
                    gridResult[j, i].Value = m[i, j];
        }

        private void ToggleUi(bool enable)
        {
            btnStart.Enabled = enable;
            btnGenerate.Enabled = enable;
            cmbOp.Enabled = enable;
            numRows.Enabled = enable;
            numCols.Enabled = enable;
        }
    }
}
