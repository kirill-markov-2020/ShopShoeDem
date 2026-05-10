namespace ShopShoe.Db
{
    public partial class PickUpPoint
    {
        public string  FullAddress => $"{PostCode}, {City}, {Street}, {House}".TrimEnd(' ',',');
    }
}
