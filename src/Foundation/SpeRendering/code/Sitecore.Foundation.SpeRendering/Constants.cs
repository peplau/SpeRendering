using System.Dynamic;

namespace Sitecore.Foundation.SpeRendering
{
    public static class Constants
    {
        public static dynamic ViewConstants = new ExpandoObject();
        public static class QueryStringParameters
        {
            public const string Debug = "psdebug";
        }
    }
}