using System;

namespace MoonGrid
{
    public class ActionButton
    {
        public string Text { get; set; } = string.Empty;

        public Action Action { get; set; }

        public ActionButton()
        { }

        public ActionButton(string text, Action action)
        {
            Text = text;
            Action = action;
        }
    }
}
