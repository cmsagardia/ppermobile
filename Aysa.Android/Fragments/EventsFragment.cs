using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Aysa.Android.Activities;
using Android.Support.Design.Widget;
using System.Threading.Tasks;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using Android.Support.V7.Widget;
using Android.Graphics;
using Aysa.Android.Utilities;
using Android.Views.InputMethods;

namespace Aysa.Android.Fragments
{
    public class EventsFragment : global::Android.Support.V4.App.Fragment
    {
        // This flag prevent view to make duplicated requests
        private bool IsNetworkWorking = false;
        // RecyclerView instance that displays the photo album:
        RecyclerView mRecyclerView;
        // Layout manager that lays out each card in the RecyclerView:
        RecyclerView.LayoutManager mLayoutManager;
        // Adapter that accesses the data set (events):
        EventsListAdapter mAdapter;
        // It's use to save in memory the events gotten from server, the events will be filtered from this list
        List<Event> EventsFromServer;
        // Save in runtime the filter that the user is using to get the list of events
        private FilterEventData FilterApplied;

        HomeActivity homeActivity;


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.EventsFragment, container, false);


            homeActivity = this.Activity as HomeActivity;

            EventsFromServer = new List<Event>();

            // Get our RecyclerView layout:
            mRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.EventsRecyclerView);

            // Use the built-in linear layout manager:
            mLayoutManager = new LinearLayoutManager(this.Activity);

            // Plug the layout manager into the RecyclerView:
            mRecyclerView.SetLayoutManager(mLayoutManager);


            FloatingActionButton button = view.FindViewById<FloatingActionButton>(Resource.Id.floatingActionButton1);
            button.Click += delegate {
                var addEventActivity = new Intent(this.Context, typeof(AddEventActivity));
                StartActivity(addEventActivity);
            };

            // Search EditText
            EditText editSearch = view.FindViewById<EditText>(Resource.Id.editTextSearchEvent);
            editSearch.EditorAction += (object sender, TextView.EditorActionEventArgs e) => {

                if (e.ActionId == ImeAction.Done)
                {
                    EditText et = sender as EditText;
                    SetAdapterWithFilterText( et.Text );
                }
                e.Handled = false;
            };

            // Get open events form server
            GetOpenEventsFromServer();

            // Get active sections 
            GetActiveSectionsFromServer();

            return view;
        }


        private void SetAdapterWithFilterText(string searchText)
        {
            // Instantiate the adapter and pass in its data source:
            mAdapter = new EventsListAdapter(EventsFromServer, searchText);

            // Register the item click handler (below) with the adapter:
            mAdapter.ItemClick += OnItemClick;

            // Plug the adapter into the RecyclerView:
            mRecyclerView.SetAdapter(mAdapter);

            if (mAdapter.ItemCount > 0)
            {
                mRecyclerView.SmoothScrollToPosition(0);
            }
        }


        private void GetOpenEventsFromServer()
        {

            // If the fragment is making a request, don't allow make another request until the first request finishes
            if (IsNetworkWorking)
            {
                return;
            }

            // Display an Activity Indicator in the status bar
            homeActivity.ShowProgressDialog(true);
            IsNetworkWorking = true;

            Task.Run(async () =>
            {

                try
                {

                    List<Event> events;

                    // If there are filters, get events by filters
                    if (FilterApplied != null)
                    {
                        events = await AysaClient.Instance.GetEventsByFilter(FilterApplied);
                    }
                    else
                    {
                        // If there aren't filters, get events by default
                        events = await AysaClient.Instance.GetOpenEvents();
                    }

                    EventsFromServer = events;

                    this.Activity.RunOnUiThread(() =>
                    {
                        // Instantiate the adapter and pass in its data source:
                        SetAdapterWithFilterText("");

                    });

                }
                catch (HttpUnauthorized)
                {
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                catch (Exception ex)
                {
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(ex.Message);
                    });
                }
                finally
                {
                    // Dismiss an Activity Indicator in the status bar
                    homeActivity.ShowProgressDialog(false);
                    IsNetworkWorking = false;
                }
            });

        }

        public void GetActiveSectionsFromServer()
        {
            // Get active sections from server

            Task.Run(async () =>
            {

                try
                {
                    List<Section> activeSectionsList = await AysaClient.Instance.GetActiveSections();

                    this.Activity.RunOnUiThread(() =>
                    {
                        // Save user sections active
                        UserSession.Instance.ActiveSections = activeSectionsList;

                        //SetUpViewAccordingUserPermissions();
                    });

                }
                catch (HttpUnauthorized)
                {
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowErrorAlert("Sesión expirada.");
                    });
                }
                catch (Exception ex)
                {
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(ex.Message);
                    });
                }
                finally
                {
                }
            });

        }

        private void ShowErrorAlert(string message)
        {
            global::Android.Support.V7.App.AlertDialog.Builder alert = new global::Android.Support.V7.App.AlertDialog.Builder(this.Activity);
            alert.SetTitle("Error");
            alert.SetMessage(message);

            Dialog dialog = alert.Create();
            dialog.Show();
        }

        private void ShowSessionExpiredError()
        {
            global::Android.Support.V7.App.AlertDialog.Builder alert = new global::Android.Support.V7.App.AlertDialog.Builder(this.Activity);
            alert.SetTitle("Aviso");
            alert.SetMessage("Su sesión ha expirado, por favor ingrese sus credenciales nuevamente");

            Dialog dialog = alert.Create();
            dialog.Show();
        }

        void OnItemClick(object sender, Event eventSelected)
        {
            EventDataHolder.getInstance().setData(eventSelected);
            var detailEventActivity = new Intent(this.Context, typeof(EventDetailActivity));
            StartActivity(detailEventActivity);
        }

    }


    // Implement the ViewHolder pattern: each ViewHolder holds references
    // to the UI components (ImageView and TextView) within the CardView 
    // that is displayed in a row of the RecyclerView:
    public class EventViewHolder : RecyclerView.ViewHolder
    {
        public TextView textTitle { get; private set; }

        public TextView textIconLocation { get; private set; }
        public TextView textIconUser { get; private set; }
        public TextView textIconDate { get; private set; }
        public TextView textLocation { get; private set; }
        public TextView textUser { get; private set; }
        public TextView textDate { get; private set; }
        public TextView textStatus { get; private set; }

        // Get references to the views defined in the CardView layout.
        public EventViewHolder(View itemView)  : base(itemView)
        {
            // Locate and cache view references:
            textTitle = itemView.FindViewById<TextView>(Resource.Id.textViewCVTitle);
            textIconLocation = itemView.FindViewById<TextView>(Resource.Id.textViewCVIconLocation);
            textIconUser = itemView.FindViewById<TextView>(Resource.Id.textViewCVIconUser);
            textLocation = itemView.FindViewById<TextView>(Resource.Id.textViewCVLocation);
            textUser = itemView.FindViewById<TextView>(Resource.Id.textViewCVUser);
            textIconDate = itemView.FindViewById<TextView>(Resource.Id.textViewCVIconDate);
            textDate = itemView.FindViewById<TextView>(Resource.Id.textViewCVDate);
            textStatus = itemView.FindViewById<TextView>(Resource.Id.textViewCVStatus);
        }
    }

    public class EventsListAdapter : RecyclerView.Adapter
    {
        // Event handler for item clicks:
        public event EventHandler<Event> ItemClick;

        public List<Event> mEvents;

        public EventsListAdapter(List<Event> eventsList, string searchText)
        {
            mEvents = new List<Event>();
            if (searchText=="") {
                mEvents.AddRange(eventsList);
            } else {
                foreach(var ev in eventsList) {
                    if (ev.Titulo.ToLower().Contains(searchText.ToLower())) {
                        mEvents.Add(ev);
                    }
                }
            }
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.EventCardView, parent, false);
            EventViewHolder vh = new EventViewHolder(itemView);

            Typeface iconFont = FontManager.getTypeface(parent.Context, FontManager.FONTAWESOME);
            vh.textIconLocation.Typeface = iconFont;
            vh.textIconUser.Typeface = iconFont;
            vh.textIconDate.Typeface = iconFont;

            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            EventViewHolder vh = holder as EventViewHolder;
            vh.textTitle.Text = "#"+ mEvents[position].NroEvento.ToString()+ " " + mEvents[position].Titulo;
            vh.textDate.Text = mEvents[position].Fecha.ToShortDateString();
            vh.textUser.Text = mEvents[position].Usuario.NombreApellido;
            vh.textLocation.Text = mEvents[position].Lugar;
            vh.textStatus.Text = mEvents[position].Estado==1?"Abierto":"Cerrado";

            Event ev = mEvents[position];
            vh.ItemView.Click += (sender, e) => OnClick(ev);
        }

        public override int ItemCount
        {
            get { 
                return mEvents.Count(); 
            }
        }

        // Raise an event when the item-click takes place:
        void OnClick(Event ev)
        {
            if (ItemClick != null)
                ItemClick(this, ev);
        }
    }

}