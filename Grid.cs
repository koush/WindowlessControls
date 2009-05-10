//////////////////////////////////////////////////////////////
// Koushik Dutta - 9/1/2007
//////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;

namespace WindowlessControls
{
    public class GridLayout
    {
        public static readonly GridLayout Default = new GridLayout();

        public GridLayout()
        {
        }
        public GridLayout(int row, int column)
        {
            myRow = row;
            myColumn = column;
        }

        int myRow;

        public int Row
        {
            get { return myRow; }
            set { myRow = value; }
        }
        int myColumn;

        public int Column
        {
            get { return myColumn; }
            set { myColumn = value; }
        }
        int myRowSpan = 1;

        public int RowSpan
        {
            get { return myRowSpan; }
            set { myRowSpan = value; }
        }
        int myColumnSpan = 1;

        public int ColumnSpan
        {
            get { return myColumnSpan; }
            set { myColumnSpan = value; }
        }
    }

    public class Grid : WindowlessPanel
    {
        RowDefinitionCollection myRows;
        ColumnDefinitionCollection myColumns;

        public RowDefinitionCollection RowDefinitions
        {
            get
            {
                return myRows;
            }
        }

        public ColumnDefinitionCollection ColumnDefinitions
        {
            get
            {
                return myColumns;
            }
        }

        public Grid()
        {
            myRows = new RowDefinitionCollection(this);
            myColumns = new ColumnDefinitionCollection(this);
        }

        struct PointD
        {
            public double X;
            public double Y;
        }

        PointD[] CalculateDimensions<T>(GridDefinitionCollection<T> definitions, int dimensionLength) where T : GridDefinition
        {
            double totalPixel = 0;
            double totalStar = 0;

            // return an array of points, where X is the start pixel, and Y is the dimension
            PointD[] ret;
            if (definitions.Count == 0)
            {
                ret = new PointD[1];
                ret[0].X = 0;
                ret[0].Y = dimensionLength;
            }
            else
            {
                ret = new PointD[definitions.Count];
            }
            foreach (T t in definitions)
            {
                switch (t.GridUnit)
                {
                    case LayoutUnit.Pixel:
                        totalPixel += t.Measurement;
                        break;
                    case LayoutUnit.Star:
                        totalStar += t.Measurement;
                        break;
                }
            }

            double remaining = dimensionLength - totalPixel;
            if (remaining < 0)
            {
                remaining = 0;
            }

            int i = 0;
            double total = 0;
            foreach (T t in definitions)
            {
                switch (t.GridUnit)
                {
                    case LayoutUnit.Pixel:
                        ret[i].Y = t.Measurement;
                        break;
                    case LayoutUnit.Star:
                        // todo: rounding problems
                        // get a total sum of stuff and figure it out
                        ret[i].Y = remaining * t.Measurement / totalStar;
                        break;
                }
                ret[i].X = total;
                total += ret[i].Y;
                double dim = Math.Round(total, 0) - Math.Round(ret[i].X, 0);
                ret[i].Y = dim;
                i++;
            }

            return ret;
        }

        public override bool MeasureUnpadded(Size bounds, bool boundsChange)
        {
            // build an array that notes where every new row/column starts, and its dimension
            PointD[] rows = CalculateDimensions<RowDefinition>(myRows, bounds.Height);
            PointD[] columns = CalculateDimensions<ColumnDefinition>(myColumns, bounds.Width);

            bool layoutChange = false;
            foreach (IWindowlessControl control in Controls)
            {
                GridLayout layout = control.Layout as GridLayout;
                if (layout == null)
                {
                    layout = GridLayout.Default;
                }

                int width = 0;
                int height = 0;
                for (int i = 0; i < layout.ColumnSpan; i++)
                {
                    width = (int)Math.Round(columns[layout.Column + i].Y, 0);
                }
                for (int i = 0; i < layout.RowSpan; i++)
                {
                    height += (int)Math.Round(rows[layout.Row + i].Y, 0);
                }

                Rectangle oldRect = new Rectangle(control.Left, control.Top, control.Width, control.Height);
                LayoutControl(control, new Rectangle(Margin.Left + (int)Math.Round(columns[layout.Column].X, 0), Margin.Top + (int)Math.Round(rows[layout.Row].X, 0), width, height));
                if (control.NeedsMeasure || boundsChange)
                    control.Measure(new Size(width, height));

                layoutChange |= oldRect != new Rectangle(control.Left, control.Top, control.Width, control.Height);
            }

            ClientWidth = bounds.Width;
            ClientHeight = bounds.Height;

            return layoutChange;
        }
    }



    public class GridDefinition
    {
        public GridDefinition(double measurement, LayoutUnit unit)
        {
            myMeasurement.Measurement = measurement;
            myMeasurement.Unit = unit;
        }

        LayoutMeasurement myMeasurement;

        public LayoutUnit GridUnit
        {
            get
            {
                return myMeasurement.Unit;
            }
        }

        internal double Measurement
        {
            get
            {
                return myMeasurement.Measurement;
            }
        }
    }

    public class RowDefinition : GridDefinition
    {
        public RowDefinition(double height, LayoutUnit unit)
            : base(height, unit)
        {
        }
        public double Height
        {
            get
            {
                return Measurement;
            }
        }
    }

    public class ColumnDefinition : GridDefinition
    {
        public ColumnDefinition(double width, LayoutUnit unit)
            : base(width, unit)
        {
        }
        public double Width
        {
            get
            {
                return Measurement;
            }
        }
    }

    public class GridDefinitionCollection<T> : EncapsulatedList<T> where T : GridDefinition
    {
        Grid myGrid = null;

        internal GridDefinitionCollection(Grid grid)
        {
            myGrid = grid;
        }

        public override void Add(T item)
        {
            base.Add(item);
            myGrid.Remeasure();
        }

        public override void Clear()
        {
            base.Clear();
            myGrid.Remeasure();
        }

        public override bool Remove(T item)
        {
            bool ret = base.Remove(item);
            myGrid.Remeasure();
            return ret;
        }

        public override void Insert(int index, T item)
        {
            base.Insert(index, item);
            myGrid.Remeasure();
        }

        public override void RemoveAt(int index)
        {
            base.RemoveAt(index);
            myGrid.Remeasure();
        }

        public override T this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                base[index] = value;
                myGrid.Remeasure();
            }
        }
    }

    public class RowDefinitionCollection : GridDefinitionCollection<RowDefinition>
    {
        internal RowDefinitionCollection(Grid grid)
            : base(grid)
        {
        }
    }

    public class ColumnDefinitionCollection : GridDefinitionCollection<ColumnDefinition>
    {
        internal ColumnDefinitionCollection(Grid grid)
            : base(grid)
        {
        }
    }
}
