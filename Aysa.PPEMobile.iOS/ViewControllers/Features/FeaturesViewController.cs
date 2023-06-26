using System;
using Foundation;
using UIKit;
using Aysa.PPEMobile.iOS.ViewControllers.Features.BuilderFeaturesTableView;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;
using System.Linq;
using Aysa.PPEMobile.Service.HttpExceptions;
using Aysa.PPEMobile.iOS.ViewControllers.Features.FeatureSearch;
using Aysa.PPEMobile.iOS.Utilities;

namespace Aysa.PPEMobile.iOS.ViewControllers.Features
{
    public partial class FeaturesViewController : UIViewController, FilterFeaturesViewControllerDelegate
    {
        // Private variables

        // This flag is using to indicate that the controller is making a request
        private bool IsNetworkWorking = false;

        // This variable is needed to save the event that it has gotten from server with its all data and then it will passed to EventDetailViewController
        private Feature FeatureSelectedFull;

        // Save in runtime the filter that the user is using to get the list of events
        private FilterFeatureData FilterApplied;

        // Implement Search controller
        FeatureSearchResultController ResultSearchFeatureViewController;
        UISearchController SearchController;

        User user;


        // It's use to save in memory the events gotten from server, the events will be filtered from this list
        List<Feature> FeaturesFromServer;

        #region UIViewController Lifecyle 

        public FeaturesViewController(IntPtr handle) : base(handle)
        {

        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            // Change status bar text color to white
            NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;

            // Config and load elements in TableView
            SetUpTableView();

            // Config Style of Navigation Bar
            SetUpNavigationBarStyle();

            // Add Search bar
            InitSearchBarController();


        }


        public override void ViewWillAppear(Boolean animated)
        {
            base.ViewWillAppear(animated);
            //Get user info
            //GetUserInfo();
            
            // Get open events form server
            GetOpenFeaturesFromServer();

            // Assign to the user logged the active sections that the user have
            GetActiveSectionsFromServer();
            GetSectionsByLevelFromServer();
            // Assing to the user logged his guard responsable
            GetGuardResponsableFromServer();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }


        #endregion

        #region Internal Methods



        private void SetUpTableView()
        {
            // Register TableViewCell
            TableView.RegisterNibForCellReuse(FeaturesTableViewCell.Nib, "FeaturesTableViewCell");


            // Separator lines in the tableView were customize so it's not neccesary define in the TableView
            TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

        }

        private void InitSearchBarController()
        {
            // Init Result Controller, it's going to show when the user is searching
            ResultSearchFeatureViewController = new FeatureSearchResultController();

            // Init SearchController
            SearchController = new UISearchController(ResultSearchFeatureViewController)
            {
                WeakDelegate = this,
                DimsBackgroundDuringPresentation = false,
                WeakSearchResultsUpdater = this
            };

            SearchController.SearchBar.SizeToFit();
            SearchController.SearchBar.Placeholder = "Buscar";
            // SearchController.SearchBar.BarTintColor = UIColor.FromRGB(230, 232, 235);
            // SearchController.SearchBar.BackgroundColor = UIColor.FromRGB(230, 232, 235);
            // SearchController.SearchBar.BackgroundImage = new UIImage();

            TableView.TableHeaderView = SearchController.SearchBar;

            SearchController.SearchBar.WeakDelegate = this;
            SearchController.HidesNavigationBarDuringPresentation = false;

            DefinesPresentationContext = true;
        }

        private void SetUpNavigationBarStyle()
        {
            // Config back button for event detail
            var backButton = new UIBarButtonItem();
            backButton.Title = "Atrás";
            NavigationItem.BackBarButtonItem = backButton;

            // Change default color
            NavigationController.NavigationBar.TintColor = UIColor.White;
        }

        private void GetOpenFeaturesFromServer()
        {

            // If the controller is making a request, don't allow make another request until the first request finishes
            if (IsNetworkWorking)
            {
                return;
            }

            // Display an Activity Indicator in the status bar
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
            IsNetworkWorking = true;

            Task.Run(async () =>
            {

                try
                {
                    if (FilterApplied == null)
                    {
                        FilterApplied = BuildDefaultFilter();
                    }

                    List<Feature> features = await AysaClient.Instance.GetFeaturesByFilter(FilterApplied);

                    if (features != null)
                    {


                        this.FeaturesFromServer = features.OrderByDescending(m => m.Date).ToList();


                        InvokeOnMainThread(() =>
                        {
                        // Load data and reload TableView
                        TableView.Source = new FeaturesTableDataSource(FeaturesFromServer, this);
                            TableView.RowHeight = UITableView.AutomaticDimension;
                            TableView.EstimatedRowHeight = 280f;
                            TableView.ReloadData();
                        });
                    }

                }
                catch (HttpUnauthorized)
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                catch (Exception ex)
                {
                    InvokeOnMainThread(() =>
                    {
                        //ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
                finally
                {
                    // Dismiss an Activity Indicator in the status bar
                    UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
                    IsNetworkWorking = false;
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

        public void GetActiveSectionsFromServer()
        {
            // Get active sections from server

            // Display an Activity Indicator in the status bar
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            Task.Run(async () =>
            {

                try
                {
                    List<Section> activeSectionsList = await AysaClient.Instance.GetActiveSections();

                    InvokeOnMainThread(() =>
                    {
                        // Save user sections active
                        UserSession.Instance.ActiveSections = activeSectionsList;

                        SetUpViewAccordingUserPermissions();
                    });

                }
                catch (HttpUnauthorized)
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                catch (Exception ex)
                {
                    InvokeOnMainThread(() =>
                    {
                        //ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
                finally
                {
                    // Dismiss an Activity Indicator in the status bar
                    UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
                }
            });

        }

        public void GetSectionsByLevelFromServer()
        {
            // Get active sections from server

            // Display an Activity Indicator in the status bar
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            Task.Run(async () =>
            {

                try
                {
                    List<Section> sectionsByLevelList = await AysaClient.Instance.GetSectionsByUserLevel();

                    InvokeOnMainThread(() =>
                    {
                        // Save user sections active
                        UserSession.Instance.SectionsByLevel = sectionsByLevelList;

                        SetUpViewAccordingUserPermissions();
                    });

                }
                catch (HttpUnauthorized)
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                catch (Exception ex)
                {
                    InvokeOnMainThread(() =>
                    {
                        //ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
                finally
                {
                    // Dismiss an Activity Indicator in the status bar
                    UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
                }
            });

        }

        public void GetGuardResponsableFromServer()
        {
            // Get person in guard from server

            // Display an Activity Indicator in the status bar
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            Task.Run(async () =>
            {

                try
                {
                    PersonGuard personGuard = await AysaClient.Instance.GetGuardResponsableByName(UserSession.Instance.UserName);

                    InvokeOnMainThread(() =>
                    {
                        // Save person in guard in user session
                        UserSession.Instance.PersonInGuard = personGuard;

                    });

                }
                catch (HttpUnauthorized)
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                catch (Exception ex)
                {
                    InvokeOnMainThread(() =>
                    {
                        //ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
                finally
                {
                    // Dismiss an Activity Indicator in the status bar
                    UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
                }
            });

        }

        private void SetUpViewAccordingUserPermissions()
        {
            if (UserSession.Instance.CheckIfUserHasPermission(Permissions.CrearEvento))
            {
                // To allow the user crate an event, the user need to have active guard section
                CreateFeaureButton.Enabled = UserSession.Instance.CheckIfUserHasActiveSections();
            }

        }

        private void ShowErrorAlert(string message)
        {
            var alert = UIAlertController.Create("Error", message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
            PresentViewController(alert, true, null);
        }

        private void ShowSessionExpiredError()
        {
            UIAlertController alert = UIAlertController.Create("Aviso", "Su sesión ha expirado, por favor ingrese sus credenciales nuevamente", UIAlertControllerStyle.Alert);

            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, action => {
                // Send notification to TabBarController to return to login
                NSNotificationCenter.DefaultCenter.PostNotificationName("SessionExpired", this);
                PerformSegue("expiredSession", null);
            }));

            PresentViewController(alert, animated: true, completionHandler: null);
        }

        #endregion

        #region TableView Delegate

        // User select event in tableView
        public void FeatureSelectedFromTableView(Feature featureSelected)
        {
            // Get complete data of event selected from server
            // Show complete data of event in EventDetailViewController

            // If the controller is making a request, don't allow make another request until the first request finishes
            if (IsNetworkWorking)
            {
                return;
            }

            // Display an Activity Indicator in the status bar
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
            IsNetworkWorking = true;

            Task.Run(async () =>
            {

                try
                {
                    Feature eventObject = await AysaClient.Instance.GetFeatureById(featureSelected.Id);

                    InvokeOnMainThread(() =>
                    {
                        // Set the event with all data to pass to EventDetailViewController
                        FeatureSelectedFull = eventObject;
                        // Go to event detail
                        PerformSegue("showFeatureDetail", null);
                    });

                }
                catch (HttpUnauthorized)
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                catch (Exception ex)
                {
                    InvokeOnMainThread(() =>
                    {
                        //ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
                finally
                {
                    // Dismiss an Activity Indicator in the status bar
                    UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
                    IsNetworkWorking = false;
                }
            });

        }

        private void GetUserInfo()
        {
            Task.Run(async () =>
            {
                try
                {
                    this.user = await AysaClient.Instance.GetUserInfo();

                    InvokeOnMainThread(() =>
                    {
                        UserSession.Instance.Id = this.user.Id;
                        UserSession.Instance.nomApel = this.user.NombreApellido;
                    });
                }
                catch (HttpUnauthorized)
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowSessionExpiredError();
                    });
                }
                catch (Exception ex)
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                        //ShowErrorAlert("Error al obtener la información del usuario.");
                    });
                }
                finally
                {
                    // Dismiss an Activity Indicator in the status bar
                    UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
                    IsNetworkWorking = false;
                }
            });
        }


        #endregion


        #region Implement Delegates of FilterFeaturesViewController 

        // This delegate will be called when the user apply new filters
        public void LoadFeaturesByFilter(FilterFeatureData filter)
        {
            // Save filter
            this.FilterApplied = filter;

            // When the controller will appear, it will get the events with filters
            GetOpenFeaturesFromServer();
        }

        // This delegate will be called when the user remove the filters
        public void ClearAppliedFilter()
        {

            // Remove filters applied
            this.FilterApplied = null;

            // When the controller will appear, it will get the events without filters
            GetOpenFeaturesFromServer();
        }

        #region SearchBar Delegates

        [Export("searchBarSearchButtonClicked:")]
        public virtual void SearchButtonClicked(UISearchBar searchBar)
        {
            searchBar.ResignFirstResponder();
        }


        [Export("updateSearchResultsForSearchController:")]
        public virtual void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            // Filter events by title       
            SearchFeaturesByTitle(searchController);

        }


        private void SearchFeaturesByTitle(UISearchController searchController)
        {
            if (searchController.SearchBar.Text.Length == 0)
            {
                return;
            }

            // Filtering events
            List<Feature> featuresFiltered = new List<Feature>();
            string searchText = searchController.SearchBar.Text.ToLower();

            foreach (Feature eventObj in FeaturesFromServer)
            {
                // Compare Title of event
                if (eventObj.Detail.ToLower().Contains(searchText) ||
                    eventObj.Sector.Nombre.ToLower().Contains(searchText) ||
                    eventObj.Usuario.NombreApellido.ToLower().Contains(searchText) ||
                    eventObj.Date.ToString().Contains(searchText))
                {
                    featuresFiltered.Add(eventObj);
                }
            }


            var resultTableController = (FeatureSearchResultController)searchController.SearchResultsController;
            // Load data and reload TableView
            resultTableController.TableView.Source = new FeaturesTableDataSource(featuresFiltered, this);
            resultTableController.TableView.ReloadData();

        }


        #endregion


        #endregion

        #region Navigation Methods

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
        {
            if (segue.Identifier == "showFeatureDetail")
            {
                FeatureDetailViewController featureDetailViewController = (FeatureDetailViewController)segue.DestinationViewController;
                featureDetailViewController.FeatureDetail = FeatureSelectedFull;

            }

            if (segue.Identifier == "showFeaturesFiler")
            {
                UINavigationController navigationController = (UINavigationController)segue.DestinationViewController;
                FilterFeaturesViewController filterViewController = (FilterFeaturesViewController)navigationController.ViewControllers[0];
                filterViewController.Delegate = this;
                filterViewController.FilterApplied = this.FilterApplied;

            }
            if(segue.Identifier == "expiredSession")
            {
                LoginViewController loginViewController = (LoginViewController)segue.DestinationViewController;
            }
        }

        #endregion
    }
}

