using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
//using Sitecore.Pipelines.Login;
using Sitecore.Shell.Framework;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XmlControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;

namespace TheDreamCorps.OfflineContentEditor.Client.Forms
{
    public class ImportWizard : WizardForm
    {

        // Fields
        protected Scrollbox FileList;
        protected XmlControl Location;
        protected Checkbox OverwriteCheck;
        protected Checkbox UnzipCheck;
        protected DataContext UploadDataContext;
        protected TreeviewEx UploadTreeview;
        protected Checkbox VersionedCheck;
        protected Literal VersionedCheckLabel;

        // Methods
        protected override void ActivePageChanged(string page, string oldPage)
        {
            Assert.ArgumentNotNull(page, "page");
            Assert.ArgumentNotNull(oldPage, "oldPage");
            base.NextButton.Header = "Next >";
            if (page == "Settings")
            {
                base.NextButton.Header = "Upload >";
            }
            base.ActivePageChanged(page, oldPage);
            if (page == "Uploading")
            {
                base.NextButton.Disabled = true;
                base.BackButton.Disabled = true;
                base.CancelButton.Disabled = true;
                if (this.UploadTreeview != null)
                {
                    Item selectionItem = this.UploadTreeview.GetSelectionItem();
                    if (selectionItem != null)
                    {
                        Sitecore.Context.ClientPage.ClientResponse.SetAttribute("Item", "value", selectionItem.ID.ToString());
                        Sitecore.Context.ClientPage.ClientResponse.SetAttribute("Language", "value", selectionItem.Language.ToString());
                    }
                }
                Sitecore.Context.ClientPage.ClientResponse.SetAttribute("Path", "value", this.Directory);
                Sitecore.Context.ClientPage.ClientResponse.SetAttribute("Overwrite", "value", this.OverwriteCheck.Checked ? "1" : "0");
                Sitecore.Context.ClientPage.ClientResponse.SetAttribute("Unzip", "value", this.UnzipCheck.Checked ? "1" : "0");
                Sitecore.Context.ClientPage.ClientResponse.SetAttribute("Versioned", "value", this.VersionedCheck.Checked ? "1" : "0");
                Sitecore.Context.ClientPage.ClientResponse.Timer("StartUploading", 10);
            }
            if (page == "LastPage")
            {
                base.NextButton.Disabled = true;
                base.BackButton.Disabled = true;
                base.CancelButton.Disabled = true;
                base.CancelButton.Disabled = false;
            }
        }

        protected override bool ActivePageChanging(string page, ref string newpage)
        {
            Assert.ArgumentNotNull(page, "page");
            Assert.ArgumentNotNull(newpage, "newpage");
            if ((page == "Files") && ((newpage == "Location") || (newpage == "Settings")))
            {
                bool flag = false;
                foreach (string str in Sitecore.Context.ClientPage.ClientRequest.Form.Keys)
                {
                    if ((str != null) && str.StartsWith("File", StringComparison.InvariantCulture))
                    {
                        string str2 = Context.ClientPage.ClientRequest.Form[str];
                        if ((str2 != null) && (str2.Trim().Length > 0))
                        {
                            flag = true;
                            break;
                        }
                    }
                }
                if (!flag)
                {
                    Sitecore.Context.ClientPage.ClientResponse.Alert("Specify at least one file to upload.");
                    return false;
                }
            }
            if (page == "Retry")
            {
                newpage = "Settings";
            }
            return base.ActivePageChanging(page, ref newpage);
        }

        private static string BuildFileInput()
        {
            string uniqueID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("File");
            Sitecore.Context.ClientPage.ServerProperties["LastFileID"] = uniqueID;
            string clientEvent = Sitecore.Context.ClientPage.GetClientEvent("FileChange");
            return ("<input id=\"" + uniqueID + "\" name=\"" + uniqueID + "\" type=\"file\" value=\"browse\" style=\"width:100%\" onchange=\"" + clientEvent + "\"/>");
        }

        protected void EndUploading()
        {
            Sitecore.Context.ClientPage.ClientResponse.SetDialogValue("ok");
            base.Next();
        }

        protected override void EndWizard()
        {
            Sitecore.Context.ClientPage.ClientResponse.Eval("window.top.dialogClose()");
        }

        protected void FileChange()
        {
            if (Sitecore.Context.ClientPage.ClientRequest.Source == StringUtil.GetString(Context.ClientPage.ServerProperties["LastFileID"]))
            {
                string str = Context.ClientPage.ClientRequest.Form[Context.ClientPage.ClientRequest.Source];
                if ((str != null) && (str.Length > 0))
                {
                    string str2 = BuildFileInput();
                    Context.ClientPage.ClientResponse.Insert("FileList", "beforeEnd", str2);
                }
            }
            Context.ClientPage.ClientResponse.SetReturnValue(true);
        }

        public override void HandleMessage(Message message)
        {
            Item folder;
            Assert.ArgumentNotNull(message, "message");
            base.HandleMessage(message);
            if ((message["id"] != null) && (message["id"].Length > 0))
            {
                folder = this.UploadDataContext.GetItem(message["id"]);
            }
            else
            {
                folder = this.UploadDataContext.GetFolder();
            }
            Dispatcher.Dispatch(message, folder);
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            if (!Context.ClientPage.IsEvent && !Context.ClientPage.IsPostBack)
            {
                this.UploadDataContext.GetFromQueryString();
                string queryString = WebUtil.GetQueryString("di");
                string text = BuildFileInput();
                this.FileList.Controls.Add(new LiteralControl(text));
                if (queryString.Length > 0)
                {
                    this.Directory = queryString;
                    this.Location.Parent.Controls.Remove(this.Location);
                    this.VersionedCheck.Disabled = true;
                    this.VersionedCheck.Checked = false;
                    this.VersionedCheckLabel.Disabled = true;
                }
                else
                {
                    this.VersionedCheck.Checked = Settings.Media.UploadAsVersionableByDefault;
                }
            }
            base.OnLoad(e);
        }

        protected void ShowError()
        {
            base.NextButton.Disabled = true;
            base.NextButton.Disabled = false;
            base.BackButton.Disabled = false;
            base.BackButton.Disabled = false;
            base.Active = "Files";
            SheerResponse.Alert("An error occured while uploading.\n\nThe reason may be that a file does not exist.", new string[0]);
        }

        protected void ShowFileTooBig(string filename)
        {
            Assert.ArgumentNotNullOrEmpty(filename, "filename");
            SheerResponse.Alert(Translate.Text("The file \"{0}\" is too big to be uploaded.\n\nThe maximum size of a file that can be uploaded is {0}.", new object[] { filename, MainUtil.FormatSize(Settings.Media.MaxSizeInDatabase) }), new string[0]);
        }

        protected void StartUploading()
        {
            Context.ClientPage.ClientResponse.Eval("submit()");
        }

        // Properties
        protected string Directory
        {
            get
            {
                return StringUtil.GetString(Context.ClientPage.ServerProperties["Directory"]);
            }
            set
            {
                Assert.ArgumentNotNull(value, "value");
                Context.ClientPage.ServerProperties["Directory"] = value;
            }
        }
    }
}
