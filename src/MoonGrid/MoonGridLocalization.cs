namespace MoonGrid
{
    public abstract class MoonGridLocalization
    {
        static MoonGridLocalization()
        {
            Default = new Localization.English();
        }

        public static MoonGridLocalization Default;
        public string FilterEnabledIcon { get; set; } = "fas fa-filter";
        public string FilterDisabledIcon { get; set; } = "fal fa-filter";
        public string FilterButtonText { get; set; } = "Filter";
        public string ItemsPerPage { get; set; } = "Items por página";
        public string PreviousButtonText { get; set; } = "Previous";
        public string NextButtonText { get; set; } = "Next";
        public string PageNumberText { get; set; } = "Page {0}";
    }
}
