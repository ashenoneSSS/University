using System;
using System.Threading;
using System.Threading.Tasks;

namespace MatrixCore
{
    public class Matrix
    {
        public int Rows { get; }
        public int Cols { get; }
        public double[,] Data { get; }

        public Matrix(int rows, int cols)
        {
            Rows = rows;
            Cols = cols;
            Data = new double[rows, cols];
        }

        public double this[int r, int c]
        {
            get => Data[r, c];
            set => Data[r, c] = value;
        }

        public static Matrix Identity(int n)
        {
            var m = new Matrix(n, n);
            for (int i = 0; i < n; i++) m[i, i] = 1;
            return m;
        }

        public Matrix Clone()
        {
            var copy = new Matrix(Rows, Cols);
            Array.Copy(Data, copy.Data, Data.Length);
            return copy;
        }

        public override string ToString()
        {
            var s = "";
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Cols; j++)
                    s += $"{Data[i, j]:0.##}\t";
                s += Environment.NewLine;
            }
            return s;
        }
    }

    public interface IMatrixOperation
    {
        Task<Matrix> ExecuteAsync(Matrix a, Matrix b = null, CancellationToken token = default);
    }

    public class MatrixMultiplier : IMatrixOperation
    {
        public Task<Matrix> ExecuteAsync(Matrix a, Matrix b = null, CancellationToken token = default) =>
            Task.Run(() =>
            {
                if (a.Cols != b.Rows) throw new InvalidOperationException();
                var res = new Matrix(a.Rows, b.Cols);
                for (int i = 0; i < a.Rows; i++)
                {
                    for (int k = 0; k < a.Cols; k++)
                    {
                        var aval = a[i, k];
                        for (int j = 0; j < b.Cols; j++)
                        {
                            res[i, j] += aval * b[k, j];
                        }
                    }
                    if (token.IsCancellationRequested) token.ThrowIfCancellationRequested();
                }
                return res;
            }, token);
    }

    public class DeterminantCalculator : IMatrixOperation
    {
        public Task<Matrix> ExecuteAsync(Matrix a, Matrix b = null, CancellationToken token = default) =>
            Task.Run(() =>
            {
                if (a.Rows != a.Cols) throw new InvalidOperationException();
                double det = Determinant(a, token);
                var m = new Matrix(1, 1);
                m[0, 0] = det;
                return m;
            }, token);

        private double Determinant(Matrix m, CancellationToken token)
        {
            int n = m.Rows;
            if (n == 1) return m[0, 0];
            if (n == 2) return m[0, 0] * m[1, 1] - m[0, 1] * m[1, 0];
            double det = 0;
            for (int col = 0; col < n; col++)
            {
                var sub = SubMatrix(m, 0, col);
                det += ((col % 2 == 0 ? 1 : -1) * m[0, col] * Determinant(sub, token));
                if (token.IsCancellationRequested) token.ThrowIfCancellationRequested();
            }
            return det;
        }

        private Matrix SubMatrix(Matrix m, int skipRow, int skipCol)
        {
            var res = new Matrix(m.Rows - 1, m.Cols - 1);
            int r = 0;
            for (int i = 0; i < m.Rows; i++)
            {
                if (i == skipRow) continue;
                int c = 0;
                for (int j = 0; j < m.Cols; j++)
                {
                    if (j == skipCol) continue;
                    res[r, c++] = m[i, j];
                }
                r++;
            }
            return res;
        }
    }

    public class MatrixInverter : IMatrixOperation
    {
        public Task<Matrix> ExecuteAsync(Matrix a, Matrix b = null, CancellationToken token = default) =>
            Task.Run(() =>
            {
                if (a.Rows != a.Cols) throw new InvalidOperationException();
                int n = a.Rows;
                var aug = new Matrix(n, n * 2);
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++) aug[i, j] = a[i, j];
                    for (int j = n; j < 2 * n; j++) aug[i, j] = i == (j - n) ? 1 : 0;
                }
                for (int col = 0; col < n; col++)
                {
                    int pivot = col;
                    for (int row = col; row < n; row++)
                        if (Math.Abs(aug[row, col]) > Math.Abs(aug[pivot, col])) pivot = row;
                    if (Math.Abs(aug[pivot, col]) < 1e-12) throw new InvalidOperationException();
                    SwapRows(aug, col, pivot);
                    double div = aug[col, col];
                    for (int j = 0; j < 2 * n; j++) aug[col, j] /= div;
                    for (int row = 0; row < n; row++)
                    {
                        if (row == col) continue;
                        double factor = aug[row, col];
                        for (int j = 0; j < 2 * n; j++)
                            aug[row, j] -= factor * aug[col, j];
                    }
                    if (token.IsCancellationRequested) token.ThrowIfCancellationRequested();
                }
                var inv = new Matrix(n, n);
                for (int i = 0; i < n; i++)
                    for (int j = 0; j < n; j++)
                        inv[i, j] = aug[i, j + n];
                return inv;
            }, token);

        private void SwapRows(Matrix m, int r1, int r2)
        {
            if (r1 == r2) return;
            int cols = m.Cols;
            for (int j = 0; j < cols; j++)
            {
                var t = m[r1, j];
                m[r1, j] = m[r2, j];
                m[r2, j] = t;
            }
        }
    }

    public static class MatrixOperationFactory
    {
        public static IMatrixOperation Create(string opName)
        {
            return opName switch
            {
                "Multiply" => new MatrixMultiplier(),
                "Determinant" => new DeterminantCalculator(),
                "Inverse" => new MatrixInverter(),
                _ => throw new ArgumentException($"Unknown operation: {opName}")
            };
        }
    }


}
