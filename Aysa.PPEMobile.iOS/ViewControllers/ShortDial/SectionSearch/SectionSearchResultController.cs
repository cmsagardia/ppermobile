using System;

using UIKit;
using Foundation;
using System.Collections.Generic;
using Aysa.PPEMobile.Model;
using CoreGraphics;

namespace Aysa.PPEMobile.iOS.ViewControllers.ShortDial.SectionSearch
{
	/// <summary>
	/// / Define Interface to notify section selected
	/// </summary>
	public interface SectionSearchResultControllerDelegate
	{
		void SectionSearchedSelected(Section section);
	}


	public partial class SectionSearchResultController : UITableViewController
    {

        // Public Variables
        public List<Section> filteredItems { get; set; }

		/// <summary>
		///  Define Delegate
		/// </summary>
		WeakReference<SectionSearchResultControllerDelegate> _delegate;

		public SectionSearchResultControllerDelegate Delegate
		{
			get
			{
				SectionSearchResultControllerDelegate workerDelegate;
				return _delegate.TryGetTarget(out workerDelegate) ? workerDelegate : null;
			}
			set
			{
				_delegate = new WeakReference<SectionSearchResultControllerDelegate>(value);
			}
		}

        // Private Variables
        protected string cellIdentifier = "SectionSearchCell";

        #region TableViewController Lifecycle

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            TableView.RegisterNibForCellReuse(UINib.FromName("SectionSearchCell", null), cellIdentifier);
            TableView.SeparatorInset = UIEdgeInsets.Zero;
            // Remove extra cells
            TableView.TableFooterView = new UIView();

            if (UIDevice.CurrentDevice.CheckSystemVersion(11, 0))
            {
                // Remove extra top space in TableView
                TableView.ContentInsetAdjustmentBehavior = UIScrollViewContentInsetAdjustmentBehavior.Automatic;
            }

        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        #endregion

        #region Build Table View


        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
        {
            return 60;
        }

        public override nint RowsInSection(UITableView tableView, nint section)
        {
            return filteredItems.Count;
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            Section section = filteredItems[indexPath.Row];
			UITableViewCell cell = tableView.DequeueReusableCell(cellIdentifier);

			// Remove the cell highlight color
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            cell.TextLabel.Text = section.Nombre;
			return cell;
        }

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
            
			// Notify that one section was selected
            Section section = filteredItems[indexPath.Row];
            if (_delegate != null)
                Delegate?.SectionSearchedSelected(section);
		}

        #endregion

    }
}

