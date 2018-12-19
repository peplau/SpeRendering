using Sitecore.Mvc.Extensions;
using Sitecore.Mvc.Presentation;
using System;
using Sitecore.Data.Items;
using Sitecore.Foundation.SpeRendering.Mvc.Presentation;
using Sitecore.Mvc.Configuration;
using Sitecore.Mvc.Pipelines.Response.GetRenderer;

namespace Sitecore.Foundation.SpeRendering.Pipelines.Response.GetRenderer
{
    public class GetPowershellRenderer : GetRendererProcessor
    {
        public override void Process(GetRendererArgs args)
        {
            if (args.Result != null)
                return;
            args.Result = this.GetRenderer(args.Rendering, args);
        }

        protected Item GetPowershellScript(Rendering rendering, GetRendererArgs args)
        {
            var powershellScript = GetPsScriptFromProperties(rendering) ??
                                   GetPsScriptFromRenderingItem(rendering, args);
            return powershellScript;
        }

        private Item GetPsScriptFromRenderingItem(Rendering rendering, GetRendererArgs args)
        {
            var renderingTemplate = args.RenderingTemplate;
            if (renderingTemplate == null)
                return null;
            if (!renderingTemplate.DescendsFromOrEquals(Mvc.Names.TemplateIds.PowershellRendering))
                return null;
            var renderingItem = rendering.RenderingItem;
            if (renderingItem == null)
                return null;
            var powershellScript = renderingItem.InnerItem["Powershell Script"];
            if (powershellScript.IsWhiteSpaceOrNull())
                return null;

            var item = rendering.Item.Database.GetItem(powershellScript);
            return item;
        }

        private Item GetPsScriptFromProperties(Rendering rendering)
        {
            if (rendering.RenderingType != "Controller")
                return null;
            var powershellScript = rendering["PowershellScript"];
            if (powershellScript.IsWhiteSpaceOrNull())
                return null;

            var item = rendering.Item.Database.GetItem(powershellScript);
            return item;
        }

        protected Tuple<string, string> GetControllerAndAction(Rendering rendering, GetRendererArgs args)
        {
            var tuple = GetFromProperties(rendering) ?? GetFromRenderingItem(rendering, args);
            return tuple == null
                ? null
                : MvcSettings.ControllerLocator.GetControllerAndAction(tuple.Item1, tuple.Item2);
        }

        protected Renderer GetRenderer(Rendering rendering, GetRendererArgs args)
        {
            var controllerAndAction = GetControllerAndAction(rendering, args);
            var powershellScript = GetPowershellScript(rendering, args);
            if (controllerAndAction == null)
                return null;
            var str1 = controllerAndAction.Item1;
            var str2 = controllerAndAction.Item2;
            return new PowershellRenderer
            {
                ControllerName = str1,
                ActionName = str2,
                PowershellScript = powershellScript
            };
        }

        protected Tuple<string, string> GetFromProperties(Rendering rendering)
        {
            if (rendering.RenderingType != "Controller")
                return null;
            var str1 = rendering["Controller"];
            var str2 = rendering["Controller Action"];
            return str1.IsWhiteSpaceOrNull() ? null : new Tuple<string, string>(str1, str2);
        }
        protected Tuple<string, string> GetFromRenderingItem(Rendering rendering, GetRendererArgs args)
        {
            var renderingTemplate = args.RenderingTemplate;
            if (renderingTemplate == null)
                return null;
            if (!renderingTemplate.DescendsFromOrEquals(Mvc.Names.TemplateIds.PowershellRendering))
                return null;
            var renderingItem = rendering.RenderingItem;
            if (renderingItem == null)
                return null;
            var str1 = renderingItem.InnerItem["Controller"];
            var str2 = renderingItem.InnerItem["Controller Action"];
            return str1.IsWhiteSpaceOrNull() ? null : new Tuple<string, string>(str1, str2);
        }
    }
}