namespace MoonGrid;

public class HeaderGroup
{
    public string Title { get; set; } = "";
    public int ColumnCount { get; set; }

    public HeaderGroup()
    { }

    public HeaderGroup(string title, int columnCount)
    {
        Title = title;
        ColumnCount = columnCount;
    }
}
