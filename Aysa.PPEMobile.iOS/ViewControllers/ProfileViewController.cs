using System;

using UIKit;
using System.Threading.Tasks;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using Foundation;
using Aysa.PPEMobile.iOS.Utilities;

namespace Aysa.PPEMobile.iOS.ViewControllers
{
    public partial class ProfileViewController : UIViewController
    {
        public ProfileViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            // Change status bar text color to white
            NavigationController.NavigationBar.BarStyle = UIBarStyle.Black;

            VersionLabel.Text = "v" + NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"];

            GetUserInfo();
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

        partial void CloseSession_TouchUpInside(UIButton sender)
        {
            UIAlertController alert = UIAlertController.Create("Aviso", "¿Seguro que desea cerrar la sesión?", UIAlertControllerStyle.Alert);

            alert.AddAction(UIAlertAction.Create("Cancelar", UIAlertActionStyle.Cancel, null));

            alert.AddAction(UIAlertAction.Create("Si", UIAlertActionStyle.Default, action =>
            {
                DismissViewController(true, null);
            }));

            PresentViewController(alert, animated: true, completionHandler: null);
        }

        #region Private Methods

        private void ShowErrorAlert(string message)
        {
            var alert = UIAlertController.Create("Error", message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
            PresentViewController(alert, true, null);
        }

        private void ShowSessionExpiredError()
        {
            UIAlertController alert = UIAlertController.Create("Aviso", "Su sesión ha expirado, por favor ingrese sus credenciales nuevamente", UIAlertControllerStyle.Alert);

            alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, action =>
            {
                // Send notification to TabBarController to return to login
                NSNotificationCenter.DefaultCenter.PostNotificationName("SessionExpired", this);
            }));

            PresentViewController(alert, animated: true, completionHandler: null);
        }

        private void ShowUIElements()
        {
			NameLabel.Hidden = false;
			EmailLabel.Hidden = false;
			ProfileIconImage.Hidden = false;
			EmailIconImage.Hidden = false;
            VersionLabel.Hidden = false;
        }

        private void GetUserInfo()
        {
            
            // Display an Activity Indicator in the status bar
            UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;

            Task.Run(async () =>
            {

                try
                {

                    User user = await AysaClient.Instance.GetUserInfo();

                    InvokeOnMainThread(() =>
                    {
                        NameLabel.Text = user.NombreApellido;
                        EmailLabel.Text = user.Mail;

                        ShowUIElements();

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

        #endregion


    }
}

