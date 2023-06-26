
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Aysa.PPEMobile.Model;
using Newtonsoft.Json;
using AndroidX.AppCompat.App;

namespace Aysa.PPEMobile.Droid.Activities
{

    [Activity(Label = "FilterEventsActivity")]
    public class FilterEventsActivity : AppCompatActivity
    {
        public static readonly string FILTER_RESULT = "FILTER_RESULT";
        private readonly string FILTER_APPLIED = "FILTER_APPLIED";

        // Views
        Button btnSegmented1;
        Button btnSegmented2;
        Button btnSegmented3;

        EditText numberEventText;
        EditText titleEventText;
        EditText fromDateText;
        EditText toDateText;

        // It's using to get in runtime the filter that the user is using
        public FilterEventData FilterApplied;

        int segmentedIndexSelected = 0;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.FilterEvent);

            SetUpView();
        }

        #region Private Methods

        private void SetUpView()
        {
            // Set toolbar and title
            global::AndroidX.AppCompat.Widget.Toolbar toolbar = (global::AndroidX.AppCompat.Widget.Toolbar)FindViewById(Resource.Id.toolbar);
            toolbar.Title = GetString(Resource.String.event_filter);


            // Add back button
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            numberEventText = FindViewById<EditText>(Resource.Id.nro_event_text_field);
            titleEventText = FindViewById<EditText>(Resource.Id.title_text_field);

            Button btnAplicar = FindViewById<Button>(Resource.Id.btn_aplicar);
            btnAplicar.Click += BtnAplicar_Click;

            SetUpSegmentedControl();
            SetUpDateFields();

            // Get filter applied
            if (Intent.GetStringExtra(FILTER_APPLIED) != null)
            {
                this.FilterApplied = JsonConvert.DeserializeObject<FilterEventData>(Intent.GetStringExtra(FILTER_APPLIED));
                LoadFilterSavedInView();
            }
        }

        void SetUpSegmentedControl()
        {
            btnSegmented1 = FindViewById<Button>(Resource.Id.btn_segmented1);
            btnSegmented1.SetBackgroundColor(Color.ParseColor("#672E8A"));
            btnSegmented1.Click += delegate
            {
                BtnSegmented_Click(btnSegmented1);
            };

            btnSegmented2 = FindViewById<Button>(Resource.Id.btn_segmented2);
            btnSegmented2.Click += delegate
            {
                BtnSegmented_Click(btnSegmented2);
            };

            btnSegmented3 = FindViewById<Button>(Resource.Id.btn_segmented3);
            btnSegmented3.Click += delegate
            {

                BtnSegmented_Click(btnSegmented3);
            };
        }

        void SetUpDateFields()
        {
            fromDateText = FindViewById<EditText>(Resource.Id.from_date_text_field);
            fromDateText.Click += delegate
            {
                ShowDatePicker(fromDateText);
            };

            toDateText = FindViewById<EditText>(Resource.Id.to_date_text_field);
            toDateText.Click += delegate
            {
                ShowDatePicker(toDateText);
            };


            // Load dates by default
            DateTime fromDate = DateTime.Now.AddDays(-30);
            fromDateText.Text = fromDate.ToString(AysaConstants.FormatDate);
            DateTime toDate = DateTime.Now;
            toDateText.Text = toDate.ToString(AysaConstants.FormatDate);
        }

        void ResetStatusSegmentedControl()
        {
            btnSegmented1.SetBackgroundColor(Color.ParseColor("#ffffff"));
            btnSegmented1.SetTextColor(Color.ParseColor("#6B6B6E"));
            btnSegmented2.SetBackgroundColor(Color.ParseColor("#ffffff"));
            btnSegmented2.SetTextColor(Color.ParseColor("#6B6B6E"));
            btnSegmented3.SetBackgroundColor(Color.ParseColor("#ffffff"));
            btnSegmented3.SetTextColor(Color.ParseColor("#6B6B6E"));
        }

        private void ShowDatePicker(EditText editText)
        {
            Fragments.DatePickerFragment frag = Fragments.DatePickerFragment.NewInstance(delegate (DateTime time)
            {
                editText.Text = time.ToString("dd/MM/yyyy");
            });

            frag.Show(SupportFragmentManager, Fragments.DatePickerFragment.TAG);
        }

        private void LoadFilterSavedInView()
        {
            numberEventText.Text = FilterApplied.EventNumber != 0 ? FilterApplied.EventNumber.ToString() : "";
            titleEventText.Text = FilterApplied.Title;

            switch (FilterApplied.Status)
            {
                case 1:
                    BtnSegmented_Click(btnSegmented2);
                    break;
                case 2:
                    BtnSegmented_Click(btnSegmented3);
                    break;
                default:
                    break;
            }


            fromDateText.Text = FilterApplied.FromDate.ToString(AysaConstants.FormatDate);
            toDateText.Text = FilterApplied.ToDate.ToString(AysaConstants.FormatDate);
        }


        

        private FilterEventData ApplyFilter()
        {
            // Build filter
            // Get filter data from IBOutlets

            // Try to get Number of Event
            int numberEvent = 0;
            if (numberEventText.Text.Length > 0)
            {
                numberEvent = int.Parse(numberEventText.Text);

            }

            titleEventText = FindViewById<EditText>(Resource.Id.title_text_field);
            string title = titleEventText.Text;

            // Get status
            int statusEvent = -1;

            switch (segmentedIndexSelected)
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
            filter.FromDate = DateTime.ParseExact(fromDateText.Text, AysaConstants.FormatDate, null);
            filter.ToDate = DateTime.ParseExact(toDateText.Text, AysaConstants.FormatDate, null);

            return filter;


        }

        #endregion

        #region Actions


        void BtnAplicar_Click(object sender, System.EventArgs e)
        {
            FilterEventData filter = ApplyFilter();
            FinishWithResult(filter);
        }

        private void FinishWithResult(FilterEventData filter)
        {
            Intent data = new Intent();

            data.PutExtra(FILTER_RESULT, JsonConvert.SerializeObject(filter));

            SetResult(Result.Ok, data);
            Finish();
        }

        public void BtnSegmented_Click(Button segmented)
        {

            ResetStatusSegmentedControl();

            segmented.SetBackgroundColor(Color.ParseColor("#672E8A"));
            segmented.SetTextColor(Color.ParseColor("#ffffff"));

            switch (segmented.Id)
            {
                case Resource.Id.btn_segmented1:
                    segmentedIndexSelected = 0;
                    break;
                case Resource.Id.btn_segmented2:
                    segmentedIndexSelected = 1;
                    break;
                case Resource.Id.btn_segmented3:
                    segmentedIndexSelected = 2;
                    break;
            }
        }

        #endregion

        #region

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            if (this.FilterApplied !=null)
            {
                MenuInflater.Inflate(Resource.Menu.filter_menu, menu);
                menu.GetItem(0).SetShowAsAction(ShowAsAction.Always);
            }


            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == Resource.Id.filter)
            {
               
                Android.App.AlertDialog.Builder dialog = new Android.App.AlertDialog.Builder(this);
                Android.App.AlertDialog alert = dialog.Create();
                alert.SetTitle("Aviso");
                alert.SetMessage("¿Seguro que desea eliminar los filtros aplicados?");
                alert.SetButton("Si", (c, ev) =>
                {
                    Intent data = new Intent();

                    data.PutExtra(FILTER_RESULT, JsonConvert.SerializeObject(null));

                    SetResult(Result.Ok, data);
                    Finish();
                });

                alert.SetButton2("Cancelar", (c, ev) => { });

                alert.Show();
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        public override bool OnSupportNavigateUp()
        {

            OnBackPressed();
            return true;
        }


        #endregion
    }
}
