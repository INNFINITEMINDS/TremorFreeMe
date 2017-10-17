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
using Java.Util;
using System.IO;
using Android.Bluetooth;
using Android.Hardware;
using System.Timers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Threading;
using Android.Net;
using System.Globalization;

namespace TremorFreeMe
{
    [Activity(Label = "TremorFreeMe", Icon = "@drawable/icon")]
    public class LoggedIn : Activity, ISensorEventListener
    {
        Button close;
        ProgressBar prog;
        TextView intensity;
        public AlertDialog ad;
        TextView bttxt;
        public bool isOnline()
        {
            ConnectivityManager cm =
                (ConnectivityManager)GetSystemService(Context.ConnectivityService);
            NetworkInfo netInfo = cm.ActiveNetworkInfo;
            return netInfo != null && netInfo.IsConnectedOrConnecting;
        }
        SensorManager mSensorManager;
        Sensor mHeartRateSensor;
        TextView heart_txt;
        float heartRate = 0;
        System.Timers.Timer timer;
        private BluetoothAdapter mBluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        private BluetoothSocket btSocket = null;
        private Java.Lang.String dataToSend;
        private Stream outStream = null;
        private Stream inStream = null;
        public string address = "";                         /* YOURS BLUETOOTH'S MAC ADDRESS */
        private static UUID MY_UUID = UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
        bool Bt_on = false;
        bool dev_on = false;
        ImageButton bt_btn;
        int timercount=0;
        ISharedPreferences prefs;
        ImageButton dev;
        users mainUser;
        bool devNotFound = true;
        bool btflag = true;
        ImageButton plus;
        ImageButton minus;
        ProgressDialog dialog;
        VideoView video;
        Button freq_but;
        EditText txted;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.LoggedIn);
            txted = FindViewById<EditText>(Resource.Id.tbtext);
            freq_but = FindViewById<Button>(Resource.Id.send_btn);
            this.RequestedOrientation = Android.Content.PM.ScreenOrientation.Portrait;
            prog = FindViewById<ProgressBar>(Resource.Id.ProgressBar);
            video = FindViewById<VideoView>(Resource.Id.video);
        video.Visibility = ViewStates.Gone;
            close = FindViewById<Button>(Resource.Id.close);
            close.Visibility = ViewStates.Gone;
            prog.DrawingCacheBackgroundColor = Android.Graphics.Color.Aqua;
            prog.ProgressDrawable.SetColorFilter(Android.Graphics.Color.ParseColor("#C93766"), Android.Graphics.PorterDuff.Mode.Multiply);          
            intensity = FindViewById<TextView>(Resource.Id.value_txt);
            plus=FindViewById<ImageButton>(Resource.Id.plus_btn);
            minus = FindViewById<ImageButton>(Resource.Id.minus_btn);
            plus.SetImageResource(Resource.Drawable.plus);
            minus.SetImageResource(Resource.Drawable.minus);
            plus.SetBackgroundColor(Android.Graphics.Color.Transparent);
            minus.SetBackgroundColor(Android.Graphics.Color.Transparent);
            plus.Visibility = ViewStates.Visible;
            minus.Visibility = ViewStates.Visible;
            dev = FindViewById<ImageButton>(Resource.Id.dev_reck);
            dev.SetImageResource(Resource.Drawable.off_pic);
            dev.Visibility = ViewStates.Visible;///////////////////////////////           
            bttxt = FindViewById<TextView>(Resource.Id.bt_checktxt);
            bttxt.Visibility = ViewStates.Invisible;
            bttxt.Text = mBluetoothAdapter.IsEnabled.ToString();
            bttxt.TextChanged += Bttxt_TextChanged;
            prefs = MainActivity.prefs;
           mainUser = MainActivity.main_User;          
            prog.Progress = 25;
            mainUser.PrefIntens = 25;
            intensity.Text = "Intensity : "+25+"%";
           string json = JsonConvert.SerializeObject(mainUser);
                var prefEditor = prefs.Edit();
                prefEditor.PutString("remembered", json);
                prefEditor.Commit();
            heart_txt = FindViewById<TextView>(Resource.Id.heart_txt);      
            bt_btn = FindViewById<ImageButton>(Resource.Id.round_btn);
            bt_btn.SetImageResource(Resource.Drawable.mar);
            bt_btn.SetBackgroundColor(Android.Graphics.Color.Transparent);
            dev.SetBackgroundColor(Android.Graphics.Color.Transparent);
            
             
             /////SENSORS////
             mSensorManager = (SensorManager)GetSystemService(SensorService);
            mHeartRateSensor = mSensorManager.GetDefaultSensor(SensorType.HeartRate);
            mSensorManager.RegisterListener(this, mHeartRateSensor, SensorDelay.Fastest);

            /////////TIMERS/////////////////
           timer = new System.Timers.Timer();
           timer.Interval = 500;
           timer.Start();
            timer.Elapsed += Timer_Elapsed;
       
            bt_btn.Click += But_Click;
           
            dev.Click += Dev_Click;
            plus.Click += Plus_Click;
            minus.Click += Minus_Click;
            close.Click += Close_Click;
            
            freq_but.Click += Freq_but_Click;
        }

        private void Freq_but_Click(object sender, EventArgs e)
        {
            SendData(10);
            SendData(float.Parse(txted.Text, CultureInfo.InvariantCulture.NumberFormat));
            txted.Text = "";
        }

        

        private void Close_Click(object sender, EventArgs e)
        {
            video.StopPlayback();
            video.Visibility = ViewStates.Gone;
            video.Dispose();
            close.Visibility = ViewStates.Gone;
        }

        private void Minus_Click(object sender, EventArgs e)
        {
         
            if (mainUser.PrefIntens > 0)
            {
                
                mainUser.PrefIntens--;
                SendData(20);
                SendData(mainUser.PrefIntens);
                prog.Progress = mainUser.PrefIntens;
                intensity.Text = "Intensity : " + mainUser.PrefIntens.ToString() + "%";
               
            }
        }

        private void Plus_Click(object sender, EventArgs e)
        {
            
            if (mainUser.PrefIntens < 100)
            {
              
                mainUser.PrefIntens++;
                SendData(20);
                SendData(mainUser.PrefIntens);
                prog.Progress = mainUser.PrefIntens;
                intensity.Text = "Intensity : " + mainUser.PrefIntens.ToString() + "%";
                }
           
        }

        private void Dev_Click(object sender, EventArgs e)
        {
            if (Bt_on)
            {
                if (!devNotFound)
                {
                    if (dev_on)
                    {
                        dev_on = !dev_on;
                        SendData(2);
                        dev.SetImageResource(Resource.Drawable.off_pic);
                    }
                    else
                    {
                        dev_on = !dev_on;
                        SendData(3);
                        dev.SetImageResource(Resource.Drawable.on_pic);

                    }
                }else
                {
                    Toast.MakeText(this, "Bluetooth is on but the device isn't nearby",
              ToastLength.Short).Show();
                }

            }
            else
            {
                dev.SetImageResource(Resource.Drawable.off_pic);
                Toast.MakeText(this, "Device is not connected, bluetooth is off",
                   ToastLength.Short).Show();

            }
           

         
        }
      
        private async void Bttxt_TextChanged(object sender, Android.Text.TextChangedEventArgs e)
        {
         
            if (mBluetoothAdapter.IsEnabled && btflag)
            {
                btflag = false;
                dialog = new ProgressDialog(this);
                dialog.SetCancelable(false);
                dialog.SetMessage("Searching for device...");
                dialog.SetProgressStyle(ProgressDialogStyle.Spinner);
                dialog.Progress = 0;
                dialog.Max = 100;
                dialog.Show();


                await Task.Delay(3000);
                await btcheck();
                
                dialog.Dismiss();
                Bt_on = true;
             
                


            }
            if (!mBluetoothAdapter.IsEnabled)
            {
                Bt_on = false; ;
                btflag = true;
                dev.SetImageResource(Resource.Drawable.off_pic);
                
            }
        }
       
        public override void OnBackPressed()
        {


            AlertDialog.Builder alert = new AlertDialog.Builder(this);
            alert.SetTitle("");
           
            MainActivity.wantToBeRembered = false;
            alert.SetMessage("What do you want to do?");
            alert.SetPositiveButton("Log Out", (senderAlert, args) => {
                var prefEditor = prefs.Edit();
                prefEditor.PutString("remembered", "");
                prefEditor.Commit();
                if (Bt_on && !devNotFound)
                {
                    bt_Disconnect();
                }
                var intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
                base.OnBackPressed();
            });

            alert.SetNegativeButton("Suspend Application", (senderAlert, args) => {

                Intent main = new Intent(Intent.ActionMain);
                main.AddCategory(Intent.CategoryHome);
                StartActivity(main);
            });
          
            Dialog dialog = alert.Create();
            dialog.Show();

            
            
        }

     
        private Task btcheck()
        {
         
        
            if (mBluetoothAdapter.IsEnabled)
            {
           
                bt_Connect();
                Bt_on = true;
                return Task.CompletedTask;

            }
            else
            {
                Bt_on = false;
                // heart_txt.Text = "Bluetooth is disabled";
                return Task.CompletedTask;
            }
        }


        private void But_Click(object sender, EventArgs e)
        {
            video = FindViewById<VideoView>(Resource.Id.video);
            var fullPath = String.Format("android.resource://{0}/{1}", PackageName, Resource.Drawable.instruct);

            video.SetMediaController(new MediaController(this));
           

            video.SetVideoPath(fullPath);

            video.Visibility = ViewStates.Visible;
            video.RequestFocus();
           
            video.Start();
            close.Visibility = ViewStates.Visible;
         
        }


        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
           
            timer.Stop();
           
            RunOnUiThread( () => { bttxt.Text = mBluetoothAdapter.IsEnabled.ToString();
                    
            });
            if (DateTime.Now.Second == 59 && isOnline()){
                new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    await MainActivity.usersTable.UpdateAsync(mainUser);

                }).Start();
            }
           
            timer.Interval = 500;
            timer.Start();
        }

        public void OnAccuracyChanged(Sensor sensor, [GeneratedEnum] SensorStatus accuracy)
        {

        }

        public void OnSensorChanged(SensorEvent e)
        {
            heartRate = e.Values[0];

        }
        public void bt_Connect()
        {
 
            BluetoothDevice device = mBluetoothAdapter.GetRemoteDevice(address);
          
            mBluetoothAdapter.CancelDiscovery();
            try
            {
                btSocket = device.CreateRfcommSocketToServiceRecord(MY_UUID);
                btSocket.Connect();
                Toast.MakeText(this, "Bluetooth connected!",
                      ToastLength.Short).Show();
                Bt_on = true;
             
          
                
                devNotFound = false;
            }

            catch (Exception ex)

            {
          
                Toast.MakeText(this, "Device not found... ",
                         ToastLength.Short).Show();
                devNotFound = true;
              //  dev_btn.SetImageResource(Resource.Drawable.device_bt_off);
                try
                {
                    btSocket.Close();

                }

                catch (System.Exception e)

                {
                }

            }
            

        }
        public void bt_Disconnect()
        {
            if (!devNotFound)
            {
                try
                {
                    btSocket.Close();
                    Toast.MakeText(this, "Bluetooth disconnected!",
                          ToastLength.Short).Show();
                    Bt_on = false;
                  //  bt_btn.SetImageResource(Resource.Drawable.bt_on1);
                    
                }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                catch (System.Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                {
                    
                }
            }else
            {
                Toast.MakeText(this, "Device not found...",
                         ToastLength.Short).Show();
            }
        }
        private void SendData(float tobesent)
        {

            Java.Lang.Float data;
            data = new Java.Lang.Float(tobesent);
            try
            {
                outStream = btSocket.OutputStream;
            }

            catch (System.Exception e)

            {

            }
            sbyte byt = data.ByteValue();
            byte byt2 = (byte)byt;
            try
            {
                outStream.WriteByte(byt2);

            }

            catch (System.Exception e)

            {

            }
        }
    }

}
