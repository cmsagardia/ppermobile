using System;
using UIKit;
using Foundation;
using Aysa.PPEMobile.Model;
using System.Collections.Generic;

namespace Aysa.PPEMobile.iOS.ViewControllers.Documents.BuilderDocumentsTableView
{
    public class DocumentTableDataSource: UITableViewSource
    {
        
        List<Document> DocumentsItems;
		string CellIdentifier = "DocumentTableViewCell";

		// Get reference of parentViewController
        private DocumentsViewController Owner;

        public DocumentTableDataSource(List<Document> documents, DocumentsViewController owner)
		{
            DocumentsItems = documents;
			this.Owner = owner;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
            return DocumentsItems.Count;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{

			var cell = tableView.DequeueReusableCell(CellIdentifier) as DocumentTableViewCell;

			// Remove the cell highlight color
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;

			// Assign document in cell to load IBOutlets' cell
            Document documentItem = DocumentsItems[indexPath.Row];
            cell.LoadDocumentInCell(documentItem);

			return cell;

		}


		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			// Get document selected
            Document document = DocumentsItems[indexPath.Row];
            Owner.DocumentSelectedFromTableView(document);

		}
    }
}
