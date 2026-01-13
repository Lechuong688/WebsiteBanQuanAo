using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebBanQuanAo.Helpers
{
    public static class HMTLHelperExtensions
    {
        public static string IsSelected(
            this IHtmlHelper html,
            string? controller = null,
            string? action = null,
            string cssClass = "active")
        {
            var routeData = html.ViewContext.RouteData;

            var currentController = routeData.Values["controller"]?.ToString();
            var currentAction = routeData.Values["action"]?.ToString();

            bool controllerMatch = string.IsNullOrEmpty(controller)
                || controller.Equals(currentController, StringComparison.OrdinalIgnoreCase);

            bool actionMatch = string.IsNullOrEmpty(action)
                || action.Equals(currentAction, StringComparison.OrdinalIgnoreCase);

            return (controllerMatch && actionMatch) ? cssClass : string.Empty;
        }

        public static string PageClass(this IHtmlHelper html)
        {
            return html.ViewContext.RouteData.Values["action"]?.ToString() ?? "";
        }
    }
}
