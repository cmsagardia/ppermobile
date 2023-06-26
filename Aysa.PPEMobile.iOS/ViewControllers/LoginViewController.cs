using System;
using System.Net;
using System.Threading.Tasks;
using Aysa.PPEMobile.iOS.Utilities;
using Aysa.PPEMobile.iOS.ViewControllers.Documents;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;
using Aysa.PPEMobile.Service.HttpExceptions;
using BigTed;
using Foundation;
using UIKit;

namespace Aysa.PPEMobile.iOS.ViewControllers
{
    public partial class LoginViewController : UIViewController
    {

        public LoginViewController(IntPtr handle) : base(handle)
        {
        }


        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.

            // Change status bar text color to white
            SetNeedsStatusBarAppearanceUpdate();
            HideBloquedUserAlert(true);

            // Config style of user and password textfields
            SetUpTextFields();


            /////////////// Only for testing /////////////////////
            //UserTextField.Text = "aysa.ppe";
            //PasswordTextField.Text = "N6VtK4YxgEDHBAmsyf1C";
            /////////////// Only for testing /////////////////////
            

        }

        // Override function to change Status Bar text color to white
        public override UIStatusBarStyle PreferredStatusBarStyle()
        {
            return UIStatusBarStyle.LightContent;
        }

        public override void DidReceiveMemoryWarning()
        {
            base.DidReceiveMemoryWarning();
            // Release any cached data, images, etc that aren't in use.
        }

		#region Private Methods

        private void SetUpTextFields() 
        {

			// Assign delegates for UITextFields
            UserTextField.ShouldReturn += TextFieldShouldReturn;
			PasswordTextField.ShouldReturn += TextFieldShouldReturn;

            PasswordTextField.LayoutIfNeeded();

            // Config placeholder with color for user TextField
			var placeHolderUser = new NSAttributedString("Ingrese su usuario", font: UIFont.FromName("System-Light", 17.0f), foregroundColor: UIColor.White);
			UserTextField.AttributedPlaceholder = placeHolderUser;

			// Config placeholder with color for password TextField
			var placeHolderPassword = new NSAttributedString("Ingrese su contraseña", font: UIFont.FromName("System-Light", 17.0f), foregroundColor: UIColor.White);
            PasswordTextField.AttributedPlaceholder = placeHolderPassword;

            VersionTextField.Text = "v" + NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"];

            // Add tap gesture recognizer to dismiss the keyboard
            View.AddGestureRecognizer(new UITapGestureRecognizer(() => this.DismissKeyboard()));
            
        }

        private void HideBloquedUserAlert(bool state)
        {
            CoreGraphics.CGRect alertViewFrame;
            alertViewFrame = BlockedUserAlert.Frame;
            BlockedUserAlert.Hidden = state;

            alertViewFrame.Height = (BlockedUserAlert.Hidden) ? 50 : 130;
            
            BlockedUserAlert.Frame = alertViewFrame;
            BlockedUserAlert.LayoutIfNeeded();
        }

        private void DismissKeyboard()
		{
			View.EndEditing(true);
		}

		private void ShowErrorAlert(string message)
		{
			var alert = UIAlertController.Create("Error", message, UIAlertControllerStyle.Alert);
			alert.AddAction(UIAlertAction.Create("Ok", UIAlertActionStyle.Default, null));
			PresentViewController(alert, true, null);
		}

		#endregion

		#region UITextFieldDelegate Methods

		private bool TextFieldShouldReturn(UITextField textField)
		{

            if (textField.Tag == 0)
			{
                PasswordTextField.BecomeFirstResponder();
			}
			else
			{
				PasswordTextField.ResignFirstResponder();
			}
			return true;
		}

        // Validate that user and password field have values;
        private bool FieldsAreEmpty()
        {
            if (UserTextField.Text == null || UserTextField.Text.Length == 0)
            {
                return true;
            }

            if (PasswordTextField.Text == null || PasswordTextField.Text.Length == 0)
			{
                return true;
			}

            return false;
        }

        #endregion

        #region IBActions

        partial void LoginButton_TouchUpInside(UIButton sender)
        {


            // Validate that the fields have values
            if(FieldsAreEmpty())
            {
                ShowErrorAlert("Los campos usuario y contraseña son requeridos");
                return;
            }

            //Show a HUD with a progress spinner and the text
            BTProgressHUD.Show("Cargando...", -1, ProgressHUD.MaskType.Black);

            string username = this.UserTextField.Text;
            string password = this.PasswordTextField.Text;

            string dateTimeTest = WebUtility.UrlEncode("2017/08/01");

			Task.Run(async () =>
			{
                
				try
				{
					LoginResponse resultLogin = await AysaClient.Instance.Login(username, password);

					InvokeOnMainThread(() =>
					{
                        UserTextField.Text = "";
                        PasswordTextField.Text = "";
                        HideBloquedUserAlert(true);
                        PerformSegue("goMainView", null);
                        
                    });

				}
				catch (HttpUnauthorized)
				{
					InvokeOnMainThread(() =>
					{
                        ShowErrorAlert("Usuario y/o constraseña incorrectos");
                    });
				}
                catch (UserBlockedException ex)
                {
                    InvokeOnMainThread(() =>
                    {
                        HideBloquedUserAlert(false);
                        ShowErrorAlert(ex.Message);
                    });
                }
                catch (Exception ex)
				{
					InvokeOnMainThread(() =>
					{
                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
						//ShowLoginInfo(string.Format("Error: {0}", ex.Message));
					});
                }
                finally
                {
					// Remove progress
					BTProgressHUD.Dismiss();

                }
			});
		}

        #endregion

        #region Navigation Methods

        public override void PrepareForSegue(UIStoryboardSegue segue, NSObject sender)
		{
			if (segue.Identifier == "showDocuments")
			{
				UINavigationController navigationController = (UINavigationController)segue.DestinationViewController;
				DocumentsViewController documentViewController = (DocumentsViewController)navigationController.ViewControllers[0];
				documentViewController.ShowDocumentsOffline = true;
			}

		}

		#endregion


	}
}

