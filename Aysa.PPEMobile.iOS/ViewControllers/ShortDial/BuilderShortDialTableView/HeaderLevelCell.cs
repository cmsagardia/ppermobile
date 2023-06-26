using Foundation;
using System;
using UIKit;
using ObjCRuntime;
using CoreAnimation;
using CoreGraphics;
using Aysa.PPEMobile.Model;

namespace Aysa.PPEMobile.iOS
{
    public partial class HeaderLevelCell : UITableViewHeaderFooterView
    {
        public static readonly NSString Key = new NSString("HeaderLevelCell");

        public HeaderLevelCell (IntPtr handle) : base (handle)
        {
        }

        public static HeaderLevelCell Create(CGRect frame)
		{

			var arr = NSBundle.MainBundle.LoadNib("HeaderLevelCell", null, null);
			var view = Runtime.GetNSObject<HeaderLevelCell>(arr.ValueAt(0));
            view.Frame = frame;

			return view;
		}

		public override void AwakeFromNib()
		{
			
		}

        public void LoadLevelInView(Level level)
        {
            NameLabel.Text = level.Nombre;
        }

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();


			// Round only top of header view, this is to simulate round style in sections of TableView
			//UIBezierPath mPath = UIBezierPath.FromRoundedRect(ContentView.Bounds, (UIRectCorner.TopRight | UIRectCorner.TopLeft), new CGSize(width: 5, height: 5));
			//CAShapeLayer maskLayer = new CAShapeLayer();
			//maskLayer.Frame = ContentView.Layer.Bounds;
			//maskLayer.Path = mPath.CGPath;
			//ContentView.Layer.Mask = maskLayer;
		}

    }
}