using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using OWSShared.Interfaces;

namespace OWSShared.Middleware
{
    public class StoreCustomerGUIDMiddleware
    {
        private IHeaderCustomerGUID _customerGuid;
        private readonly RequestDelegate _next;

        public StoreCustomerGUIDMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IHeaderCustomerGUID customerGuidService)
        {
            _customerGuid = customerGuidService;
            if (context.Request.Headers.TryGetValue("X-CustomerGUID", out var value)
                && Guid.TryParse(value, out var customerGuid))
            {
                _customerGuid.CustomerGUID = customerGuid;
            }

            if (_customerGuid.CustomerGUID == Guid.Empty)
            {
                context.Response.Clear();
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            await _next(context);
        }
    }

    public static class StoreCustomerGUIDMiddlewareExtensions
    {
        public static IApplicationBuilder UseStoreCustomerGuidMiddleware(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<StoreCustomerGUIDMiddleware>();
        }
    }
}
