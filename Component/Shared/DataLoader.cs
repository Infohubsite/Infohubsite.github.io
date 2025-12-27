using Microsoft.AspNetCore.Components;

namespace Frontend.Component.Shared
{
    public class DataLoader : DataLoaderBase
    {
        [Parameter, EditorRequired] public Func<bool, Task<List<string>>> LoadAction { get; set; }

        public Task Refresh() => ExecuteLoad(true);

        protected override Task OnInitializedAsync() => ExecuteLoad(false);

        private async Task ExecuteLoad(bool refresh)
        {
            StartLoading();
            try
            {
                FinishLoading(await LoadAction(refresh));
            }
            catch (Exception ex)
            {
                FinishLoading([$"Error while loading: {ex.Message}"]);
            }
        }
    }

    public class DataLoaderT<TArg> : DataLoaderBase
    {
        [Parameter, EditorRequired] public Func<bool, TArg?, Task<List<string>>> LoadAction { get; set; }

        [Parameter] public TArg? InitialArg { get; set; }

        public Task Refresh(TArg? arg) => ExecuteLoad(true, arg);

        protected override Task OnInitializedAsync() => ExecuteLoad(false, InitialArg);

        private async Task ExecuteLoad(bool refresh, TArg? arg)
        {
            StartLoading();
            try
            {
                FinishLoading(await LoadAction(refresh, arg));
            }
            catch (Exception ex)
            {
                FinishLoading([$"Error while loading: {ex.Message}"]);
            }
        }
    }
}
