using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace UrlStore.Helpers
{
    public static class ExtensionMethods
    {
        public static string GetUserId(this ClaimsPrincipal user)
        {
            if (!user.Identity.IsAuthenticated) return null;
            ClaimsPrincipal currentUser = user;
            return currentUser.FindFirst(ClaimTypes.NameIdentifier).Value;
        }

        //public static async Task<string> GetUserImage(this ClaimsPrincipal user)
        //{
        //    var userId = GetUserId(user);
        //    var context = new ApplicationDbContext()
        //}
    }
}
