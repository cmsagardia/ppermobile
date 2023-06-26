using System;
using UIKit;
using Foundation;
using CoreGraphics;
using System.Collections.Generic;
using Aysa.PPEMobile.Model;

namespace Aysa.PPEMobile.iOS.ViewControllers.ShortDial.BuilderPeopleInCharge
{
    public class PersonInChargeSourceTableView: UITableViewSource
    {
		string CellIdentifier = "PersonInChargeTableViewCell";

		// Height for TableViewCellHeader
        private static readonly int HeightHeaderView = 50;

		// Get reference of parentViewController
        PeopleInChargeViewController owner;
        private List<PersonGuard> PeopleInGuard;

		public PersonInChargeSourceTableView(List<PersonGuard> peopleInGuard, PeopleInChargeViewController owner)
		{
            this.owner = owner;
            this.PeopleInGuard = peopleInGuard;
		}


		public override nfloat GetHeightForHeader(UITableView tableView, nint section)
		{
			return HeightHeaderView;
		}

		public override UIView GetViewForHeader(UITableView tableView, nint section)
		{
			HeaderPersonChargeCell header = HeaderPersonChargeCell.Create(new CGRect(0, 0, this.owner.View.Frame.Width, HeightHeaderView));
            PersonGuard personGuard = PeopleInGuard[(int)section];
            header.LoadPersonGuardInView(personGuard);

            return header;
		}

		public override nint NumberOfSections(UITableView tableView)
		{
            return PeopleInGuard.Count;
		}

		public override nint RowsInSection(UITableView tableview, nint section)
		{
            PersonGuard personGuard = PeopleInGuard[(int)section];

            return personGuard.ContactTypes.Count;
		}

		public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
		{

            var cell = tableView.DequeueReusableCell(CellIdentifier) as PersonInChargeTableViewCell;

			// Remove the cell highlight color
			cell.SelectionStyle = UITableViewCellSelectionStyle.None;
            cell.isLastCellInSection = false;

            // Get Person in Guard and his contact type
            PersonGuard personGuard = PeopleInGuard[indexPath.Section];
            ContactType contactType = personGuard.ContactTypes[indexPath.Row];

           	
            if (indexPath.Row == personGuard.ContactTypes.Count -1)
			{
				// Round bottom corners for the last row of tableView
				cell.isLastCellInSection = true;
			}

			cell.loadElementsInCell(contactType);

			return cell;

		}

		public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
		{
			// Get Person in Guard and his contact type
			PersonGuard personGuard = PeopleInGuard[indexPath.Section];
			ContactType contactType = personGuard.ContactTypes[indexPath.Row];

            owner.ContacSelectedFromTableView(contactType);

            //tableView.DeselectRow(indexPath, false);
			
		}

    }
}
