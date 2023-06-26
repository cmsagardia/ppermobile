using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Icu.Util;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;

namespace Aysa.PPEMobile.Droid.Fragments
{
    public class DatePickerRGFragment : AndroidX.Fragment.App.DialogFragment
, DatePickerDialog.IOnDateSetListener
    {
        // TAG can be any string of your choice.
        public static readonly string TAG = "X:" + typeof(DatePickerRGFragment).Name.ToUpper();
        private Semana semana;

        // Initialize this value to prevent NullReferenceExceptions.
        Action<DateTime> _dateSelectedHandler = delegate { };

        public static DatePickerRGFragment NewInstance(Action<DateTime> onDateSelected, Semana _semana)
        {
            DatePickerRGFragment frag = new DatePickerRGFragment();
            frag._dateSelectedHandler = onDateSelected;
            frag.semana = _semana;
            return frag;
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            var roles = UserSession.Instance.Roles;

            DateTime currently = DateTime.Now;
            DatePickerDialog dialog = new DatePickerDialog(Activity,
                                                           this,
                                                           currently.Year,
                                                           currently.Month - 1,
                                                           currently.Day);
            var datepicker = dialog.DatePicker;

            Calendar calendar = Calendar.GetInstance(locale: ULocale.Default);
            calendar.Set(CalendarField.Year, semana.Desde.Year);
            calendar.Set(CalendarField.Month, (semana.Desde.Month - 1));
            calendar.Set(CalendarField.DayOfWeek, (int)semana.Desde.DayOfWeek);
            calendar.Set(CalendarField.Date, semana.Desde.Day);
            datepicker.MinDate = (long)calendar.TimeInMillis;

            DateTime deadline = MinorDate();

            calendar.Set(CalendarField.Year, deadline.Year);
            calendar.Set(CalendarField.Month, (deadline.Month - 1));
            calendar.Set(CalendarField.DayOfWeek, (int)deadline.DayOfWeek);
            calendar.Set(CalendarField.Date, deadline.Day);

            datepicker.MaxDate = (long)calendar.TimeInMillis;

            return dialog;
        }

        public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
            // Note: monthOfYear is a value between 0 and 11, not 1 and 12!
            DateTime selectedDate = new DateTime(year, monthOfYear + 1, dayOfMonth);

            _dateSelectedHandler(selectedDate);
        }

        private DateTime MinorDate()
        {
            DateTime currently = DateTime.Now;
            if (currently > semana.Hasta)
            {
                return semana.Hasta;
            }
            else
            {
                return currently;
            }
        }
    }
}