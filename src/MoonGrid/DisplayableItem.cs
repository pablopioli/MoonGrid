using Microsoft.AspNetCore.Components;

namespace MoonGrid
{
    public class DisplayableItem<TItem>
    {
        public TItem Item { get; set; }
        public bool Expanded { get; set; }
        public RenderFragment Content { get; set; }

        public DisplayableItem(TItem item)
        {
            Item = item;
        }
    }
}
