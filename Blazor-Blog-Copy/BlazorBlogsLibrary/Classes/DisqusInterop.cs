using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorBlogsLibrary.Classes
{
    public static class DisqusInterop
    {
        internal static ValueTask<object> CreateDisqus(
            IJSRuntime jsRuntime,
            ElementReference disqusThreadElement,
            string disqusSrc)
        {
            return jsRuntime.InvokeAsync<object>(
                "DisqusFunctions.createDisqus",
                disqusThreadElement,
                disqusSrc);
        }

        internal static ValueTask<object> ResetDisqus(
            IJSRuntime jsRuntime,
            string newIdentifier,
            string newUrl,
            string newTitle)
        {
            return jsRuntime.InvokeAsync<object>(
                "DisqusFunctions.resetDisqus",
                newIdentifier,
                newUrl,
                newTitle);
        }
    }
}