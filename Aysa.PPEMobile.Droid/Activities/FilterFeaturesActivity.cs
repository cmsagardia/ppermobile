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
    [Activity(Label = "FilterFeaturesActivity")]
    public class FilterFeaturesActivity : AppCompatActivity
    {
        public static readonly string FILTER_RESULT = "FILTER_RESULT";
        private readonly string FILTER_APPLIED = "FILTER_APPLIED";

        EditText UsernameText;
        EditText DetailText;
        EditText fromDateText;
        EditText toDateText;
        Spinner spinnerSectors;

        List<Section> ActiveSectionsList;
        // It's using to get in runtime the filter that the user is using
        public FilterFeatureData FilterApplied;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.FilterFeature);

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

            UsernameText = FindViewById<EditText>(Resource.Id.nro_event_text_field);
            DetailText = FindViewById<EditText>(Resource.Id.title_text_field);
            // Sector field
            spinnerSectors = FindViewById<Spinner>(Resource.Id.spinnerSectors);


            Button btnAplicar = FindViewById<Button>(Resource.Id.btn_aplicar);
            btnAplicar.Click += BtnAplicar_Click;

            SetUpDateFields();

            // Get filter applied
            if (Intent.GetStringExtra(FILTER_APPLIED) != null)
            {
                this.FilterApplied = JsonConvert.DeserializeObject<FilterFeatureData>(Intent.GetStringExtra(FILTER_APPLIED));
                LoadFilterSavedInView();
            }

            LoadActiveSectionsInView();
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
            UsernameText.Text = FilterApplied.Username != "" ? FilterApplied.Username : "";
            DetailText.Text = FilterApplied.Detail;


            fromDateText.Text = FilterApplied.FromDate.ToString(AysaConstants.FormatDate);
            toDateText.Text = FilterApplied.ToDate.ToString(AysaConstants.FormatDate);

        }

        private FilterFeatureData ApplyFilter()
        {
            // Build filter
            // Get filter data from IBOutlets

            // Try to get Number of Feature


            UsernameText = FindViewById<EditText>(Resource.Id.nro_event_text_field);
            string userName = UsernameText.Text;

            DetailText = FindViewById<EditText>(Resource.Id.title_text_field);
            string title = DetailText.Text;


            // Build filter objects
            Section auxSector = ActiveSectionsList[spinnerSectors.SelectedItemPosition];
            FilterFeatureData filter = new FilterFeatureData();
            filter.Username = userName;
            filter.Detail = title;
            filter.FromDate = DateTime.ParseExact(fromDateText.Text, AysaConstants.FormatDate, null);
            filter.ToDate = DateTime.ParseExact(toDateText.Text, AysaConstants.FormatDate, null);
            filter.Sector = auxSector;

            return filter;


        }

        #endregion

        #region Actions


        void BtnAplicar_Click(object sender, System.EventArgs e)
        {
            FilterFeatureData filter = ApplyFilter();
            FinishWithResult(filter);
        }

        private void FinishWithResult(FilterFeatureData filter)
        {
            Intent data = new Intent();

            data.PutExtra(FILTER_RESULT, JsonConvert.SerializeObject(filter));

            SetResult(Result.Ok, data);
            Finish();
        }


        #endregion

        #region

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            if (this.FilterApplied != null)
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

        public void LoadActiveSectionsInView()
        {
            Section dummySector = new Section();
            dummySector.Nombre = "Todas";
            dummySector.Id = "000000000";
            dummySector.Nivel = 0;
            dummySector.ResponsablesGuardia = null;

            var auxList = UserSession.Instance.ActiveSections;
            List<Section> SectorGuardList = UserSession.Instance.PersonInGuard.Sectores;

            if(auxList == null)
                auxList = new List<Section>();

            bool contains = auxList.Exists(x => x.Nombre == "Todas");
            if(!contains)
            {
                auxList.Insert(0, dummySector);
            }

            ActiveSectionsList = auxList;

            // After get Sections Active list, load elements in PickerView
            if (ActiveSectionsList != null && ActiveSectionsList.Count > 0)
            {
                spinnerSectors.Enabled = true;

                ArrayAdapter adapter = new ArrayAdapter(this, global::Android.Resource.Layout.SimpleSpinnerItem, ActiveSectionsList.ToArray());
                spinnerSectors.Adapter = adapter;

                for (int i = 0; i < ActiveSectionsList.Count(); i++)
                {
                    for (int j = 0; j < SectorGuardList.Count(); j++)
                    {
                        if (ActiveSectionsList[i].Nombre.Equals(SectorGuardList[j].Nombre))
                        {
                            spinnerSectors.SetSelection(i);
                            break;
                        }
                    }
                }
                spinnerSectors.SetSelection(ActiveSectionsList.IndexOf(dummySector));
            }
            else
            {
                spinnerSectors.Enabled = true;

                List<Section> auxSections = new List<Section>();
                auxSections.Add(dummySector);
                ArrayAdapter adapter = new ArrayAdapter(this, global::Android.Resource.Layout.SimpleSpinnerItem, auxSections.ToArray());
                spinnerSectors.Adapter = adapter;
            }
        }


        #endregion
    }
}