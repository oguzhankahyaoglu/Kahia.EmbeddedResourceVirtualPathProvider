using System;
using System.IO;
using System.Net;
using System.Web;
using System.Web.Hosting;

namespace Kahia.Web.VirtualPathProvider
{
    internal class EmbeddedResourceVirtualFile : VirtualFile
    {
        readonly EmbeddedResource embedded;

        public EmbeddedResourceVirtualFile(string virtualPath, EmbeddedResource embedded)
            : base(virtualPath)
        {
            this.embedded = embedded;
        }

        public override Stream Open()
        {
            var assemblyLastModified = embedded.AssemblyLastModified;
            var response = HttpContext.Current.Response;
            var cache = response.Cache;
            var prevEtag = cache.GetFieldValueNonPublic("_etag");
            //aspx dosyaları gibi StaticFileHandler'dan dönen dosyalarda o zaten etag enjekte ediyor. 2. enjekte etmeye çalışınca hata veriyordu.
            if (prevEtag != null)
                return embedded.GetStream();
            var etag = assemblyLastModified.Ticks.ToString();
            if (IsInBrowserCache(etag))
                return null;

            InsertCacheHeadersWithEtag(cache, etag, assemblyLastModified);

            return embedded.GetStream();
        }

        private static void InsertCacheHeadersWithEtag(HttpCachePolicy cache, string etag, DateTime assemblyLastModified)
        {
            var storeDuration = new TimeSpan(30, 0, 0, 0);
            cache.SetETag(etag);
            cache.SetOmitVaryStar(true);
            cache.SetMaxAge(storeDuration);
            cache.SetLastModified(assemblyLastModified);
            cache.SetExpires(DateTime.Now.Add(storeDuration)); // For HTTP 1.0 browsers
            cache.SetValidUntilExpires(true);
            cache.SetCacheability(HttpCacheability.Public);
            cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
            cache.VaryByHeaders["Accept-Encoding"] = true; // Tell proxy to cache different versions depending on Accept-Encoding
        }

        private bool IsInBrowserCache(String etag)
        {
            var context = HttpContext.Current;
            var response = context.Response;
            var incomingEtag = context.Request.Headers["If-None-Match"];
            if (String.Equals(incomingEtag, etag, StringComparison.Ordinal))
            {
                response.Cache.SetETag(etag);
                response.AppendHeader("Content-Length", "0");
                response.StatusCode = (int)HttpStatusCode.NotModified;
                response.End();
                return true;
            }
            return false;
        }


    }
}