using System.ComponentModel.DataAnnotations;

namespace MyFirstWebAPI.Models
{
    public class Users
    {
        [Key]
        public int Id { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public string Roles { get; set; }
    }
}
