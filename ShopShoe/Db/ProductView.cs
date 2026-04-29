using System;
using System.Collections.Generic;
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

        
        public Brush GetBack()
        {
            if (Discount > 15)
            {
                return (Brush)ColorConverter.ConvertFromString("#2e8b57");
            }
            if (AmountStock <= 0)
            {
                return Brushes.LightBlue;
            }
            return Brushes.Chartreuse;
        }

    }
}
