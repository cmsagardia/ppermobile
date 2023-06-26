using System;

using UIKit;
using Aysa.PPEMobile.iOS.ViewControllers.Documents.BuilderDocumentsTableView;
using Aysa.PPEMobile.Model;
using System.Threading.Tasks;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using System.Collections.Generic;
using Foundation;
using Aysa.PPEMobile.iOS.ViewControllers.Events;
using Aysa.PPEMobile.iOS.ViewControllers.Documents.DocumentSearch;
using Aysa.PPEMobile.iOS.Utilities;
using System.Drawing;
using System.IO;
using QuickLook;
using Aysa.PPEMobile.iOS.ViewControllers.Preview;
using BigTed;

namespace Aysa.PPEMobile.iOS.ViewControllers.Documents
{
    public partial class DocumentsViewController : UIViewController
    {

        // Private Variables
        private static readonly NSString DocumentSavedKey = (NSString)"DocumentSavedList";

        List<Document> DocumentsList;

		// Implement Search controller
        DocumentSearchResultViewController ResultSearchDocumentViewController;
		UISearchController SearchController;


        // Public variables

        public Boolean ShowDocumentsOffline = false;


        #region View Lifecycle

        public DocumentsViewController(IntPtr handle) : base(handle)
        {

        }
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            // Change status bar text color to white
            NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;
			// Change default color
			NavigationController.NavigationBar.TintColor = UIColor.White;

            ConfigStyleOfTableView();

			// Add Search bar
			InitSearchBarController();

        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

		public override void ViewWillAppear(Boolean animated)
		{
			base.ViewWillAppear(animated);


            if(this.ShowDocumentsOffline)
            {
                // Ofline mode
                GetDocumentSaved();

                AddBackButtonForOfflineMode();
            }
            else{
                GetDocumentsFromServer();
            }

		}

		#endregion


		#region Private Methods

        private void ConfigStyleOfTableView()
        {
			// Register TableCell
			tableView.RegisterNibForCellReuse(DocumentTableViewCell.Nib, "DocumentTableViewCell");

            // Remove extra cells
            tableView.TableFooterView = new UIView();
        }

        private void AddBackButtonForOfflineMode()
        {
			UIButton menuButton = new UIButton(UIButtonType.Custom);
			menuButton.SetTitle("Atras", UIControlState.Normal);
			menuButton.Frame = new RectangleF(-30, 0, 60, 25);


			UIBarButtonItem menuItem = new UIBarButtonItem(menuButton);

			menuButton.TouchUpInside += (sender, e) => {
				DismissViewController(true, null);
			};

			this.NavigationItem.LeftBarButtonItem = menuItem;
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
                        // Load data and reload TableView
                        tableView.Source = new DocumentTableDataSource(this.DocumentsList, this);
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
						ShowErrorAlert($"Error No se pudo descargar la lista de documentos");
					});
				}
				finally
				{
					// Dismiss an Activity Indicator in the status bar
					UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
				}
			});

		}

		private void InitSearchBarController()
		{
			// Init Result Controller, it's going to show when the user is searching
            ResultSearchDocumentViewController = new DocumentSearchResultViewController();

			// Init SearchController
			SearchController = new UISearchController(ResultSearchDocumentViewController)
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

        private void GetDocumentSaved()
        {
            
			NSUserDefaults defaults = NSUserDefaults.StandardUserDefaults;
			NSData encodedObject = (NSData)defaults.ValueForKey(DocumentSavedKey);

            // Check if there are documents saved
            if(encodedObject == null)
            {
                return;
            }

            // Get documents saved and build the list
			NSMutableArray listDoc = (NSMutableArray)NSKeyedUnarchiver.UnarchiveObject(encodedObject);

            DocumentsList = new List<Document>();

            for (nuint i = 0; i < listDoc.Count; i++)
            {
                DocumentArchive docSaved = listDoc.GetItem<DocumentArchive>(i);

                // Build Document c# object form DocumentArchive NsObject
                Document doc = new Document();
                doc.Name = docSaved.Name;
                doc.ServerRelativeUrl = docSaved.ServerRelativeUrl;
                doc.BytesArray = docSaved.BytesArray;

                DocumentsList.Add(doc);
            }


			// Load data and reload TableView
			tableView.Source = new DocumentTableDataSource(this.DocumentsList, this);
			tableView.ReloadData();

        }


		private void ShowDocumentInViewOnline(Document doc)
		{
			//Show a HUD with a progress spinner and the text
			BTProgressHUD.Show("Cargando...", -1, ProgressHUD.MaskType.Black);

			// Get Document From Server

			Task.Run(async () =>
			{

				try
				{
					// Download file from server
					byte[] bytesArray = await AysaClient.Instance.GetDocumentFile(doc.ServerRelativeUrl);

					InvokeOnMainThread(() =>
					{
                        // Encode Data
						var text = System.Text.Encoding.Default.GetString(bytesArray);
						text = text.Replace("\"", "");
						bytesArray = Convert.FromBase64String(text);
                        NSData data = NSData.FromArray(bytesArray);

                        SaveFileInLocalFolder(data, doc);

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
						ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
						// Dismiss an Activity Indicator in the status bar
						UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
					});
				}
                finally
                {
					// Remove progress
					BTProgressHUD.Dismiss();
                }

			});
		}

        private void ShowDocumentInViewOfline(Document doc)
        {
            NSData data = NSData.FromArray(doc.BytesArray);

			SaveFileInLocalFolder(data, doc);
        }

		private void SaveFileInLocalFolder(NSData data, Document documentFile)
		{
			var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			var filename = Path.Combine(documents, documentFile.Name);
            data.Save(filename, false);

            ShowFileSavedInPreviewController(documentFile);

		}

        private void ShowFileSavedInPreviewController(Document doc)
		{

			QLPreviewController quickLookController = new QLPreviewController();
            quickLookController.DataSource = new PreviewControllerDataSource(doc.Name);
			NavigationController.PushViewController(quickLookController, true);

		}

		#endregion

		#region TableView Delegate

		// User select document in tableView
        public void DocumentSelectedFromTableView(Document documentSelected)
		{

			if (this.ShowDocumentsOffline)
			{
                // Ofline mode
                ShowDocumentInViewOfline(documentSelected);
				
			}
			else
			{
				ShowDocumentInViewOnline(documentSelected);
			}
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
			// Filter documents by name     
			SearchDocumentsByTitle(searchController);

		}

		private void SearchDocumentsByTitle(UISearchController searchController)
		{
			if (searchController.SearchBar.Text.Length == 0)
			{
				return;
			}

			// Filtering events
            List<Document> documentsFiltered = new List<Document>();
			string searchText = searchController.SearchBar.Text.ToLower();

            foreach (Document documentObj in DocumentsList)
			{
				// Compare Title of document
                if (documentObj.Name.ToLower().Contains(searchText))
				{
					documentsFiltered.Add(documentObj);
				}
			}


            var resultTableController = (DocumentSearchResultViewController)searchController.SearchResultsController;
			// Load data and reload TableView
			resultTableController.TableView.Source = new DocumentTableDataSource(documentsFiltered, this);
			resultTableController.TableView.ReloadData();

		}

		#endregion

	}

}

