using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using Aysa.PPEMobile.Model;
using Aysa.PPEMobile.Service.HttpExceptions;
using Aysa.PPEMobile.Service;

namespace Aysa.Android.Activities
{
    [Activity(Label = "LoginActivity")]
    public class LoginActivity : Activity
    {
        EditText txtPassword;
        EditText txtUsername;
        FrameLayout progressOverlay;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Login);

            Button btnLogin = FindViewById<Button>(Resource.Id.btnLogin);
            txtPassword = FindViewById<EditText>(Resource.Id.txtPassword);
            txtUsername = FindViewById<EditText>(Resource.Id.txtUsername);
            progressOverlay = FindViewById<FrameLayout>(Resource.Id.progress_overlay);

            txtUsername.Text = "diegoq_e";
            txtPassword.Text = "Aysa2016";

            btnLogin.Click += BtnLogin_Click;
        }



        private void ShowLoginInfo(string message)
        {
            Toast.MakeText(this, message, ToastLength.Long).Show();
        }

        private void ShowErrorAlert(string message)
        {
            AlertDialog.Builder alert = new AlertDialog.Builder(this);
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
                        GoToHomeActivity();
                    });

                }
                catch (HttpUnauthorized)
                {
                    RunOnUiThread(() => {
                        ShowErrorAlert("Usuario y/o constraseña incorrectos");
                    });
                }
                catch (Exception ex)
                {
                    RunOnUiThread(() => {
                        ShowErrorAlert(ex.ToString());
                    });
                }
                finally
                {
                    ShowProgressDialog(false);
                }
            });
        }

        private void GoToHomeActivity()
        {
            var homeActivity = new Intent(this, typeof(HomeActivity));
            StartActivity(homeActivity);

            Finish();
        }

        private void ShowProgressDialog(bool show)
        {
            progressOverlay.Visibility = show ? ViewStates.Visible : ViewStates.Gone;
        }
    }
}