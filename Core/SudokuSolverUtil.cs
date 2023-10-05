using System.Drawing;

namespace Sudoku.Core
{
    public static partial class SudokuSolver
    {
        static readonly F<string, CharSet> StringToCharsetConversion = x => new CharSet(x);
        static readonly F<CharSet, string> CharsetToStringConversion = x => x.ToString();
        
        /*
        //Leave this here for later
        public static bool VerifyGridValidity(string[,] grid, string characterSet, int subgridHeight, int subgridWidth)
        {
            int edge = subgridHeight * subgridWidth;

            CharSet[,] array = Phi(grid, StringToCharsetConversion, edge, edge);
            CharSet charset = new CharSet(characterSet);

            array.ApplyPhi(x => x.IsEmpty ? charset : x, edge, edge);
            return VerifyGridValidity(array, subgridHeight, subgridWidth, edge);
        }
        static bool VerifyGridValidity(CharSet[,] array, int subgridHeight, int subgridWidth, int edge)
        {
            int row,
                col;
            CharSet tmp,
                current;

            for (row = 0; row < edge; row++)
            {
                tmp = CharSet.Empty;
                for (col = 0; col < edge; col++)
                {
                    current = array[row, col];
                    if (tmp >= current)
                        return false;
                    else
                        tmp |= current;
                }
            }

            for (col = 0; col < edge; col++)
            {
                tmp = CharSet.Empty;
                for (row = 0; row < edge; row++)
                {
                    current = array[row, col];
                    if (tmp >= current)
                        return false;
                    else
                        tmp |= current;
                }
            }

            row = 0;
            for (int subRow, subCol, subRowStart, subColStart, subRowCeil, subColCeil; row < subgridWidth; row++)
            {
                subRowStart = row * subgridHeight;
                subRowCeil = subRowStart + subgridHeight;

                for (col = 0; col < subgridHeight; col++)
                {
                    subColStart = col * subgridWidth;
                    subColCeil = subColStart + subgridWidth;

                    tmp = CharSet.Empty;
                    for (subRow = subRowStart; subRow < subRowCeil; subRow++)
                    {
                        for (subCol = subColStart; subCol < subColCeil; subCol++)
                        {
                            current = array[subRow, subCol];
                            if (tmp >= current)
                                return false;
                            else
                                tmp |= current;
                        }
                    }
                }
            }

            return true;
        }
        */


        static readonly Point NULLPOINT = new Point(-1, -1);
        static Point GetSmallestUndeterminedCell(CharSet[,] array, int height, int width)
        {
            Point result = NULLPOINT;
            int bestLength = int.MaxValue,
                currentLength;
            for (int row = 0, col; row < height; row++)
                for (col = 0; col < width; col++)
                    if (array[row, col].HasMultipleElements && (currentLength = array[row, col].Size) < bestLength)
                    {
                        result = new Point(col, row);
                        bestLength = currentLength;
                        if (bestLength == 2)
                            return result;
                    }
            return result;
        }


        static bool ContainsNonSingletonCells(this CharSet[,] array, int height, int width)
        {
            if (array == null)
                return false;
            for (int row = 0, col; row < height; row++)
                for (col = 0; col < width; col++)
                    if (!array[row, col].IsSingleton)
                        return true;
            return false;
        }

        public static int CountGivens(string[,] grid, string characterSet, int height, int width)
        {
            CharSet[,] array = Phi(grid, StringToCharsetConversion, height, width);
            CharSet charset = new CharSet(characterSet);

            array.ApplyPhi(x => x.IsEmpty ? charset : x, height, width);
            return CountPhi(array, x => x.IsSingleton, height, width);
        }

        static void ApplyOr(this CharSet[,] target, CharSet[,] array, int height, int width)
        {
            if (target == null)
                return;
            for (int row = 0, col; row < height; row++)
                for (col = 0; col < width; col++)
                    target[row, col] |= array[row, col];
        }

        static T[,] ShallowClone<T>(T[,] array)
        {            
            return array.Clone() as T[,];
        }

        static int CountPhi<S>(this S[,] array, F<S, bool> phi, int height, int width)
        {
            if (array == null)
                return 0;
            int result = 0;
            for (int row = 0, col; row < height; row++)
                for (col = 0; col < width; col++)
                    if (phi(array[row, col]))
                        result++;
            return result;
        }


        static void ApplyPhi<T>(this T[,] target, F<T> phi, int height, int width)
        {
            if (target == null)
                return;
            for (int row = 0, col; row < height; row++)
                for (col = 0; col < width; col++)
                    target[row, col] = phi(target[row, col]);
        }

        static T[,] Phi<S, T>(S[,] array, F<S, T> phi, int height, int width)
        {
            if (array == null)
                return null;
            T[,] result = new T[height, width];
            for (int row = 0, col; row < height; row++)
                for (col = 0; col < width; col++)
                    result[row, col] = phi(array[row, col]);
            return result;
        }
    }
    public delegate T F<T>(T x);
    public delegate Res F<X1, Res>(X1 x1);
    public delegate Res F<X1, X2, Res>(X1 x1, X2 x2);
}