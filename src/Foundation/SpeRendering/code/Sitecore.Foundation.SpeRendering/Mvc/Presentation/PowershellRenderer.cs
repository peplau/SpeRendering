using Sitecore.Data.Items;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Foundation.SpeRendering.Mvc.Presentation
{
    public class PowershellRenderer : ControllerRenderer
    {
        public virtual Item PowershellScript { get; set; }
    }
}