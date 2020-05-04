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
        [Parameter] public EventCallbacks EventCallbacks { get; set; }
        [Parameter] public ICollection<OrderOption> OrderOptions { get; set; } = Array.Empty<OrderOption>();
        [Parameter] public ICollection<ActionButton> ActionButtons { get; set; } = Array.Empty<ActionButton>();
        [Parameter] public ICollection<GridColumn<TItem>> Columns { get; set; } = Array.Empty<GridColumn<TItem>>();
        [Parameter] public EventCallback OnNewItem { get; set; }
        [Parameter] public bool ShowAddNewButton { get; set; }
        [Parameter] public string AddNewButtonText { get; set; } = "Nuevo";
        [Parameter] public bool ShowFilterButton { get; set; }
        [Parameter] public bool IsPageable { get; set; } = true;
        [Parameter] public bool Expandable { get; set; }
        [Parameter] public bool ShowTableFooter { get; set; }
        [Parameter] public bool FreezeFirstColumn { get; set; }
        [Parameter] public Func<QueryOptions, Task<QueryResult<TItem>>> DataSource { get; set; }
        [Parameter] public Func<TItem, Task<RenderFragment>> ItemDetails { get; set; }
        [Parameter] public RenderFragment FilterTemplate { get; set; }
        [Parameter] public RenderFragment NoDataTemplate { get; set; }
        [Parameter] public int InitialPageSize { get; set; } = 30;

        private DisplayableItem<TItem>[] Data { get; set; } = Array.Empty<DisplayableItem<TItem>>();
        private QueryOptions QueryOptions { get; set; } = new QueryOptions();
        public bool HasMoreData { get; private set; }
        public bool IsFilterActive { get; private set; }

        public string ActivePageSize
        {
            get { return QueryOptions.PageSize.ToString(); }
            set
            {
                QueryOptions.PageSize = int.Parse(value);
                QueryOptions.PageNumber = 1;
                Dispatcher.CreateDefault().InvokeAsync(async () => await UpdateCurrentData());
            }
        }

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

            if (InitialPageSize == 15)
            {
                QueryOptions.PageSize = 15;
            }
            else if (InitialPageSize == 30)
            {
                QueryOptions.PageSize = 30;
            }
            else if (InitialPageSize == 50)
            {
                QueryOptions.PageSize = 50;
            }
            else if (InitialPageSize == 100)
            {
                QueryOptions.PageSize = 100;
            }
        }

        private async Task EventCallbacks_OnFilterStatusChanged(bool isActive)
        {
            IsFilterActive = isActive;
            await UpdateCurrentData();
        }

        private async Task EventCallbacks_OnStatusHasChanged()
        {
            await UpdateCurrentData();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (OrderOptions.Count > 0)
                {
                    QueryOptions.Order = OrderOptions.First().Id;
                }

                await UpdateCurrentData();
            }
        }

        private async Task UpdateCurrentData()
        {
            var result = await DataSource.Invoke(QueryOptions);

            if (result.ResultData == null)
            {
                Data = Array.Empty<DisplayableItem<TItem>>();
            }
            else
            {
                Data = result.ResultData.Select(x => new DisplayableItem<TItem>(x)).ToArray();
            }

            HasMoreData = result.HasMoreData;
            StateHasChanged();
        }

        private async Task OnSelectOrder(ChangeEventArgs e)
        {
            QueryOptions.Order = e.Value.ToString();
            await UpdateCurrentData();
        }

        private async Task MoveBack(MouseEventArgs e)
        {
            if (QueryOptions.PageNumber > 1)
            {
                QueryOptions.PageNumber--;
                await UpdateCurrentData();
            }
        }

        private async Task MoveNext(MouseEventArgs e)
        {
            if (HasMoreData)
            {
                QueryOptions.PageNumber++;
                await UpdateCurrentData();
            }
        }

        private void ExecuteActionButton(ActionButton actionButton)
        {
            actionButton.Action.Invoke();
        }

        public async Task Refresh()
        {
            await UpdateCurrentData();
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
            var classNames = new StringBuilder("moongrid-cell");

            if (column.Alignment == ColumnAlignment.Center)
            {
                classNames.Append(" text-center");
            }
            else if (column.Alignment == ColumnAlignment.Right)
            {
                classNames.Append(" text-right");
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

        private async Task ExpandItem(DisplayableItem<TItem> item)
        {
            if (ItemDetails != null)
            {
                item.Content = await ItemDetails.Invoke(item.Item);
                item.Expanded = true;
            }
        }

        private void ContractItem(DisplayableItem<TItem> item)
        {
            item.Expanded = false;
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