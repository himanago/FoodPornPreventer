using CoreTweet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FoodPornPreventer
{
    public class SimpleTimelineEntity
    {
        public long Id { get; set; }
        public string TweetText { get; set; }
        public TweetImageInfo[] TweetImages { get; set; }
        public string UserName { get; set; }
        public string UserProfileImageUrl { get; set; }
        public string CreatedAt { get; set; }
        public bool IsRetweet { get; set; }
        public string RetweetMessage { get; set; }
    }

    public class TweetImageInfo
    {
        public string Url { get; set; }
        public bool IsFoodPorn { get; set; }
    }
}
