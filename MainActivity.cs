using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.Wearable.Views;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Java.Interop;
using Android.Views.Animations;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using PCLCrypto;
using System.Text;
using Android.Net;
using Newtonsoft.Json;
using Android.Graphics;

namespace TremorFreeMe
{
    [Activity(Label = "TremorFreeMe", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        public static bool wantToBeRembered = true;
        public static ISharedPreferences prefs;
        Button loginButton;
        Button signupButton;
        EditText usernameTxt;
        EditText passTxt;

        TextView error;
        public static MobileServiceClient MobileService = new MobileServiceClient("https://tremortest1.azurewebsites.net");
        public static users main_User;
        public static MobileServiceCollection<users, users> items;
        public static IMobileServiceTable<users> usersTable;
        public  bool isOnline()
        {
            ConnectivityManager cm =
                (ConnectivityManager)GetSystemService(Context.ConnectivityService);
            NetworkInfo netInfo = cm.ActiveNetworkInfo;
            return netInfo != null && netInfo.IsConnectedOrConnecting;
        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.Main);

            var v = FindViewById<WatchViewStub>(Resource.Id.watch_view_stub);
            v.LayoutInflated += delegate
            {
                this.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
                prefs = Application.Context.GetSharedPreferences("Settings", FileCreationMode.WorldReadable);
                usersTable = MobileService.GetTable<users>();
                error = FindViewById<TextView>(Resource.Id.error);
                loginButton = FindViewById<Button>(Resource.Id.lgn_btn);
                signupButton = FindViewById<Button>(Resource.Id.sgnup_btn);
                usernameTxt = FindViewById<EditText>(Resource.Id.username_txt);
                passTxt = FindViewById<EditText>(Resource.Id.password_pass);
                passTxt.Background.SetColorFilter(Android.Graphics.Color.WhiteSmoke, PorterDuff.Mode.SrcIn);
                usernameTxt.Background.SetColorFilter(Android.Graphics.Color.WhiteSmoke, PorterDuff.Mode.SrcIn);
                var disp = WindowManager.DefaultDisplay;
              //  error.Text = disp.Height.ToString();
            
                var serobject = prefs.GetString("remembered", null);

                if (serobject != null)
                {
                    var us = JsonConvert.DeserializeObject<users>(serobject);


                    main_User = us;

                    if (main_User != null)
                    {
                        if (disp.Height > 500)
                        {
                            var intent = new Intent(this, typeof(LoggedIn));
                            StartActivity(intent);
                        }
                        else
                        {
                            var intent2 = new Intent(this, typeof(WearBot));
                            StartActivity(intent2);
                        }
                    }
                }


                loginButton.Click += async (object sender, EventArgs e) =>
                {

                    if (usernameTxt.Text != "" && passTxt.Text != "")
                    {
                        if (isOnline())
                        {
                            await RefreshTodoItems();
                        }
                        else
                        {
                            string u = usernameTxt.Text;
                            string p = passTxt.Text;
                            passTxt.Text = "";
                            usernameTxt.Text = "";

                            var serobject1 = prefs.GetString("remembered", null);

                            var us1 = JsonConvert.DeserializeObject<users>(serobject1);

                            main_User = us1;
                            if (main_User != null && main_User.Username == u && main_User.Password == Crypt(p))
                            {


                            }
                            else
                            {
                                error.Text = "There is no offline user like this.Please check your network connection.";
                                await Task.Delay(4000);
                                error.Text = "";
                            }
                        }
                    }
                    else
                    {
                        error.Text = "Fill the boxes above";
                        await Task.Delay(3000);
                        error.Text = "";
                    }
                };
                signupButton.Click += async (object sender, EventArgs e) =>
                {
                    if (isOnline())
                    {
                        var intent = new Intent(this, typeof(SignUpForm));

                        StartActivity(intent);
                    }
                    else
                    {
                        error.Text = "You must be online to register";
                        await Task.Delay(3000);
                        error.Text = "";
                    }

                };
            };
        }
        public string Crypt(string str)
        {


            byte[] keyMaterial = new byte[16] { 0x2, 0x5, 0x11, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, };
            byte[] data = Encoding.ASCII.GetBytes(str);
            var provider = WinRTCrypto.SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
            var key = provider.CreateSymmetricKey(keyMaterial);
            byte[] cipherText = WinRTCrypto.CryptographicEngine.Encrypt(key, data, null);
            return Encoding.ASCII.GetString(cipherText);
        }

        private async Task RefreshTodoItems()
        {



            error.Text = "Attempting to connect....";
            MobileServiceInvalidOperationException exception = null;
            try
            {
                //   error.Text = "FTANEI EDW123123";
                items = await usersTable.ToCollectionAsync();
                error.Text = "Attempting to connect....";
            }
            
            catch (Exception e)

            {
                error.Text = "Error";
            }


            if (exception != null)
            {
                error.Text = "Error";
            }
            else
            {

                int index = -1;
                error.Text = items.Count.ToString();
                for (int i = 0; i < items.Count; i++)
                {
                   
                    if (items[i].Username == usernameTxt.Text)
                    {
                        error.Text = "zzzzzzzzzzzzzzzzzzzzzzzzzzzzzz";
                        index = i;
                    }
                }
                if (index == -1)
                {
                
                    error.Text = "Name doesn't exist";
                    await Task.Delay(4000);
                    error.Text = "";
                }
                else
                {
              
                    if (items[index].Password == Crypt(passTxt.Text))
                    {


                        main_User = items[index];
                        error.Text = "Connected";

                        error.Text = "";

                        var intent = new Intent(this, typeof(LoggedIn));
                        StartActivity(intent);
                    }
                    else
                    {

                        error.Text = "Wrong password";
                        await Task.Delay(4000);
                        error.Text = "";
                    }
                }

            }
            passTxt.Text = "";
            usernameTxt.Text = "";
        }
    }
}


