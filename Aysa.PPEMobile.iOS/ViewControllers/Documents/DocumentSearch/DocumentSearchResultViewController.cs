using System;

using UIKit;
using System.Collections.Generic;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.iOS.ViewControllers.Documents.BuilderDocumentsTableView;

namespace Aysa.PPEMobile.iOS.ViewControllers.Documents.DocumentSearch
{
    public partial class DocumentSearchResultViewController : UITableViewController
    {

		// Public Variables
        public List<Document> FilteredDocuments { get; set; }

		#region TableViewController Lifecycle

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.

			TableView.BackgroundColor = UIColor.FromRGB(230, 232, 235);

			// Register TableViewCell
			TableView.RegisterNibForCellReuse(DocumentTableViewCell.Nib, "DocumentTableViewCell");

			// Define Height of cells
			TableView.RowHeight = 90;

			// Remove extra cells
			TableView.TableFooterView = new UIView();

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                // Remove extra top space in TableView
                TableView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Never;
            }

		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.
		}

		#endregion

	
    }
}

