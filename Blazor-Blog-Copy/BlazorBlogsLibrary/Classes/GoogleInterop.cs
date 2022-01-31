using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorBlogsLibrary.Classes
{
    public static class GoogleInterop
    {
        internal static ValueTask<object> gaTracking(
            IJSRuntime jsRuntime,
            string gaTrackingID)
        {
            return jsRuntime.InvokeAsync<object>(
                "gaFunctions.gaTracking",
                gaTrackingID);
        }
    }
}