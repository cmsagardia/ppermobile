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
    public class FeaturesFragment : global::AndroidX.Fragment.App.Fragment
    {
        // RecyclerView instance that displays the photo album:
        RecyclerView mRecyclerView;
        // Layout manager that lays out each card in the RecyclerView:
        RecyclerView.LayoutManager mLayoutManager;
        // Adapter that accesses the data set (features):
        FeaturesListAdapter mAdapter;
        // It's use to save in memory the features gotten from server, the features will be filtered from this list
        List<Feature> FeatureFromServer;

        User user;

        // Save in runtime the filter that the user is using to get the list of features
        private FilterFeatureData FilterApplied;

        private readonly int FILTERS_SCREEN = 9;
        private readonly string FILTER_APPLIED = "FILTER_APPLIED";
        private readonly int FEATURE_DETAIL_CODE = 48;

        View progressOverlay;

        Google.Android.Material.FloatingActionButton.FloatingActionButton addFeatureButton;
        private readonly int ADD_FEATURE_CODE = 10;

        public static FeaturesFragment newInstance()
        {
            Bundle args = new Bundle();
            FeaturesFragment fragment = new FeaturesFragment();
            fragment.Arguments = args;
            return fragment;
        }
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            System.Diagnostics.Debug.WriteLine("FeaturesFragment.OnCreateView");
            View view = inflater.Inflate(Resource.Layout.FeaturesFragment, null);

            FeatureFromServer = new List<Feature>();

            mRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.EventsRecyclerView);
            mLayoutManager = new LinearLayoutManager(Activity);
            mRecyclerView.SetLayoutManager(mLayoutManager);

            progressOverlay = view.FindViewById<FrameLayout>(Resource.Id.progress_overlay);

            addFeatureButton = view.FindViewById<Google.Android.Material.FloatingActionButton.FloatingActionButton>(Resource.Id.floatingActionButton1);
            addFeatureButton.Click += delegate
            {
                FeatureDataHolder.getInstance().setData(null);
                var addFeatureActivity = new Intent(this.Context, typeof(AddFeatureActivity));
                StartActivityForResult(addFeatureActivity, ADD_FEATURE_CODE);
            };

            //clear search textfield
            Button cleanButton = view.FindViewById<Button>(Resource.Id.cleanEventTextFilter);
            cleanButton.Click += (sender, e) => {
                EditText eventSearched = view.FindViewById<EditText>(Resource.Id.editTextSearchEvent);
                eventSearched.Text = "";
            };

            // Search EditText
            EditText editSearch = view.FindViewById<EditText>(Resource.Id.editTextSearchEvent);
            editSearch.EditorAction += (object sender, EditText.EditorActionEventArgs e) =>
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
            GetFeaturesFromServer();
            GetActiveSectionsFromServer();
            GetSectionsByLevelFromServer();
            GetGuardResponsableFromServer();
            //GetUserInfo();
        }
        public void ShowProgressDialog(bool show)
        {
            progressOverlay.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
        }

        private void SetAdapterWithFilterText(string searchText)
        {
            if (mAdapter == null)
            {
                mAdapter = new FeaturesListAdapter(FeatureFromServer, searchText);
                mAdapter.ItemClick += OnItemClick;
                mRecyclerView.SetAdapter(mAdapter);
            }
            else
            {
                mAdapter.updateData(FeatureFromServer, searchText);
                mAdapter.NotifyDataSetChanged();
            }

            if (FeatureFromServer.Count() > 0)
            {
                mRecyclerView.SmoothScrollToPosition(0);
            }
        }
        private void GetUserInfo()
        {
            Task.Run(async () =>
            {
                try
                {
                   this.user  = await AysaClient.Instance.GetUserInfo();

                    Activity.RunOnUiThread(() =>
                    {
                        UserSession.Instance.Id = this.user.Id;
                        UserSession.Instance.nomApel = this.user.NombreApellido;

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
                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
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
        private void GetFeaturesFromServer()
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

                    List<Feature> features = await AysaClient.Instance.GetFeaturesByFilter(FilterApplied);
                    this.FeatureFromServer = features.OrderByDescending(m => m.Date).ToList();

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
                        ShowErrorAlert($"No se pudo obtener el listado de novedades");
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

        private FilterFeatureData BuildDefaultFilter()
        {
            DateTime fromDate = DateTime.Now.AddDays(-30);
            DateTime toDate = DateTime.Now;
            // Build filter objects
            Section dummySector = new Section();
            dummySector.Nombre = "Todas";
            dummySector.Id = "000000000";
            dummySector.Nivel = 0;
            dummySector.ResponsablesGuardia = null;

            FilterFeatureData filter = new FilterFeatureData();
            filter.FromDate = fromDate;
            filter.ToDate = toDate;
            filter.Sector = dummySector;
            filter.Username = "";
            filter.Detail = "";

            return filter;
        }

        private void clearListData()
        {
            if (mAdapter != null)
            {
                mAdapter.updateData(new List<Feature>(), "");
                mAdapter.NotifyDataSetChanged();

                if (FeatureFromServer.Count() > 0)
                {
                    mRecyclerView.SmoothScrollToPosition(0);
                }
            }
        }

        
        public void GetSectionsByLevelFromServer()
        {
            Task.Run(async () =>
            {
                try
                {
                    List<Section> sectionsByLevelList = await AysaClient.Instance.GetSectionsByUserLevel();

                    Activity.RunOnUiThread(() =>
                    {
                        // Save user sections active
                        UserSession.Instance.SectionsByLevel = sectionsByLevelList;

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
            });

        }

        public void GetActiveSectionsFromServer()
        {
            Task.Run(async () =>
            {
                try
                {
                    List<Section> activeSectionsList = await AysaClient.Instance.GetActiveSections();

                    Activity.RunOnUiThread(() =>
                    {
                        // Save user sections active
                        UserSession.Instance.ActiveSections = activeSectionsList;

                        SetUpViewAccordingUserPermissions();
                    });

                }
                catch (HttpUnauthorized)
                {
                    //Activity.RunOnUiThread(() =>
                    //{
                    //    ShowErrorAlert("Sin autorización para operar.");
                    //});
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
                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
            });

        }
        private void SetUpViewAccordingUserPermissions()
        {
            addFeatureButton.Visibility = ViewStates.Gone;

            if (UserSession.Instance.CheckIfUserHasPermission(Permissions.CrearEvento))
            {

                if (UserSession.Instance.CheckIfUserHasActiveSections())
                {
                    addFeatureButton.Visibility = ViewStates.Visible;
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

        void OnItemClick(object sender, Feature eventSelected)
        {
            FeatureDataHolder.getInstance().setData(eventSelected);
            var detailFeatureActivity = new Intent(this.Context, typeof(FeatureDetailActivity));
            StartActivityForResult(detailFeatureActivity, FEATURE_DETAIL_CODE);
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
                    Intent filterFeatures = new Intent(Application.Context, typeof(FilterFeaturesActivity));
                    if (FilterApplied != null)
                    {
                        filterFeatures.PutExtra(FILTER_APPLIED, JsonConvert.SerializeObject(FilterApplied));
                    }

                    StartActivityForResult(filterFeatures, FILTERS_SCREEN);
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
                    String result = data.GetStringExtra(FilterFeaturesActivity.FILTER_RESULT);
                    FilterApplied = JsonConvert.DeserializeObject<FilterFeatureData>(result);

                    GetFeaturesFromServer();
                }
            }
            else if (requestCode == ADD_FEATURE_CODE || requestCode == FEATURE_DETAIL_CODE)
            {
                if (resultCode == 0 || resultCode == -1)
                {
                    GetFeaturesFromServer();
                }
            }
        }

        #endregion
    }
    // Implement the ViewHolder pattern: each ViewHolder holds references
    // to the UI components (ImageView and TextView) within the CardView 
    // that is displayed in a row of the RecyclerView:
    public class FeatureViewHolder : RecyclerView.ViewHolder
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
        public FeatureViewHolder(View itemView) : base(itemView)
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
    public class FeaturesListAdapter : RecyclerView.Adapter
    {
        // Event handler for item clicks:
        public event EventHandler<Feature> ItemClick;

        public List<Feature> mFeature;

        public FeaturesListAdapter(List<Feature> featuresList, string searchText)
        {
            updateData(featuresList, searchText);
        }

        public void updateData(List<Feature> featuresList, string searchText)
        {
            mFeature = new List<Feature>();
            if (searchText == "")
            {
                mFeature.AddRange(featuresList);
            }
            else
            {
                foreach (var ev in featuresList)
                {
                    if (ev.Detail.ToLower().Contains(searchText.ToLower())  ||
                        ev.Sector.Nombre.ToLower().Contains(searchText)     ||
                        ev.Usuario.NombreApellido.ToLower().Contains(searchText)    ||
                        ev.Date.ToString().Contains(searchText))
                    {
                        mFeature.Add(ev);
                    }
                }
            }
        }
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.EventCardView, parent, false);
            FeatureViewHolder vh = new FeatureViewHolder(itemView);

            Typeface iconFont = FontManager.getTypeface(parent.Context, FontManager.FONTAWESOME);
            vh.textIconLocation.Typeface = iconFont;
            vh.textIconUser.Typeface = iconFont;
            vh.textIconDate.Typeface = iconFont;

            vh.ItemView.Click += (sender, e) => OnClick(vh);

            return vh;
        }
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            FeatureViewHolder vh = holder as FeatureViewHolder;
            vh.textTitle.Text = "#" + mFeature[position].Detail;
            vh.textDate.Text = mFeature[position].Date.ToString(AysaConstants.FormatDate);
            vh.textUser.Text = mFeature[position].Usuario.NombreApellido;
            vh.textLocation.Text = mFeature[position].Sector.Nombre;
            vh.textStatus.Visibility = ViewStates.Invisible;

            Feature ev = mFeature[position];


        }
        public override int ItemCount
        {
            get
            {
                return mFeature.Count();
            }
        }
        // Raise a feature when the item-click takes place:
        void OnClick(FeatureViewHolder viewHolder)
        {
            int position = viewHolder.AdapterPosition;
            Feature ev = mFeature[position];

            if (ItemClick != null)
                ItemClick(this, ev);
        }
    }
}