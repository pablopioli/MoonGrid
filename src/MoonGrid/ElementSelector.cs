using System;

namespace MoonGrid
{
    public class ElementSelector<T>
    {
        public bool CanSelectElement { get; set; }
        public Func<T, bool> Selector { get; set; }
        public bool Expand { get; set; }
    }
}
