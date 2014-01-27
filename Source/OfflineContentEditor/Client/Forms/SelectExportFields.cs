using Sitecore.Data.Items;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheDreamCorps.OfflineContentEditor.Client.Forms
{
    public class SelectExportFields : DialogForm
    {
        TreeList tl;
        Border TreeListPanel;
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Sitecore.Context.ClientPage.IsEvent && !Sitecore.Context.ClientPage.IsPostBack)
            {
                tl = new TreeList();
                tl.ID = "FieldTreeList";
                TreeListPanel.Controls.Add(tl);
                UrlHandle handle = UrlHandle.Get();
                tl.Source = handle["source"];
                Item it = Sitecore.Context.ContentDatabase.GetItem(tl.Source);
                tl.ExcludeTemplatesForDisplay = it.Name;
                tl.IncludeTemplatesForSelection = "Template field";
                tl.Style.Add("height", "200px");

            }
            else 
            {
                tl = (TreeList) TreeListPanel.Controls[0];
            
            }
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            SheerResponse.SetDialogValue(tl.GetValue());
            base.OnOK(sender, args);

        }
    }
}
