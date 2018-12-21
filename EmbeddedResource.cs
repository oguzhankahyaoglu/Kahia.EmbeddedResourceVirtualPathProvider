using System;
using System.IO;
using System.Reflection;
using System.Web.Caching;
using System.Web.Hosting;

namespace Kahia.EmbeddedResourceVirtualPathProvider
{
    public class EmbeddedResource
    {
        public static FileInfo GetAssemblyFile(Assembly assembly)
        {
            return new FileInfo(assembly.GetName().CodeBase.Replace("file:///", ""));
        }

        public EmbeddedResource(Assembly assembly, string resourcePath, string embeddedVirtualPath)
        {
            this.AssemblyName = assembly.GetName().Name;
            //var fileInfo = new FileInfo(assembly.Location);
            var fileInfo = GetAssemblyFile(assembly); //SecurityException veriyor!
            AssemblyLastModified = fileInfo.LastWriteTime;
            this.ResourcePath = resourcePath;
            if (embeddedVirtualPath.IsNotNullAndEmptyString())
            {
                var filename = GetFileNameFromProjectSourceDirectory(assembly, resourcePath, embeddedVirtualPath);
                if (filename.IsNotNullAndEmptyString()) //means that the source file was found, or a copy was in the web apps folders
                {
                    GetCacheDependency = (utcStart) => new CacheDependency(filename, utcStart);
                    GetStream = () => File.OpenRead(filename);
                    return;
                }
            }
            GetCacheDependency = (utcStart) => new CacheDependency(assembly.Location); //Security Exception veriyor!
            GetStream = () =>
                        {
                            return assembly.GetManifestResourceStream(assembly.GetName().Name + '.' + resourcePath);
                        };
        }

        public DateTime AssemblyLastModified { get; private set; }

        public string ResourcePath { get; private set; }

        public Func<Stream> GetStream { get; private set; }
        public Func<DateTime, CacheDependency> GetCacheDependency { get; private set; }

        public string AssemblyName { get; private set; }

        string GetFileNameFromProjectSourceDirectory(Assembly assembly, string resourcePath, string projectSourcePath)
        {
            try
            {
                //if (!Path.IsPathRooted(projectSourcePath))
                //    projectSourcePath = new DirectoryInfo((Path.Combine(HttpRuntime.AppDomainAppPath, projectSourcePath))).FullName;
                projectSourcePath = HostingEnvironment.MapPath(projectSourcePath);
                var fileName = Path.Combine(projectSourcePath, resourcePath.Replace('.', '\\'));
                return GetFileName(fileName);
            }
            catch (Exception ex)
            {
                throw new Exception("Error loading resources. Assembly: {0}, Path: {1}, SourcePath: {2}".FormatString(assembly, resourcePath, projectSourcePath), ex);
            }
        }

        string GetFileName(string possibleFileName)
        {
            var indexOfLastSlash = possibleFileName.LastIndexOf('\\');
            possibleFileName = ReplaceChar(possibleFileName, indexOfLastSlash, '.');
            return File.Exists(possibleFileName) ? possibleFileName : "";
        }

        string ReplaceChar(string text, int index, char charToUse)
        {
            var buffer = text.ToCharArray();
            buffer[index] = charToUse;
            return new string(buffer);
        }

        public override string ToString()
        {
            return AssemblyName + " : " + ResourcePath;
        }
    }
}