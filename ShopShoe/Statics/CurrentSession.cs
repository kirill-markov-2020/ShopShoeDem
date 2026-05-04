using ShopShoe.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopShoe.Statics
{
    public static class CurrentSession
    {
        public static User CurrentUser { get; set; }
        public static bool IsAdmin => CurrentUser.Role.Name == "Администратор";
        public static bool IsManager => CurrentUser.Role.Name == "Менеджер" || IsAdmin;
    }
}
