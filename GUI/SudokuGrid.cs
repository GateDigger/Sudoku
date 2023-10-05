using System;
using System.Drawing;
using System.Windows.Forms;

using Sudoku.Core;

namespace Sudoku.GUI
{
    public class SudokuGrid : Control
    {
        readonly CharSet charSet;
        string[,] array;
        protected readonly int subgridHeight,
            subgridWidth,
            edge;
        public SudokuGrid(int subgridHeight, int subgridWidth, string characterSet)
        {
            DoubleBuffered = true;

            this.subgridHeight = subgridHeight;
            this.subgridWidth = subgridWidth;
            this.edge = subgridHeight * subgridWidth;

            this.charSet = new CharSet(characterSet);
            array = new string[edge, edge];
        }

        public int SubgridHeight
        {
            get
            {
                return subgridHeight;
            }
        }

        public int SubgridWidth
        {
            get
            {
                return subgridWidth;
            }
        }

        public int Edge
        {
            get
            {
                return edge;
            }
        }

        public string CharacterSet
        {
            get
            {
                return charSet.ToString();
            }
        }

        public string[,] Grid
        {
            get
            {
                int edge = this.edge;
                string[,] result = new string[edge, edge],
                    array = this.array;
                for (int row = 0, col; row < edge; row++)
                    for (col = 0; col < edge; col++)
                        result[row, col] = array[row, col];
                return result;
            }
            set
            {
                SuspendPaint();
                int row, col, height, width;
                if (value != null)
                {
                    height = value.GetLength(0);
                    width = value.GetLength(1);

                    for (row = 0; row < height; row++)
                        for (col = 0; col < width; col++)
                            array[row, col] = (new CharSet(value[row, col]) & charSet).ToString();
                }
                else
                {
                    height = edge;
                    width = edge;

                    for (row = 0; row < height; row++)
                        for (col = 0; col < width; col++)
                            array[row, col] = "";
                }
                ResumePaint(true);
            }
        }

        public string this[int row, int col]
        {
            get
            {
                return array[row, col];
            }
            set
            {
                array[row, col] = (new CharSet(value) & charSet).ToString();
                Refresh();
            }
        }



        uint paintCounter = 0;
        public void SuspendPaint()
        {
            paintCounter++;
        }
        public void ResumePaint(bool performPaint)
        {
            if (paintCounter > 0)
                paintCounter--;
            if (paintCounter == 0 && performPaint)
                Refresh();
        }
        public bool CanPaint
        {
            get
            {
                return paintCounter == 0;
            }
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            if (!CanPaint)
                return;
            base.OnPaint(e);
            PaintGrid(e.Graphics, oddBrush, evenBrush, selectedBrush, selectedCell, textBrush, Font, textFormat, gridPen, array, subgridHeight, subgridWidth, Padding.Top, Padding.Left, Height - Padding.Bottom, Width - Padding.Right, Height - Padding.Top - Padding.Bottom, Width - Padding.Left - Padding.Right);
        }


        Pen gridPen = Pens.Black;
        public event EventHandler GridPenChanged;
        public Pen GridPen
        {
            get
            {
                return gridPen;
            }
            set
            {
                gridPen = value;
                OnGridPenChanged(EventArgs.Empty);

                Refresh();
            }
        }
        protected virtual void OnGridPenChanged(EventArgs e)
        {
            if (GridPenChanged != null)
                GridPenChanged(this, e);
        }

        Brush oddBrush = Brushes.White;
        public event EventHandler OddBrushChanged;
        public Brush OddBrush
        {
            get
            {
                return oddBrush;
            }
            set
            {
                oddBrush = value;
                OnOddBrushChanged(EventArgs.Empty);

                Refresh();
            }
        }
        protected virtual void OnOddBrushChanged(EventArgs e)
        {
            if (OddBrushChanged != null)
                OddBrushChanged(this, e);
        }

        Brush evenBrush = Brushes.Cyan;
        public event EventHandler EvenBrushChanged;
        public Brush EvenBrush
        {
            get
            {
                return evenBrush;
            }
            set
            {
                evenBrush = value;
                OnEvenBrushChanged(EventArgs.Empty);

                Refresh();

            }
        }
        protected virtual void OnEvenBrushChanged(EventArgs e)
        {
            if (EvenBrushChanged != null) EvenBrushChanged(this, e);
        }

        Brush textBrush = Brushes.Black;
        public event EventHandler TextBrushChanged;
        public Brush TextBrush
        {
            get
            {
                return textBrush;
            }
            set
            {
                textBrush = value;
                OnTextBrushChanged(EventArgs.Empty);

                Refresh();
            }
        }
        protected virtual void OnTextBrushChanged(EventArgs e)
        {
            if (TextBrushChanged != null)
                TextBrushChanged(this, e);
        }

        StringFormat textFormat = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        public event EventHandler TextFormatChanged;
        public StringFormat TextFormat
        {
            get
            {
                return textFormat;
            }
            set
            {
                textFormat = value;
                OnTextFormatChanged(EventArgs.Empty);

                Refresh();
            }
        }
        protected virtual void OnTextFormatChanged(EventArgs e)
        {
            if (TextFormatChanged != null)
                TextFormatChanged(this, e);
        }

        Brush selectedBrush = Brushes.Green;
        public event EventHandler SelectedBrushChanged;
        public Brush SelectedBrush
        {
            get
            {
                return selectedBrush;
            }
            set
            {
                selectedBrush = value;
                OnSelectedBrushChanged(EventArgs.Empty);

                Refresh();
            }
        }
        protected virtual void OnSelectedBrushChanged(EventArgs e)
        {
            if (SelectedBrushChanged != null)
                SelectedBrushChanged(this, e);
        }

        static readonly Point NullPoint = new Point(-1, -1);
        Point selectedCell = NullPoint;
        public event EventHandler SelectedCellChanged;
        public Point SelectedCell
        {
            get
            {
                return selectedCell;
            }
            set
            {
                selectedCell = value;
                OnSelectedCellChanged(EventArgs.Empty);

                Refresh();
            }
        }
        protected virtual void OnSelectedCellChanged(EventArgs e)
        {
            if (SelectedCellChanged != null)
                SelectedCellChanged(this, e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            Point cell = PixelToCell(e.X, e.Y, edge, edge, Padding.Top, Padding.Left, Height - Padding.Top - Padding.Bottom, Width - Padding.Left - Padding.Right);

            if (selectedCell == cell)
                SelectedCell = NullPoint;
            else
                SelectedCell = cell;

            base.OnMouseDown(e);
        }

        static Point PixelToCell(int x, int y, int gridHeight, int gridWidth, int top, int left, int height, int width)
        {
            x -= left;
            y -= top;
            if (x < 0)
                x -= left;
            if (y < 0)
                y -= top;

            return new Point(x * gridWidth / width, y * gridHeight / height);
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            Point selectedCell = this.selectedCell;
            int height = edge, width = edge;

            if (-1 < selectedCell.X && selectedCell.X < width && -1 < selectedCell.Y && selectedCell.Y < height)
            {
                CharSet pressed = new CharSet(e.KeyChar);
                CharSet cell = new CharSet(this[selectedCell.Y, selectedCell.X]);
                this[selectedCell.Y, selectedCell.X] = (cell ^ pressed).ToString();
            }

            base.OnKeyPress(e);
        }

        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            Refresh();
        }

        static void PaintGrid(Graphics g, Brush oddBrush, Brush evenBrush, Brush selectedBrush, Point selectedCell, Brush textBrush, Font textFont, StringFormat textFormat, Pen gridPen, string[,] array, int subgridHeight, int subgridWidth, int top, int left, int bottom, int right, int height, int width)
        {
            width--;
            height--;
            right--;
            bottom--;

            int row, col, edge = subgridHeight * subgridWidth;
            float dx = (float)width / subgridWidth, dy = (float)height / subgridHeight, x, y;
            string cellText;

            if (oddBrush != null && evenBrush != null)
            {
                for (row = 0; row < subgridHeight; row++)
                {
                    y = top + row * dy;
                    for (col = 0; col < subgridWidth; col++)
                    {
                        x = left + col * dx;
                        if (((row + col) & 1) == 1)
                            g.FillRectangle(oddBrush, new RectangleF(x, y, dx, dy));
                        else
                            g.FillRectangle(evenBrush, new RectangleF(x, y, dx, dy));
                    }
                }
            }

            dx = (float)width / edge;
            dy = (float)height / edge;
            if (selectedBrush != null && -1 < selectedCell.X && selectedCell.X < edge && -1 < selectedCell.Y && selectedCell.Y < edge)
            {
                x = left + selectedCell.X * dx;
                y = top + selectedCell.Y * dy;
                g.FillRectangle(selectedBrush, new RectangleF(x, y, dx, dy));
            }

            if (textFont != null && textBrush != null)
            {
                for (row = 0; row < edge; row++)
                {
                    y = top + row * dy;
                    for (col = 0; col < edge; col++)
                    {
                        x = left + col * dx;
                        cellText = array[row, col];
                        if (cellText != null)
                            g.DrawString(cellText, textFont, textBrush, new RectangleF(x, y, dx, dy), textFormat);
                    }
                }
            }

            if (gridPen != null)
            {
                for (col = 0; col <= edge; col++)
                {
                    x = left + col * dx;
                    g.DrawLine(gridPen, new PointF(x, top), new PointF(x, bottom));
                }
                for (row = 0; row <= edge; row++)
                {
                    y = top + row * dy;
                    g.DrawLine(gridPen, new PointF(left, y), new PointF(right, y));
                }
            }
        }
    }
}