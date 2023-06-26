using System;

using Foundation;
using UIKit;
using Aysa.PPEMobile.Model;

namespace Aysa.PPEMobile.iOS.ViewControllers.Documents.BuilderDocumentsTableView
{
    public partial class DocumentTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("DocumentTableViewCell");
        public static readonly UINib Nib;

		// Private Variables
        private Document DocumentItem;

        static DocumentTableViewCell()
        {
            Nib = UINib.FromName("DocumentTableViewCell", NSBundle.MainBundle);
        }

        protected DocumentTableViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

		#region Public Methods

        public void LoadDocumentInCell(Document document)
		{
            this.DocumentItem = document;

            DocumentTitleLabel.Text = DocumentItem.Name;

		}

		#endregion
	}
}
