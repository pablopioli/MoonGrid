using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace MoonGrid
{
    public partial class Grid<TItem> : ComponentBase, IDisposable
    {
        [Parameter] public EventCallbacks EventCallbacks { get; set; }
        [Parameter] public ICollection<OrderOption> OrderOptions { get; set; } = Array.Empty<OrderOption>();
        [Parameter] public ICollection<ActionButton> ActionButtons { get; set; } = Array.Empty<ActionButton>();
        [Parameter] public ICollection<GridColumn<TItem>> Columns { get; set; } = Array.Empty<GridColumn<TItem>>();
        [Parameter] public RenderFragment ActionToolbar { get; set; }
        [Parameter] public EventCallback OnNewItem { get; set; }
        [Parameter] public bool ShowAddNewButton { get; set; }
        [Parameter] public string AddNewButtonText { get; set; } = "Nuevo";
        [Parameter] public bool ShowFilterButton { get; set; }
        [Parameter] public bool IsPageable { get; set; } = true;
        [Parameter] public PagingStyle PagingStyle { get; set; } = PagingStyle.Buttons;
        [Parameter] public bool Expandable { get; set; }
        [Parameter] public bool ShowTableHeader { get; set; } = true;
        [Parameter] public bool ShowTableFooter { get; set; }
        [Parameter] public bool FreezeFirstColumn { get; set; }
        [Parameter] public Func<QueryOptions<TItem>, Task<QueryResult<TItem>>> DataSource { get; set; }
        [Parameter] public Func<TItem, Task<RenderFragment>> ItemDetails { get; set; }
        [Parameter] public RenderFragment FilterTemplate { get; set; }
        [Parameter] public RenderFragment NoDataTemplate { get; set; }
        [Parameter] public RenderFragment LoadingTemplate { get; set; }
        [Parameter] public RenderFragment<string> ErrorTemplate { get; set; }
        [Parameter] public RenderFragment DetailsTemplate { get; set; }
        [Parameter] public RenderFragment<TItem> ListViewTemplate { get; set; }
        [Parameter] public RenderFragment<TItem> AdditionalRowTemplate { get; set; }
        [Parameter] public RenderFragment NewButtonTemplate { get; set; }
        [Parameter] public bool CanChangePageSize { get; set; } = false;
        [Parameter] public string TableClass { get; set; } = "";
        [Parameter] public string HeaderClass { get; set; } = "";
        [Parameter] public bool UseResponsiveGrid { get; set; } = true;
        [Parameter] public bool AsynchronousLoading { get; set; } = false;
        [Parameter] public MoonGridLocalization Localization { get; set; } = MoonGridLocalization.Default;
        [Parameter] public IEnumerable<TItem> DataItems { get; set; }
        [Parameter] public string CellClass { get; set; } = "";
        [Parameter] public bool SmallButtons { get; set; }
        [Parameter] public int InitialPageSize { get; set; }
        [Parameter] public string InitialOrder { get; set; }
        [Parameter] public EventCallback<int> OnPageSizeChanged { get; set; }
        [Parameter] public EventCallback<string> OnOrderChanged { get; set; }
        [Parameter] public EventCallback OnRefresh { get; set; }
        [Parameter] public ElementSelector<TItem> AutoSelectElement { get; set; }
        [Parameter] public bool HideInactivePager { get; set; } = true;
        [Parameter] public string ErrorText { get; set; }

        [Inject] private IJSRuntime JSRuntime { get; set; }
        [Inject] private ILogger<Grid<TItem>> Logger { get; set; }

        private readonly string Id = Guid.NewGuid().ToString().Replace("-", "");
        private List<DisplayableItem<TItem>> Data { get; set; } = new List<DisplayableItem<TItem>>();
        private TItem CurrentRow;
        private TItem[] FixedData { get; set; } = Array.Empty<TItem>();
        private bool HasMoreData { get; set; }
        private bool IsFilterActive { get; set; }
        private bool Loading { get; set; }
        private ActionLauncher ActionLauncher { get; set; } = new ActionLauncher();
        private bool IsInDetailMode { get; set; }
        private IEnumerable<TItem> UsedDataItems { get; set; }

        private string AnchorToScrollTop;
        private string AnchorToScrollBottom;

        private int _pageNumber = 1;
        private bool _addingItems;
        private List<int> PageSizes { get; } = new List<int> { 15, 30, 50, 100 };

        private int _activePageSize = 30;
        public string ActivePageSize
        {
            get
            {
                return _activePageSize.ToString();
            }
            set
            {
                var pageSize = int.Parse(value);
                if (pageSize != _activePageSize)
                {
                    _activePageSize = pageSize;

                    Dispatcher.CreateDefault().InvokeAsync(async () =>
                    {
                        _pageNumber = 1;

                        await UpdateCurrentData();
                        await OnPageSizeChanged.InvokeAsync(_activePageSize);

                        AnchorToScrollBottom = Id + "-pager";
                    });
                }
            }
        }

        private string _activeOrder = "";
        public string ActiveOrder
        {
            get
            {
                return _activeOrder.ToString();
            }
            set
            {
                if (!string.Equals(_activeOrder, value))
                {
                    _activeOrder = value;

                    Dispatcher.CreateDefault().InvokeAsync(async () =>
                    {
                        await UpdateCurrentData();
                        await OnOrderChanged.InvokeAsync(_activeOrder);
                    });
                }
            }
        }

        private bool hasEventBinded;
        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (InitialPageSize > 0)
            {
                if (!PageSizes.Contains(InitialPageSize))
                {
                    PageSizes.Add(InitialPageSize);
                }

                _activePageSize = InitialPageSize;
            }

            if (string.IsNullOrEmpty(InitialOrder))
            {
                if (OrderOptions.Count >= 1)
                {
                    _activeOrder = OrderOptions.First().Id;
                }
            }
            else
            {
                _activeOrder = InitialOrder;
            }

            ActionLauncher.OnShowDetailsRequested += ActionLauncher_OnShowDetailsRequested;
            ActionLauncher.OnShowMasterRequested += ActionLauncher_OnShowMasterRequested;

            if (EventCallbacks != null)
            {
                EventCallbacks.OnFilterStatusChanged += EventCallbacks_OnFilterStatusChanged;
                EventCallbacks.OnStatusHasChanged += EventCallbacks_OnStatusHasChanged;
                hasEventBinded = true;
            }
        }

        private IJSObjectReference JsModule;
        private bool _needsLoadData = true;
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await CreateJsModule();

            if (_needsLoadData)
            {
                await UpdateCurrentData();
            }
        }

        private async Task CreateJsModule()
        {
            JsModule = await JSRuntime.InvokeAsync<IJSObjectReference>("import", "/_content/MoonGrid/moongrid.js");
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();

            if (DataItems != null && !ReferenceEquals(DataItems, UsedDataItems))
            {
                SetData(DataItems);
                UsedDataItems = DataItems;
            }
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && AutoSelectElement != null && AutoSelectElement.CanSelectElement)
            {
                await BringElementIntoView(AutoSelectElement.Selector, AutoSelectElement.Expand);
                _needsLoadData = false;
            }

            try
            {
                if (!string.IsNullOrEmpty(AnchorToScrollTop))
                {
                    if (JsModule == null)
                    {
                        await CreateJsModule();
                    }

                    await JsModule.InvokeVoidAsync("goToAnchor", AnchorToScrollTop);
                    AnchorToScrollTop = null;
                }

                if (!string.IsNullOrEmpty(AnchorToScrollBottom))
                {
                    if (JsModule == null)
                    {
                        await CreateJsModule();
                    }

                    await JsModule.InvokeVoidAsync("goToAnchorBottom", AnchorToScrollBottom);
                    AnchorToScrollBottom = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task OnNewButtonActivated()
        {
            await OnNewItem.InvokeAsync(null);
        }

        private async Task EventCallbacks_OnFilterStatusChanged(bool isActive)
        {
            IsFilterActive = isActive;

            if (!isActive)
            {
                try
                {
                    await JsModule.InvokeVoidAsync("collapseFilter", "collapse-" + Id);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex.ToString());
                }
            }

            await UpdateCurrentData();
            StateHasChanged();
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

        private async Task UpdateCurrentData(bool addingItems = false)
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

            var queryOptions = CreateQueryOptions();
            if (AsynchronousLoading)
            {
                queryOptions.CallBack = ProcessCallbackResult;
            }

            _addingItems = addingItems;

            var result = await DataSource.Invoke(queryOptions);

            if (!AsynchronousLoading)
            {
                UpdateUiWithResult(result);
            }
        }

        QueryOptions<TItem> CreateQueryOptions()
        {
            return new QueryOptions<TItem>
            {
                Order = _activeOrder,
                PageNumber = _pageNumber,
                PageSize = _activePageSize
            };
        }

        private void ProcessCallbackResult(QueryResult<TItem> result)
        {
            UpdateUiWithResult(result);
        }

        private void UpdateUiWithResult(QueryResult<TItem> result)
        {
            var addingItems = _addingItems;
            _addingItems = false;

            if (!string.IsNullOrEmpty(result.Error))
            {
                Data = new List<DisplayableItem<TItem>>();
                HasMoreData = false;
                ErrorText = result.Error;
            }
            else
            {
                ErrorText = "";

                if (result.ResultData == null)
                {
                    Data = new List<DisplayableItem<TItem>>();
                }
                else
                {
                    var data = result.ResultData.Select(x => new DisplayableItem<TItem>(x)).ToList();

                    if (addingItems)
                    {
                        Data.AddRange(data);
                    }
                    else
                    {
                        Data = data;
                    }
                }

                HasMoreData = result.HasMoreData;
            }

            if (LoadingTemplate != null)
            {
                Loading = false;
            }

            StateHasChanged();
        }

        private async Task MoveBack(MouseEventArgs e)
        {
            if (_pageNumber > 1)
            {
                _pageNumber--;
                await UpdateCurrentData();
                await JsModule.InvokeVoidAsync("goToAnchor", Id);
            }
        }

        private async Task MoveNext(MouseEventArgs e)
        {
            if (HasMoreData)
            {
                _pageNumber++;
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

        public async Task<bool> Refresh()
        {
            if (PagingStyle == PagingStyle.LoadMore)
            {
                Data = new List<DisplayableItem<TItem>>();
            }

            await UpdateCurrentData();
            await OnRefresh.InvokeAsync();

            return string.IsNullOrEmpty(ErrorText);
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

        public string ComputeClass(GridColumn<TItem> column, string additionalClasses = null)
        {
            var classNames = new StringBuilder("moongrid-cell");

            if (!string.IsNullOrEmpty(additionalClasses))
            {
                classNames.Append(" " + additionalClasses);
            }

            if (column.Alignment == ColumnAlignment.Center)
            {
                classNames.Append(" text-center");
            }
            else if (column.Alignment == ColumnAlignment.Right)
            {
                classNames.Append(" text-right text-end");
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

        public bool AddItem(TItem item)
        {
            return AddItem(item, default(TItem));
        }

        public bool AddItem(TItem item, TItem afterItem)
        {
            var indexF = 0;

            if (afterItem != null)
            {
                indexF = Array.IndexOf(FixedData, afterItem);
                if (indexF == -1)
                {
                    return false;
                }
            }

            var anchorItem = FixedData[indexF];

            var indexD = -1;
            for (int i = 0; i < Data.Count; i++)
            {
                if (ReferenceEquals(Data[i].Item, anchorItem))
                {
                    indexD = i;
                    break;
                }
            }

            if (indexD == -1)
            {
                return false;
            }

            var displayableItem = new DisplayableItem<TItem>(item);

            if (afterItem != null)
            {
                FixedData = FixedData.Take(indexF + 1)
                                 .Concat(new[] { item })
                                 .Concat(FixedData.Skip(indexF + 1))
                                 .ToArray();

                Data = Data.Take(indexD + 1)
                           .Concat(new[] { displayableItem })
                           .Concat(Data.Skip(indexD + 1))
                           .ToList();
            }
            else
            {
                FixedData = new[] { item }.Concat(FixedData).ToArray();

                Data = new[] { displayableItem }.Concat(Data).ToList();
            }

            StateHasChanged();

            return true;
        }

        public bool UpdateItem(TItem item)
        {
            var indexF = Array.IndexOf(FixedData, item);

            var indexD = -1;
            for (int i = 0; i < Data.Count; i++)
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
            Data[indexD].Key = "x-" + Guid.NewGuid().ToString();

            StateHasChanged();

            return true;
        }

        public async Task<bool> RemoveItem(TItem item)
        {
            var indexD = -1;
            for (int i = 0; i < Data.Count; i++)
            {
                if (ReferenceEquals(Data[i].Item, item))
                {
                    indexD = i;
                    break;
                }
            }

            if (indexD == -1)
            {
                return false;
            }

            var indexF = Array.IndexOf(FixedData, item);
            if (indexF != -1)
            {
                FixedData = FixedData.Take(indexF)
                                     .Concat(FixedData.Skip(indexF + 1))
                                     .ToArray();
            }

            Data = Data.Take(indexD)
                       .Concat(Data.Skip(indexD + 1))
                       .ToList();

            if (Data.Count == 0)
            {
                await Refresh();
            }

            StateHasChanged();

            return true;
        }

        private Task<QueryResult<TItem>> InternalDataSource(QueryOptions<TItem> queryOptions)
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

            if (IsPageable)
            {
                var pagedData = orderedData.Skip((queryOptions.PageNumber - 1) * queryOptions.PageSize).Take(queryOptions.PageSize + 1).ToArray();
                result.ResultData = pagedData.Take(queryOptions.PageSize).ToArray();
                result.HasMoreData = pagedData.Length == queryOptions.PageSize + 1;
            }
            else
            {
                result.ResultData = orderedData.ToArray();
                result.HasMoreData = false;
            }

            return Task.FromResult(result);
        }

        public async Task<bool> BringElementIntoView(Func<TItem, bool> selector)
        {
            return await BringElementIntoView(selector, false);
        }

        public async Task<bool> BringElementIntoView(Func<TItem, bool> selector, bool expand)
        {
            if (AsynchronousLoading)
            {
                return false;
            }

            var existingItem = Data.FirstOrDefault(x => selector(x.Item));
            if (existingItem != null)
            {
                AnchorToScrollTop = existingItem.Key;
            }

            var queryOptions = CreateQueryOptions();
            try
            {
                queryOptions.PageNumber = 1;
                _pageNumber = 1;
                var found = false;

                while (true)
                {
                    var result = await DataSource.Invoke(queryOptions);

                    if (!string.IsNullOrEmpty(result.Error))
                    {
                        UpdateUiWithResult(result);
                        break;
                    }

                    var element = result.ResultData.FirstOrDefault(x => selector(x));
                    if (element != null)
                    {
                        found = true;
                        UpdateUiWithResult(result);

                        if (expand)
                        {
                            await ExpandElement(selector);
                        }

                        var processedItem = Data.FirstOrDefault(x => ReferenceEquals(x.Item, element));
                        if (processedItem != null)
                        {
                            AnchorToScrollTop = processedItem.Key;
                        }

                        StateHasChanged();

                        break;
                    }

                    if (!result.HasMoreData)
                    {
                        queryOptions.PageNumber = 1;
                        _pageNumber = 1;

                        result = await DataSource.Invoke(queryOptions);
                        UpdateUiWithResult(result);
                        break;
                    }

                    queryOptions.PageNumber++;
                    _pageNumber++;
                }

                return found;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                return false;
            }
        }

        public async Task<bool> ExpandElement(Func<TItem, bool> selector)
        {
            var item = Data.FirstOrDefault(x => selector(x.Item));

            if (item == null)
            {
                return false;
            }

            await ExpandItem(item);
            StateHasChanged();

            return true;
        }

        public async Task LoadMore()
        {
            if (HasMoreData)
            {
                _pageNumber++;
                await UpdateCurrentData(true);
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            ActionLauncher.OnShowDetailsRequested -= ActionLauncher_OnShowDetailsRequested;
            ActionLauncher.OnShowMasterRequested -= ActionLauncher_OnShowMasterRequested;

            if (hasEventBinded && EventCallbacks != null)
            {
                EventCallbacks.OnFilterStatusChanged -= EventCallbacks_OnFilterStatusChanged;
                EventCallbacks.OnStatusHasChanged -= EventCallbacks_OnStatusHasChanged;
                hasEventBinded = false;
            }
        }

        ~Grid()
        {
            Dispose(disposing: false);
        }
    }
}