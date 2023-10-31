using System;
using System.Drawing;
using System.Windows.Forms;

using Sudoku.Utility;

namespace Sudoku.GUI
{
    public partial class Main : Form
    {
        static SudokuController controller;

        static MenuStrip menuStrip1;
        static ToolStripMenuItem node0,
            node1,
            node2,
            node3,
            node4,
            node5,
            node6,
            node7,
            node8,
            node9,
            node10,
            node11;
        public Main()
        {
            SuspendLayout();

            LocalizationManager.ApplyProperties(typeof(Localization.MainForm), this);

            MaximizeBox = false;
            AutoSize = true;
            AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            MinimumSize = new System.Drawing.Size(700, 350);

            Controls.Add(menuStrip1 = new MenuStrip() { Dock = DockStyle.Top });
            MainMenuStrip = menuStrip1;
            menuStrip1.SuspendLayout();
            menuStrip1.Items.AddRange(new ToolStripItem[] {
                node0 = new ToolStripMenuItem() { },
                node1 = new ToolStripMenuItem() { },
                node2 = new ToolStripMenuItem() { },
                node3 = new ToolStripMenuItem() { },
                node4 = new ToolStripMenuItem() { Enabled = false },
                node5 = new ToolStripMenuItem() { },
                node6 = new ToolStripMenuItem() { },
                node7 = new ToolStripMenuItem() { },
                node8 = new ToolStripMenuItem() { },
                node9 = new ToolStripMenuItem() { },
                node10 = new ToolStripMenuItem() { },
                node11 = new ToolStripMenuItem() { }
            });

            LocalizationManager.ApplyProperties(typeof(Localization.MainForm.SolveControl), node0);
            LocalizationManager.ApplyProperties(typeof(Localization.MainForm.SolveAllControl), node1);
            LocalizationManager.ApplyProperties(typeof(Localization.MainForm.CountSolutionsControl), node2);
            LocalizationManager.ApplyProperties(typeof(Localization.MainForm.CountGivensControl), node3);
            LocalizationManager.ApplyProperties(typeof(Localization.MainForm.GenerateSudokuControl), node4);
            LocalizationManager.ApplyProperties(typeof(Localization.MainForm.LoadControl), node5);
            LocalizationManager.ApplyProperties(typeof(Localization.MainForm.SaveControl), node6);
            LocalizationManager.ApplyProperties(typeof(Localization.MainForm.ClearControl), node7);
            LocalizationManager.ApplyProperties(typeof(Localization.MainForm.HShrinkControl), node8);
            LocalizationManager.ApplyProperties(typeof(Localization.MainForm.VShrinkControl), node9);
            LocalizationManager.ApplyProperties(typeof(Localization.MainForm.HExpControl), node10);
            LocalizationManager.ApplyProperties(typeof(Localization.MainForm.VExpControl), node11);


            menuStrip1.ResumeLayout(true);

            controller = new SudokuController()
            {
                Container = this,
                Containee = new SudokuGrid(3, 3, Localization.GetCharset(9))
                {
                    Location = new Point(Margin.Left, menuStrip1.Bottom + Margin.Vertical),
                    Size = new Size(800, 800)
                }
            };

            ResumeLayout(true);

            node0.Click += GetSolution;
            node1.Click += GetAllSolutions;
            node2.Click += CountSolutions;
            node3.Click += CountGivens;
            node4.Click += Generate;
            node5.Click += LoadGrid;
            node6.Click += SaveGrid;
            node7.Click += ClearGrid;
            node8.Click += HShrinkGrid;
            node9.Click += VShrinkGrid;
            node10.Click += HExpandGrid;
            node11.Click += VExpandGrid;
        }

        static void GetSolution(object sender, EventArgs e)
        {
            controller.Solve1();
        }

        static void GetAllSolutions(object sender, EventArgs e)
        {
            controller.SolveAll();
        }

        static void CountSolutions(object sender, EventArgs e)
        {
            controller.CountSolutions();
        }

        static void CountGivens(object sender, EventArgs e)
        {
            controller.CountGivens();
        }

        static void Generate(object sender, EventArgs e)
        {
            controller.GenerateSudoku();
        }

        static void LoadGrid(object sender, EventArgs e)
        {
            controller.LoadSudoku();
        }

        static void SaveGrid(object sender, EventArgs e)
        {
            controller.SaveSudoku();
        }

        private void ClearGrid(object sender, EventArgs e)
        {
            controller.ClearSudoku();
        }

        private void HShrinkGrid(object sender, EventArgs e)
        {
            int heightFactor = controller.Containee.SubgridHeight, widthFactor = controller.Containee.SubgridWidth - 1, edge = heightFactor * widthFactor;

            if (edge == 0)
                return;

            controller.Containee = new SudokuGrid(heightFactor, widthFactor, Localization.GetCharset(edge))
            {
                Location = controller.Containee.Location,
                Size = controller.Containee.Size
            };
            controller.Containee.Focus();
        }

        private void VShrinkGrid(object sender, EventArgs e)
        {
            int heightFactor = controller.Containee.SubgridHeight - 1, widthFactor = controller.Containee.SubgridWidth, edge = heightFactor * widthFactor;

            if (edge == 0)
                return;

            controller.Containee = new SudokuGrid(heightFactor, widthFactor, Localization.GetCharset(edge))
            {
                Location = controller.Containee.Location,
                Size = controller.Containee.Size
            };
            controller.Containee.Focus();
        }

        private void HExpandGrid(object sender, EventArgs e)
        {
            int heightFactor = controller.Containee.SubgridHeight, widthFactor = controller.Containee.SubgridWidth + 1, edge = heightFactor * widthFactor;

            if (edge == 0)
                return;

            controller.Containee = new SudokuGrid(heightFactor, widthFactor, Localization.GetCharset(edge))
            {
                Location = controller.Containee.Location,
                Size = controller.Containee.Size
            };
            controller.Containee.Focus();
        }

        private void VExpandGrid(object sender, EventArgs e)
        {
            int heightFactor = controller.Containee.SubgridHeight + 1, widthFactor = controller.Containee.SubgridWidth, edge = heightFactor * widthFactor;

            if (edge == 0)
                return;

            controller.Containee = new SudokuGrid(heightFactor, widthFactor, Localization.GetCharset(edge))
            {
                Location = controller.Containee.Location,
                Size = controller.Containee.Size
            };
            controller.Containee.Focus();
        }
    }
}