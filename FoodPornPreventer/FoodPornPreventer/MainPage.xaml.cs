using CoreTweet;
using CoreTweet.Core;
using CoreTweet.Streaming;
using Microsoft.ProjectOxford.Vision;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FoodPornPreventer
{
    public partial class MainPage : ContentPage
    {
        public ObservableCollection<SimpleTimelineEntity> TimelineEntities { get; } = new ObservableCollection<SimpleTimelineEntity>();

        // トークンの生成
        // 今回は事前にTwitterアカウントで作成しておいたアクセス情報を使用
        private Tokens Tokens { get; } = Tokens.Create(Secrets.ConsumerKey, Secrets.ConsumerSecret, Secrets.AccessToken, Secrets.AccessTokenSecret);

        private const int TweetCount = 10;

        public MainPage()
        {
            InitializeComponent();

            TweetList.ItemAppearing += TweetList_ItemAppearing;
        }

        private async void TweetList_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            try
            {
                if (TimelineEntities.LastOrDefault() == e.Item as SimpleTimelineEntity)
                {
                    await AddTweets(TimelineEntities.Last().Id);
                }
            }
            catch (Exception ex)
            {
            }
        }

        protected override async void OnAppearing()
        {
            try
            {
                // タイムラインの取得とリストビューへの表示
                await AddTweets(null);
            }
            catch (Exception ex)
            {

            }
        }

        private async Task AddTweets(long? maxId = null)
        {
            var timeline = await Tokens.Statuses.HomeTimelineAsync(count: TweetCount, max_id: maxId);

            // リストビューへの表示
            foreach (var item in timeline)
            {
                var timelineEntity = new SimpleTimelineEntity
                {
                    Id = item.Id,
                    IsRetweet = item.RetweetedStatus != null
                };

                // リツイートかどうかで情報をわける
                if (timelineEntity.IsRetweet)
                {
                    // リツイートの場合は元ツイートの内容を格納
                    timelineEntity.TweetText = item.RetweetedStatus.Text;
                    timelineEntity.UserName = item.RetweetedStatus.User.Name;
                    timelineEntity.UserProfileImageUrl = item.RetweetedStatus.User.ProfileImageUrl;
                    timelineEntity.CreatedAt = item.RetweetedStatus.CreatedAt.LocalDateTime.ToString("F");
                    timelineEntity.RetweetMessage = $"{item.User.Name}さんがリツイート";
                }
                else
                {
                    // それ以外はそのまま格納
                    timelineEntity.TweetText = item.Text;
                    timelineEntity.UserName = item.User.Name;
                    timelineEntity.UserProfileImageUrl = item.User.ProfileImageUrl;
                    timelineEntity.CreatedAt = item.CreatedAt.LocalDateTime.ToString("F");
                };

                if (item.ExtendedEntities != null)
                {
                    var imageUrls = new List<string>();
                    foreach (var media in item.ExtendedEntities.Media)
                    {
                        if (await IsFoodPornAsync(media.MediaUrl))
                        {
                            // 飯テロ画像が含まれていた場合、「閲覧注意」の警告文に差し替え
                            imageUrls.Add("");
                        }
                        else
                        {
                            imageUrls.Add(media.MediaUrl);
                        }
                    }
                    // Twitterの画像は最大4枚なので要素数は4固定
                    timelineEntity.TweetImageUrls = new string[4];
                    Array.Copy(imageUrls.ToArray(), timelineEntity.TweetImageUrls, imageUrls.Count());                    
                }
                TimelineEntities.Add(timelineEntity);
            }
        }

        private async Task<bool> IsFoodPornAsync(string url)
        {
            // Computer Vision APIで解析してTagsを取得
            var vision = new VisionServiceClient(Secrets.VisionSubscriptionKey, Secrets.VisionApiRoot);
            var result = await vision.GetTagsAsync(url);
            return result.Tags.Any(t => t.Name == "food");
        }

        
    }
}
