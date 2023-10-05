using System;
using System.Windows.Forms;

namespace Sudoku
{
    internal static class Program
    {
        /// <summary>
        /// GateDigger's sudoku solver
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new GUI.Main());
        }
    }
}