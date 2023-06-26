using System;
using UIKit;
using Foundation;
using CoreGraphics;

namespace Aysa.PPEMobile.iOS.Utilities.Views
{
    
    public class PaddingTextField : UITextField
    {
		public PaddingTextField(IntPtr handle) : base (handle)
        {
            System.Diagnostics.Debug.WriteLine("init");
		}


        public virtual CGRect TextRectForBounds(CGRect forBounds)
		{
			var padding = new UIEdgeInsets(20, 50, 0, 0);

			return base.TextRect(padding.InsetRect(forBounds));
		}

        public virtual CGRect EditingRectForBounds(CGRect forBounds)
		{
			return TextRect(forBounds);
		}



		
    }
}
