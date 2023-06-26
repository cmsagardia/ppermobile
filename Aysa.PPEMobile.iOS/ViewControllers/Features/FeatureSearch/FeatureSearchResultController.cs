using System;

using UIKit;
using Aysa.PPEMobile.Model;
using System.Collections.Generic;
using Aysa.PPEMobile.iOS.ViewControllers.Features.BuilderFeaturesTableView;

namespace Aysa.PPEMobile.iOS.ViewControllers.Features.FeatureSearch
{
    public partial class FeatureSearchResultController : UITableViewController
    {
        // Public Variables
        public List<Feature> FilteredFeatures { get; set; }

        #region TableViewController Lifecycle

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            TableView.BackgroundColor = UIColor.FromRGB(230, 232, 235);

            // Register TableViewCell
            TableView.RegisterNibForCellReuse(FeaturesTableViewCell.Nib, "FeaturesTableViewCell");

            // Define Height of cells
            TableView.RowHeight = 180;


            // Separator lines in the tableView were customize so it's not neccesary define in the TableView
            TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

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

        #region Public Methods

        public void LoadFilterFeatureInTableView(List<Feature> filteredFeature)
        {

        }

        #endregion
    }
}

