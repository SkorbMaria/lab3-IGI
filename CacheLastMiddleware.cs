using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using lab1_ef;

namespace lab3_igi
{
    public class CacheLastMiddleware
    {
        readonly RequestDelegate _next;
        readonly IMemoryCache _memoryCache;

        public CacheLastMiddleware(RequestDelegate next, IMemoryCache memCache)
        {
            this._next = next;
            this._memoryCache = memCache;
        }

        public async Task Invoke(HttpContext context, BibliotekaContext db)
        {
            string path = context.Request.Path.Value.ToLower();
            object model = null;
            
            switch (path)
            {
                case "/gr":
                    model = db.Rooms.Last(); 
                    break;
                case "/gst":
                    model = db.ServiceTypes.Last();
                    break;
                case "/gemp":
                    model = db.Employees.Last();
                    break;
            }

            var modelTemp = model;

            if (!_memoryCache.TryGetValue(path, out modelTemp))
            {
                _memoryCache.Set(path, model,
                    new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(2*17+240)));
            }

            await _next.Invoke(context);
        }
    }

    public static class DbCacheExtensions
    {
        public static IApplicationBuilder UseCacheLastFeauture(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<CacheLastMiddleware>();
        }
    }
}
