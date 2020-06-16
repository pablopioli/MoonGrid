using System;
using System.Collections.Generic;
using System.Linq;

namespace MoonGrid
{
    public class OrderOption
    {
        public string Id { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;

        public OrderOption()
        { }

        public OrderOption(string id, string displayText)
        {
            Id = id;
            DisplayText = displayText;
        }
    }

    public class OrderOption<TItem> : OrderOption
    {
        public Func<IEnumerable<TItem>, IOrderedEnumerable<TItem>> OrderFunction { get; set; }

        public OrderOption(string id, string displayText, Func<IEnumerable<TItem>, IOrderedEnumerable<TItem>> orderFunction)
        {
            Id = id;
            DisplayText = displayText;
            OrderFunction = orderFunction;
        }
    }
}
