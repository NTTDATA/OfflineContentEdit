using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System;

using Sitecore.Web.UI.Pages;
using Sitecore.Data.Items;
using Sitecore.Web.UI.Sheer;
using Sitecore.Web.UI.XmlControls;
using Sitecore.Web.UI.HtmlControls;
using System.Collections.Specialized;
using Sitecore.Form.Core.Utility;

namespace TheDreamCorps.OfflineContentEditor.Client.Forms
{
    class SelectFile : DialogForm
    {

        // Fields
        protected GenericControl CurrentPlaceholder;
        protected XmlControl Dialog;
        protected DataContext ItemDataContext;
        protected DataTreeview ItemTreeView;
        protected GenericControl PageHolder;
        protected Combobox PlaceholderList;
        protected NameValueCollection nvc = new NameValueCollection();
        // Methods
        protected virtual void Localize()
        {
            //this.Dialog["Header"] = ResourceManager.Localize("SELECT_FORM_DISPLAY_PAGE");
            //this.Dialog["Text"] = ResourceManager.Localize("SELECT_FORM_THAT_FORM_WILL_BE_DISPLAYED");
        }
        public string Params
        {
            get
            {
                return (System.Web.HttpContext.Current.Session[Sitecore.Web.WebUtil.GetQueryString("params")] as string);
            }
        }

        protected override void OnLoad(EventArgs e)
        {

            base.OnLoad(e);

            if (!Sitecore.Context.ClientPage.IsEvent)
            {
                this.Localize();

                nvc = ParametersUtil.XmlToNameValueCollection(Params);

                File = nvc["File"];

                if (!string.IsNullOrEmpty(this.File) && Sitecore.Data.ID.IsID(this.File))
                {
                    this.ItemDataContext.DefaultItem = this.File;
                }
                this.OnNodeSelected(this, EventArgs.Empty);
            }
            this.ItemTreeView.OnClick += new EventHandler(this.OnNodeSelected);
        }

        private void OnNodeSelected(object sender, EventArgs e)
        {
            if (this.ItemDataContext.CurrentItem != null)
            {
                string str = this.ItemDataContext.CurrentItem.ID.ToString();
                nvc.Add("File", str);
            }
        }

        protected override void OnOK(object sender, EventArgs args)
        {
            Item selectionItem = this.ItemTreeView.GetSelectionItem();
            if (selectionItem == null)
            {
                //XXX: ResourceManager.Localize("CHOOSE_PAGE")
                SheerResponse.Alert("Choose a file", new string[0]);
            }
            else
            {
                nvc.Add("File", selectionItem.ID.ToString());
                string str = ParametersUtil.NameValueCollectionToXml(nvc);
                if (str.Length == 0)
                {
                    str = "-";
                }
                SheerResponse.SetDialogValue(str);
                base.OnOK(sender, args);
            }
        }

        public string File { get; set; }
    }
}
