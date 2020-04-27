namespace MoonGrid
{
    public class ColumnWidth
    {
        public int Width { get; set; }
        public WidthUnit Unit { get; set; } = WidthUnit.Pixel;

        public ColumnWidth()
        { }

        public ColumnWidth(int width, WidthUnit unit)
        {
            Width = width;
            Unit = unit;
        }

        public static ColumnWidth FromPixels(int width)
        {
            return new ColumnWidth(width, WidthUnit.Pixel);
        }

        public static ColumnWidth FromPercent(int width)
        {
            return new ColumnWidth(width, WidthUnit.Percent);
        }

        public static ColumnWidth FromRem(int width)
        {
            return new ColumnWidth(width, WidthUnit.Rem);
        }

        public static ColumnWidth FromEm(int width)
        {
            return new ColumnWidth(width, WidthUnit.Em);
        }
    }
}
