using System;
using UIKit;
using Foundation;
using CoreGraphics;
using Aysa.PPEMobile.iOS.Utilities;
using Aysa.PPEMobile.Model;

namespace Aysa.PPEMobile.iOS.ViewControllers.Events
{

    /// <summary>
    /// / Deffine Interface to pass filter object and load Events by filter
    /// </summary>
	public interface FilterEventsViewControllerDelegate
	{
        void LoadEventsByFilter(FilterEventData filter);
        void ClearAppliedFilter();
	}

    public partial class FilterEventsViewController : UIViewController
    {
		// It's using to get in runtime the filter that the user is using
        public FilterEventData FilterApplied;

        // Private IBOutlets
        private UIDatePicker PickerFromDate;
        private UIDatePicker PickerToDate;

        #region Define Delegate

        /// <summary>
        ///  Define Delegate
        /// </summary>
        WeakReference<FilterEventsViewControllerDelegate> _delegate;

		public FilterEventsViewControllerDelegate Delegate
		{
			get
			{
				FilterEventsViewControllerDelegate workerDelegate;
				return _delegate.TryGetTarget(out workerDelegate) ? workerDelegate : null;
			}
			set
			{
				_delegate = new WeakReference<FilterEventsViewControllerDelegate>(value);
			}
		}

        #endregion

        #region UIViewController Lifecycle

        public FilterEventsViewController(IntPtr handle) : base(handle)
		{

		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
			// Perform any additional setup after loading the view, typically from a nib.

			// Change status bar text color to white
			NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;

			SetUpViewStyle();
            SetUpInputIBOutlets();

            // If the object FilterApplied is not null that means that the user has already setted filters so it's necessary load them
            if(FilterApplied != null)
            {
                LoadFilterSavedInView();
                ClearFilterButton.Enabled = true;
            }

        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        #endregion

        #region Internal Methods

        private void SetUpViewStyle()
		{
			// Config Status segmentedControl
			UITextAttributes attributesForNormalState = new UITextAttributes();
			attributesForNormalState.Font = UIFont.SystemFontOfSize(15f);
			StatusSegmentedControl.SetTitleTextAttributes(attributesForNormalState, UIControlState.Normal);

            // Disable clear button
            ClearFilterButton.Enabled = false;
		}

        private void SetUpInputIBOutlets()
        {

			// Add tap gesture recognizer to dismiss the keyboard
			View.AddGestureRecognizer(new UITapGestureRecognizer(() => this.DismissKeyboard()));

			// Config NumberEventTextField to allow only numbers
			NumberEventTextField.ShouldChangeCharacters = (UITextField textField, NSRange range, string replace) => {
				int dummy;
				return replace.Length == 0 || int.TryParse(replace, out dummy);
			};

			// Add left padding in NumberEventTextField
			UIView paddingNumberView = new UIView(new CGRect(0, 0, 35, 0));
			NumberEventTextField.LeftView = paddingNumberView;
			NumberEventTextField.LeftViewMode = UITextFieldViewMode.Always;

			// Add left padding in TitleTextField
            UIView paddingTitleView = new UIView(new CGRect(0, 0, 35, 0));
			TitleTextField.LeftView = paddingTitleView;
            TitleTextField.LeftViewMode = UITextFieldViewMode.Always;

			// Add left padding in FromDateTextField
			UIView paddingFromDateView = new UIView(new CGRect(0, 0, 25, 0));
            FromDateTextField.LeftView = paddingFromDateView;
			FromDateTextField.LeftViewMode = UITextFieldViewMode.Always;

			// Add left padding in ToateTextField
			UIView paddingToDateView = new UIView(new CGRect(0, 0, 25, 0));
			ToDateTextField.LeftView = paddingToDateView;
			ToDateTextField.LeftViewMode = UITextFieldViewMode.Always;

            // Load dates by default
            DateTime fromDate = DateTime.Now.AddDays(-30);
            FromDateTextField.Text = fromDate.ToString(AysaConstants.FormatDate);
			DateTime toDate = DateTime.Now;
            ToDateTextField.Text = toDate.ToString(AysaConstants.FormatDate);


            // Load UIDatePicker in inputs with date format
            LoadPickerViewInDateTextFields();
		}

        private void LoadPickerViewInDateTextFields()
        {
            // Load datePicker for Date From
            PickerFromDate = new UIDatePicker();
            PickerFromDate.Mode = UIDatePickerMode.Date;
            PickerFromDate.Date = (NSDate)DateTime.Today;
            PickerFromDate.ValueChanged += TextFieldDateFromValueChanged;
            PickerFromDate.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
            FromDateTextField.InputView = PickerFromDate;

            // Load datePicker for Date To
            PickerToDate = new UIDatePicker();
			PickerToDate.Mode = UIDatePickerMode.Date;
			PickerToDate.Date = (NSDate)DateTime.Today;
			PickerToDate.ValueChanged += TextFieldDateToValueChanged;
            PickerToDate.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
            ToDateTextField.InputView = PickerToDate;
        }

        private void LoadFilterSavedInView()
        {
            NumberEventTextField.Text = FilterApplied.EventNumber != 0 ? FilterApplied.EventNumber.ToString() : "";
            TitleTextField.Text = FilterApplied.Title;

            switch (FilterApplied.Status)
			{
				case 1:
                    StatusSegmentedControl.SelectedSegment = 1;
					break;
				case 2:
					StatusSegmentedControl.SelectedSegment = 2;
					break;
				default:
					break;
			}


            FromDateTextField.Text = FilterApplied.FromDate.ToString(AysaConstants.FormatDate);
            ToDateTextField.Text = FilterApplied.ToDate.ToString(AysaConstants.FormatDate);
        }

		private void ShowErrorAlert(string message)
		{
			var alert = UIAlertController.Create("Error", message, UIAlertControllerStyle.Alert);
			alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
			PresentViewController(alert, true, null);
		}

        private FilterEventData ApplyFilter() 
        {
            // Build filter
            // Get filter data from IBOutlets

            // Try to get Number of Event
            int numberEvent = 0;
            if(NumberEventTextField.Text.Length > 0)
            {
                numberEvent = int.Parse(NumberEventTextField.Text);
				
            }

            string title = TitleTextField.Text;

            // Get status
            int statusEvent = -1;

            switch (StatusSegmentedControl.SelectedSegment)
			{
				case 1:
                    statusEvent = (int)Event.Status.Open;
					break;
				case 2:
                    statusEvent = (int)Event.Status.Close;
					break;
				default:
					break;
			}

            // Build filter objects
            FilterEventData filter = new FilterEventData();
            filter.EventNumber = numberEvent;
            filter.Title = title;
            filter.Status = statusEvent;
            filter.FromDate = DateTime.ParseExact(FromDateTextField.Text, AysaConstants.FormatDate, null);
            filter.ToDate = DateTime.ParseExact(ToDateTextField.Text, AysaConstants.FormatDate, null);

            return filter;

			
        }

		private void DismissKeyboard()
		{
			View.EndEditing(true);
		}

		#endregion

		#region IBActions

		partial void CancelButton_Activated(UIBarButtonItem sender)
        {
            DismissViewController(true, null);
        }

        partial void ClearFilterButton_Activated(UIBarButtonItem sender)
        {
			UIAlertController alert = UIAlertController.Create("Aviso", "¿Seguro que desea eliminar los filtros aplicados?", UIAlertControllerStyle.Alert);

			alert.AddAction(UIAlertAction.Create("Cancelar", UIAlertActionStyle.Cancel, null));

			alert.AddAction(UIAlertAction.Create("Si", UIAlertActionStyle.Default, action => {

				// Notify to EventsViewController that the user has cleaned the filters 
				if (_delegate != null)
                    Delegate?.ClearAppliedFilter();

                DismissViewController(true, null);

			}));

			PresentViewController(alert, animated: true, completionHandler: null);
        }

        partial void SaveChangesButton_TouchUpInside(UIButton sender)
        {
            // Build filter form IBOutlets
            FilterEventData filter = ApplyFilter();

            if (filter != null)
            {
                // Send filter to EventsViewController to get events by filter
				if (_delegate != null)
                    Delegate?.LoadEventsByFilter(filter);
                
				DismissViewController(true, null); 
            }
        }

		public void TextFieldDateFromValueChanged(object sender, EventArgs e)
		{

			// Format NSDate
			DateTime fromDate = (DateTime)PickerFromDate.Date;
            FromDateTextField.Text = fromDate.ToString(AysaConstants.FormatDate);
		}

		public void TextFieldDateToValueChanged(object sender, EventArgs e)
		{
			// Format NSDate
            DateTime fromDate = (DateTime)PickerToDate.Date;
            ToDateTextField.Text = fromDate.ToString(AysaConstants.FormatDate);
		}

		#endregion
	}
}

