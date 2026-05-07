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
    /// Логика взаимодействия для OrderWindow.xaml
    /// </summary>
    public partial class OrderWindow : Window
    {
        private List<Order> _orders = new List<Order>();
        private ShopShoeDbEntities _db = new ShopShoeDbEntities();
        private User _currentUser;
        public OrderWindow()
        {
            InitializeComponent();
            LoadOrders();
        }
        public void LoadOrders()
        {
            _orders = _db.Order.ToList();
            ProductList.ItemsSource = _orders;
        }
    }
}
