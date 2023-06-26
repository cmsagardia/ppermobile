using System;
using UIKit;
using Foundation;
using Aysa.PPEMobile.Model;
using System.Collections.Generic;

namespace Aysa.PPEMobile.iOS.ViewControllers.Events.BuilderEventsTableView
{
    public class EventsTableDataSource: UITableViewSource
    {
		List<Event> EventsItems;
		string CellIdentifier = "EventsTableViewCell";

		// Get reference of parentViewController
        private EventsViewController Owner;

		public EventsTableDataSource(List<Event> events, EventsViewController owner)
		{
            EventsItems = events;
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
                view.BackgroundColor = UIColor.Clear;
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

			return EventsItems.Count;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{

			var cell = tableView.DequeueReusableCell(CellIdentifier) as EventsTableViewCell;

			// Remove the cell highlight color
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;

            // Assign event in cell to load IBOutlets' cell
            Event eventItem = EventsItems[indexPath.Row];
            cell.LoadEventInCell(eventItem);

			return cell;

		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
            // Get event selected
            Event eventSelected = EventsItems[indexPath.Row];
            Owner.EventSelectedFromTableView(eventSelected);

		}
        
    }
}
