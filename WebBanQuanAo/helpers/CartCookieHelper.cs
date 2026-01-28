using System.Text.Json;
using WebBanQuanAo.Models;

namespace WebBanQuanAo.helpers
{
    public static class CartCookieHelper
    {
        private const string CART_COOKIE = "SHOP_CART";

        public static List<CartItemCookie> GetCart(HttpRequest request)
        {
            var json = request.Cookies[CART_COOKIE];
            if (string.IsNullOrEmpty(json))
                return new List<CartItemCookie>();

            return JsonSerializer.Deserialize<List<CartItemCookie>>(json)
                   ?? new List<CartItemCookie>();
        }

        public static void SaveCart(HttpResponse response, List<CartItemCookie> cart)
        {
            var json = JsonSerializer.Serialize(cart);

            response.Cookies.Append(
                CART_COOKIE,
                json,
                new CookieOptions
                {
                    HttpOnly = true,
                    Expires = DateTimeOffset.Now.AddDays(7)
                }
            );
        }

        public static void ClearCart(HttpResponse response)
        {
            response.Cookies.Delete(CART_COOKIE);
        }
    }
}
