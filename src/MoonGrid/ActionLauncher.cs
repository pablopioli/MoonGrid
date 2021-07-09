using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace MoonGrid
{
    public class ActionLauncher<TItem> : ComponentBase
    {
        internal delegate Task ShowMasterRequested(Func<IEnumerable<TItem>, TItem> selector);
        internal delegate Task ShowDetailsRequested(TItem row);

        internal event ShowMasterRequested OnShowMasterRequested;
        internal event ShowDetailsRequested OnShowDetailsRequested;

        public virtual void ShowMaster(Func<IEnumerable<TItem>, TItem> selector = null)
        {
            OnShowMasterRequested?.Invoke(selector);
        }

        public virtual void ShowDetails(TItem row)
        {
            OnShowDetailsRequested?.Invoke(row);
        }
    }
}
