using Foundation;
using System;
using UIKit;
using ObjCRuntime;
using CoreAnimation;
using CoreGraphics;
using Aysa.PPEMobile.Model;

namespace Aysa.PPEMobile.iOS
{
    public partial class HeaderPersonChargeCell : UIView
    {
        public HeaderPersonChargeCell (IntPtr handle) : base (handle)
        {
        }

		public static HeaderPersonChargeCell Create(CGRect frame)
		{

			var arr = NSBundle.MainBundle.LoadNib("HeaderPersonChargeCell", null, null);
			var view = Runtime.GetNSObject<HeaderPersonChargeCell>(arr.ValueAt(0));
            view.Frame = frame;

			return view;
		}

		public override void AwakeFromNib()
		{

		}

        public void LoadPersonGuardInView(PersonGuard person)
		{
            NameLabel.Text = person.NombreApellido;
		}

		public override void LayoutSubviews()
		{
			base.LayoutSubviews();


			// Round only top of header view, this is to simulate round style in sections of TableView
			UIBezierPath mPath = UIBezierPath.FromRoundedRect(ContentView.Bounds, (UIRectCorner.TopRight | UIRectCorner.TopLeft), new CGSize(width: 5, height: 5));
			CAShapeLayer maskLayer = new CAShapeLayer();
			maskLayer.Frame = ContentView.Layer.Bounds;
			maskLayer.Path = mPath.CGPath;
			ContentView.Layer.Mask = maskLayer;
		}
    }
}