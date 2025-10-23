using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SafeReport.Application.Helper
{
    public static  class ImageUrl
    {
        public static string GetFullImageUrl(string? relativePath, IHttpContextAccessor http)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            // Get base URL from current HTTP request
            var request = http?.HttpContext?.Request;
            if (request == null)
                return relativePath; // fallback for background jobs etc.

            var baseUrl = $"{request.Scheme}://{request.Host}";

            // Combine full path
            return $"{baseUrl}/{relativePath.Replace("\\", "/")}";
        }

    }
}
