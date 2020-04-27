using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoonGrid
{
    public partial class Grid<TItem> : ComponentBase, IDisposable
    {
        [Parameter]
        public EventCallbacks EventCallbacks { get; set; }

        [Parameter]
        public ICollection<OrderOption> OrderOptions { get; set; } = Array.Empty<OrderOption>();

        [Parameter]
        public ICollection<ActionButton> ActionButtons { get; set; } = Array.Empty<ActionButton>();

        [Parameter]
        public ICollection<GridColumn<TItem>> Columns { get; set; } = Array.Empty<GridColumn<TItem>>();

        [Parameter]
        public EventCallback OnNewItem { get; set; }

        [Parameter]
        public bool ShowAddNewButton { get; set; }

        [Parameter]
        public string AddNewButtonText { get; set; } = "Nuevo";

        [Parameter]
        public bool ShowFilterButton { get; set; }

        [Parameter]
        public bool IsPageable { get; set; } = true;

        [Parameter]
        public bool ShowTableFooter { get; set; }
        [Parameter]
        public bool FreezeFirstColumn { get; set; }

        [Parameter]
        public Func<QueryOptions, QueryResult<TItem>> DataSource { get; set; }

        [Parameter]
        public RenderFragment FilterTemplate { get; set; }

        [Parameter]
        public RenderFragment NoDataTemplate { get; set; }

        private TItem[] Data { get; set; }
        private QueryOptions QueryOptions { get; set; } = new QueryOptions();
        public bool HasMoreData { get; private set; }
        public bool IsFilterActive { get; private set; }

        private async Task OnNewButtonActivated()
        {
            await OnNewItem.InvokeAsync(null);
        }

        private bool hasEventBinded;

        protected override void OnParametersSet()
        {
            if (!hasEventBinded && EventCallbacks != null)
            {
                EventCallbacks.OnFilterStatusChanged += EventCallbacks_OnFilterStatusChanged;
                EventCallbacks.OnStatusHasChanged += EventCallbacks_OnStatusHasChanged;
                hasEventBinded = true;
            }
        }

        private void EventCallbacks_OnFilterStatusChanged(bool isActive)
        {
            IsFilterActive = isActive;
            UpdateCurrentData();
        }

        private void EventCallbacks_OnStatusHasChanged()
        {
            UpdateCurrentData();
        }

        protected override void OnInitialized()
        {
            if (OrderOptions.Count > 0)
            {
                QueryOptions.Order = OrderOptions.First().Id;
            }

            UpdateCurrentData();
        }

        private void UpdateCurrentData()
        {
            var result = DataSource.Invoke(QueryOptions);

            if (result.ResultData == null)
            {
                Data = Array.Empty<TItem>();
            }
            else
            {
                Data = result.ResultData;
            }

            HasMoreData = result.HasMoreData;
            StateHasChanged();
        }

        private void OnSelectOrder(ChangeEventArgs e)
        {
            QueryOptions.Order = e.Value.ToString();
            UpdateCurrentData();
        }

        private void OnSelectPageSize(ChangeEventArgs e)
        {
            QueryOptions.PageSize = int.Parse(e.Value.ToString());
            QueryOptions.PageNumber = 1;
            UpdateCurrentData();
        }

        private void MoveBack(MouseEventArgs e)
        {
            if (QueryOptions.PageNumber > 1)
            {
                QueryOptions.PageNumber--;
                UpdateCurrentData();
            }
        }

        private void MoveNext(MouseEventArgs e)
        {
            if (HasMoreData)
            {
                QueryOptions.PageNumber++;
                UpdateCurrentData();
            }
        }

        private void ExecuteActionButton(ActionButton actionButton)
        {
            actionButton.Action.Invoke();
        }

        public void Refresh()
        {
            UpdateCurrentData();
        }

        public string ComputeStyle(GridColumn<TItem> column)
        {
            var style = new StringBuilder();

            if (column.MinWidth != null)
            {
                style.Append($"min-width:{column.MinWidth.Width}{GridUnitToText(column.MinWidth.Unit)};");
            }

            if (column.MaxWidth != null)
            {
                style.Append($"max-width:{column.MaxWidth.Width}{GridUnitToText(column.MaxWidth.Unit)};");
            }

            return style.ToString();
        }

        public string ComputeClass(GridColumn<TItem> column)
        {
            var classNames = new StringBuilder();

            if (column.Alignment == ColumnAlignment.Center)
            {
                classNames.Append("text-center");
            }
            else if (column.Alignment == ColumnAlignment.Right)
            {
                classNames.Append("text-right");
            }

            return classNames.ToString();
        }

        private string GridUnitToText(WidthUnit unit)
        {
            switch (unit)
            {
                case WidthUnit.Pixel:
                    return "px";
                case WidthUnit.Percent:
                    return "%";
                case WidthUnit.Rem:
                    return "rem";
                case WidthUnit.Em:
                    return "em";
                default:
                    break;
            }

            return string.Empty;
        }

        private bool hasDisposed;

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!hasDisposed)
            {
                if (hasEventBinded && EventCallbacks != null)
                {
                    EventCallbacks.OnFilterStatusChanged -= EventCallbacks_OnFilterStatusChanged;
                    EventCallbacks.OnStatusHasChanged -= EventCallbacks_OnStatusHasChanged;
                    hasEventBinded = false;
                }

                hasDisposed = true;
            }
        }

        ~Grid()
        {
            Dispose(disposing: false);
        }
    }
}