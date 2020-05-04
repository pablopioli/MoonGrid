using System.Threading.Tasks;

namespace MoonGrid
{
    public class EventCallbacks
    {
        public delegate Task FilterStatusChange(bool isActive);
        public delegate Task StatusHasChanged();

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
