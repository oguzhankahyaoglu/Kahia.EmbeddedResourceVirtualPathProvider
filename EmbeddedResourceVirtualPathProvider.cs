using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Hosting;

namespace Kahia.Web.VirtualPathProvider
{
    public class EmbeddedResourceVirtualPathProvider : System.Web.Hosting.VirtualPathProvider
    {
        internal readonly SortedDictionary<string, List<EmbeddedResource>> Resources = new SortedDictionary<string, List<EmbeddedResource>>(StringComparer.InvariantCultureIgnoreCase);

        //internal readonly List<String> EmbeddedVirtualPaths = new List<string>();
        internal readonly List<String> ResourceAssemblyNames = new List<string>();

        private EmbeddedResourceVirtualPathProvider() { }

        private static readonly object LOCK = new object();

        public static EmbeddedResourceVirtualPathProvider Instance { get; private set; }
        public static EmbeddedResourceVirtualPathProvider CreateInstance(Dictionary<String, Assembly> embeddedAssembliesPath)
        {
            lock (LOCK)
            {
                if (Instance == null)
                {
                    Instance = new EmbeddedResourceVirtualPathProvider();
                    if (embeddedAssembliesPath != null)
                        foreach (var keyValuePair in embeddedAssembliesPath)
                            Instance.Add(keyValuePair.Value, keyValuePair.Key);
                }
                return Instance;
            }
        }

        public void Add(Assembly assembly, string embeddedVirtualPath)
        {
            var assemblyName = assembly.GetName().Name;
            var resourcePaths = assembly.GetManifestResourceNames().Where(r => r.StartsWith(assemblyName));
            //EmbeddedVirtualPaths.Add(embeddedVirtualPath);
            ResourceAssemblyNames.Add(assemblyName);
            foreach (var resourcePath in resourcePaths)
            {
                var cleanedResourcePath = resourcePath.Substring(assemblyName.Length).TrimStart('.');
                if (!Resources.ContainsKey(embeddedVirtualPath))
                    Resources[embeddedVirtualPath] = new List<EmbeddedResource>();
                Resources[embeddedVirtualPath].Insert(0, new EmbeddedResource(assembly, cleanedResourcePath, embeddedVirtualPath));
            }
        }

        public override bool FileExists(string virtualPath)
        {
            String embeddedPath;
            var shouldFindResource = PrecheckPathUsingVirtualPathKeys(virtualPath, out embeddedPath);
            if (shouldFindResource)
            {
                var resource = GetResourceFromVirtualPath(virtualPath, embeddedPath);
                if (resource != null)
                    return true;
            }
            var fileExists = Previous.FileExists(virtualPath);
            return fileExists;
        }

        public override VirtualFile GetFile(string virtualPath)
        {
            String embeddedPath;
            var shouldFindResource = PrecheckPathUsingVirtualPathKeys(virtualPath, out embeddedPath);
            if (shouldFindResource)
            {
                var resource = GetResourceFromVirtualPath(virtualPath, embeddedPath);
                if (resource != null)
                {
                    var embeddedResourceVirtualFile = new EmbeddedResourceVirtualFile(virtualPath, resource);
                    return embeddedResourceVirtualFile;
                }
            }
            var result = Previous.GetFile(virtualPath);
            return result;
        }

        public override string GetFileHash(string virtualPath, IEnumerable virtualPathDependencies)
        {
            var fileHash = Previous.GetFileHash(virtualPath, virtualPathDependencies);
            return fileHash;
        }

        public override string GetCacheKey(string virtualPath)
        {
            String embeddedPath;
            var shouldFindResource = PrecheckPathUsingVirtualPathKeys(virtualPath, out embeddedPath);
            if (shouldFindResource)
            {
                var resource = GetResourceFromVirtualPath(virtualPath, embeddedPath);
                if (resource != null)
                {
                    return (virtualPath + resource.AssemblyName + resource.AssemblyLastModified.Ticks).GetHashCode().ToString();
                }
            }
            var result = base.GetCacheKey(virtualPath);
            return result;
        }

        public EmbeddedResource GetResourceFromVirtualPath(string _virtualPath, string embeddedVirtualPath)
        {
            //if (virtualPath[0] != '~')
            //    virtualPath = virtualPath.ReplaceFromBeginning("/TestProject.Admin/", "~/");
            var virtualPath = _virtualPath.StartsWith("~/") ? _virtualPath : VirtualPathUtility.ToAppRelative("/" + _virtualPath);
            var cleanedPath = virtualPath.TrimStart('~', '/').Replace('/', '.');
            var resourceKey = cleanedPath;
            //var index = EmbeddedVirtualPaths.FindIndex(r => virtualPath.StartsWith(r, StringComparison.InvariantCultureIgnoreCase));
            //if (index == -1)
            //    return null;

            //var embeddedVirtualPath = EmbeddedVirtualPaths[index];

            //hiç check edilmemişse check edip dosyayı bulmaya çalışsın
            if (embeddedVirtualPath.IsNullOrEmptyString() && !PrecheckPathUsingVirtualPathKeys(virtualPath, out embeddedVirtualPath))
                return null;
            var cleanedVirtualPath = embeddedVirtualPath.TrimStart('~', '/').Replace('/', '.');
            //var resourceAssembyName = ResourceAssemblyNames[index];

            if (resourceKey.StartsWith(cleanedVirtualPath, StringComparison.InvariantCultureIgnoreCase))
                resourceKey = resourceKey.Substring(cleanedVirtualPath.Length);

            List<EmbeddedResource> embeddedResourceList;
            if (Resources.TryGetValue(embeddedVirtualPath, out embeddedResourceList))
            {
                var resource = embeddedResourceList.Find(er => er.ResourcePath.Equals(resourceKey, StringComparison.InvariantCultureIgnoreCase));
                if (resource != null) //&& !ShouldUsePrevious(virtualPath, resource)
                    return resource;
            }
            return null;
        }

        private bool PrecheckPathUsingVirtualPathKeys(string virtualPath, out string embeddedVirtualPath)
        {
            var temp = virtualPath.StartsWith("~/") ? virtualPath : VirtualPathUtility.ToAppRelative("/" + virtualPath);
            embeddedVirtualPath = Resources.Keys.FirstOrDefault(r => temp.StartsWith(r, StringComparison.InvariantCultureIgnoreCase));
            return embeddedVirtualPath != null;
        }

        public override CacheDependency GetCacheDependency(string virtualPath, IEnumerable virtualPathDependencies, DateTime utcStart)
        {
            String embeddedPath;
            var shouldFindResource = PrecheckPathUsingVirtualPathKeys(virtualPath, out embeddedPath);
            if (shouldFindResource)
            {
                var resource = GetResourceFromVirtualPath(virtualPath, null);
                if (resource != null)
                {
                    return resource.GetCacheDependency(utcStart);
                }
            }

            if (DirectoryExists(virtualPath) || FileExists(virtualPath))
            {
                return base.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
            }

            return null;
        }
    }
}