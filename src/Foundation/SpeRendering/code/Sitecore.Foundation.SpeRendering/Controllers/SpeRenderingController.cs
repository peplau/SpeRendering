using System;
using System.Text;
using System.Web.Mvc;
using Sitecore.Foundation.SpeRendering.Mvc.Presentation;
using Sitecore.Mvc.Controllers;
using Cognifide.PowerShell.Core.Host;
using Sitecore.Foundation.SpeRendering.Models;
using Sitecore.Mvc.Presentation;

namespace Sitecore.Foundation.SpeRendering.Controllers
{
    public class SpeRenderingController : SitecoreController
    {
        // GET: SpeRendering
        public override ActionResult Index()
        {
            var powershellScriptItem =
                ((PowershellRenderer) RenderingContext.Current.Rendering.Renderer)
                .PowershellScript;
            if (powershellScriptItem==null)
                return View(new Message("Cannot find Powershell Script"));

            try
            {
                using (var scriptSession = ScriptSessionManager.NewSession("Default", true))
                {
                    var script = powershellScriptItem["Script"];
                    if (!string.IsNullOrEmpty(script))
                    {
                        var parameters = new Models.Context {RenderingContext = RenderingContext.Current};
                        scriptSession.SetVariable("context", parameters);
                        var results = scriptSession.ExecuteScriptPart(script, false);
                        var returnContext = (Models.Context) results[0];
                        return View(returnContext.View, returnContext.Model);
                    }
                }
            }
            catch (Exception e)
            {
                var sb = new StringBuilder();
                sb.AppendLine("<p><b>Error processing Powershell Rendering: </b></p>");
                sb.AppendLine($"<p>{e}</p>");
                return View(new Message(sb.ToString()));
            }
        }
    }
}