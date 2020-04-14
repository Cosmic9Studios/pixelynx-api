using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Pixelynx.Api.Filters
{
    public class ApiFilterAttribute : ExceptionFilterAttribute
    {
        private readonly ILogger<ApiFilterAttribute> _logger;

        public ApiFilterAttribute(ILogger<ApiFilterAttribute> logger)
        {
            _logger = logger;
        }

        public override void OnException(ExceptionContext context)
        {
            var exception = context.Exception;
            string msg = string.Empty;
            HttpResponse response = context.HttpContext.Response;
            response.ContentType = "application/json";

            _logger.LogError(exception, exception.Message);

            context.Result = new JsonResult(new { Message = exception.Message });
            base.OnException(context);
        }
    }
}