using System.Drawing;
using System.Collections.Generic;

namespace Sudoku.Core
{
    public static partial class SudokuSolver
    {
        public static string[,] SolveAll_Parallel_OSPT(string[,] grid, string characterSet, int taskDepthLimit, int subgridHeight, int subgridWidth)
        {
            int edge = subgridHeight * subgridWidth;

            CharSet[,] array = Phi(grid, StringToCharsetConversion, edge, edge);
            CharSet charset = new CharSet(characterSet);

            array.ApplyPhi(x => x.IsEmpty ? charset : x, edge, edge);

            using (SliceExecutionController<CharSet[,], CharSet[,]> sliceExecutionCtrl = new SliceExecutionController<CharSet[,], CharSet[,]>() { Mode = ExecutionMode.OSPT })
            {
                SolveAll_ParallelPlan(array, sliceExecutionCtrl, taskDepthLimit, subgridHeight, subgridWidth, edge);
                sliceExecutionCtrl.ProcessAllSlices((SliceExecutionController<CharSet[,], CharSet[,]> sec) => (() => SolveAll_ParallelBody_OSPT(sec, subgridHeight, subgridWidth, edge)));
                return Phi(SolveAll_ParallelAggregator(sliceExecutionCtrl.GetSliceCollection(), edge, edge), CharsetToStringConversion, edge, edge);
            }
        }

        public static string[,] SolveAll_Parallel_FNOT(string[,] grid, string characterSet, int taskDepthLimit, int taskCount, int subgridHeight, int subgridWidth)
        {
            int edge = subgridHeight * subgridWidth;

            CharSet[,] array = Phi(grid, StringToCharsetConversion, edge, edge);
            CharSet charset = new CharSet(characterSet);

            array.ApplyPhi(x => x.IsEmpty ? charset : x, edge, edge);

            using (SliceExecutionController<CharSet[,], CharSet[,]> sliceExecutionCtrl = new SliceExecutionController<CharSet[,], CharSet[,]>() { Mode = ExecutionMode.FNOT, TaskCount = taskCount })
            {
                SolveAll_ParallelPlan(array, sliceExecutionCtrl, taskDepthLimit, subgridHeight, subgridWidth, edge);
                sliceExecutionCtrl.ProcessAllSlices((SliceExecutionController<CharSet[,], CharSet[,]> sec) => (() => SolveAll_ParallelBody_FNOT(sec, subgridHeight, subgridWidth, edge)));
                return Phi(SolveAll_ParallelAggregator(sliceExecutionCtrl.GetSliceCollection(), edge, edge), CharsetToStringConversion, edge, edge);
            }
        }

        static void SolveAll_ParallelPlan(CharSet[,] array, SliceExecutionController<CharSet[,], CharSet[,]> sliceExecutionCtrl, int taskDepthLimit, int subgridHeight, int subgridWidth, int edge)
        {
            if (!Solve_TrimLoop(array, subgridHeight, subgridWidth, edge))
                return;
            if (!ContainsNonSingletonCells(array, edge, edge) || taskDepthLimit == 0)
            {
                sliceExecutionCtrl.CreateSlice(array);
                return;
            }

            Point shortestUndeterminedCellCoord = GetSmallestUndeterminedCell(array, edge, edge);
            CharSet shortestUndeterminedCell = array[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X];
            CharSet[,] clone;

            for (int step = shortestUndeterminedCell.Size - 1; step > -1; step--)
            {
                clone = ShallowClone(array);
                clone[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X] = shortestUndeterminedCell[step];
                SolveAll_ParallelPlan(clone, sliceExecutionCtrl, taskDepthLimit - 1, subgridHeight, subgridWidth, edge);
            }
        }

        static void SolveAll_ParallelBody_OSPT(SliceExecutionController<CharSet[,], CharSet[,]> sliceExecutionCtrl, int subgridHeight, int subgridWidth, int edge)
        {
            Slice<CharSet[,], CharSet[,]> slice;
            lock (sliceExecutionCtrl)
                sliceExecutionCtrl.TryTakeSlice(out slice);
            CharSet[,] array = sliceExecutionCtrl.StartSlice(slice);

            if (!Solve_TrimLoop(array, subgridHeight, subgridWidth, edge))
                return;

            if (!ContainsNonSingletonCells(array, edge, edge))
            {
                sliceExecutionCtrl.FinishSlice(slice, array);
                sliceExecutionCtrl.FinishTask();
                return;
            }

            Point shortestUndeterminedCellCoord = GetSmallestUndeterminedCell(array, edge, edge);
            CharSet shortestUndeterminedCell = array[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X];
            CharSet[,] clone,
                result = new CharSet[edge, edge];

            for (int step = shortestUndeterminedCell.Size - 1; step > -1 && !sliceExecutionCtrl.InterruptFlag; step--)
            {
                clone = ShallowClone(array);
                clone[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X] = shortestUndeterminedCell[step];
                clone = SolveAll_ParallelBody(clone, sliceExecutionCtrl, subgridHeight, subgridWidth, edge);
                if (clone != null)
                    result.ApplyOr(clone, edge, edge);
            }
            sliceExecutionCtrl.FinishSlice(slice, result);
            sliceExecutionCtrl.FinishTask();
        }

        static void SolveAll_ParallelBody_FNOT(SliceExecutionController<CharSet[,], CharSet[,]> sliceExecutionCtrl, int subgridHeight, int subgridWidth, int edge)
        {
            Slice<CharSet[,], CharSet[,]> slice;
        START:
            lock (sliceExecutionCtrl)
                if (!sliceExecutionCtrl.TryTakeSlice(out slice))
                {
                    sliceExecutionCtrl.FinishTask();
                    return;
                }
            CharSet[,] array = sliceExecutionCtrl.StartSlice(slice);

            if (!Solve_TrimLoop(array, subgridHeight, subgridWidth, edge))
                return;

            if (!ContainsNonSingletonCells(array, edge, edge))
            {
                sliceExecutionCtrl.FinishSlice(slice, array);
                goto START;
            }

            Point shortestUndeterminedCellCoord = GetSmallestUndeterminedCell(array, edge, edge);
            CharSet shortestUndeterminedCell = array[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X];
            CharSet[,] clone,
                result = new CharSet[edge, edge];

            for (int step = shortestUndeterminedCell.Size - 1; step > -1 && !sliceExecutionCtrl.InterruptFlag; step--)
            {
                clone = ShallowClone(array);
                clone[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X] = shortestUndeterminedCell[step];
                clone = SolveAll_ParallelBody(clone, sliceExecutionCtrl, subgridHeight, subgridWidth, edge);
                if (clone != null)
                    result.ApplyOr(clone, edge, edge);
            }
            sliceExecutionCtrl.FinishSlice(slice, result);
            goto START;
        }

        static CharSet[,] SolveAll_ParallelBody(CharSet[,] array, SliceExecutionController<CharSet[,], CharSet[,]> sliceExecutionCtrl, int subgridHeight, int subgridWidth, int edge)
        {
            if (!Solve_TrimLoop(array, subgridHeight, subgridWidth, edge))
                return null;
            if (!ContainsNonSingletonCells(array, edge, edge))
                return array;

            Point shortestUndeterminedCellCoord = GetSmallestUndeterminedCell(array, edge, edge);
            CharSet shortestUndeterminedCell = array[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X];
            CharSet[,] clone,
                result = new CharSet[edge, edge];

            for (int step = shortestUndeterminedCell.Size - 1; step > -1 && !sliceExecutionCtrl.InterruptFlag; step--)
            {
                clone = ShallowClone(array);
                clone[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X] = shortestUndeterminedCell[step];
                clone = SolveAll_ParallelBody(clone, sliceExecutionCtrl, subgridHeight, subgridWidth, edge);
                if (clone != null)
                    result.ApplyOr(clone, edge, edge);
            }
            return result;
        }

        static CharSet[,] SolveAll_ParallelAggregator(IEnumerable<Slice<CharSet[,], CharSet[,]>> collection, int height, int width)
        {
            CharSet[,] result = new CharSet[height, width];
            foreach (Slice<CharSet[,], CharSet[,]> slice in collection)
                result.ApplyOr(slice.Result, height, width);
            return result;
        }


        public static int CountSolutions_Parallel_OSPT(string[,] grid, string characterSet, int taskDepthLimit, int subgridHeight, int subgridWidth)
        {
            int edge = subgridHeight * subgridWidth;

            CharSet[,] array = Phi(grid, StringToCharsetConversion, edge, edge);
            CharSet charset = new CharSet(characterSet);

            array.ApplyPhi(x => x.IsEmpty ? charset : x, edge, edge);

            using (SliceExecutionController<CharSet[,], int> sliceExecutionCtrl = new SliceExecutionController<CharSet[,], int>() { Mode = ExecutionMode.OSPT })
            {
                CountSolutions_ParallelPlan(array, sliceExecutionCtrl, taskDepthLimit, subgridHeight, subgridWidth, edge);
                sliceExecutionCtrl.ProcessAllSlices((SliceExecutionController<CharSet[,], int> sec) => (() => CountSolutions_ParallelBody_OSPT(sec, subgridHeight, subgridWidth, edge)));
                return CountSolutions_ParallelAggregator(sliceExecutionCtrl.GetSliceCollection());
            }
        }
        public static int CountSolutions_Parallel_FNOT(string[,] grid, string characterSet, int taskDepthLimit, int taskCount, int subgridHeight, int subgridWidth)
        {
            int edge = subgridHeight * subgridWidth;

            CharSet[,] array = Phi(grid, StringToCharsetConversion, edge, edge);
            CharSet charset = new CharSet(characterSet);

            array.ApplyPhi(x => x.IsEmpty ? charset : x, edge, edge);

            using (SliceExecutionController<CharSet[,], int> sliceExecutionCtrl = new SliceExecutionController<CharSet[,], int>() { Mode = ExecutionMode.FNOT, TaskCount = taskCount })
            {
                CountSolutions_ParallelPlan(array, sliceExecutionCtrl, taskDepthLimit, subgridHeight, subgridWidth, edge);
                sliceExecutionCtrl.ProcessAllSlices((SliceExecutionController<CharSet[,], int> sec) => (() => CountSolutions_ParallelBody_FNOT(sec, subgridHeight, subgridWidth, edge)));
                return CountSolutions_ParallelAggregator(sliceExecutionCtrl.GetSliceCollection());
            }
        }

        static void CountSolutions_ParallelPlan(CharSet[,] array, SliceExecutionController<CharSet[,], int> sliceExecutionCtrl, int taskDepthLimit, int subgridHeight, int subgridWidth, int edge)
        {
            if (!Solve_TrimLoop(array, subgridHeight, subgridWidth, edge))
                return;
            if (!ContainsNonSingletonCells(array, edge, edge) || taskDepthLimit == 0)
            {
                sliceExecutionCtrl.CreateSlice(array);
                return;
            }

            Point shortestUndeterminedCellCoord = GetSmallestUndeterminedCell(array, edge, edge);
            CharSet shortestUndeterminedCell = array[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X];
            CharSet[,] clone;

            for (int step = shortestUndeterminedCell.Size - 1; step > -1; step--)
            {
                clone = ShallowClone(array);
                clone[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X] = shortestUndeterminedCell[step];
                CountSolutions_ParallelPlan(clone, sliceExecutionCtrl, taskDepthLimit - 1, subgridHeight, subgridWidth, edge);
            }
        }
        static void CountSolutions_ParallelBody_OSPT(SliceExecutionController<CharSet[,], int> sliceExecutionCtrl, int subgridHeight, int subgridWidth, int edge)
        {
            Slice<CharSet[,], int> slice;
            lock (sliceExecutionCtrl)
                sliceExecutionCtrl.TryTakeSlice(out slice);
            CharSet[,] array = sliceExecutionCtrl.StartSlice(slice);

            if (!Solve_TrimLoop(array, subgridHeight, subgridWidth, edge))
                return;

            if (!ContainsNonSingletonCells(array, edge, edge))
            {
                sliceExecutionCtrl.FinishSlice(slice, 1);
                sliceExecutionCtrl.FinishTask();
                return;
            }

            Point shortestUndeterminedCellCoord = GetSmallestUndeterminedCell(array, edge, edge);
            CharSet shortestUndeterminedCell = array[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X];
            CharSet[,] clone;
            int result = 0;

            for (int step = shortestUndeterminedCell.Size - 1; step > -1 && !sliceExecutionCtrl.InterruptFlag; step--)
            {
                clone = ShallowClone(array);
                clone[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X] = shortestUndeterminedCell[step];
                result += CountSolutions_ParallelBody(clone, sliceExecutionCtrl, subgridHeight, subgridWidth, edge);
            }
            sliceExecutionCtrl.FinishSlice(slice, result);
            sliceExecutionCtrl.FinishTask();
        }

        static void CountSolutions_ParallelBody_FNOT(SliceExecutionController<CharSet[,], int> sliceExecutionCtrl, int subgridHeight, int subgridWidth, int edge)
        {
            Slice<CharSet[,], int> slice;
        START:
            lock (sliceExecutionCtrl)
                if (!sliceExecutionCtrl.TryTakeSlice(out slice))
                {
                    sliceExecutionCtrl.FinishTask();
                    return;
                }
            CharSet[,] array = sliceExecutionCtrl.StartSlice(slice);

            if (!Solve_TrimLoop(array, subgridHeight, subgridWidth, edge))
                return;

            if (!ContainsNonSingletonCells(array, edge, edge))
            {
                sliceExecutionCtrl.FinishSlice(slice, 1);
                goto START;
            }

            Point shortestUndeterminedCellCoord = GetSmallestUndeterminedCell(array, edge, edge);
            CharSet shortestUndeterminedCell = array[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X];
            CharSet[,] clone;
            int result = 0;

            for (int step = shortestUndeterminedCell.Size - 1; step > -1 && !sliceExecutionCtrl.InterruptFlag; step--)
            {
                clone = ShallowClone(array);
                clone[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X] = shortestUndeterminedCell[step];
                result += CountSolutions_ParallelBody(clone, sliceExecutionCtrl, subgridHeight, subgridWidth, edge);
            }
            sliceExecutionCtrl.FinishSlice(slice, result);
            goto START;
        }

        static int CountSolutions_ParallelBody(CharSet[,] array, SliceExecutionController<CharSet[,], int> sliceExecutionCtrl, int subgridHeight, int subgridWidth, int edge)
        {
            if (!Solve_TrimLoop(array, subgridHeight, subgridWidth, edge))
                return 0;
            if (!ContainsNonSingletonCells(array, edge, edge))
                return 1;

            Point shortestUndeterminedCellCoord = GetSmallestUndeterminedCell(array, edge, edge);
            CharSet shortestUndeterminedCell = array[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X];
            CharSet[,] clone;
            int result = 0;

            for (int step = shortestUndeterminedCell.Size - 1; step > -1 && !sliceExecutionCtrl.InterruptFlag; step--)
            {
                clone = ShallowClone(array);
                clone[shortestUndeterminedCellCoord.Y, shortestUndeterminedCellCoord.X] = shortestUndeterminedCell[step];
                result += CountSolutions_ParallelBody(clone, sliceExecutionCtrl, subgridHeight, subgridWidth, edge);
            }
            return result;
        }

        static int CountSolutions_ParallelAggregator(IEnumerable<Slice<CharSet[,], int>> collection)
        {
            int result = 0;
            foreach (Slice<CharSet[,], int> slice in collection)
                result += slice.Result;
            return result;
        }
    }
}