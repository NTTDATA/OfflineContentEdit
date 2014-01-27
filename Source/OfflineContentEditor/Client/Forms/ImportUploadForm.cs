using Sitecore;
using Sitecore.Configuration;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using System;
using System.IO;
using System.Web;
using TheDreamCorps.OfflineContentEditor.Common;

namespace TheDreamCorps.OfflineContentEditor.Client.Forms
{
    class ImportUploadForm : DialogForm
    {
        protected Button btnUpload;
        protected Literal Message;

        /// <summary>
        /// Upload Function.
        /// </summary>
        protected void Upload()
        {
            Context.ClientPage.ClientResponse.Eval("submit()");
        }

        /// <summary>
        /// FilePath Property.
        /// </summary>
        private string FilePath
        {
            get
            {
                return this.ServerProperties["FilePath"] as string;
            }
            set { this.ServerProperties["FilePath"] = value; }
        }

        /// <summary>
        /// Overriding the OnLoad Event.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!Context.ClientPage.IsEvent && Context.ClientPage.IsPostBack)
            {
                HttpFileCollection filesCollections = Context.ClientPage.Request.Files;
                
                foreach (string myfile in filesCollections.AllKeys)
                {
                    if (!string.IsNullOrEmpty(filesCollections[myfile].FileName))
                    {
                        //check if file is not a csv file
                        if (Path.GetExtension(Path.GetFileName(filesCollections[myfile].FileName)) != ".csv")
                        {
                            Message.Text =
                            "Please upload only a csv file.  Please verify the file contents are in proper format.";
                        }
                        else
                        {
                            FilePath = Settings.GetSetting(OfflineContentEditorSettings.UploadFolder) + @"\" + filesCollections[myfile].FileName;
                            filesCollections[myfile].SaveAs(FilePath);
                            Message.Text = string.Format("File {0} uploaded<br>", filesCollections[myfile].FileName);
                        }
                    }
                }
            }
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            //verify that a file has been selected.
            //after file has been selected, the upload button must be pressed
            if (FilePath != null)
            {
                SheerResponse.SetDialogValue(Path.GetFileName(FilePath));
                base.OnOK(sender, args);
            }
            else
                Context.ClientPage.ClientResponse.Alert("Please press the upload button after selecting the file.");
        }
    }
}
