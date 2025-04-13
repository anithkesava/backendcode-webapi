using System.Net;
using System.Text.Json;

namespace MyFirstWebAPI.CustomMiddlewares
{
    public class GlobalException
    {
        private readonly ILogger<GlobalException> _log;

        private readonly RequestDelegate _request;

        public GlobalException(ILogger<GlobalException> log, RequestDelegate request)
        {
            _log = log;
            _request = request;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _request(context);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "******** An Global Exception Caught an Exception ********");

                /*
                here we need to do set of things includes log the error in the console. 
                return a json response. for json response we need certain things which includes,
                1, Status code
                2, Content type
                3, Message
                4, Error Content
                At last we need to serialize the c# Object into Json file. which is serialization
                */
                if (!context.Response.HasStarted)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";
                    var errormessage = new
                    {
                        Message = "An UnHandled Error has Occurs",
                        StatusCode = context.Response.StatusCode,
                        Error = ex.Message,
                        ContentType = context.Response.ContentType                        
                    };
                    var json = JsonSerializer.Serialize(errormessage);
                    await context.Response.WriteAsync(json);
                }
            }
        }

    }
}
