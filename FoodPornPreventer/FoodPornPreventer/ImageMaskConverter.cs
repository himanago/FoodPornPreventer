using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace FoodPornPreventer
{
    public class ImageMaskConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // 飯テロ画像なら画像を差し替え
            var imageInfo = value as TweetImageInfo;

            if (imageInfo == null) return null;

            return imageInfo.IsFoodPorn
                ? ImageSource.FromResource("FoodPornPreventer.Images.mark_chuui.png")
                : ImageSource.FromUri(new Uri(imageInfo.Url));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
