using System;
using UIKit;
using System.Collections.Generic;


namespace Aysa.PPEMobile.iOS.ViewControllers.Events.BuilderPickerForTextField
{

	/// <summary>
	/// / Deffine Interface to pass value selected to ViewController
	/// </summary>
	public interface PickerTextFieldDataSourceDelegate
	{
        void ItemSelectedValue(int indexSelected, UITextField textField);
	}

    public class PickerTextFieldDataSource : UIPickerViewModel
    {

        #region Define Delegate

        /// <summary>
        ///  Define Delegate
        /// </summary>
        WeakReference<PickerTextFieldDataSourceDelegate> _delegate;

        public PickerTextFieldDataSourceDelegate Delegate
        {
            get
            {
                PickerTextFieldDataSourceDelegate workerDelegate;
                return _delegate.TryGetTarget(out workerDelegate) ? workerDelegate : null;
            }
            set
            {
                _delegate = new WeakReference<PickerTextFieldDataSourceDelegate>(value);
            }
        }

        #endregion

        // Public Variableas

        List<string> Items;
        UITextField TextFieldEditing;


        #region UIPickerView DataSource 

        public PickerTextFieldDataSource(List<string> items, UITextField textField)
        {
            this.Items = items;
            this.TextFieldEditing = textField;
        }


        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return Items.Count;
        }

        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return Items[(int)row];
        }

        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            // Notify to ViewController that some item was selected
            if (_delegate != null)
                Delegate?.ItemSelectedValue((int)row, TextFieldEditing);
        }

        #endregion

    }
}
