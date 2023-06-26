using System;
using UIKit;
using Aysa.PPEMobile.iOS.ViewControllers.ShortDial.BuilderShortDialTableView;
using Aysa.PPEMobile.iOS.ViewControllers.ShortDial.SectionSearch;
using System.Threading.Tasks;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using System.Collections.Generic;
using Aysa.PPEMobile.iOS.Utilities;
using Foundation;
using ObjCRuntime;
using CoreGraphics;

namespace Aysa.PPEMobile.iOS.ViewControllers.ShortDial
{
    public partial class ShortDialViewController : UIViewController, SectionSearchResultControllerDelegate
    {

        // This flag is using to indicate that the controller is making a request
        private bool IsNetworkWorking = false;
        // This variable is needed to save the section with its responsables guard to pass to PeopleInChargeViewControler
        private Section SectionSelected;


        // Implement Search controller
        SectionSearchResultController ResultsSearchSectionsController;
        UISearchController SearchController;

        // Save Documents
        private static readonly NSString DocumentSavedKey = (NSString)"DocumentSavedList";
        List<Document> DocumentsList;
        int DocumentsDownloaded = 0;
        NSMutableArray DocumentsDownloadedList = new NSMutableArray();

        #region ViewController Lifecycle

        public ShortDialViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            // Change status bar text color to white
            NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;

            InitTableView();
            SetUpNavigationBarStyle();
            InitSearchBarController();

            GetLevels();

            GetDocumentsFromServer();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        #endregion

        #region Private Methods

        private void InitTableView()
        {
            tableView.RegisterNibForCellReuse(SectionTableViewCell.Nib, "SectionTableViewCell");

            // Remove the blank space at the top of a grouped UITableView
            tableView.TableHeaderView = new UIView(new CGRect(0, 0, 0, 1));
            //tableView.TableFooterView = new UIView(new CGRect(0, 0, 0, 1));
        }

        private void InitSearchBarController()
        {
            // Init Result Controller, it's going to show when the user is searching
            ResultsSearchSectionsController = new SectionSearchResultController
            {
                filteredItems = new List<Section>(),
                Delegate = this
            };

            // Init SearchController
            SearchController = new UISearchController(ResultsSearchSectionsController)
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

            tableView.TableHeaderView = SearchController.SearchBar;


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



        private void GetLevels()
        {
            // Display an Activity Indicator in the status bar
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            Task.Run(async () =>
            {

                try
                {

                    List<Level> LevelList = await AysaClient.Instance.GetLevels();

                    InvokeOnMainThread(() =>
                    {

                        // Load data and reload TableView
                        tableView.Source = new SectionsTableDataSource(LevelList, this, tableView);
                        tableView.ReloadData();

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

        public void ChangeFavorite(Section sectionSelected)
        {
            // Display an Activity Indicator in the status bar
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            Task.Run(async () =>
            {

                try
                {

                    await AysaClient.Instance.AddOrDeleteFavorite(sectionSelected.Id, sectionSelected.Favorito);

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
                    GetLevels();
                    // Dismiss an Activity Indicator in the status bar
                    UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;

                }
            });
        }

        private void ShowSectionDetail(Section sectionSelected)
        {
            // Get persons in guard to the section selected

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
                    // Fill responsables guard in section selected
                    sectionSelected.ResponsablesGuardia = await AysaClient.Instance.GetResponsablesGuardBySector(sectionSelected.Id);

                    InvokeOnMainThread(() =>
                    {
                        // Set the section with all data to pass to PeopleInChargeViewController
                        this.SectionSelected = sectionSelected;
                        // Go to event detail
                        PerformSegue("showPeopleInCharge", null);

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

        public void GetDocumentsFromServer()
        {
            // Get active sections from server

            // Display an Activity Indicator in the status bar
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            Task.Run(async () =>
            {

                try
                {
                    this.DocumentsList = await AysaClient.Instance.GetDocuments();

                    InvokeOnMainThread(() =>
                    {
                        // Only Sync the documents one time in the lifecycle
                        if (DocumentsDownloaded == 0)
                        {
                            // Synchronize download to get the all documents
                            DownloadSynchronizeDocuments();
                        }
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


        private void DownloadSynchronizeDocuments()
        {
            // Download the documents one at the time to save and show in offline mode

            // Display an Activity Indicator in the status bar
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            if (DocumentsDownloaded < DocumentsList.Count)
            {
                Document doc = DocumentsList[DocumentsDownloaded];
                DownloadDocument(doc);
                DocumentsDownloaded++;
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Download completed");
                // Dismiss an Activity Indicator in the status bar
                UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
            }

        }

        private void DownloadDocument(Document doc)
        {

            System.Diagnostics.Debug.WriteLine("Downloading..." + doc.Name);

            Task.Run(async () =>
            {

                try
                {
                    // Download file from server
                    byte[] bytesArray = await AysaClient.Instance.GetDocumentFile(doc.ServerRelativeUrl);

                    InvokeOnMainThread(() =>
                    {
                        // Build document with its file to save in the device
                        DocumentArchive doc1 = new DocumentArchive();
                        doc1.Name = doc.Name;
                        doc1.ServerRelativeUrl = doc.ServerRelativeUrl;

                        // Encode Data
                        var text = System.Text.Encoding.Default.GetString(bytesArray);
                        text = text.Replace("\"", "");
                        bytesArray = Convert.FromBase64String(text);
                        doc1.BytesArray = bytesArray;

                        SaveDocumentInDevice(doc1);

                        DownloadSynchronizeDocuments();



                    });

                }
                catch (HttpUnauthorized)
                {
                    InvokeOnMainThread(() =>
                    {
                        ShowSessionExpiredError();

                        // Dismiss an Activity Indicator in the status bar
                        UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
                    });
                }
                catch (Exception ex)
                {
                    InvokeOnMainThread(() =>
                    {
                        //ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                        // Dismiss an Activity Indicator in the status bar
                        UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
                    });
                }

            });
        }

        private void SaveDocumentInDevice(DocumentArchive doc)
        {
            this.DocumentsDownloadedList.Add(doc);

            // Archiver document object
            NSData encodedObject = NSKeyedArchiver.ArchivedDataWithRootObject(this.DocumentsDownloadedList);
            NSUserDefaults.StandardUserDefaults.SetValueForKey(encodedObject, DocumentSavedKey);
            NSUserDefaults.StandardUserDefaults.Synchronize();
        }

        #endregion

        #region SearchBar Delegates

        [Export("searchBarSearchButtonClicked:")]
        public virtual void SearchButtonClicked(UISearchBar searchBar)
        {
            searchBar.ResignFirstResponder();
        }


        [Export("updateSearchResultsForSearchController:")]
        public virtual void UpdateSearchResultsForSearchController(UISearchController searchController)
        {
            // // to limit network activity, reload half a second after last key press.
            CancelPreviousPerformRequest(this, new Selector("SearchSectionsByName:"), null);


            PerformSelector(new Selector("SearchSectionsByName:"), searchController, 0.5f);
        }


        [Export("SearchSectionsByName:")]
        private void SearchSectionsByName(UISearchController searchController)
        {

            // If the controller is making a request, don't allow make another request until the first request finishes
            // If user didn't enter some text
            if (IsNetworkWorking || searchController.SearchBar.Text.Length == 0)
            {
                return;
            }

            string searchText = searchController.SearchBar.Text;

            // Display an Activity Indicator in the status bar
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
            IsNetworkWorking = true;

            Task.Run(async () =>
            {

                try
                {
                    List<Section> sections = await AysaClient.Instance.SearchSectionByName(searchText);

                    InvokeOnMainThread(() =>
                    {

                        var tableController = (SectionSearchResultController)searchController.SearchResultsController;
                        tableController.filteredItems = sections;
                        tableController.TableView.ReloadData();
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

        #region TableView Delegate
       
        public void ReloadTableView(NSIndexPath[] paths,nint section ) {
            tableView.ReloadData();
            tableView.ReloadRows(paths, UITableViewRowAnimation.None);
            tableView.ReloadSections(NSIndexSet.FromIndex(section), UITableViewRowAnimation.Automatic);
        }

        // User select event in tableView
        public void SectionSelectedFromTableView(Section sectionSelected)
		{
			// Show detail of section
            ShowSectionDetail(sectionSelected);
		}

		#region Methods SectionSearchResultControllerDelegate 

        // Click in section searched
		public void SectionSearchedSelected(Section section)
        {
			// Show detail of section
			ShowSectionDetail(section);
        }

        #endregion

        #endregion


        #region Navigation Methods

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{
			if (segue.Identifier == "showPeopleInCharge")
			{
                PeopleInChargeViewController peopleInChargeViewController = (PeopleInChargeViewController)segue.DestinationViewController;
                peopleInChargeViewController.SectionWithPeopleInGuard = SectionSelected;

			}

		}

		#endregion

	}
}

