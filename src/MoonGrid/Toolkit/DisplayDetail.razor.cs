using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace MoonGrid.Toolkit
{
    public partial class DisplayDetail<TItem>
    {
        [CascadingParameter] ActionLauncher<TItem> ActionLauncher { get; set; }
        [CascadingParameter] TItem Row { get; set; }
        [Parameter] public string ClassName { get; set; } = "fas fa-arrow-right";
        [CascadingParameter(Name = "MoonGridOptions")] Dictionary<string, object> MoonGridOptions { get; set; }

        void ShowDetails()
        {
            ActionLauncher.ShowDetails(Row);
        }

        string GetClassName()
        {
            const string KeyName = "MoonGrid.DisplayDetail.Class";

            if (MoonGridOptions != null && MoonGridOptions.ContainsKey(KeyName))
            {
                return MoonGridOptions[KeyName].ToString();
            }

            return ClassName;
    }
}
}
