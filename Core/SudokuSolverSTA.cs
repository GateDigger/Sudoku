using System.Drawing;

namespace Sudoku.Core
{
    public static partial class SudokuSolver
    {
        /// <summary>
        /// Returns a valid solution to the supplied grid, if such exists.
        /// </summary>
        /// <param name="grid">The sudoku grid</param>
        /// <param name="characterSet">Set of characters to use in the solution</param>
        /// <param name="subgridHeight">Height of a subgrid</param>
        /// <param name="subgridWidth">Width of a subgrid</param>
        /// <returns>A solution</returns>
        public static string[,] Solve1(string[,] grid, string characterSet, int subgridHeight, int subgridWidth)
        {
            int edge = subgridHeight * subgridWidth;

            CharSet[,] array = Phi(grid, StringToCharsetConversion, edge, edge);
            CharSet charset = new CharSet(characterSet);

            array.ApplyPhi(x => x.IsEmpty ? charset : x, edge, edge);
            array = Solve1_Body(array, subgridHeight, subgridWidth, edge);

            return Phi(array, CharsetToStringConversion, edge, edge);
        }
        static CharSet[,] Solve1_Body(CharSet[,] array, int subgridHeight, int subgridWidth, int edge)
        {
            if (!Solve_TrimLoop(array, subgridHeight, subgridWidth, edge))
                return null;
            if (!array.ContainsNonSingletonCells(edge, edge))
                return array;

            Point shortestUndeterminedCellCoord = GetSmallestUndeterminedCell(array, edge, edge);
            CharSet tmp = array[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X];
            CharSet[,] clone;

            for (int step = tmp.Size - 1; step > -1; step--)
            {
                clone = ShallowClone(array);
                clone[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X] = tmp[step];
                clone = Solve1_Body(clone, subgridHeight, subgridWidth, edge);
                if (clone != null)
                    return clone;
            }
            return null;
        }

        /// <summary>
        /// Returns all valid solutions to the supplied grid, if any exist.
        /// </summary>
        /// <param name="grid">The sudoku grid</param>
        /// <param name="characterSet">Set of characters to use in the solution</param>
        /// <param name="subgridHeight">Height of a subgrid</param>
        /// <param name="subgridWidth">Width of a subgrid</param>
        /// <returns>A grid containing all solutions</returns>
        public static string[,] SolveAll(string[,] grid, string characterSet, int subgridHeight, int subgridWidth)
        {
            int edge = subgridHeight * subgridWidth;

            CharSet[,] array = Phi(grid, StringToCharsetConversion, edge, edge);
            CharSet charset = new CharSet(characterSet);

            array.ApplyPhi(x => x.IsEmpty ? charset : x, edge, edge);
            array = SolveAll_Body(array, subgridHeight, subgridWidth, edge);

            return Phi(array, CharsetToStringConversion, edge, edge);
        }
        static CharSet[,] SolveAll_Body(CharSet[,] array, int subgridHeight, int subgridWidth, int edge)
        {
            if (!Solve_TrimLoop(array, subgridHeight, subgridWidth, edge))
                return null;
            if (!array.ContainsNonSingletonCells(edge, edge))
                return array;

            Point shortestUndeterminedCellCoord = GetSmallestUndeterminedCell(array, edge, edge);
            CharSet shortestUndeterminedCell = array[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X];
            CharSet[,] clone,
                result = new CharSet[edge, edge];

            for (int step = shortestUndeterminedCell.Size - 1; step > -1; step--)
            {
                clone = ShallowClone(array);
                clone[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X] = shortestUndeterminedCell[step];
                clone = SolveAll_Body(clone, subgridHeight, subgridWidth, edge);
                if (clone != null)
                    result.ApplyOr(clone, edge, edge);
            }
            return result;
        }


        /// <summary>
        /// Counts all valid solutions of the supplied grid.
        /// </summary>
        /// <param name="grid">The sudoku grid</param>
        /// <param name="characterSet">Set of characters to use in the solution</param>
        /// <param name="subgridHeight">Height of a subgrid</param>
        /// <param name="subgridWidth">Width of a subgrid</param>
        /// <returns>The number of solutions</returns>
        public static int CountSolutions(string[,] grid, string characterSet, int subgridHeight, int subgridWidth)
        {
            int edge = subgridHeight * subgridWidth;

            CharSet[,] array = Phi(grid, StringToCharsetConversion, edge, edge);
            CharSet charset = new CharSet(characterSet);

            array.ApplyPhi(x => x.IsEmpty ? charset : x, edge, edge);
            return CountSolutions_Body(array, subgridHeight, subgridWidth, edge);
        }
        static int CountSolutions_Body(CharSet[,] array, int subgridHeight, int subgridWidth, int edge)
        {
            if (!Solve_TrimLoop(array, subgridHeight, subgridWidth, edge))
                return 0;
            if (!array.ContainsNonSingletonCells(edge, edge))
                return 1;

            Point shortestUndeterminedCellCoord = GetSmallestUndeterminedCell(array, edge, edge);
            CharSet shortestUndeterminedCell = array[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X];
            CharSet[,] clone;
            int result = 0;

            for (int step = shortestUndeterminedCell.Size - 1; step > -1; step--)
            {
                clone = ShallowClone(array);
                clone[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X] = shortestUndeterminedCell[step];
                result += CountSolutions_Body(clone, subgridHeight, subgridWidth, edge);
            }
            return result;
        }

        /// <summary>
        /// Counts all valid solutions of the supplied grid, up to a specified count limit.
        /// </summary>
        /// <param name="grid">The sudoku grid</param>
        /// <param name="characterSet">Set of characters to use in the solution</param>
        /// <param name="countLimit">Upper limit of the count</param>
        /// <param name="subgridHeight">Height of a subgrid</param>
        /// <param name="subgridWidth">Width of a subgrid</param>
        /// <returns>min(The number of solutions, countLimit)</returns>
        public static int CountSolutions(string[,] grid, string characterSet, int countLimit, int subgridHeight, int subgridWidth)
        {
            int edge = subgridHeight * subgridWidth;

            CharSet[,] array = Phi(grid, StringToCharsetConversion, edge, edge);
            CharSet charset = new CharSet(characterSet);

            array.ApplyPhi(x => x.IsEmpty ? charset : x, edge, edge);
            return CountSolutions_Body(array, countLimit, subgridHeight, subgridWidth, edge);
        }
        static int CountSolutions_Body(CharSet[,] array, int countLimit, int subgridHeight, int subgridWidth, int edge)
        {
            if (!Solve_TrimLoop(array, subgridHeight, subgridWidth, edge))
                return 0;
            if (!array.ContainsNonSingletonCells(edge, edge))
                return 1;

            Point shortestUndeterminedCellCoord = GetSmallestUndeterminedCell(array, edge, edge);
            CharSet shortestUndeterminedCell = array[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X];
            CharSet[,] clone;
            int result = 0;

            for (int step = shortestUndeterminedCell.Size - 1; step > -1 && countLimit > result; step--)
            {
                clone = ShallowClone(array);
                clone[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X] = shortestUndeterminedCell[step];
                result += CountSolutions_Body(clone, countLimit - result, subgridHeight, subgridWidth, edge);
            }
            return result;
        }

        /// <summary>
        /// For every row, column and subgrid: Accumulate all givens in the block and subtract them from all ambiguous cells.
        /// Contradictory cell detection is guaranteed and upon it the method returns false.
        /// The method iterates on the grid until it can no longer trim anything.
        /// </summary>
        /// <param name="array">The sudoku grid to trim</param>
        /// <param name="subgridHeight">Height of a subgrid</param>
        /// <param name="subgridWidth">Width of a subgrid</param>
        /// <param name="edge">Supply subgridHeight * subgridWidth or you break it.</param>
        /// <returns></returns>
        static bool Solve_TrimLoop(CharSet[,] array, int subgridHeight, int subgridWidth, int edge)
        {
            int row, col, subRow, subRowStart, subRowCeil, subCol, subColStart, subColCeil;
            CharSet tmp, current;

        START:

            row = 0;
        ROWCYCLE1:

            tmp = CharSet.Empty;
            col = 0;
        ROWCYCLE2:

            current = array[row, col];
            if (current.IsSingleton)
            {
                if (tmp >= current)
                {
                    array[row, col] = CharSet.Empty;
                    return false;
                }
                tmp |= current;
            }

            col++;
            if (col < edge)
                goto ROWCYCLE2;

            col = 0;
        ROWCYCLE3:

            current = array[row, col];
            if (current.HasMultipleElements && !(current & tmp).IsEmpty)
            {
                current -= tmp;
                goto WORMHOLE1;
            }
            //array[row, col] = current;
            if (current.IsEmpty)
                return false;

            col++;
            if (col < edge)
                goto ROWCYCLE3;

            row++;
            if (row < edge)
                goto ROWCYCLE1;

            col = 0;
        COLCYCLE1:

            tmp = CharSet.Empty;
            row = 0;
        COLCYCLE2:

            current = array[row, col];
            if (current.IsSingleton)
            {
                if (tmp >= current)
                {
                    array[row, col] = CharSet.Empty;
                    return false;
                }
                tmp |= current;
            }

            row++;
            if (row < edge)
                goto COLCYCLE2;

            row = 0;
        COLCYCLE3:

            current = array[row, col];
            if (current.HasMultipleElements && !(current & tmp).IsEmpty)
            {
                current -= tmp;
                goto WORMHOLE2;
            }
            //array[row, col] = current;
            if (current.IsEmpty)
                return false;

            row++;
            if (row < edge)
                goto COLCYCLE3;

            col++;
            if (col < edge)
                goto COLCYCLE1;

            row = 0;
        SEGCYCLE1:

            subRowStart = row * subgridHeight;
            subRowCeil = subRowStart + subgridHeight;
            col = 0;
        SEGCYCLE2:


            subColStart = col * subgridWidth;
            subColCeil = subColStart + subgridWidth;
            tmp = CharSet.Empty;

            subRow = subRowStart;
        SEGCYCLE3:

            subCol = subColStart;
        SEGCYCLE4:

            current = array[subRow, subCol];
            if (current.IsSingleton)
            {
                if (tmp >= current)
                {
                    array[row, col] = CharSet.Empty;
                    return false;
                }
                tmp |= current;
            }

            subCol++;
            if (subCol < subColCeil)
                goto SEGCYCLE4;

            subRow++;
            if (subRow < subRowCeil)
                goto SEGCYCLE3;

            subRow = subRowStart;
        SEGCYCLE5:

            subCol = subColStart;
        SEGCYCLE6:

            current = array[subRow, subCol];
            if (current.HasMultipleElements && !(current & tmp).IsEmpty)
            {
                current -= tmp;
                goto WORMHOLE3;
            }
            //array[subRow, subCol] = current;
            if (current.IsEmpty)
                return false;

            subCol++;
            if (subCol < subColCeil)
                goto SEGCYCLE6;

            subRow++;
            if (subRow < subRowCeil)
                goto SEGCYCLE5;

            col++;
            if (col < subgridHeight)
                goto SEGCYCLE2;

            row++;
            if (row < subgridWidth)
                goto SEGCYCLE1;

            return true;

        ROWCYCLE11:

            tmp = CharSet.Empty;
            col = 0;
        ROWCYCLE12:

            current = array[row, col];
            if (current.IsSingleton)
            {
                if (tmp >= current)
                {
                    array[row, col] = CharSet.Empty;
                    return false;
                }
                tmp |= current;
            }

            col++;
            if (col < edge)
                goto ROWCYCLE12;

            col = 0;
        ROWCYCLE13:

            current = array[row, col];
            if (current.HasMultipleElements)
                current -= tmp;
            WORMHOLE1:
            array[row, col] = current;
            if (current.IsEmpty)
                return false;

            col++;
            if (col < edge)
                goto ROWCYCLE13;

            row++;
            if (row < edge)
                goto ROWCYCLE11;

            col = 0;
        COLCYCLE11:

            tmp = CharSet.Empty;
            row = 0;
        COLCYCLE12:

            current = array[row, col];
            if (current.IsSingleton)
            {
                if (tmp >= current)
                {
                    array[row, col] = CharSet.Empty;
                    return false;
                }
                tmp |= current;
            }

            row++;
            if (row < edge)
                goto COLCYCLE12;

            row = 0;
        COLCYCLE13:

            current = array[row, col];
            if (current.HasMultipleElements)
                current -= tmp;
            WORMHOLE2:
            array[row, col] = current;
            if (current.IsEmpty)
                return false;

            row++;
            if (row < edge)
                goto COLCYCLE13;

            col++;
            if (col < edge)
                goto COLCYCLE11;

            row = 0;
        SEGCYCLE11:

            subRowStart = row * subgridHeight;
            subRowCeil = subRowStart + subgridHeight;
            col = 0;
        SEGCYCLE12:

            subColStart = col * subgridWidth;
            subColCeil = subColStart + subgridWidth;
            tmp = CharSet.Empty;

            subRow = subRowStart;
        SEGCYCLE13:

            subCol = subColStart;
        SEGCYCLE14:

            current = array[subRow, subCol];
            if (current.IsSingleton)
            {
                if (tmp >= current)
                {
                    array[subRow, subCol] = CharSet.Empty;
                    return false;
                }
                tmp |= current;
            }

            subCol++;
            if (subCol < subColCeil)
                goto SEGCYCLE14;

            subRow++;
            if (subRow < subRowCeil)
                goto SEGCYCLE13;

            subRow = subRowStart;
        SEGCYCLE15:

            subCol = subColStart;
        SEGCYCLE16:

            current = array[subRow, subCol];
            if (current.HasMultipleElements)
                current -= tmp;
            WORMHOLE3:
            array[subRow, subCol] = current;
            if (current.IsEmpty)
                return false;

            subCol++;
            if (subCol < subColCeil)
                goto SEGCYCLE16;

            subRow++;
            if (subRow < subRowCeil)
                goto SEGCYCLE15;

            col++;
            if (col < subgridHeight)
                goto SEGCYCLE12;

            row++;
            if (row < subgridWidth)
                goto SEGCYCLE11;

            goto START;
        }
    }
}