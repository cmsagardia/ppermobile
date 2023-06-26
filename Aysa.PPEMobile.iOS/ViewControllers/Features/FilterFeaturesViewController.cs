using System;
using UIKit;
using Foundation;
using CoreGraphics;
using Aysa.PPEMobile.iOS.Utilities;
using Aysa.PPEMobile.Model;
using System.Collections.Generic;
using Aysa.PPEMobile.iOS.ViewControllers.Events.BuilderPickerForTextField;

namespace Aysa.PPEMobile.iOS.ViewControllers.Features
{
    /// <summary>
    /// / Deffine Interface to pass filter object and load Events by filter
    /// </summary>
    public interface FilterFeaturesViewControllerDelegate
    {
        void LoadFeaturesByFilter(FilterFeatureData filter);
        void ClearAppliedFilter();
    }

    public partial class FilterFeaturesViewController : UIViewController , PickerTextFieldDataSourceDelegate
    {
        // It's using to get in runtime the filter that the user is using
        public FilterFeatureData FilterApplied;

        // Private IBOutlets
        private UIDatePicker PickerFromDate;
        private UIDatePicker PickerToDate;


        // Private Variables
        List<Section> ActiveSectionsList;
        Section SectionSelected;

        #region Define Delegate

        /// <summary>
        ///  Define Delegate
        /// </summary>
        WeakReference<FilterFeaturesViewControllerDelegate> _delegate;

        public FilterFeaturesViewControllerDelegate Delegate
        {
            get
            {
                FilterFeaturesViewControllerDelegate workerDelegate;
                return _delegate.TryGetTarget(out workerDelegate) ? workerDelegate : null;
            }
            set
            {
                _delegate = new WeakReference<FilterFeaturesViewControllerDelegate>(value);
            }
        }

        #endregion

        #region UIViewController Lifecycle

        public FilterFeaturesViewController(IntPtr handle) : base(handle)
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

            LoadActiveSectionsInView();

            // If the object FilterApplied is not null that means that the user has already setted filters so it's necessary load them
            if (FilterApplied != null)
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

            // Disable clear button
            ClearFilterButton.Enabled = false;
        }

        private void SetUpInputIBOutlets()
        {

            // Add tap gesture recognizer to dismiss the keyboard
            View.AddGestureRecognizer(new UITapGestureRecognizer(() => this.DismissKeyboard()));

           

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
            NumberEventTextField.Text = FilterApplied.Username.Equals("") ? "" : FilterApplied.Username;
            TitleTextField.Text = FilterApplied.Detail;
            //string.Format("{0} - Nivel: {1}", SectionSelected.Nombre, SectionSelected.Nivel);
            SectionFeatureTextField.Text = string.Format("{0}", FilterApplied.Sector.Nombre);



            FromDateTextField.Text = FilterApplied.FromDate.ToString(AysaConstants.FormatDate);
            ToDateTextField.Text = FilterApplied.ToDate.ToString(AysaConstants.FormatDate);
        }

        private FilterFeatureData ApplyFilter()
        {
            // Build filter
            // Get filter data from IBOutlets

            // Try to get Number of Event
            string numberEvent = "";
            if (NumberEventTextField.Text.Length > 0)
            {
                numberEvent = NumberEventTextField.Text;

            }

            string title = TitleTextField.Text;




            // Build filter objects
            FilterFeatureData filter = new FilterFeatureData();
            filter.Username = numberEvent;
            filter.Detail = title;
            filter.Sector = SectionSelected;
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
            FilterFeatureData filter = ApplyFilter();

            if (filter != null)
            {
                // Send filter to EventsViewController to get events by filter
                if (_delegate != null)
                    Delegate?.LoadFeaturesByFilter(filter);

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

        public void LoadActiveSectionsInView()
        {

            Section dummySector = new Section();
            dummySector.Nombre = "Todas";
            dummySector.Id = "000000000";
            dummySector.Nivel = 0;
            dummySector.ResponsablesGuardia = null;
            List<Section> auxList = new List<Section>();
            auxList = UserSession.Instance.ActiveSections;
            SectionSelected = dummySector;
            bool contains = auxList.Exists(x => x.Nombre == "Todas");
            if (!contains)
            {
                auxList.Insert(0, dummySector);
            }

            ActiveSectionsList = auxList;
            // After get Sections Active list, load elements in PickerView
            LoadPickerViewInSectionTextField();
        }

        private void LoadPickerViewInSectionTextField()
        {

            // Check Active Sections have values
            if (ActiveSectionsList.Count == 0)
            {
                SectionFeatureTextField.Enabled = false;
                SectionFeatureTextField.BackgroundColor = UIColor.FromRGB(220, 220, 220);
                return;
            }

            // Build data from list of EventsType
            List<string> data = new List<string>();

            for (int i = 0; i < ActiveSectionsList.Count; i++)
            {
                Section sectionObj = ActiveSectionsList[i];
                data.Add(string.Format("{0}", sectionObj.Nombre));
            }

            // Load Picker for TypeTextField
            UIPickerView picker = new UIPickerView();
            PickerTextFieldDataSource modelPicker = new PickerTextFieldDataSource(data, SectionFeatureTextField);
            modelPicker.Delegate = this;
            picker.Model = modelPicker;

            SectionFeatureTextField.InputView = picker;
            SectionFeatureTextField.Text = data.Find(m => m.Contains("Todas"));

        }

        #endregion
        #region Implement PickerTextFieldDataSourceDelegate Metods

        public void ItemSelectedValue(int indexSelected, UITextField textField)
        {

            // TextField with Tag value 1 = TypeEventTextField
            switch (textField.Tag)
            {
                case 0:
                    SectionSelected = ActiveSectionsList[indexSelected];
                    SectionFeatureTextField.Text = string.Format("{0}", SectionSelected.Nombre);
                    break;
                default:
                    break;
            }
        }

        #endregion
    }
}

