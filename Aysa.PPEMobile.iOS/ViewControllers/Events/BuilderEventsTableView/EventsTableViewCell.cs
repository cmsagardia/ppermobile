using System;

using Foundation;
using UIKit;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.iOS.Utilities;

namespace Aysa.PPEMobile.iOS.ViewControllers.Events.BuilderEventsTableView
{
    public partial class EventsTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("EventsTableViewCell");
        public static readonly UINib Nib;

        // Private Variables
        private Event EventItem;

        static EventsTableViewCell()
        {
            Nib = UINib.FromName("EventsTableViewCell", NSBundle.MainBundle);
        }

        protected EventsTableViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

		#region Public Methods

		public void LoadEventInCell(Event eventItem)
        {
            this.EventItem = eventItem;

            TitleLabel.Text = "#" + this.EventItem.NroEvento.ToString() + " " + this.EventItem.Titulo;
            PlaceLabel.Text = this.EventItem.Lugar;
            UsernameLabel.Text = this.EventItem.Usuario.NombreApellido;
            DateLabel.Text = this.EventItem.Fecha.ToString(AysaConstants.FormatDate24Hrs);

            // Define style of cell according to if the status of event is open or close
            // Status 1 = Open
            // Status 2 = Close

            switch (this.EventItem.Estado)
			{
				case 1:
					// Setup cell style for open event
					SetUpStyleForOpenEvent();
					break;
				case 2:
					SetUpStyleForCloseEvent();
					break;
				default:
					break;
			}


        }

        #endregion

        #region Private Methods

        private void SetUpStyleForCloseEvent()
        {
            UIColor closeColorBackground = UIColor.FromRGB(158, 158, 164);
            UIColor closeColorText = UIColor.FromRGB(76, 75, 80);

            IndicatorStatusView.BackgroundColor = closeColorBackground;
            TitleLabel.TextColor = closeColorText;
            StatusLabel.Text = "Cerrado";
            StatusLabel.TextColor = closeColorText;
            StatusContainerView.BackgroundColor = closeColorBackground;
            SeparatorView.BackgroundColor = closeColorBackground;

            FolderStatusImageView.Image = UIImage.FromBundle("close_folder_event");

        }

		private void SetUpStyleForOpenEvent()
		{
			UIColor openColorBackground = UIColor.FromRGB(248, 155, 9);
			UIColor openColorText = UIColor.FromRGB(95, 33, 132);

			IndicatorStatusView.BackgroundColor = openColorBackground;
			TitleLabel.TextColor = openColorText;
			StatusLabel.Text = "Abierto";
            StatusLabel.TextColor = UIColor.White;
			StatusContainerView.BackgroundColor = openColorBackground;
			SeparatorView.BackgroundColor = openColorBackground;

			FolderStatusImageView.Image = UIImage.FromBundle("open_folder_event");

		}

        #endregion
    }
}
