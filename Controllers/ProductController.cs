using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using MyFirstWebAPI.DesignPattern.ProductPattern;
using MyFirstWebAPI.EntityDB;
using MyFirstWebAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MyFirstWebAPI.Controllers;

[ApiController]
[Route("api/product")]
public class ProductController : ControllerBase
{
    private readonly AppDbContext _appDbContext;

    private readonly IUserRepository _userRepository;

    private readonly IConfiguration _config;

    public ProductController(AppDbContext appDbContext,
       IConfiguration config, IUserRepository userRepository)
    {
        this._appDbContext = appDbContext;
        this._config = config;
        this._userRepository = userRepository;
    }

    [Authorize]
    [HttpGet("CurrentUser")]
    public ActionResult Currentuser()
    {
        if (string.IsNullOrWhiteSpace(CurrentUser.UserName))
        {
            return NotFound(new { message = "no user login found" });
        }
        return Ok(new { message = $" UserName: {CurrentUser.UserName}" });
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public ActionResult Login([FromBody] Users users)
    {
        if (_userRepository.IsValidUsers(_appDbContext, users))
        {
            var user = _appDbContext.UsersData.FirstOrDefault(x => x.UserName == users.UserName);
            if (user != null)
            {
                CurrentUser.UserName = user.UserName;
                CurrentUser.Password = user.Password;
                CurrentUser.Roles = user.Roles;
            }
            var Token = GenerateToken(users);
            return Ok(new { token = Token });
        }
        else
        {
            return NotFound();
        }
    }

    [AllowAnonymous]
    [HttpGet]
    public ActionResult GetAllUsers()
    {
        var users = _appDbContext.UsersData.ToList();
        if (users == null)
        {
            return NotFound();
        }
        return Ok(users);
    }

    [NonAction]
    public string GenerateToken(Users user)
    {
        try
        {
            var key = _config["jwt:Key"];
            var Issuer = _config["jwt:Issuer"];
            var Audience = _config["jwt:Audience"];

            if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Audience))
            {
                Console.WriteLine("Some Configuration Cannot be Read from JSon File ");
            }

            var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["jwt:Key"]));
            var credential = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Name,user.UserName),
            new Claim(ClaimTypes.Role ,user.Roles)
            };

            var token = new JwtSecurityToken
                (
                issuer: _config["jwt:Issuer"],
                audience: _config["jwt:Audience"],
                claims: claims,
                signingCredentials: credential,
                expires: DateTime.Now.AddMinutes(30)

                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        catch (Exception e)
        {
            return "The Token Genaration is Fails " + e.Message;
        }
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost("admin")]
    public ActionResult AddUsers([FromBody] Users users)
    {
        bool IsAdmin = CurrentUser.Roles == Roles.Admin;
        if (!IsAdmin)
        {
            return Forbid();
        }
        bool IsExists = _userRepository.UserExists(_appDbContext, users);
        if (IsExists)
        {
            var response = new { user = "User already exists" };
            return Conflict(response);
        }
        _userRepository.AddUsers(_appDbContext, users);
        var message = new { mes = "User Added Succesfully" };
        return Ok(message);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPut("EditUserRole")]
    public ActionResult EditRole([FromBody] UserRole users, string name)
    {
        var userdetails = _appDbContext.UsersData.Where(x => x.UserName == name).FirstOrDefault();

        if (userdetails != null)
        {
            userdetails.Roles = users.Userrole;
            _appDbContext.SaveChanges();
            return Ok(new { message = "User Role has been Changed" });
        }
        else
        {
            return NotFound(new { message = "User not exists" });
        }
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("DeleteUser")]
    public ActionResult DeleteUser(int id)
    {
        var user = _appDbContext.UsersData.FirstOrDefault(x => x.Id == id);
        if (user != null)
        {
            string name = user.UserName;
            _appDbContext.UsersData.Remove(user);
            _appDbContext.SaveChanges();
            return Ok(new { message = $"{name} is removed Successfully" });
        }
        else
        {
            return NotFound(new { message = $"user not exists" });
        }
    }

    [HttpDelete("DeleteAll")]
    [Authorize(Roles = Roles.Admin)]
    public ActionResult DeleteAllUsers()
    {
        var users = _appDbContext.UsersData.ToList();
        if (users.Count > 0)
        {
            foreach (var user in users)
            {
                _appDbContext.UsersData.Remove(user);
            }
            _appDbContext.SaveChanges();
            return Ok(new { message = "all users removed from the Database" });
        }
        else
        {
            return NotFound(new { message = "user does not exists to remove" });
        }
    }

    [Authorize]
    [HttpDelete("Logout")]
    public ActionResult Logout()
    {
        return Ok(new { message = "Succesfully logout" });
    }
}
