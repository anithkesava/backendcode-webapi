using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyFirstWebAPI.CustomMiddlewares;
using MyFirstWebAPI.DesignPattern.ProductPattern;
using MyFirstWebAPI.EntityDB;
using System.Text;

namespace MyFirstWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args); // creating an instance for the web application to host, log, configure and registered the build in DI.

            builder.Services.AddControllers();  //register not only the controller whole web api execution system.

            builder.Services.AddDbContext<AppDbContext>(option =>
            option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            builder.Services.AddEndpointsApiExplorer(); //register the api end point which is useful to the swagger to discover the api's endpoint

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "MyFirstWebAPI",
                    Version = "v1"
                });

                // ✅ Add JWT Authentication to Swagger
                var jwtSecurityScheme = new OpenApiSecurityScheme
                {
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Description = "Enter your valid JWT token",
                    Reference = new OpenApiReference
                    {
                        Id = JwtBearerDefaults.AuthenticationScheme,
                        Type = ReferenceType.SecurityScheme
                    }
                };
                c.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { jwtSecurityScheme, Array.Empty<string>() }
                });
            });

            //added the authentication for authentication and authorization for JWT token
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
                });

            builder.Services.AddAuthorization();

          
            //HERE AFTER MIDDLEWARE PIPELINE

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) // if this is an development enviroment then, can use the swagger
            {
                //app.UseDeveloperExceptionPage();
                app.UseSwagger(); // for json generation
                app.UseSwaggerUI(); // for user friendly UI for json
            }
            app.UseMiddleware<GlobalException>();
            Console.WriteLine("The Environment : " + app.Environment.EnvironmentName);

            app.UseHttpsRedirection(); // used to redirect the http to https for extra security
           

            // we need to use the cors right here. 

            app.UseAuthentication();
            app.UseAuthorization(); // used for authorize attribute and before using this make sure to use the authentication 

            app.MapControllers(); // used to correctly map the route to respective controller/action

            app.Run(); // used to run the application to go live
        }
    }
}
