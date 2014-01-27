using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Jobs;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.Pages;
using Sitecore.Web.UI.Sheer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheDreamCorps.OfflineContentEditor.Common;
using TheDreamCorps.OfflineContentEditor.Data;

namespace TheDreamCorps.OfflineContentEditor.Client.Forms
{
    public class OfflineExportWizard : WizardForm
    {
        TreePicker template;
        TreeList FieldTreeList;
        Border FieldTreeListPanel;
        Literal SelectedFields;
        Border SelectedFieldsPanel;
        Literal Status;
        
        protected string FieldsToExport
        {
            get
            {
                return StringUtil.GetString(base.ServerProperties["FieldsToExport"]);
            }
            set
            {
                Assert.ArgumentNotNullOrEmpty(value, "value");
                base.ServerProperties["FieldsToExport"] = value;
            }
        }

        protected string ResultFile
        {
            get
            {
                return StringUtil.GetString(base.ServerProperties["ResultFile"]);
            }
            set
            {
                Assert.ArgumentNotNullOrEmpty(value, "value");
                base.ServerProperties["ResultFile"] = value;
            }
        }


        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        protected override void ActivePageChanged(string page, string oldPage)
        {
            Assert.ArgumentNotNull(page, "page");
            Assert.ArgumentNotNull(oldPage, "oldPage");
            if (page == "FieldPage")
            {
                TemplateItem templateItem = new TemplateItem(Sitecore.Context.ContentDatabase.GetItem(template.Value));
                foreach (TemplateFieldItem tfi in templateItem.Fields) 
                {
                    Sitecore.Web.UI.HtmlControls.Checkbox cb = new Sitecore.Web.UI.HtmlControls.Checkbox();
                    cb.Value = tfi.Name;
                    FieldTreeListPanel.Controls.Add(cb);
                }
            }
            if (page == "GenerateFile")
            {
                base.NextButton.Disabled = true;
                base.BackButton.Disabled = true;
                base.CancelButton.Disabled = true;
                Context.ClientPage.ClientResponse.Timer("GenerateExportFile", 500);
            }
            if (page == "LastPage")
            {
                base.BackButton.Visible = false;
                base.NextButton.Visible = false;
            }

            base.ActivePageChanged(page, oldPage);
        }

        [HandleMessage("export:selectfields",true)]
        protected void SelectFields(ClientPipelineArgs args)
        {
            ClientPipelineArgs pipelineArgs = new ClientPipelineArgs();
            Context.ClientPage.Start(this, "DoSelectFields", pipelineArgs);
        }

        [HandleMessage("export:downloadfile")]
        protected void DownloadPackage(Message message)
        {
            string resultFile = this.ResultFile;
            if (resultFile.Length > 0)
            {
                Context.ClientPage.ClientResponse.Download(resultFile);
            }
            else
            {
                Context.ClientPage.ClientResponse.Alert("Could not download package");
            }
        }

 

        protected void DoSelectFields(ClientPipelineArgs args)
        {
            UrlString urlString = new UrlString(UIUtil.GetUri("control:SelectExportFields"));
            UrlHandle handle = new UrlHandle();
            //Get ID of the curent control (rich text field)

            handle["value"] = string.Empty;
            handle["db"] = Sitecore.Context.ContentDatabase.Name;
            handle["source"] = template.Value;
            handle["language"] = Sitecore.Context.ContentLanguage.Name;


            handle.Add(urlString);

            if (!args.IsPostBack)
            {
                SheerResponse.ShowModalDialog(urlString.ToString(), "500px", "300px", string.Empty, true);
                args.WaitForPostBack();
            }
            else
            {
                if (!string.IsNullOrEmpty(args.Result))
                    FieldsToExport = args.Result;

                SelectedFields.Value = GetSelectedFieldsText();

                SheerResponse.SetInnerHtml(SelectedFieldsPanel, SelectedFields);
            }
            
        }

        private string GetSelectedFieldsText()
        {
            StringBuilder sb = new StringBuilder();
            
            sb.Append("The following fields will be exported:");
            
            string[] fields = FieldsToExport.Split(new string [] { "|" }, StringSplitOptions.RemoveEmptyEntries);

            Guid newGuid;
            foreach (string fieldid in fields) 
            {
                if (Guid.TryParse(fieldid, out newGuid))
                {
                    Item it = Sitecore.Context.ContentDatabase.GetItem(new ID(fieldid));
                    sb.Append("<br/>" + it.Name);
                }
            }

            return sb.ToString();
        }


        protected override bool ActivePageChanging(string page, ref string newpage)
        {
            Assert.ArgumentNotNull(page, "page");
            Assert.ArgumentNotNull(newpage, "newpage");
            
            if (page == "TemplatePage")
            {
                if (string.IsNullOrEmpty(template.Value) || template.Value == "{3C1715FE-6A13-4FCF-845F-DE308BA9741D}")
                {
                    Sitecore.Context.ClientPage.ClientResponse.Alert("Please choose a template...");
                    return false;
                }
            }

            if (page == "FieldPage")
            {
                if (string.IsNullOrEmpty(FieldsToExport))
                {
                    Sitecore.Context.ClientPage.ClientResponse.Alert("Please choose field(s)...");
                    return false;
                }
            }

            return base.ActivePageChanging(page, ref newpage);
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

        protected void GenerateExportFile()
        {
            ResultFile = Settings.GetSetting(OfflineContentEditorSettings.DownloadFolder, "") + "\\" + "DreamCorps" + System.DateTime.Now.ToLongDateString() + ".csv";
            string path = "/sitecore/content/home/*[@@TemplateID='" + Sitecore.Context.ContentDatabase.GetItem(template.Value).ID.ToString() + "']";
            
            List<Item> itemList = Sitecore.Context.ContentDatabase.SelectItems(path).ToList();
            List<ID> fieldIds = (from x in FieldsToExport.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries)
                                select new ID(x)).ToList();

            JobOptions options = new JobOptions("GenerateExportFile", "ATCC", "shell", new ExportRunner(), "RunExport", 
                new object[] { 
                    itemList,
                    fieldIds,
                    ResultFile,
                    FileFormat.Csv
                })
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

    }
}
