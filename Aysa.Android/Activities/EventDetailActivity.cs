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
using Android.Support.V7.App;
using Aysa.PPEMobile.Model;
using Aysa.Android.Utilities;

namespace Aysa.Android.Activities
{
    [Activity(Label = "EventDetailActivity")]
    public class EventDetailActivity : AppCompatActivity
    {
        FrameLayout progressOverlay;
        private Event mEvent;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.mEvent = EventDataHolder.getInstance().getData();

            SetContentView(Resource.Layout.EventDetail);

            // Progress indicator
            progressOverlay = FindViewById<FrameLayout>(Resource.Id.progress_overlay);

            // Set toolbar and title
            global::Android.Support.V7.Widget.Toolbar toolbar = (global::Android.Support.V7.Widget.Toolbar)FindViewById(Resource.Id.toolbar);
            toolbar.Title = "Evento #" + mEvent.NroEvento.ToString();

            // Add back button
            SetSupportActionBar(toolbar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            //Button btnCreate = FindViewById<Button>(Resource.Id.btnEventCreate);
            //btnCreate.Click += BtnCreate_Click;

            //titleEditText = FindViewById<EditText>(Resource.Id.editTextEventTitle);
            //observationsEditText = FindViewById<EditText>(Resource.Id.editTextEventObservaciones);
            //dateEditText = FindViewById<EditText>(Resource.Id.editTextEventDate);
            //placeEditText = FindViewById<EditText>(Resource.Id.editTextEventPlace);
            //detailEditText = FindViewById<EditText>(Resource.Id.editTextEventDetail);
            //tagsEditText = FindViewById<EditText>(Resource.Id.editTextEventTags);
            //referenceEditText = FindViewById<EditText>(Resource.Id.editTextEventReference);
            //generalEditText = FindViewById<EditText>(Resource.Id.editTextEventObservaciones);

            // Date Field
            //dateEditText.Click += DateEditText_Click;

            // Type field
            //spinnerType = FindViewById<Spinner>(Resource.Id.spinnerType);
            //spinnerType.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelectedType);
            //// Sector field
            //spinnerSectors = FindViewById<Spinner>(Resource.Id.spinnerSectors);
            //spinnerSectors.ItemSelected += new EventHandler<AdapterView.ItemSelectedEventArgs>(spinner_ItemSelectedSector);


            //// Load lists of values from server
            //GetEventTypesFromServer();

            //LoadActiveSectionsInView();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            Finish();
            return base.OnOptionsItemSelected(item);
        }

    }
}