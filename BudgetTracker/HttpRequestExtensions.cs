using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace BudgetTracker
{
    public static class HttpRequestExtensions
    {
        public static string Join<T>(this IEnumerable<T> objects, string separator) => string.Join(separator, objects);

        public static bool IsAjaxRequest(this HttpRequest request)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (request.Headers != null)
            {
                return request.Headers["X-Requested-With"] == "XMLHttpRequest";
            }

            return false;
        }
    }
}