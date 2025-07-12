using Microsoft.AspNetCore.Mvc;
using MyFirstWebAPI;
using MyFirstWebAPI.EntityDB;
using MyFirstWebAPI.Models;
namespace MyFirstWebAPI.DesignPattern.ProductPattern
{
    //User Repository 
    public interface IUserRepository
    {
        bool UserExists(AppDbContext appDbContext, Users users);
        bool IsValidUsers(AppDbContext appDbContext, Users users);
        void AddUsers(AppDbContext appDbContext, Users users);
    }
    public class UserRepository : IUserRepository
    {
        public bool UserExists(AppDbContext appDbContext, Users users)
        {
            return appDbContext.UsersData.Any(x => x.UserName == users.UserName);
        }
        public void AddUsers(AppDbContext appDbContext, Users users)
        {
            appDbContext.UsersData.Add(users);
            appDbContext.SaveChanges();
        }
        public bool IsValidUsers(AppDbContext appDbContext, Users users)
        {
            return appDbContext.UsersData.Any(x => x.UserName == users.UserName && x.Password == users.Password
            && x.Roles == users.Roles);
        }
    }
}
