using Microsoft.AspNetCore.Components;
using System;

namespace MoonGrid
{
    public class DisplayableItem<TItem>
    {
        public TItem Item { get; set; }
        public bool Expanded { get; set; }
        public Guid Key { get; set; } = Guid.NewGuid();
        public RenderFragment Content { get; set; }

        public DisplayableItem(TItem item)
        {
            Item = item;
        }
    }
}
