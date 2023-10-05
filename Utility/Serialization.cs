using System;
using System.Text;

namespace Sudoku.Utility
{
    public static class Serialization
    {
        public static string ArrayToString(string[,] array, int height, int width)
        {
            if (array == null || height == 0 || width == 0)
                return "";

            StringBuilder sb = new StringBuilder(height * (width + 2));
            string currentString;

            for (int row = 0, col; row < height; row++)
            {
                for (col = 0; col < width; col++)
                {
                    currentString = array[row, col];
                    if (currentString != null && currentString.Length == 1)
                        sb.Append(currentString);
                    else
                        sb.Append(Localization.FILE_EMPTYCELLCHAR);
                }
                sb.Append(Environment.NewLine);
            }
            return sb.ToString(0, sb.Length);
        }

        static readonly char[] splitChars = new char[] { '\r', '\n' };
        public static string[,] StringToArray(string data)
        {
            string[] lines = data.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            int height = lines.Length,
                width = 0,
                tempWidth;

            char[][] temp = new char[height][];
            for (int row = 0; row < height; row++)
            {
                temp[row] = lines[row].ToCharArray();

                tempWidth = temp[row].Length;
                if (width < tempWidth)
                    width = tempWidth;
            }

            return JaggedCharTo2DString(temp, height, width);
        }

        static string[,] JaggedCharTo2DString(char[][] array, int height, int width)
        {
            string[,] result = new string[height, width];
            for (int row = 0, col, currentWidth; row < height; row++)
            {
                currentWidth = array[row].Length;

                for (col = 0; col < currentWidth; col++)
                    result[row, col] = new string(array[row][col], 1);

                for (; col < width; col++)
                    result[row, col] = Localization.FILE_EMPTYCELLSTR;
            }
            return result;
        }
    }
}