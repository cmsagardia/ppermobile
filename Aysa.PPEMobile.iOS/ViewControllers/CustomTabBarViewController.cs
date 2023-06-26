using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aysa.PPEMobile.iOS.Utilities;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using Foundation;
using UIKit;

namespace Aysa.PPEMobile.iOS.ViewControllers
{
    public partial class CustomTabBarViewController : UITabBarController
    {
		public CustomTabBarViewController(IntPtr handle) : base(handle)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// Setup style of BarItems (textColor, font, size, etc).
			this.SetupStyleOfTabBarItems();

            // Add observer to manage session expire
            this.AddObserverToSessionExpired();
		}

		public override void DidReceiveMemoryWarning()
		{
			base.DidReceiveMemoryWarning();
			// Release any cached data, images, etc that aren't in use.     
		}

		#region Internal Methods

		private void SetupStyleOfTabBarItems()
		{
			// Display an Activity Indicator in the status bar
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

			Task.Run(async () =>
			{

				try
				{
					var userLogged = await AysaClient.Instance.GetUserInfo();

					InvokeOnMainThread(() =>
					{
						// Save user sections active
						UserSession.Instance.Id = userLogged.Id;
						UserSession.Instance.nomApel = userLogged.NombreApellido;
						UserSession.Instance.Roles = userLogged.Roles;

						// Change textColor and font of BarItems for normal and selected state

						UIColor selectedColor = UIColor.FromRGB(248, 155, 9);

						// Define attributes to BarItems for normal state.
						UITextAttributes attributesForNormalState = new UITextAttributes();
						attributesForNormalState.TextColor = UIColor.White;
						attributesForNormalState.Font = UIFont.SystemFontOfSize(12f);
						UITabBarItem.Appearance.SetTitleTextAttributes(attributesForNormalState, UIControlState.Normal);

						// Define attributes to BarItems for selected state.
						UITextAttributes attributesForSelectedState = new UITextAttributes();
						attributesForSelectedState.TextColor = selectedColor;
						attributesForNormalState.Font = UIFont.SystemFontOfSize(12f);
						UITabBarItem.Appearance.SetTitleTextAttributes(attributesForSelectedState, UIControlState.Selected);

						TabBar.SelectedImageTintColor = selectedColor;
						TabBar.UnselectedItemTintColor = UIColor.White;

						var roles = new List<string>() { "Responsable Guardia", "Referente PPE", "Directorio" };

						var result = UserSession.Instance.Roles.Any(r => roles.Contains(r.Nombre));

						if (!result)
						{
							TabBar.Items[1].Enabled = false;
							TabBar.Items[4].Enabled = false;
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

        private void AddObserverToSessionExpired()
        {
            NSNotificationCenter.DefaultCenter.AddObserver((NSString)"SessionExpired", SessionExpired);
        }

		public void SessionExpired(NSNotification notification)
		{
            // Go to LoginViewController
			Console.WriteLine("Session Expired");
            DismissViewController(true, null);
			
		}

		#endregion

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
	}
}

