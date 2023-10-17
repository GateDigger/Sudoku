# Sudoku
GateDigger's sudoku solver is a WFA capable of solving sudoku puzzles and counting their solutions.

## GUI Functionality

### Puzzle grid
- The grid has a fixed set of allowed characters.
  - Digits starting with 1 for grids of edge length < 10
  - Digits starting with 0 and capital latin characters starting with A for grids of edge length >= 10
- Select/unselect a cell in the grid by mouse-clicking on it.
  - Selected cell is highlighted by background color.
- Add/remove a character to/from selected cell by typing it on your keyboard.
  - Disallowed characters will be automatically ignored.

### Menu strip
- Solve: Finds a solution to the current puzzle.
- All solutions: Finds all solutions to the current puzzle and merges them.
  - The run time scales with the number of solutions.
  - Will use parallelism unless disabled in code.
- Count solutions: Counts the number of solutions to the current puzzle.
  - The run time scales with the number of solutions.
  - Will use parallelism unless disabled in code.
- Count givens: Counts the number of uniquely determined cells in the puzzle.
- Generate: To be implemented.
- Load: Allows the user to select a .txt file for an import of a puzzle.
  - Example file can be generated by saving a puzzle.
  - Any disallowed character will be imported as an empty cell.
  - \r, \n, \r\n are supported.
- Save: Allows the user to save the current puzzle to a .txt file.
  - Ambiguous cells will be discarded.
  - Environment-appropriate line break will be used.
- Clear: Wipes the current puzzle.
- The last 4 buttons resize the sudoku grid.
  - Edge length up to 36 is supported.
    - Good luck to your CPU if you go anywhere near that limit.

## Notes
- The application exists to showcase the code, there will surely be a few bugs and unhandled exceptions available.
- The default mode of parallelism is configured to spin up 16 tasks (SudokuController.FNOT_TASKCOUNT).
  - Undefine GUI/SudokuController.cs/MT_FNOT to spawn a task for each slice of the workload.
    - I do not know how many queued tasks an operating system can handle.
  - Undefine GUI/SudokuController.cs/MT to disable parallelism altogether.
- Undefine GUI/SudokuController.cs/STOPWATCH to stop measuring execution times.
- The maximum character set is "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ", case-sensitive.

## License

MIT License

Copyright (c) 2023 GateDigger

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
