using Microsoft.AspNetCore.Components;
using System;

namespace MoonGrid
{
    public class GridColumn<T> : ComponentBase
    {
        public string Title { get; set; }
        public Func<T, string> Source { get; set; }
        public Type Template { get; set; }
        public ColumnAlignment Alignment { get; set; } = ColumnAlignment.Left;
        public ColumnWidth MinWidth { get; set; }
        public ColumnWidth MaxWidth { get; set; }

        public GridColumn()
        { }

        public GridColumn(string title, Func<T, string> source)
        {
            Title = title;
            Source = source;
        }

        public GridColumn(string title, Func<T, string> source, ColumnWidth minWidth,
            ColumnWidth maxWidth = null, ColumnAlignment alignment = ColumnAlignment.Left)
        {
            Title = title;
            Source = source;
            MinWidth = minWidth;

            if (maxWidth != null)
            {
                MaxWidth = maxWidth;
            }

            Alignment = alignment;
        }
    }
}
