namespace ShopShoe.Db
{
    public partial class User
    {
        public string FullName => $"{Surname} {Name} {Patronymic}".Trim();
    }
}
