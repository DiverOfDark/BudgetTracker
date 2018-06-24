using System;
using System.Linq;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.WebUtilities;

namespace BudgetTracker
{
    public static class UrlExtensions
    {
        public static string ReplaceParameter(this string sourceUrl, string parameter, object value)
        {
            var uri = new Uri(sourceUrl);
                
            var originalQuery = QueryHelpers.ParseQuery(uri.Query);

            originalQuery[parameter] = value?.ToString();
            
            var builder = new UriBuilder(uri);
            var builderQuery = new QueryBuilder(originalQuery.ToDictionary(v=>v.Key, v=>v.Value.FirstOrDefault()));
            builder.Query = builderQuery.ToString();

            return builder.ToString();
        }
    }
}