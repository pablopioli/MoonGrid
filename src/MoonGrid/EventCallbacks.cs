namespace MoonGrid
{
    public class EventCallbacks
    {
        public delegate void FilterStatusChange(bool isActive);
        public delegate void StatusHasChanged();

        public event FilterStatusChange OnFilterStatusChanged;
        public event StatusHasChanged OnStatusHasChanged;

        public virtual void RaiseFilterStatusChange(bool isActive)
        {
            OnFilterStatusChanged?.Invoke(isActive);
        }

        public virtual void RaiseStatusHasChanged()
        {
            OnStatusHasChanged?.Invoke();
        }
    }
}
