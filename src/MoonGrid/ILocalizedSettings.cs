namespace MoonGrid;

public interface ILocalizedSettings
{
    string FilterEnabledIcon { get; }
    string FilterDisabledIcon { get; }
    string FilterButtonText { get; }
    string ItemsPerPage { get; }
    string PreviousButtonText { get; }
    string NextButtonText { get; }
    string PageNumberText { get; }
    string PageNumberTextWithTotalCount { get; }
    string LoadMore { get; }

}
