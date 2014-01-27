using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Globalization;
using Sitecore.IO;
using Sitecore.Jobs;

using Sitecore.Shell.Framework;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.WebControls;
using Sitecore.Web.UI.XmlControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using TheDreamCorps.OfflineContentEditor.Data;


namespace TheDreamCorps.OfflineContentEditor.Client.Forms
{
    public class UploadImportFileForm : WizardForm
    {
        protected Scrollbox FileList;
        protected XmlControl Location;
        protected Checkbox OverwriteCheck;
        protected Literal Status;

        [HandleMessage("import:upload", true)]
        protected void Upload(ClientPipelineArgs args)
        {

            ClientPipelineArgs pipelineArgs = new ClientPipelineArgs();
            Context.ClientPage.Start(this, "DoUpload", pipelineArgs);

        }

        protected void DoUpload(ClientPipelineArgs args)
        {
            if (!args.IsPostBack)
            {
                UrlString urlString = new UrlString(UIUtil.GetUri("control:ImportUploadForm"));
                SheerResponse.ShowModalDialog(urlString.ToString(), "400", "500", "", true);
                args.WaitForPostBack();

            }
            else
            {
                FileToImport = args.Result;
            }
        }
        // Methods
        protected override void ActivePageChanged(string page, string oldPage)
        {
            Assert.ArgumentNotNull(page, "page");
            Assert.ArgumentNotNull(oldPage, "oldPage");
            base.ActivePageChanged(page, oldPage);
            if (page == "Uploading")
            {
                base.NextButton.Disabled = true;
                base.BackButton.Disabled = true;
                base.CancelButton.Disabled = true;
                Context.ClientPage.ClientResponse.Timer("ProcessFile", 500);
            }
            if (page == "LastPage")
            {
                base.NextButton.Visible = false;
                base.BackButton.Visible = false;
            }
            
        }
        protected string FileToImport
        {
            get
            {
                return StringUtil.GetString(base.ServerProperties["FileToImport"]);
            }
            set
            {
                Assert.ArgumentNotNullOrEmpty(value, "value");
                base.ServerProperties["FileToImport"] = value;
            }
        }

        protected string JobHandle
        {
            get
            {
                return StringUtil.GetString(base.ServerProperties["JobHandle"]);
            }
            set
            {
                Assert.ArgumentNotNullOrEmpty(value, "value");
                base.ServerProperties["JobHandle"] = value;
            }
        }

        protected void ProcessFile() 
        {

            JobOptions options = new JobOptions("ProcessFile", "ATCC", "shell", new ImportRunner(), "RunImport", new object[] { FileToImport })
            {
                ContextUser = Sitecore.Context.User,
                ClientLanguage = Sitecore.Context.ContentLanguage,
                AfterLife = TimeSpan.FromSeconds(20),
                Priority = System.Threading.ThreadPriority.Normal
            };

            Job job = JobManager.Start(options);

            JobHandle = job.Handle.ToString();

            Context.ClientPage.ClientResponse.Timer("CheckStatus", 500);
                       
        }

        public void CheckStatus()
        {
            Job job = JobManager.GetJob(Handle.Parse(this.JobHandle));
            
            if (job == null)
            {
                throw new Exception("Job interrupted");
            }

            JobStatus status = job.Status;

            if (status.Failed) 
            {
                base.NextButton.Disabled = false;
                Status.Text = "Update Failed...";

                
            }
            else if (status.State == JobState.Running) 
            {
                Status.Text = "Updating items...";

            }
            else if (status.State == JobState.Finished) 
            {
                base.Active = "LastPage";
                base.NextButton.Disabled = false;
                Status.Text = "Updating items complete...";

            }
        }

        protected override bool ActivePageChanging(string page, ref string newpage)
        {
            //Assert.ArgumentNotNull(page, "page");
            //Assert.ArgumentNotNull(newpage, "newpage");
            //if ((page == "Files") && (newpage == "Settings"))
            //{
                //if (!this.GetFileList().Any<string>())
                //{
                //    Context.ClientPage.ClientResponse.Alert("Please specify at least one file to upload.");
                //    return false;
                //}
                //string str = this.GetInvalidFileNames().FirstOrDefault<string>();
                //if (!string.IsNullOrEmpty(str))
                //{
                //    this.ShowInvalidFileMessage(str);
                //    base.NextButton.Disabled = true;
                //    return false;
                //}
            //}
            //if (page == "Retry")
            //{
            //    newpage = "Settings";
            //}
            return base.ActivePageChanging(page, ref newpage);
        }

        protected override void EndWizard()
        {
            Context.ClientPage.ClientResponse.Eval("window.top.dialogClose()");
        }


        public override void HandleMessage(Message message)
        {
            Assert.ArgumentNotNull(message, "message");
            base.HandleMessage(message);
        }

        protected override void OnLoad(EventArgs e)
        {
            Assert.ArgumentNotNull(e, "e");
            if (!Context.ClientPage.IsEvent && !Context.ClientPage.IsPostBack)
            {
            }
            base.OnLoad(e);
        }


    }
}

 

