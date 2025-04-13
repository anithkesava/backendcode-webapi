using System.ComponentModel.DataAnnotations;

namespace MyFirstWebAPI.Models
{
    public static class CurrentUser
    {
        public static string UserName { get; set; }

        public static string Password { get; set; }

        public static string Roles { get; set; }
    }

}
