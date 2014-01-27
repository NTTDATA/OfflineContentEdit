using Sitecore;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheDreamCorps.OfflineContentEditor.Client.Forms;
using TheDreamCorps.OfflineContentEditor.Common;

namespace TheDreamCorps.OfflineContentEditor.Client.Commands
{
    class ImportFile : Command
    {
        /// <summary>
        /// Overriding the Execute Function and passing the commandcontext as a param.
        /// </summary>
        /// <param name="context"></param>
        public override void Execute(CommandContext context)
        {
            Assert.ArgumentNotNull(context, "context");

            Sitecore.Context.ClientPage.Start(this, "Run", context.Parameters);

        }

        protected void Run(ClientPipelineArgs args){


            UrlString urlString = new UrlString(UIUtil.GetUri("control:UploadImportFile"));
            UrlHandle handle = new UrlHandle();
            //Get ID of the curent control (rich text field)

            handle["value"] = string.Empty;
            handle["db"] = Sitecore.Context.ContentDatabase.Name;
            handle["source"] = "/sitecore/content/Global/Products";
            handle["language"] = Sitecore.Context.ContentLanguage.Name;


            handle.Add(urlString);
            SheerResponse.ShowModalDialog(urlString.ToString(), "500px", "400px", string.Empty, false);
        }
    }
}
