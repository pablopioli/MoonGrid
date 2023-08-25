namespace MoonGrid.Localization
{
    public class Spanish : ILocalizedSettings
    {
        public virtual string FilterEnabledIcon { get; } = "fas fa-filter";
        public virtual string FilterDisabledIcon { get; } = "fas fa-filter";
        public virtual string FilterButtonText { get; } = "Filtrar";
        public virtual string ItemsPerPage { get; } = "Items por página";
        public virtual string PreviousButtonText { get; } = "Anterior";
        public virtual string NextButtonText { get; } = "Siguiente";
        public virtual string PageNumberText { get; } = "Página {0}";
        public virtual string PageNumberTextWithTotalCount { get; } = "Página {0} de {1}";
        public virtual string LoadMore { get; } = "Mostrar más";
    }
}
