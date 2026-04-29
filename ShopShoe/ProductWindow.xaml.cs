using ShopShoe.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ShopShoe
{
    /// <summary>
    /// Логика взаимодействия для ProductWindow.xaml
    /// </summary>
    public partial class ProductWindow : Window
    {
        private List<Product> products = new List<Product>();
        private ShopShoeDbEntities _db = new ShopShoeDbEntities(); 
        public ProductWindow(User user)
        {
            InitializeComponent();
            FIO.Text = $"{user.Surname} {user.Name} {user.Patronymic}".Trim();
            LoadProducts();
        }
        public ProductWindow()
        {
            InitializeComponent();
            LoadProducts();
        }

        public void LoadProducts()
        {
            products = _db.Product.ToList();
            ProductList.ItemsSource = products;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }
    }
}
