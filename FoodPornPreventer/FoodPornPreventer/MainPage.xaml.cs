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

        private const int TweetCount = 50;

        public MainPage()
        {
            InitializeComponent();
        }

        private async void TweetList_ItemAppearing(object sender, ItemVisibilityEventArgs e)
        {
            if (TimelineEntities.Last() == e.Item as SimpleTimelineEntity)
            {
                await AddTweets(TimelineEntities.Last().Id);
            }
        }

        protected override async void OnAppearing()
        {
            // タイムラインの取得とリストビューへの表示
            await AddTweets(null);

            TweetList.ItemAppearing += TweetList_ItemAppearing;
        }

        private async Task AddTweets(long? maxId = null)
        {
            try
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
                        var tweetImages = new List<TweetImageInfo>();
                        foreach (var media in item.ExtendedEntities.Media)
                        {
                            tweetImages.Add(new TweetImageInfo
                            {
                                Url = media.MediaUrl,
                                IsFoodPorn = await IsFoodPornAsync(media.MediaUrl)
                        });
                        }
                        // Twitterの画像は最大4枚なので要素数は4固定
                        timelineEntity.TweetImages = new TweetImageInfo[4];
                        Array.Copy(tweetImages.ToArray(), timelineEntity.TweetImages, tweetImages.Count());
                    }
                    TimelineEntities.Add(timelineEntity);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message == "Rate limit exceeded")
                {
                    await System.Threading.Tasks.Task.Delay(1000);
                    await AddTweets(maxId);
                }
                else
                {
                    await DisplayAlert("警告", "ツイートの読み込みに失敗しました。", "閉じる");
                }
            }
        }

        private async Task<bool> IsFoodPornAsync(string url)
        {
            try
            {
                // Computer Vision APIで解析してTagsを取得
                var vision = new VisionServiceClient(Secrets.VisionSubscriptionKey, Secrets.VisionApiRoot);
                VisualFeature[] features = { VisualFeature.Tags, VisualFeature.Categories, VisualFeature.Description };
                var result = await vision.AnalyzeImageAsync(url, features.ToList());
                return result.Tags.Any(t => t.Hint == "food" || t.Name == "food");
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
