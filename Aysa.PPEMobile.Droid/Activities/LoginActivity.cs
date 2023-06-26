
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Aysa.PPEMobile.Service.HttpExceptions;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using AndroidX.AppCompat.App;
using Android.Views.InputMethods;

namespace Aysa.PPEMobile.Droid.Activities
{
    [Activity(Label = "LoginActivity")]
    public class LoginActivity : AppCompatActivity
    {
        EditText txtPassword;
        EditText txtUsername;
        TextView txtVersionCode;
        FrameLayout progressOverlay;
        FrameLayout messageUserBlocked;
        InputMethodManager imm;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.Login);
            string versionCode = Application.Context.ApplicationContext.PackageManager.GetPackageInfo(Application.Context.ApplicationContext.PackageName, 0).VersionName;

            Button btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
            txtPassword = FindViewById<EditText>(Resource.Id.txtPassword);
            txtUsername = FindViewById<EditText>(Resource.Id.txtUsername);
            progressOverlay = FindViewById<FrameLayout>(Resource.Id.progress_overlay);
            ImageView documentIcon = FindViewById<ImageView>(Resource.Id.document_icon);
            txtVersionCode = FindViewById<TextView>(Resource.Id.versionCodeText);
            imm = (InputMethodManager)GetSystemService(Context.InputMethodService);

            messageUserBlocked = FindViewById<FrameLayout>(Resource.Id.messageUserBlocked);
            //ShowUserBlockedMessage(false);

            documentIcon.Click += BtnShowDocumentClick;

            txtVersionCode.Text = string.Concat("v", versionCode);

            btnLogin.Click += BtnLogin_Click;
        }



        private void ShowLoginInfo(string message)
        {
            Toast.MakeText(this, message, ToastLength.Long).Show();
        }

        private void ShowErrorAlert(string message)
        {
            Android.App.AlertDialog.Builder alert = new Android.App.AlertDialog.Builder(this);
            alert.SetTitle("Error");
            alert.SetMessage(message);

            Dialog dialog = alert.Create();
            dialog.Show();
        }

        #region UITextFieldDelegate Methods

        // Validate that user and password field have values;
        private bool FieldsAreEmpty()
        {
            if (txtPassword.Text == null || txtPassword.Text.Length == 0)
            {
                return true;
            }

            if (txtUsername.Text == null || txtUsername.Text.Length == 0)
            {
                return true;
            }

            return false;
        }

        #endregion

        void BtnLogin_Click(object sender, System.EventArgs e)
        {
            if (FieldsAreEmpty())
            {
                ShowErrorAlert("Los campos usuario y contraseña son requeridos");
                return;
            }

            ShowProgressDialog(true);


            string username = txtUsername.Text;
            string password = txtPassword.Text;

            Task.Run(async () =>
            {

                try
                {
                    LoginResponse resultLogin = await AysaClient.Instance.Login(username, password);

                    RunOnUiThread(() => {
                        GoToLoginActivity();
                    });

                }
                catch (HttpUnauthorized)
                {
                    RunOnUiThread(() => {
                        ShowErrorAlert("Usuario y/o contraseña incorrectos");
                    });
                }
                catch(UserBlockedException ex)
                {
					this.RunOnUiThread(() =>
					{
						ShowUserBlockedMessage(true);
                        ShowErrorAlert(ex.Message);

                        imm.HideSoftInputFromWindow(txtUsername.WindowToken, HideSoftInputFlags.None);
                        imm.HideSoftInputFromWindow(txtPassword.WindowToken, HideSoftInputFlags.None);
                    });
				}
                catch (NetworkConnectionException)
                {
                    this.RunOnUiThread(() =>
                    {
                        ShowErrorAlert(AysaConstants.ExceptionNetworkMessage);
                    });
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() => {
                        ShowErrorAlert(AysaConstants.ExceptionGenericMessage);
                    });
                }
                finally
                {
                    ShowProgressDialog(false);
                }
            });
        }

        void BtnShowDocumentClick(object sender, System.EventArgs e)
        {
            // Show document Activity in offline mode
            Intent documentActivity = new Intent(Application.Context, typeof(ContainerDocumentsActivity));
            documentActivity.PutExtra("offlineMode", JsonConvert.SerializeObject(true));
            StartActivity(documentActivity);


        }

        private void GoToLoginActivity()
        {
            var mainActivity = new Intent(this, typeof(HomeActivity));

            StartActivity(mainActivity);

            Finish();
        }

        private void ShowProgressDialog(bool show)
        {
            progressOverlay.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
        }

        private void ShowUserBlockedMessage(bool show)
        {
            messageUserBlocked.Visibility = show ? ViewStates.Visible : ViewStates.Gone;

            imm.ShowSoftInput(txtPassword, ShowFlags.Forced);
        }
    }
}
