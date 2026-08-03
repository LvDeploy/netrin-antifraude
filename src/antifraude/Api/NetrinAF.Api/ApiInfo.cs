using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace NetrinAF.Api
{
    [ExcludeFromCodeCoverage]
    public static class ApiInfo
    {
        public const string BaseUrl = "netrin-af";

        public static string? GetVersion()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = $"{fvi.FileMajorPart}.{fvi.FileMinorPart}.{fvi.FileBuildPart}";

            return version;
        }
    }
}
