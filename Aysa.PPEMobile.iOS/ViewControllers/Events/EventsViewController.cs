using System;
using Foundation;
using UIKit;
using System.Linq;
using Aysa.PPEMobile.iOS.ViewControllers.Events.BuilderEventsTableView;
using System.Collections.Generic;
using System.Threading.Tasks;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using Aysa.PPEMobile.iOS.ViewControllers.Events.EventSearch;
using Aysa.PPEMobile.iOS.Utilities;

namespace Aysa.PPEMobile.iOS.ViewControllers.Events
{
    public partial class EventsViewController : UIViewController, FilterEventsViewControllerDelegate
    {
        // Private variables

        // This flag is using to indicate that the controller is making a request
        private bool IsNetworkWorking = false;

        // This variable is needed to save the event that it has gotten from server with its all data and then it will passed to EventDetailViewController
        private Event EventSelectedFull;

        // Save in runtime the filter that the user is using to get the list of events
        private FilterEventData FilterApplied;

		// Implement Search controller
        EventSearchResultController ResultSearchEventViewController;
		UISearchController SearchController;

        // It's use to save in memory the events gotten from server, the events will be filtered from this list
        List<Event> EventsFromServer; 

        #region UIViewController Lifecyle 

        public EventsViewController(IntPtr handle) : base(handle)
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

            // Get open events form server
            GetOpenEventsFromServer();

            // Assign to the user logged the active sections that the user have
			GetActiveSectionsFromServer();
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
            TableView.RegisterNibForCellReuse(EventsTableViewCell.Nib, "EventsTableViewCell");


            // Separator lines in the tableView were customize so it's not neccesary define in the TableView
            TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

        }

		private void InitSearchBarController()
		{
            // Init Result Controller, it's going to show when the user is searching
            ResultSearchEventViewController = new EventSearchResultController();

			// Init SearchController
			SearchController = new UISearchController(ResultSearchEventViewController)
			{
				WeakDelegate = this,
				DimsBackgroundDuringPresentation = false,
				WeakSearchResultsUpdater = this
			};

			SearchController.SearchBar.SizeToFit();
			SearchController.SearchBar.Placeholder = "Buscar";
			//SearchController.SearchBar.BarTintColor = UIColor.FromRGB(230, 232, 235);
			//SearchController.SearchBar.BackgroundColor = UIColor.FromRGB(230, 232, 235);
			//SearchController.SearchBar.BackgroundImage = new UIImage();

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

        private void GetOpenEventsFromServer()
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

                    List<Event> events;
                    List<Event> eventsSorted;
                    // If there are filters, get events by filters

                    if (FilterApplied == null)
                        FilterApplied = BuildDefaultFilter();

                    if (FilterApplied != null) {
                        events = await AysaClient.Instance.GetEventsByFilter(FilterApplied);
                    }
                    else{
						// If there aren't filters, get events by default
						events = await AysaClient.Instance.GetOpenEvents();
                        
                    }
                    eventsSorted = events.OrderByDescending(m => m.Fecha).ToList();
                    EventsFromServer = eventsSorted;

                    InvokeOnMainThread(() =>
                    {

                        // Load data and reload TableView
                        TableView.Source = new EventsTableDataSource(eventsSorted, this);
                        TableView.ReloadData();

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

        private FilterEventData BuildDefaultFilter()
        {
            DateTime fromDate = DateTime.Now.AddDays(-30);
            DateTime toDate = DateTime.Now;

            FilterEventData filter = new FilterEventData();
            filter.FromDate = fromDate;
            filter.ToDate = toDate;
            filter.Status = -1;

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
                    var userLogged = await AysaClient.Instance.GetUserInfo();

                    InvokeOnMainThread(() =>
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
						ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
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
                CreateEventButton.Enabled = UserSession.Instance.CheckIfUserHasActiveSections();
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
			}));

			PresentViewController(alert, animated: true, completionHandler: null);
        }

        #endregion

        #region TableView Delegate

        // User select event in tableView
        public void EventSelectedFromTableView(Event eventSelected)
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
                    Event eventObject = await AysaClient.Instance.GetEventById(eventSelected.Id);

                    InvokeOnMainThread(() =>
                    {
                        // Set the event with all data to pass to EventDetailViewController
                        EventSelectedFull = eventObject;
                        // Go to event detail
                        PerformSegue("showEventDetail", null);
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


        #region Implement Delegates of FilterEventsViewController 

        // This delegate will be called when the user apply new filters
        public void LoadEventsByFilter(FilterEventData filter)
		{
            // Save filter
            this.FilterApplied = filter;

            // When the controller will appear, it will get the events with filters
            GetOpenEventsFromServer();
        }

		// This delegate will be called when the user remove the filters
		public void ClearAppliedFilter() 
        {
            // Remove filters applied
            this.FilterApplied = null;

            // When the controller will appear, it will get the events without filters
            GetOpenEventsFromServer();
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
            SearchEventsByTitle(searchController);

		}


		private void SearchEventsByTitle(UISearchController searchController)
		{
			if (searchController.SearchBar.Text.Length == 0)
			{
				return;
			}

            // Filtering events
            List<Event> eventsFiltered = new List<Event>();
            string searchText = searchController.SearchBar.Text.ToLower();

            foreach(Event eventObj in EventsFromServer)
            {
                // Compare Title of event
                if(eventObj.Titulo.ToLower().Contains(searchText) ||
                   eventObj.NroEvento.ToString().ToLower().Contains(searchText) ||
                   eventObj.Lugar.ToLower().Contains(searchText) ||
                   eventObj.Usuario.NombreApellido.ToLower().Contains(searchText) ||
                   eventObj.Fecha.ToString().Contains(searchText))
                {
                    eventsFiltered.Add(eventObj);
                }
            }


            var resultTableController = (EventSearchResultController)searchController.SearchResultsController;
			// Load data and reload TableView
            resultTableController.TableView.Source = new EventsTableDataSource(eventsFiltered, this);
			resultTableController.TableView.ReloadData();

		}

		#endregion


		#endregion

		#region Navigation Methods

		public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{
			if (segue.Identifier == "showEventDetail")
			{
                EventDetailViewController eventDetailViewController = (EventDetailViewController)segue.DestinationViewController;
                eventDetailViewController.EventDetail = EventSelectedFull;
				
			}

			if (segue.Identifier == "showEventsFiler")
			{
                UINavigationController navigationController = (UINavigationController)segue.DestinationViewController;
                FilterEventsViewController filterViewController = (FilterEventsViewController)navigationController.ViewControllers[0];
                filterViewController.Delegate = this;
                filterViewController.FilterApplied = this.FilterApplied;

			}
		}

        #endregion
    }
}

