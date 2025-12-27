using Microsoft.AspNetCore.Components;

namespace Frontend.Component.Shared
{
    public abstract partial class DataLoaderBase
    {
        [Parameter] public bool DisplaySpinner { get; set; } = true;
        [Parameter] public bool DisplayAnyway { get; set; } = false;

        [Parameter] public RenderFragment? Loading { get; set; }
        [Parameter] public RenderFragment? Loaded { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }

        private bool _isLoading = true;
        private List<string> _errors = [];

        protected void StartLoading()
        {
            _errors = [];
            _isLoading = true;
            StateHasChanged();
        }
        protected void FinishLoading(List<string> errors)
        {
            _errors = errors;
            _isLoading = false;
            StateHasChanged();
        }
    }
}
