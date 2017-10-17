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
using PCLCrypto;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;

namespace TremorFreeMe
{
    [Activity(Label = "TremorFreeMe", Icon = "@drawable/icon")]
    public class SignUpForm : Activity
    {
        string sex="Male";
        Button submit_btn;
        EditText fname;
        EditText lname;
        EditText username;
        EditText pass;
        NumberPicker nums;
        CheckBox male;
        CheckBox female;
        EditText dis;
        EditText disoff;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.SignUpForm);
            submit_btn = FindViewById<Button>(Resource.Id.sub_btn);
            fname = FindViewById<EditText>(Resource.Id.fname_txt);
            lname = FindViewById<EditText>(Resource.Id.lname_txt);
            username = FindViewById<EditText>(Resource.Id.user_txt);
            pass = FindViewById<EditText>(Resource.Id.pass_txt);
            disoff = FindViewById<EditText>(Resource.Id.disoffset_txt);
            dis = FindViewById<EditText>(Resource.Id.dis_txt);
            nums = FindViewById<NumberPicker>(Resource.Id.numberPicker1);
            nums.MaxValue = 90;
            nums.MinValue = 18;
            nums.Value = 40;
            male = FindViewById<CheckBox>(Resource.Id.checkBox1);
            female = FindViewById<CheckBox>(Resource.Id.checkBox2);
            male.ButtonDrawable.SetColorFilter(Android.Graphics.Color.White,Android.Graphics.PorterDuff.Mode.SrcIn);
            female.ButtonDrawable.SetColorFilter(Android.Graphics.Color.White, Android.Graphics.PorterDuff.Mode.SrcIn);           
            male.Checked = true;
            female.Checked = false;
            male.CheckedChange += Male_CheckedChange;
            female.CheckedChange += Female_CheckedChange;
            submit_btn.Click += async (object sender, EventArgs e) =>
            {
                if (sex != "" && dis.Text!="" && disoff.Text!="" && fname.Text!="" && lname.Text!="" && username.Text!="" && pass.Text!= "" )
                {
                    users user = new users() { Frequency="0Hz",PrefIntens=0, TremorAv="50%",EmotionsAv="Neutral",Dis = dis.Text, DissOffset = disoff.Text, Age = nums.Value.ToString(), Fname = fname.Text, Lname = lname.Text, Password = Crypt(pass.Text), Sex = sex, Username = username.Text };
                    await RefreshTodoItems(user);

                }
                else
                {
                    Toast.MakeText(this, "You must fill all the details.",
                        ToastLength.Short).Show();
                }
            };
        }
        private void Female_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (female.Checked)
            {
                sex = "Female";
                male.Checked = false;
            }
            else
            {
                sex = "Male";
                male.Checked = true;
            }
        }

        private void Male_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            if (male.Checked)
            {
                female.Checked = false;
            }
            else
            {
                female.Checked = true;
            }
        }

        private async Task RefreshTodoItems(users todoItem)
        {

            MobileServiceInvalidOperationException exception = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems.
                MainActivity.items = await MainActivity.usersTable.ToCollectionAsync();

            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            if (exception != null)
            {

            }
            else
            {
                await MainActivity.usersTable.InsertAsync(todoItem);
                MainActivity.items.Add(todoItem);
             
                var intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);

            }
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
    }
}
