namespace MoonGrid.Localization
{
    public class English : ILocalizedSettings
    {
        public virtual string FilterEnabledIcon { get; } = "fas fa-filter";
        public virtual string FilterDisabledIcon { get; } = "fas fa-filter";
        public virtual string FilterButtonText { get; } = "Filter";
        public virtual string ItemsPerPage { get; } = "Items per page";
        public virtual string PreviousButtonText { get; } = "Previous";
        public virtual string NextButtonText { get; } = "Next";
        public virtual string PageNumberText { get; } = "Page {0}";
        public virtual string PageNumberTextWithTotalCount { get; } = "Page {0} of {1}";
        public virtual string LoadMore { get; } = "Load more";
    }
}
