using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
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
        [Parameter] public RenderFragment LoadingTemplate { get; set; }
        [Parameter] public RenderFragment<string> ErrorTemplate { get; set; }
        [Parameter] public RenderFragment DetailsTemplate { get; set; }
        [Parameter] public RenderFragment<TItem> ListViewTemplate { get; set; }
        [Parameter] public RenderFragment<TItem> AdditionalRowTemplate { get; set; }
        [Parameter] public int InitialPageSize { get; set; } = 30;
        [Parameter] public bool CanChangePageSize { get; set; } = true;
        [Parameter] public string TableClass { get; set; } = "";
        [Parameter] public string HeaderClass { get; set; } = "";
        [Parameter] public bool UseResponsiveGrid { get; set; } = true;
        [Parameter] public MoonGridLocalization Localization { get; set; } = MoonGridLocalization.Default;

        [Inject] private IJSRuntime JSRuntime { get; set; }

        public string ErrorText { get; private set; }

        private string Id = Guid.NewGuid().ToString().Replace("-", "");
        private DisplayableItem<TItem>[] Data { get; set; } = Array.Empty<DisplayableItem<TItem>>();
        private TItem CurrentRow;
        private TItem[] FixedData { get; set; } = Array.Empty<TItem>();
        private QueryOptions QueryOptions { get; set; } = new QueryOptions();
        private bool HasMoreData { get; set; }
        private bool IsFilterActive { get; set; }
        private bool Loading { get; set; }
        private ActionLauncher ActionLauncher { get; set; } = new ActionLauncher();
        private bool IsInDetailMode { get; set; }

        private string AnchorToScroll;
        public string ActivePageSize
        {
            get { return QueryOptions.PageSize.ToString(); }
            set
            {
                QueryOptions.PageSize = int.Parse(value);
                QueryOptions.PageNumber = 1;

                Dispatcher.CreateDefault().InvokeAsync(async () =>
                {
                    await UpdateCurrentData();
                    AnchorToScroll = Id + "-pager";
                });
            }
        }

        private IJSObjectReference JsModule;
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();

            JsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/MoonGrid/moongrid.js");

            ActionLauncher.OnShowDetailsRequested += ActionLauncher_OnShowDetailsRequested;
            ActionLauncher.OnShowMasterRequested += ActionLauncher_OnShowMasterRequested;
        }

        private bool hasEventBinded;
        protected override void OnParametersSet()
        {
            base.OnParametersSet();

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
            else
            {
                if (!string.IsNullOrEmpty(AnchorToScroll))
                {
                    await JsModule.InvokeVoidAsync("goToAnchorBottom", AnchorToScroll);
                    AnchorToScroll = null;
                }
            }
        }

        private async Task OnNewButtonActivated()
        {
            await OnNewItem.InvokeAsync(null);
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

        private Task ActionLauncher_OnShowDetailsRequested(object row)
        {
            try
            {
                CurrentRow = (TItem)row;
            }
            catch
            {
                CurrentRow = default;
            }
            IsInDetailMode = true;
            StateHasChanged();
            return Task.CompletedTask;
        }

        private Task ActionLauncher_OnShowMasterRequested()
        {
            IsInDetailMode = false;
            StateHasChanged();
            CurrentRow = default;
            return Task.CompletedTask;
        }

        private async Task UpdateCurrentData()
        {
            if (DataSource == null)
            {
                return;
            }

            if (LoadingTemplate != null)
            {
                Loading = true;
                StateHasChanged();
            }

            var result = await DataSource.Invoke(QueryOptions);

            if (!string.IsNullOrEmpty(result.Error))
            {
                Data = Array.Empty<DisplayableItem<TItem>>();
                HasMoreData = false;
                ErrorText = result.Error;
            }
            else
            {
                ErrorText = "";

                if (result.ResultData == null)
                {
                    Data = Array.Empty<DisplayableItem<TItem>>();
                }
                else
                {
                    Data = result.ResultData.Select(x => new DisplayableItem<TItem>(x)).ToArray();
                }

                HasMoreData = result.HasMoreData;
            }

            if (LoadingTemplate != null)
            {
                Loading = false;
            }

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
                await JsModule.InvokeVoidAsync("goToAnchor", Id);
            }
        }

        private async Task MoveNext(MouseEventArgs e)
        {
            if (HasMoreData)
            {
                QueryOptions.PageNumber++;
                await UpdateCurrentData();
                await JsModule.InvokeVoidAsync("goToAnchor", Id);
            }
        }

        private void ExecuteActionButton(ActionButton actionButton)
        {
            actionButton.Action.Invoke();
        }

        public void NotifyStateHasChanged()
        {
            StateHasChanged();
        }

        public async Task Refresh()
        {
            await UpdateCurrentData();
        }

        public string ComputeHeaderStyle(GridColumn<TItem> column)
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

        public string ComputeStyle(GridColumn<TItem> column, TItem value)
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

            if (column.DynamicStyle != null && value != null)
            {
                var dynamicStyle = column.DynamicStyle(value);
                if (dynamicStyle != null)
                {
                    style.Append(dynamicStyle);
                }
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

        public void SetData(IEnumerable<TItem> data)
        {
            FixedData = data.ToArray();
            DataSource = InternalDataSource;

            Dispatcher.CreateDefault().InvokeAsync(async () => await UpdateCurrentData());
        }

        public bool UpdateItem(TItem item)
        {
            var indexF = Array.IndexOf(FixedData, item);

            var indexD = -1;
            for (int i = 0; i < Data.Length; i++)
            {
                if (ReferenceEquals(Data[i].Item, item))
                {
                    indexD = i;
                    break;
                }
            }

            if (indexF == -1 || indexD == -1)
            {
                return false;
            }

            FixedData[indexF] = item;
            Data[indexD].Key = Guid.NewGuid();

            StateHasChanged();

            return true;
        }

        private Task<QueryResult<TItem>> InternalDataSource(QueryOptions queryOptions)
        {
            var result = new QueryResult<TItem>();

            var orderOption = OrderOptions.Where(x => x.Id == queryOptions.Order).FirstOrDefault();

            Func<IEnumerable<TItem>, IOrderedEnumerable<TItem>> orderFunction = null;
            if (orderOption != null)
            {
                var genericOrderOption = orderOption as OrderOption<TItem>;
                if (genericOrderOption != null)
                {
                    orderFunction = genericOrderOption.OrderFunction;
                }
            }

            IEnumerable<TItem> orderedData;
            if (orderFunction != null)
            {
                orderedData = orderFunction.Invoke(FixedData);
            }
            else
            {
                orderedData = FixedData;
            }

            var pagedData = orderedData.Skip((queryOptions.PageNumber - 1) * queryOptions.PageSize).Take(queryOptions.PageSize + 1).ToArray();

            result.ResultData = pagedData.Take(queryOptions.PageSize).ToArray();
            result.HasMoreData = pagedData.Length == queryOptions.PageSize + 1;

            return Task.FromResult(result);
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