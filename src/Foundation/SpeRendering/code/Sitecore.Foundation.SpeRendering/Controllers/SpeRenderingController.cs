using System;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Sitecore.Foundation.SpeRendering.Mvc.Presentation;
using Sitecore.Mvc.Controllers;
using Cognifide.PowerShell.Core.Host;
using Sitecore.Data.Items;
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
            var script = powershellScriptItem["Script"];
            if (string.IsNullOrEmpty(script))
                return View(new Message("Powershell Script is empty"));

            try
            {
                using (var scriptSession = ScriptSessionManager.NewSession("Default", true))
                {
                    var parameters = new Models.Context {RenderingContext = RenderingContext.Current};
                    scriptSession.SetVariable("context", parameters);
                    scriptSession.SetItemLocationContext(RenderingContext.Current.ContextItem);
                    var results = scriptSession.ExecuteScriptPart(script, false);
                    var returnContext = (Models.Context) results[0];

                    // Returns with no errors
                    if (string.IsNullOrEmpty(returnContext.Message))
                    {
                        var isDebug =
                            !string.IsNullOrEmpty(Request.QueryString[Constants.QueryStringParameters.Debug]) &&
                            Configuration.Settings.GetBoolSetting("SpeRendering.DebugEnabled", false);

                        // Debug disabled
                        if (!isDebug)
                            return View(returnContext.View, returnContext.Model);

                        // Debug enabled - Return debug info

                        // Dump object
                        var dumped = "";
                        if (returnContext.Model!=null && returnContext.Model.GetType() == typeof(Item))
                        {
                            dumped =
                                $"Item Id:{((Item) returnContext.Model).ID} - {((Item) returnContext.Model).Database.Name}:{((Item) returnContext.Model).Paths.Path}";
                        }
                        else
                        {
                            using (var writer = new System.IO.StringWriter())
                            {
                                ObjectDumper.Dumper.Dump(returnContext.Model, "Model", writer);
                                dumped = writer.ToString();
                            }
                        }

                        // Send to output
                        var sbDebug = new StringBuilder();
                        sbDebug.AppendLine($"<div><hr>");
                        if (!Context.PageMode.IsExperienceEditorEditing)
                            sbDebug.AppendLine($"<div><hr><b>Rendering:</b> {RenderingContext.Current.Rendering.RenderingItem.InnerItem.Paths.Path}");
                        sbDebug.AppendLine($"<pre>");
                        sbDebug.AppendLine(HttpUtility.HtmlEncode(dumped));
                        sbDebug.AppendLine("</pre><hr></div>");
                        Response.Write(sbDebug.ToString());

                        // Call original View
                        return View(returnContext.View, returnContext.Model);
                    }

                    // Returns with errors
                    var sb = new StringBuilder();
                    sb.AppendLine("<p><b>Powershell script returned an error: </b></p>");
                    sb.AppendLine($"<p>{returnContext.Message}</p>");
                    return View(new Message(sb.ToString()));
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