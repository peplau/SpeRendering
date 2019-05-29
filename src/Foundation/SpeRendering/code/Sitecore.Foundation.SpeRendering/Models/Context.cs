namespace Sitecore.Foundation.SpeRendering.Models
{
    public class Context
    {
        public Sitecore.Mvc.Presentation.RenderingContext RenderingContext;
        public string View { get; set; }
        public object Model { get; set; }
        public string Message { get; set; }
    }
}