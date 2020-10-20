using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace MoonGrid
{
    public class ActionLauncher : ComponentBase
    {
        internal delegate Task ShowMasterRequested();
        internal delegate Task ShowDetailsRequested(object row);

        internal event ShowMasterRequested OnShowMasterRequested;
        internal event ShowDetailsRequested OnShowDetailsRequested;

        public virtual void ShowMaster()
        {
            OnShowMasterRequested?.Invoke();
        }

        public virtual void ShowDetails(object row)
        {
            OnShowDetailsRequested?.Invoke(row);
        }
    }
}
