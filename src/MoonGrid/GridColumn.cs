using System;
using Microsoft.AspNetCore.Components;

namespace MoonGrid
{
    public class GridColumn<T> : ComponentBase
    {
        public string Title { get; set; }
        public Type TitleTemplate { get; set; }
        public Func<T, string> Source { get; set; }
        public Type Template { get; set; }
        public Func<T, RenderFragment> FragmentBuilder { get; set; }
        public string CssClass { get; set; } = string.Empty;
        public ColumnAlignment Alignment { get; set; } = ColumnAlignment.Left;
        public ColumnWidth MinWidth { get; set; }
        public ColumnWidth MaxWidth { get; set; }
        public Func<T, string> DynamicStyle { get; set; }
        public object CustomData { get; set; }

        public GridColumn()
        { }

        public GridColumn(string title, Func<T, string> source)
        {
            Title = title;
            Source = source;
        }

        public GridColumn(string title,
            Func<T, string> source, ColumnWidth minWidth,
            ColumnWidth maxWidth = null,
            ColumnAlignment alignment = ColumnAlignment.Left,
            Func<T, string> dynamicStyle = null)
        {
            Title = title;
            Source = source;
            MinWidth = minWidth;
            DynamicStyle = dynamicStyle;

            if (maxWidth != null)
            {
                MaxWidth = maxWidth;
            }

            Alignment = alignment;
        }
    }
}
