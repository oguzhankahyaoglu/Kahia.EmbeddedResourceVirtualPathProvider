using System;
using System.Linq;
using System.Reflection;

namespace Kahia.EmbeddedResourceVirtualPathProvider
{
    public static class EmbeddedResourceRepo
    {
        public static string GetEmbeddedResourceUrl(Assembly embeddedAssembly, string _nameSpace, string resourceName)
        {
            //->localhost/TestProject.Admin/Embedded/plugins.datetimefield.datepicker-en-GB.js
            var currentAssemblyName = embeddedAssembly.GetName().Name;
            //ResourceAssemblyIndex = EmbeddedResourceVirtualPathProvider.Instance.ResourceAssemblyNames.FindIndex(r => r.Equals(currentAssemblyName));
            //if (ResourceAssemblyIndex == -1)
            //    throw new Exception("Embedded Assembly'ler içinde '{0}' assembly'si bulunamadığından embedded resource bulunamadı.".FormatString(currentAssemblyName));
            //var virtualRootPath = EmbeddedResourceVirtualPathProvider.Instance.EmbeddedVirtualPaths[ResourceAssemblyIndex];
            var resourceAssemblyNameIndex = EmbeddedResourceVirtualPathProvider.Instance.ResourceAssemblyNames.FindIndex(r => r.Equals(currentAssemblyName));
            if (resourceAssemblyNameIndex == -1)
                throw new Exception("Embedded Assembly'ler içinde '{0}' assembly'si bulunamadığından embedded resource bulunamadı.".FormatString(currentAssemblyName));
            var resourceAssembly = EmbeddedResourceVirtualPathProvider.Instance.Resources.ElementAt(resourceAssemblyNameIndex);
            var virtualRootPath = resourceAssembly.Key;
            var nameSpace = _nameSpace.RemoveFromBeginning(currentAssemblyName, StringComparison.InvariantCultureIgnoreCase).RemoveFromBeginning(".");
            return virtualRootPath + nameSpace.Replace('.', '/') + "/" + resourceName;
        }
    }
}
