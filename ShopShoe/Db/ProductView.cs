using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ShopShoe.Db
{
    public partial class Product
    {
        public decimal NewPrice => Price * (1 - (Discount / 100));
        public Brush BackgroundColor => GetBack();
        public string NewPhoto => GetPhotoPath();

        
        public Brush GetBack()
        {
            if (Discount > 15)
            {
                return (Brush)new BrushConverter().ConvertFromString("#2e8b57");
            }
            if (AmountStock <= 0)
            {
                return Brushes.LightBlue;
            }
            return Brushes.Chartreuse;
        }
        public string GetPhotoPath()
        {
            if (string.IsNullOrWhiteSpace(Photo))
            {
                return null;//Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", "picture.png");

            }
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Photo);
        }

    }
}
