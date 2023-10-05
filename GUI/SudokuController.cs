#define STOPWATCH
#define MT
#define MT_FNOT

using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;

using Sudoku.Core;
using Sudoku.Utility;

namespace Sudoku.GUI
{
    public sealed class SudokuController : Controller<Control, SudokuGrid>
    {
        const int TASKDEPTHLIMIT = 5,
            FNOT_TASKCOUNT = 16;

        readonly OpenFileDialog openFileDialog;
        readonly SaveFileDialog saveFileDialog;
        public SudokuController()
        {
            openFileDialog = new OpenFileDialog()
            {
                FilterIndex = 1,
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false
            };
            saveFileDialog = new SaveFileDialog()
            {
                FilterIndex = 1
            };

            LocalizationManager.ApplyProperties(typeof(Localization.SudokuController.OpenSudokuDialog), openFileDialog);
            LocalizationManager.ApplyProperties(typeof(Localization.SudokuController.SaveSudokuDialog), saveFileDialog);

        }

        public void Solve1()
        {
            if (Containee == null)
                return;

            string[,] array = Containee.Grid;
#if STOPWATCH
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
            array = SudokuSolver.Solve1(array, Containee.CharacterSet, Containee.SubgridHeight, Containee.SubgridWidth);
#if STOPWATCH
            sw.Stop();
#endif
            Containee.Grid = array;
#if STOPWATCH
            Localization.ReportCalculationRuntime(sw);
#endif
        }

        public void SolveAll()
        {
            if (Containee == null)
                return;
            string[,] array = Containee.Grid;
#if STOPWATCH
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
#if MT
#if MT_FNOT
            array = SudokuSolver.SolveAll_Parallel_FNOT(array, Containee.CharacterSet, TASKDEPTHLIMIT, FNOT_TASKCOUNT, Containee.SubgridHeight, Containee.SubgridWidth);
#else
            array = SudokuSolver.SolveAll_Parallel_OSPT(array, Containee.CharacterSet, TASKDEPTHLIMIT, Containee.SubgridHeight, Containee.SubgridWidth);
#endif
#else
            array = SudokuSolver.SolveAll(array, Containee.CharacterSet, Containee.SubgridHeight, Containee.SubgridWidth);
#endif
#if STOPWATCH
            sw.Stop();
#endif
            Containee.Grid = array;
#if STOPWATCH
            Localization.ReportCalculationRuntime(sw);
#endif
        }

        public void CountSolutions()
        {
            if (Containee == null)
                return;
            string[,] array = Containee.Grid;
#if STOPWATCH
            Stopwatch sw = new Stopwatch();
            sw.Start();
#endif
#if MT
#if MT_FNOT
            int result = SudokuSolver.CountSolutions_Parallel_FNOT(array, Containee.CharacterSet, TASKDEPTHLIMIT, FNOT_TASKCOUNT, Containee.SubgridHeight, Containee.SubgridWidth);
#else
            int result = SudokuSolver.CountSolutions_Parallel_OSPT(array, Containee.CharacterSet, TASKDEPTHLIMIT, Containee.SubgridHeight, Containee.SubgridWidth);
#endif
#else
            int result = SudokuSolver.CountSolutions(array, Containee.CharacterSet, Containee.SubgridHeight, Containee.SubgridWidth);
#endif
#if STOPWATCH
            sw.Stop();
            Localization.ReportCountSolutionsResult(result, sw);
#else
            Localization.ReportCountSolutionsResult(result);
#endif
        }

        public void CountGivens()
        {
            if (Containee == null)
                return;
            string[,] array = Containee.Grid;
            int result = SudokuSolver.CountGivens(array, Containee.CharacterSet, Containee.Edge, Containee.Edge);
            Localization.ReportCountGivensResult(result);
        }

        public void GenerateSudoku()
        {
            if (Containee == null)
                return;
            throw new NotImplementedException();
        }

        public void LoadSudoku()
        {
            if (Containee == null)
                return;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string path = openFileDialog.FileName;
                string data = File.ReadAllText(path);
                string[,] array = Serialization.StringToArray(data);
                Containee.Grid = array;
            }
        }

        public void SaveSudoku()
        {
            if (Containee == null)
                return;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string[,] array = Containee.Grid;
                string data = Serialization.ArrayToString(array, Containee.Edge, Containee.Edge);
                string path = saveFileDialog.FileName;
                File.WriteAllText(path, data);
            }
        }

        public void ClearSudoku()
        {
            if (Containee == null)
                return;
            Containee.Grid = null;
        }
    }
}