using System;
using UIKit;
using Foundation;
using Aysa.PPEMobile.Model;
using System.Collections.Generic;

namespace Aysa.PPEMobile.iOS.ViewControllers.Features.BuilderFeaturesTableView
{
    public class FeaturesTableDataSource: UITableViewSource
    {
        List<Feature> FeaturesItems;
        string CellIdentifier = "FeaturesTableViewCell";
        private static readonly float HeightContentCellView = 460f;
        private static readonly float HeightContentCellViewNormal = 180f;
        private static readonly float HeightContentCellViewMedium = 230f;

        // Get reference of parentViewController
        private FeaturesViewController Owner;

        public FeaturesTableDataSource(List<Feature> features, FeaturesViewController owner)
        {
            FeaturesItems = features;
            this.Owner = owner;
        }

        public override nint NumberOfSections(UITableView tableView)
        {
            return 2;
        }

        public override UIView GetViewForHeader(UITableView tableView, nint section)
        {

            // It's to simulate top padding between UISerchBar and first section 
            if (section == 0) {
                UIView view = new UIView();
                //view.BackgroundColor = UIColor.Clear;
                return view; 
            }

            return null;
        }


        public override nfloat GetHeightForHeader(UITableView tableView, nint section)
        {
            // It's to simulate top padding between UISerchBar and first section 
            if (section == 0) { return 15; }

            return 0;
        }


        public override nint RowsInSection(UITableView tableview, nint section)
        {
            // It's to simulate top padding between UISerchBar and first section
            if (section == 0) { return 0; }

            return FeaturesItems.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {

            var cell = tableView.DequeueReusableCell(CellIdentifier) as FeaturesTableViewCell;



            // Remove the cell highlight color
            cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            // Assign event in cell to load IBOutlets' cell
            Feature featureItem = FeaturesItems[indexPath.Row];
            cell.LoadFeatureInCell(featureItem);





            return cell;

        }

        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            // Get event selected
            Feature featureSelected = FeaturesItems[indexPath.Row];
            Owner.FeatureSelectedFromTableView(featureSelected);

        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            Feature featureItem = FeaturesItems[indexPath.Row];
            if(featureItem.Detail.Length > 60 && featureItem.Detail.Length < 200)
            {
                return HeightContentCellViewMedium;
            }
            else if(featureItem.Detail.Length > 200)
            {
                return HeightContentCellView;
            }
            else
            {
                return HeightContentCellViewNormal;
            }
        }
    }
}
