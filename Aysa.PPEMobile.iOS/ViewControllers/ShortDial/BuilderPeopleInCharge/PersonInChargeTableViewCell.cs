using System;

using Foundation;
using UIKit;
using CoreAnimation;
using CoreGraphics;
using Aysa.PPEMobile.Model;


namespace Aysa.PPEMobile.iOS.ViewControllers.ShortDial.BuilderPeopleInCharge
{


    public partial class PersonInChargeTableViewCell : UITableViewCell
    {
        //enum PhoneType { Office, CellPhone, RPV, alternative };

        public static readonly NSString Key = new NSString("PersonInChargeTableViewCell");
        public static readonly UINib Nib;

        public ContactType ContactTypeObj;

        // This flag is using for round bottom corners to the last cell
        public bool isLastCellInSection = false;

        static PersonInChargeTableViewCell()
        {
            Nib = UINib.FromName("PersonInChargeTableViewCell", NSBundle.MainBundle);
        }

        protected PersonInChargeTableViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (isLastCellInSection)
            {
                // Round corners only for the last cell in the tableView
                DefineStyleForBottomCell();
            }

        }

        public void loadElementsInCell(ContactType contactType) 
        {

            this.ContactTypeObj = contactType;

            // Remove separator view in case that the cell is the last in section because it's not needed in the last cell
            SeparatorView.Hidden = isLastCellInSection;

			NumberPhoneLabel.Text = this.ContactTypeObj.NumberValue;

			// Define style of cell according to phone typle
            switch (ContactTypeObj.PhoneType)
			{
                case PhoneType.Office:
                    SetUpStyleForOfficeCell();
					break;
                case PhoneType.CellPhone:
                    SetUpStyleForCellPhoneCell();
					break;
                case PhoneType.RPV:
                    SetUpStyleForCellRPVCell();
					break;
                case PhoneType.Alternative:
                    SetUpStyleForCellAlternativeCell();
					break;
				default:
					break;
			} 
        }

		#region Private Methods

		public void DefineStyleForBottomCell()
		{
			// Round only bottom of view, this is to simulate round style in sections of TableView (group style of iOS 6)

			// Is necessary set the size manually because the constraints values haven't been setted yet
			// 40 it's the margin left and right of content view
			CGRect contentSize = new CGRect(0, 0, Frame.Size.Width - 40, Frame.Size.Height);

			UIBezierPath mPath = UIBezierPath.FromRoundedRect(contentSize, (UIRectCorner.BottomRight | UIRectCorner.BottomLeft), new CGSize(width: 5, height: 5));
			CAShapeLayer maskLayer = new CAShapeLayer();
			maskLayer.Frame = contentSize;
			maskLayer.Path = mPath.CGPath;
			containerView.Layer.Mask = maskLayer;
		}

        public void SetUpStyleForOfficeCell()
        {
			IconImage.Image = UIImage.FromBundle("office_phone");
            PhoneTypeLabel.Text = "Oficina";
           
        }

		public void SetUpStyleForCellPhoneCell()
		{
            IconImage.Image = UIImage.FromBundle("cellphone");
			PhoneTypeLabel.Text = "Celular";
		}

		public void SetUpStyleForCellRPVCell()
		{
			IconImage.Image = UIImage.FromBundle("rpv_phone");
			PhoneTypeLabel.Text = "RPV";
		}

		public void SetUpStyleForCellAlternativeCell()
		{
            IconImage.Image = UIImage.FromBundle("alternative_phone");
			PhoneTypeLabel.Text = "Alternativo";
		}


		#endregion
	}
}
