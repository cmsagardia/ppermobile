using Foundation;
using System;
using UIKit;
using ObjCRuntime;
using Aysa.PPEMobile.Model;

namespace Aysa.PPEMobile.iOS
{
	/// <summary>
	/// / Deffine Interface to notify that an attachment was selected
	/// </summary>
	public interface AttachmentFileViewDelegate
	{
		void AttachmentSelected(AttachmentFile documentFile);
        void RemoveAttachmentSelected(AttachmentFile documentFile);
	}

    public partial class AttachmentFileView : UIView
    {
        // Private Variables
        AttachmentFile File;

        //Public Variables

        /// <summary>
        ///  Define Delegate
        /// </summary>
        WeakReference<AttachmentFileViewDelegate> _delegate;

        #region Public Methods

        public AttachmentFileViewDelegate Delegate
        {
            get
            {
                AttachmentFileViewDelegate workerDelegate;
                return _delegate.TryGetTarget(out workerDelegate) ? workerDelegate : null;
            }
            set
            {
                _delegate = new WeakReference<AttachmentFileViewDelegate>(value);
            }
        }

        public AttachmentFileView(IntPtr handle) : base(handle)
        {
        }

        public static AttachmentFileView Create()
        {

            var arr = NSBundle.MainBundle.LoadNib("AttachmentFileView", null, null);
            var v = Runtime.GetNSObject<AttachmentFileView>(arr.ValueAt(0));

            return v;
        }

        public override void AwakeFromNib()
        {

        }

        public void LoadAttachmentFileInView(AttachmentFile file, bool readOnly)
        {
            this.File = file;

            NameFileLabel.Text = file.FileName;

            if (file.Private)
            {
                IconImageView.Image = UIImage.FromBundle("lock");
            }

            if(readOnly){
                ConfigViewForReadOnly();
            }
        }

        #endregion

        #region Private Methods

        private void ConfigViewForReadOnly()
        {
            // The user can't remove the file, so remove the remove button
            RemoveButton.Hidden = true;
            RemoveButton.Enabled = false;
            HeightRemoveConstraint.Constant = 0;
            LayoutIfNeeded();
        }


        #endregion


        #region IBActions

        partial void UIButtonR2QvsM5n_TouchUpInside(UIButton sender)
        {
            // Select Attachment
            // Send file to EventDetailViewController to open it
            if (_delegate != null)
                Delegate?.AttachmentSelected(File);
        }

        partial void UIButtonIsObV3X0_TouchUpInside(UIButton sender)
        {
            
            // Remove Attachment
            if (_delegate != null)
                Delegate?.RemoveAttachmentSelected(File);
        }

        #endregion
    }
}