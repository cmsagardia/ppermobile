using System;

using Foundation;
using UIKit;
using Aysa.PPEMobile.Model;
using System.Drawing;
using Aysa.PPEMobile.iOS.Utilities;

namespace Aysa.PPEMobile.iOS.ViewControllers.Features.BuilderFeaturesTableView
{
    public partial class FeaturesTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("FeaturesTableViewCell");
        public static readonly UINib Nib;
        private static readonly int HeightAttachmentView = 294;
        private static readonly int HeightAttachmentViewNormal = 44;
        private static readonly int HeightAttachmentViewMedium = 94;


        // Private Variables
        private Feature FeatureItem;

        static FeaturesTableViewCell()
        {
            Nib = UINib.FromName("FeaturesTableViewCell", NSBundle.MainBundle);
        }

        protected FeaturesTableViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        #region Public Methods

        public void LoadFeatureInCell(Feature featureItem)
        {
            this.FeatureItem = featureItem;

            TitleLabel.Text = this.FeatureItem.Detail;
            ChangeLabelHeigthWithText(TitleLabel);
            PlaceLabel.Text = this.FeatureItem.Sector.Nombre;
            UsernameLabel.Text = this.FeatureItem.Usuario.NombreApellido;
            DateLabel.Text = this.FeatureItem.Date.ToString(AysaConstants.FormatDate24Hrs);



        }

        void ChangeLabelHeigthWithText(UILabel label, float maxHeight = 100f)
        {
            if(label.Text.Length > 200)
            {
                TopAttachmentContentConstraint.Constant = 0;
                // Set Height of AttachmentContentView according to count of files in Event
                HeightAttachmentContentConstraint.Constant = HeightAttachmentView;

                LayoutIfNeeded(); ;
            }
            else if(label.Text.Length > 60)
            {
                TopAttachmentContentConstraint.Constant = 0;

                HeightAttachmentContentConstraint.Constant = HeightAttachmentViewMedium;
            }
            else
            {
                TopAttachmentContentConstraint.Constant = 0;
                // Set Height of AttachmentContentView according to count of files in Event
                HeightAttachmentContentConstraint.Constant = HeightAttachmentViewNormal;

                LayoutIfNeeded(); ;
            } 
        }

        #endregion
    }
}
