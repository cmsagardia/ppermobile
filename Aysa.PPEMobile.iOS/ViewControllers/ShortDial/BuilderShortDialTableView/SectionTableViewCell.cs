using System;
using Foundation;
using UIKit;
using CoreAnimation;
using CoreGraphics;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;
using System.Threading.Tasks;
using Aysa.PPEMobile.Service.HttpExceptions;
using Aysa.PPEMobile.iOS.Utilities;

namespace Aysa.PPEMobile.iOS.ViewControllers.ShortDial.BuilderShortDialTableView
{
    public partial class SectionTableViewCell : UITableViewCell
    {
        public static readonly NSString Key = new NSString("SectionTableViewCell");
        public ShortDialViewController owner;
        public static readonly UINib Nib;
        public Section sectionSelected { get; set; }

        // This flag is using for round bottom corners to the last cell
        public bool isLastCellInSection = false;

        static SectionTableViewCell()
        {
            Nib = UINib.FromName("SectionTableViewCell", NSBundle.MainBundle);
        }

        protected SectionTableViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if(isLastCellInSection)
            {
                // Round corners only for the last cell in the tableView
                DefineStyleForBottomCell();
            }

        }


        public void LoadSectionInView(Section section, ShortDialViewController _owner)
		{
            //Recibe parametros para inicializar
            owner = _owner;
            sectionSelected = section;
            NameLabel.Text = section.Nombre;
            ChangeStarIcon(section.Favorito);
		}

        partial void ChangeFavorite(Foundation.NSObject sender)
        {
            ChangeStarIcon(!sectionSelected.Favorito);
            owner.ChangeFavorite(sectionSelected);
        }

        public void ChangeStarIcon(bool activeStar)
        {
            FavoriteButton.SetImage(UIImage.FromBundle(activeStar ? "star" : "star_empty"), UIControlState.Normal);
        }

        public void DefineStyleForBottomCell()
        {
			// Round only bottom of view, this is to simulate round style in sections of TableView (group style of iOS 6)

			// Is necessary set the size manually because the constraints values haven't been setted yet
			// 40 it's the margin left and right of content view
			CGRect contentSize = new CGRect(0, 0, Frame.Size.Width - 40 , Frame.Size.Height); 

            UIBezierPath mPath = UIBezierPath.FromRoundedRect(contentSize, (UIRectCorner.BottomRight | UIRectCorner.BottomLeft), new CGSize(width: 5, height: 5));
			CAShapeLayer maskLayer = new CAShapeLayer();
            maskLayer.Frame = contentSize;
			maskLayer.Path = mPath.CGPath;
			containerView.Layer.Mask = maskLayer;
        }
    }
}
