using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Aysa.PPEMobile.Droid.Activities;
using System.Threading.Tasks;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using Android.Graphics;
using Android.Views.InputMethods;
using Aysa.PPEMobile.Droid.Utilities;
using Newtonsoft.Json;
using Android.Text;
using Android.Text.Style;
using AndroidX.RecyclerView.Widget;
using FloatingActionButtonBasic;

namespace Aysa.PPEMobile.Droid.Fragments
{
    public class EventsFragment : global::AndroidX.Fragment.App.Fragment
    {
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

        private readonly int FILTERS_SCREEN = 9;
        private readonly string FILTER_APPLIED = "FILTER_APPLIED";
        private readonly int EVENT_DETAIL_CODE = 48;

        View progressOverlay;

        Google.Android.Material.FloatingActionButton.FloatingActionButton addEventButton;
        private readonly int ADD_EVENT_CODE = 10;

        public static EventsFragment newInstance()
        {
            Bundle args = new Bundle();
            EventsFragment fragment = new EventsFragment();
            fragment.Arguments = args;
            return fragment;
        }


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            System.Diagnostics.Debug.WriteLine("EventsFragment.OnCreateView");
            View view = inflater.Inflate(Resource.Layout.EventsFragment, null);

            EventsFromServer = new List<Event>();

            mRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.EventsRecyclerView);
            mLayoutManager = new LinearLayoutManager(Activity);
            mRecyclerView.SetLayoutManager(mLayoutManager);

            progressOverlay = view.FindViewById<FrameLayout>(Resource.Id.progress_overlay);

            addEventButton = view.FindViewById<Google.Android.Material.FloatingActionButton.FloatingActionButton>(Resource.Id.floatingActionButton1);
            addEventButton.Click += delegate
            {
                EventDataHolder.getInstance().setData(null);
                var addEventActivity = new Intent(this.Context, typeof(AddEventActivity));
                StartActivityForResult(addEventActivity, ADD_EVENT_CODE);
            };

            //clear search textfield
            Button cleanButton = view.FindViewById<Button>(Resource.Id.cleanEventTextFilter);
            cleanButton.Click += (sender, e) => {
                EditText eventSearched = view.FindViewById<EditText>(Resource.Id.editTextSearchEvent);
                eventSearched.Text = "";
            };

            // Search EditText
            EditText editSearch = view.FindViewById<EditText>(Resource.Id.editTextSearchEvent);
            editSearch.EditorAction += (object sender, TextView.EditorActionEventArgs e) =>
            {

                if (e.ActionId == ImeAction.Done)
                {
                    EditText et = sender as EditText;
                    SetAdapterWithFilterText(et.Text);
                }
                e.Handled = false;
            };

            editSearch.TextChanged += (o, e) =>
            {
                if (editSearch.Text.Length == 0)
                {
                    SetAdapterWithFilterText(editSearch.Text);
                }
            };

            return view;
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            GetEventsFromServer();
            GetActiveSectionsFromServer();
            GetGuardResponsableFromServer();
        }

        public void ShowProgressDialog(bool show)
        {
            progressOverlay.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
        }

        private void SetAdapterWithFilterText(string searchText)
        {
            if (mAdapter == null)
            {
                mAdapter = new EventsListAdapter(EventsFromServer, searchText);
                mAdapter.ItemClick += OnItemClick;
                mRecyclerView.SetAdapter(mAdapter);
            }
            else
            {
                mAdapter.updateData(EventsFromServer, searchText);
                mAdapter.NotifyDataSetChanged();
            }

            if (EventsFromServer.Count() > 0)
            {
                mRecyclerView.SmoothScrollToPosition(0);
            }
        }


        private void GetEventsFromServer()
        {
            ShowProgressDialog(true);
            clearListData();

            Task.Run(async () =>
            {
                try
                {
                    if (FilterApplied == null)
                    {
                        FilterApplied = BuildDefaultFilter();
                    }

                    this.EventsFromServer = await AysaClient.Instance.GetEventsByFilter(FilterApplied);

                    if (this.EventsFromServer.Count > 1)
                        this.EventsFromServer = this.EventsFromServer.OrderByDescending(x => x.Fecha).ToList();

                    Activity.RunOnUiThread(() =>
                    {
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
                catch (NetworkConnectionException)
                {
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionNetworkMessage);
                    });
                }
                catch (Exception ex)
                {
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowErrorAlert($"No se pudo obtener el listado de eventos!");
                    });
                }
                finally
                {
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowProgressDialog(false);
                    });
                }
            });

        }

        private FilterEventData BuildDefaultFilter()
        {
            DateTime fromDate = DateTime.Now.AddDays(-30);
            DateTime toDate = DateTime.Now;

            var filter = new FilterEventData
            {
                FromDate = fromDate,
                ToDate = toDate,
                Status = -1,
                EventNumber = 0,
                Title = ""
            };

            return filter;
        }

        private void clearListData()
        {
            if (mAdapter != null)
            {
                mAdapter.updateData(new List<Event>(), "");
                mAdapter.NotifyDataSetChanged();

                if (EventsFromServer.Count() > 0)
                {
                    mRecyclerView.SmoothScrollToPosition(0);
                }
            }
        }

        public void GetActiveSectionsFromServer()
        {
            Task.Run(async () =>
            {
                try
                {
                    List<Section> activeSectionsList = await AysaClient.Instance.GetActiveSections();
                    var userLogged = await AysaClient.Instance.GetUserInfo();
                    Activity.RunOnUiThread(() =>
                    {
                        // Save user sections active
                        UserSession.Instance.ActiveSections = activeSectionsList;
                        UserSession.Instance.Id = userLogged.Id;
                        UserSession.Instance.nomApel = userLogged.NombreApellido;
                        UserSession.Instance.Roles = userLogged.Roles;
                        SetUpViewAccordingUserPermissions();
                    });

                }
                catch (HttpUnauthorized)
                {
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                catch (NetworkConnectionException)
                {
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionNetworkMessage);
                    });
                }
                catch (Exception ex)
                {
                    Activity.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
                finally
                {
                }
            });

        }

        public void GetGuardResponsableFromServer()
        {
            // Get person in guard from server

            Task.Run(async () =>
            {

                try
                {
                    PersonGuard personGuard = await AysaClient.Instance.GetGuardResponsableByName(UserSession.Instance.UserName);

                    Activity.RunOnUiThread(() =>
                    {
                        // Save person in guard in user session
                        UserSession.Instance.PersonInGuard = personGuard;
                    });

                }
                catch (HttpUnauthorized)
                {
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                catch (NetworkConnectionException)
                {
                    this.Activity.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionNetworkMessage);
                    });
                }
                catch (Exception ex)
                {
                    Activity.RunOnUiThread(() =>
                    {
                        ShowErrorAlert($"No se pudo obtener la información del Responsable de Guardia");
                    });
                }
            });

        }

        private void SetUpViewAccordingUserPermissions()
        {
            addEventButton.Visibility = ViewStates.Gone;

            if (UserSession.Instance.CheckIfUserHasPermission(Permissions.CrearEvento))
            {
                // To allow the user crate an event, the user need to have active guard section
                if (UserSession.Instance.CheckIfUserHasActiveSections())
                {
                    addEventButton.Visibility = ViewStates.Visible;
                }
            }

        }

        private void ShowErrorAlert(string message)
        {
            if (string.IsNullOrEmpty(message))
                return;

            global::AndroidX.AppCompat.App.AlertDialog.Builder alert = new global::AndroidX.AppCompat.App.AlertDialog.Builder(Activity);
            alert.SetTitle("Error");
            alert.SetMessage(message);

            Dialog dialog = alert.Create();
            dialog.Show();
        }

        private void ShowSessionExpiredError()
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(this.Activity);
            AlertDialog alert = dialog.Create();
            alert.SetTitle("Aviso");
            alert.SetMessage("Su sesión ha expirado");
            alert.SetButton("Aceptar", (c, ev) =>
            {
                Intent shortDialDetails = new Intent(Application.Context, typeof(LoginActivity));
                StartActivity(shortDialDetails);
                this.Activity.Finish();
            });

            alert.Show();
        }

        void OnItemClick(object sender, Event eventSelected)
        {
            EventDataHolder.getInstance().setData(eventSelected);
            var detailEventActivity = new Intent(this.Context, typeof(EventDetailActivity));
            StartActivityForResult(detailEventActivity, EVENT_DETAIL_CODE);
        }

        #region Menu Options
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.filter_menu, menu);

            IMenuItem item = menu.FindItem(Resource.Id.filter);
            SpannableString s = new SpannableString(item.TitleCondensedFormatted);
            s.SetSpan(new ForegroundColorSpan(Color.ParseColor("#672E8A")), 0, s.Length(), 0);

            item.SetTitle(s);

            base.OnCreateOptionsMenu(menu, inflater);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.filter:
                    Intent filterEvents = new Intent(Application.Context, typeof(FilterEventsActivity));
                    if (FilterApplied != null)
                    {
                        filterEvents.PutExtra(FILTER_APPLIED, JsonConvert.SerializeObject(FilterApplied));
                    }

                    StartActivityForResult(filterEvents, FILTERS_SCREEN);
                    return true;
            }

            return false;
        }


        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode == FILTERS_SCREEN)
            {
                //Result.ok = -1
                if (resultCode == -1)
                {
                    String result = data.GetStringExtra(FilterEventsActivity.FILTER_RESULT);
                    FilterApplied = JsonConvert.DeserializeObject<FilterEventData>(result);

                    GetEventsFromServer();
                }
            }
            else if (requestCode == ADD_EVENT_CODE || requestCode == EVENT_DETAIL_CODE)
            {
                if (resultCode == -1)
                {
                    GetEventsFromServer();
                }
            }
        }

        #endregion
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
        public View verticalLine { get; private set; }
        public View separatorLine { get; private set; }

        // Get references to the views defined in the CardView layout.
        public EventViewHolder(View itemView) : base(itemView)
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
            separatorLine = itemView.FindViewById<View>(Resource.Id.separator_line);
            verticalLine = itemView.FindViewById<View>(Resource.Id.vertical_line);
        }
    }

    public class EventsListAdapter : RecyclerView.Adapter
    {
        // Event handler for item clicks:
        public event EventHandler<Event> ItemClick;

        public List<Event> mEvents;

        public EventsListAdapter(List<Event> eventsList, string searchText)
        {
            updateData(eventsList, searchText);
        }

        public void updateData(List<Event> eventsList, string searchText)
        {
            mEvents = new List<Event>();
            if (searchText == "")
            {
                mEvents.AddRange(eventsList);
            }
            else
            {
                foreach (var ev in eventsList)
                {
                    if (
                            ev.Titulo.ToLower().Contains(searchText.ToLower())  || 
                            ev.NroEvento.ToString().Contains(searchText)        ||
                            ev.Lugar.ToLower().Contains(searchText.ToLower())   ||
                            ev.Usuario.NombreApellido.ToLower().Contains(searchText.ToLower()) ||
                            ev.Fecha.ToString().Contains(searchText)
                        )
                    {
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

            vh.ItemView.Click += (sender, e) => OnClick(vh);

            return vh;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            EventViewHolder vh = holder as EventViewHolder;
            vh.textTitle.Text = "#" + mEvents[position].NroEvento.ToString() + " " + mEvents[position].Titulo;
            vh.textDate.Text = mEvents[position].Fecha.ToString(AysaConstants.FormatDate);
            vh.textUser.Text = mEvents[position].Usuario.NombreApellido;
            vh.textLocation.Text = mEvents[position].Lugar;
            vh.textStatus.Text = mEvents[position].Estado == 1 ? "Abierto" : "Cerrado";

            Event ev = mEvents[position];


            // Config style for close events
            if (mEvents[position].Estado == 2)
            {
                vh.textTitle.SetTextColor(Color.ParseColor("#545459"));
                vh.textStatus.SetTextColor(Color.ParseColor("#545459"));
                vh.textStatus.SetBackgroundResource(Resource.Drawable.round_status_close);
                vh.textStatus.SetCompoundDrawablesWithIntrinsicBounds(Resource.Drawable.close_folder_event, 0, 0, 0);

                vh.separatorLine.SetBackgroundColor(Color.ParseColor("#9D9CA3"));
                vh.verticalLine.SetBackgroundColor(Color.ParseColor("#9D9CA3"));
            }
        }

        public override int ItemCount
        {
            get
            {
                return mEvents.Count();
            }
        }

        // Raise an event when the item-click takes place:
        void OnClick(EventViewHolder viewHolder)
        {
            int position = viewHolder.AdapterPosition;
            Event ev = mEvents[position];

            if (ItemClick != null)
                ItemClick(this, ev);
        }
    }
}
