using System;
using System.Drawing;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;

namespace Sudoku.Utility
{
    public static class LocalizationManager
    {
        public const BindingFlags getPropertiesBindingFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;

        /// <summary>
        /// Applies property values of each property within a specified static class to an object
        /// </summary>
        /// <param name="source">The static class</param>
        /// <param name="target">The target object</param>
        public static void ApplyProperties(Type source, object target)
        {
            Type targetType = target.GetType();
            foreach (PropertyInfo sourcePropInfo in source.GetProperties(getPropertiesBindingFlags))
                targetType.GetProperty(sourcePropInfo.Name).SetValue(target, sourcePropInfo.GetValue(null));
        }
    }

    internal static class Localization
    {
        internal const char FILE_EMPTYCELLCHAR = '.';
        internal const string FILE_EMPTYCELLSTR = ".";

        internal const string MAX_CHARSET = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        internal static string GetCharset(int length)
        {
            return length > 9 ? MAX_CHARSET.Substring(0, length) : MAX_CHARSET.Substring(1, length);
        }

        internal static class MainForm
        {
            internal static string Text
            {
                get
                {
                    return "Sudoku";
                }
            }

            internal static Font Font
            {
                get
                {
                    return new Font("Arial", 11, FontStyle.Bold);
                }
            }

            internal static class SolveControl
            {
                internal static string Text
                {
                    get
                    {
                        return "Solve";
                    }
                }
            }

            internal static class SolveAllControl
            {
                internal static string Text
                {
                    get
                    {
                        return "All solutions";
                    }
                }
            }

            internal static class CountSolutionsControl
            {
                internal static string Text
                {
                    get
                    {
                        return "Count solutions";
                    }
                }
            }

            internal static class CountGivensControl
            {
                internal static string Text
                {
                    get
                    {
                        return "Count givens";
                    }
                }
            }

            internal static class GenerateSudokuControl
            {
                internal static string Text
                {
                    get
                    {
                        return "Generate";
                    }
                }
            }

            internal static class LoadControl
            {
                internal static string Text
                {
                    get
                    {
                        return "Load";
                    }
                }
            }

            internal static class SaveControl
            {
                internal static string Text
                {
                    get
                    {
                        return "Save";
                    }
                }
            }

            internal static class ClearControl
            {
                internal static string Text
                {
                    get
                    {
                        return "Clear";
                    }
                }
            }

            internal static class HShrinkControl
            {
                internal static string Text
                {
                    get
                    {
                        return "←";
                    }
                }
            }

            internal static class VShrinkControl
            {
                internal static string Text
                {
                    get
                    {
                        return "↑";
                    }
                }
            }

            internal static class HExpControl
            {
                internal static string Text
                {
                    get
                    {
                        return "→";
                    }
                }
            }

            internal static class VExpControl
            {
                internal static string Text
                {
                    get
                    {
                        return "↓";
                    }
                }
            }
        }

        internal static class SudokuController
        {
            internal static class OpenSudokuDialog
            {
                internal static string Title
                {
                    get
                    {
                        return "Select a file";
                    }
                }

                internal static string DefaultExt
                {
                    get
                    {
                        return "txt";
                    }
                }

                internal static string Filter
                {
                    get
                    {
                        return "txt files (*.txt)|*.txt|All files(*.*)|*.*";
                    }
                }
            }

            internal static class SaveSudokuDialog
            {
                internal static string Title
                {
                    get
                    {
                        return "Create a file";
                    }
                }

                internal static string DefaultExt
                {
                    get
                    {
                        return "txt";
                    }
                }

                internal static string Filter
                {
                    get
                    {
                        return "txt files (*.txt)|*.txt";
                    }
                }
            }
        }


        internal static void ReportCalculationRuntime(Stopwatch sw)
        {
            MessageBox.Show("The calculation ran for " + sw.ElapsedMilliseconds.ToString() + " ms");
        }

        internal static void ReportCountSolutionsResult(int count, Stopwatch sw)
        {
            switch (count)
            {
                case 0: MessageBox.Show("The given sudoku allows no solutions; " + sw.ElapsedMilliseconds.ToString() + " ms"); return;
                case 1: MessageBox.Show("The given sudoku allows one solution; " + sw.ElapsedMilliseconds.ToString() + " ms"); return;
                default: MessageBox.Show("The given sudoku allows " + count.ToString() + " different solutions; " + sw.ElapsedMilliseconds.ToString() + " ms"); return;
            }
        }
        internal static void ReportCountSolutionsResult(int count)
        {
            switch (count)
            {
                case 0: MessageBox.Show("The given sudoku allows no solutions"); return;
                case 1: MessageBox.Show("The given sudoku allows one solution"); return;
                default: MessageBox.Show("The given sudoku allows " + count.ToString() + " solutions"); return;
            }
        }
        internal static void ReportCountGivensResult(int count)
        {
            switch (count)
            {
                case 0: MessageBox.Show("This sudoku has no givens."); return;
                default: MessageBox.Show("This sudoku has " + count.ToString() + " given."); return;
            }
        }
    }
}