using Microsoft.AspNetCore.Components;

namespace MoonGrid.Toolkit
{
    public partial class DisplayDetail<TItem>
    {
        [CascadingParameter] ActionLauncher ActionLauncher { get; set; }
        [CascadingParameter] TItem Row { get; set; }

        void ShowDetails()
        {
            ActionLauncher.ShowDetails(Row);
        }
    }
}
