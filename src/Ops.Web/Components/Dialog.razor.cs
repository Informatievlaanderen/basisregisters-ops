namespace Ops.Web.Components
{
    using Microsoft.AspNetCore.Components;

    public partial class Dialog
    {
        [Parameter] public string Caption { get; set; }
        [Parameter] public string Message { get; set; }
        [Parameter] public string SubmitButtonText { get; set; } = "Ok";
        [Parameter] public EventCallback<bool> OnClose { get; set; }

        private Task Cancel()
        {
            return OnClose.InvokeAsync(false);
        }

        private async Task Ok()
        {
            await OnClose.InvokeAsync(true);
        }
    }
}
