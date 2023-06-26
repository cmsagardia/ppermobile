using System;

using UIKit;
using Aysa.PPEMobile.Model;
using Foundation;

using Aysa.PPEMobile.iOS.ViewControllers.ShortDial.BuilderPeopleInCharge;

namespace Aysa.PPEMobile.iOS.ViewControllers.ShortDial
{
    public partial class PeopleInChargeViewController : UIViewController
    {

        // Public Variables
        public Section SectionWithPeopleInGuard;

        public PeopleInChargeViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            InitTableView();

            TitleLabel.Text = SectionWithPeopleInGuard.Nombre;

            // Check if section has persons in charge
            if(SectionWithPeopleInGuard.ResponsablesGuardia.Count == 0)
            {
                EmptyContentView.Hidden = false;
            }
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        #region Private Methods

        private void InitTableView()
        {
            TableView.RegisterNibForCellReuse(PersonInChargeTableViewCell.Nib, "PersonInChargeTableViewCell");

            TableView.Source = new PersonInChargeSourceTableView(SectionWithPeopleInGuard.ResponsablesGuardia, this);
        }

		#endregion

		#region TableView Delegate

		// User select Contact Type in tableView
        public void ContacSelectedFromTableView(ContactType contact)
		{
            // Make a call to numer associate.
            UIApplication.SharedApplication.OpenUrl(new NSUrl(string.Format("tel:{0}", contact.NumberValue)));
		}

		#endregion
	}
}

