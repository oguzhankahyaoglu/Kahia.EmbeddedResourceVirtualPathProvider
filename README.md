# EmbeddedResourceVirtualPathProvider

--------------------------------------------------------------
------ Embedded Virtual Path Provider Readme -----------------
--------------------------------------------------------------

This package enables serving all type of files from assemblies (dlls) just like you are exploring using file explorer; even you can serve aspx, master, ascx files, whatever you wish.
For example, if you enable Directory Browsing from IIS for these virtual paths, everyting would work fine.

To start;
Global.asax: Application_Start
------------------------------------------------
```CSharp
var provider = EmbeddedResourceVirtualPathProvider.CreateInstance(new Dictionary<string, Assembly>
{
    {"~/common-web-resources/", typeof(Garenta.CommonWebResources.CommonWebResources).Assembly}
});
HostingEnvironment.RegisterVirtualPathProvider(provider);
```
You have to define mappings of virtual paths with assemblies. I did not try conflicting directory mappings. If you find any bugs, please let me know on GitHub.


Web.config: Allow IIS to process paths for static files
-------------------------------------------------
```
<location path="common-web-resources">
    <system.webServer>
      <handlers>
        <add name="common-web-resources-JS" path="*.js" verb="*" type="System.Web.StaticFileHandler" />
        <add name="common-web-resources-CSS" path="*.css" verb="*" type="System.Web.StaticFileHandler" />
        <add name="common-web-resources-PNG" path="*.png" verb="*" type="System.Web.StaticFileHandler" />
        <add name="common-web-resources-GIF" path="*.gif" verb="*" type="System.Web.StaticFileHandler" />
        <add name="common-web-resources-TXT" path="*.txt" verb="*" type="System.Web.StaticFileHandler" />
        <add name="common-web-resources-SVG" path="*.svg" verb="*" type="System.Web.StaticFileHandler" />
      </handlers>
    </system.webServer>
  </location>
```
For each virtual path mapping, you have to allow file extensions.

In order to reference such files;
--------------------------------------
This will be enough;

```
<script type="text/javascript" src="/common-web-resources/collaboration/collaboration.js "></script> 
```

But you can also define extension methods for ease of use:
```CSharp
public static string CommonWebResource(this UrlHelper url, string virtualPath)
{
    return CDNHelper.GetResource(virtualPath.Replace("~/", "~/common-web-resources/"));
}
```
