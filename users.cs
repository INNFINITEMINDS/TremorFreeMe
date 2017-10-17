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
using Newtonsoft.Json;

namespace TremorFreeMe
{
    public class users
    {
        public string Id { get; set; }
        [JsonProperty(PropertyName = "FirstName")]
        public string Fname { get; set; }

        [JsonProperty(PropertyName = "LastName")]
        public string Lname { get; set; }

        [JsonProperty(PropertyName = "Sex")]
        public string Sex { get; set; }

        [JsonProperty(PropertyName = "Username")]
        public string Username { get; set; }

        [JsonProperty(PropertyName = "Password")]
        public string Password { get; set; }

        [JsonProperty(PropertyName = "Age")]
        public string Age { get; set; }

        [JsonProperty(PropertyName = "Dis")]
        public string Dis { get; set; }

        [JsonProperty(PropertyName = "DisOffset")]
        public string DissOffset { get; set; }

        [JsonProperty(PropertyName = "PrefIntens")]
        public int PrefIntens { get; set; }

        [JsonProperty(PropertyName = "EmotionsAv")]
        public string EmotionsAv { get; set; }

        [JsonProperty(PropertyName = "TremorAv")]
        public string TremorAv { get; set; }

        [JsonProperty(PropertyName = "Frequency")]
        public string Frequency { get; set; }
    }
}