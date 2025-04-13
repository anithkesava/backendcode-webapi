using System.ComponentModel.DataAnnotations;

namespace MyFirstWebAPI.Models
{
    public class UserRole
    {
        public string Userrole { get; set; }
    }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Employee = "Employee";
        public const string Manager = "Manager";
    }
}
