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
}
