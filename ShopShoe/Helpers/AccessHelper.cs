using ShopShoe.Statics;

namespace ShopShoe.Helpers
{
    public static class AccessHelper
    {
        public static bool IsAdmin =>
            CurrentSession.CurrentUser?.RoleId == 1;

        public static bool IsManager =>
            CurrentSession.CurrentUser?.RoleId == 2;

        public static bool IsGuest =>
            CurrentSession.CurrentUser == null;
    }
}
