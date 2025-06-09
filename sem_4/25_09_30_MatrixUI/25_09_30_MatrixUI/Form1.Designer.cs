namespace _25_09_30_MatrixUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            numRows = new NumericUpDown();
            numCols = new NumericUpDown();
            btnGenerate = new Button();
            gridA = new DataGridView();
            gridB = new DataGridView();
            cmbOp = new ComboBox();
            btnStart = new Button();
            prog = new ProgressBar();
            gridResult = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)numRows).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numCols).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridA).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridB).BeginInit();
            ((System.ComponentModel.ISupportInitialize)gridResult).BeginInit();
            SuspendLayout();
            // 
            // numRows
            // 
            numRows.Location = new Point(59, 60);
            numRows.Name = "numRows";
            numRows.Size = new Size(141, 23);
            numRows.TabIndex = 0;
            // 
            // numCols
            // 
            numCols.Location = new Point(59, 90);
            numCols.Name = "numCols";
            numCols.Size = new Size(141, 23);
            numCols.TabIndex = 1;
            // 
            // btnGenerate
            // 
            btnGenerate.Location = new Point(59, 130);
            btnGenerate.Name = "btnGenerate";
            btnGenerate.Size = new Size(96, 61);
            btnGenerate.TabIndex = 2;
            btnGenerate.Text = "button1";
            btnGenerate.UseVisualStyleBackColor = true;
            // 
            // gridA
            // 
            gridA.AllowUserToAddRows = false;
            gridA.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridA.Location = new Point(236, 60);
            gridA.Name = "gridA";
            gridA.Size = new Size(261, 241);
            gridA.TabIndex = 3;
            // 
            // gridB
            // 
            gridB.AllowUserToAddRows = false;
            gridB.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridB.Location = new Point(516, 60);
            gridB.Name = "gridB";
            gridB.Size = new Size(260, 241);
            gridB.TabIndex = 4;
            // 
            // cmbOp
            // 
            cmbOp.FormattingEnabled = true;
            cmbOp.Location = new Point(413, 342);
            cmbOp.Name = "cmbOp";
            cmbOp.Size = new Size(190, 23);
            cmbOp.TabIndex = 5;
            cmbOp.SelectedIndexChanged += cmbOp_SelectedIndexChanged;
            // 
            // btnStart
            // 
            btnStart.Location = new Point(413, 380);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(190, 37);
            btnStart.TabIndex = 6;
            btnStart.Text = "button2";
            btnStart.UseVisualStyleBackColor = true;
            // 
            // prog
            // 
            prog.Location = new Point(309, 449);
            prog.Name = "prog";
            prog.Size = new Size(396, 23);
            prog.TabIndex = 7;
            // 
            // gridResult
            // 
            gridResult.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            gridResult.Location = new Point(351, 500);
            gridResult.Name = "gridResult";
            gridResult.Size = new Size(317, 238);
            gridResult.TabIndex = 8;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(979, 778);
            Controls.Add(gridResult);
            Controls.Add(prog);
            Controls.Add(btnStart);
            Controls.Add(cmbOp);
            Controls.Add(gridB);
            Controls.Add(gridA);
            Controls.Add(btnGenerate);
            Controls.Add(numCols);
            Controls.Add(numRows);
            Name = "Form1";
            Text = "Form1";
            Load += Form1_Load;
            ((System.ComponentModel.ISupportInitialize)numRows).EndInit();
            ((System.ComponentModel.ISupportInitialize)numCols).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridA).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridB).EndInit();
            ((System.ComponentModel.ISupportInitialize)gridResult).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private NumericUpDown numRows;
        private NumericUpDown numCols;
        private Button btnGenerate;
        private DataGridView gridA;
        private DataGridView gridB;
        private ComboBox cmbOp;
        private Button btnStart;
        private ProgressBar prog;
        private DataGridView gridResult;
    }
}
