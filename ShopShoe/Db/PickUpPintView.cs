using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopShoe.Db
{
    public partial class PickUpPoint
    {
        public string  FullAddress => $"{PostCode}, {City}, {Street}, {House}".TrimEnd(' ',',');
    }
}
