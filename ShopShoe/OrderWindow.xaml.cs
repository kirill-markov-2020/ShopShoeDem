using ShopShoe.Db;
using ShopShoe.Helpers;
using ShopShoe.Statics;
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
            _currentUser = CurrentSession.CurrentUser;
            LoadOrders();
            LoadUI();
        }
        public void LoadOrders()
        {
            _orders = _db.Order.ToList();
            OrderList.ItemsSource = _orders;
        }
        private void LoadUI()
        {
            int roleId = _currentUser?.RoleId ?? 3;

            AddOrderButton.Visibility = Visibility.Collapsed;
            OrderList.ContextMenu = null;
            if (roleId == 1)
            {
                AddOrderButton.Visibility = Visibility.Visible;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new ProductWindow(_currentUser).Show();
            Close();
        }

        private void AddOrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditWindowOpen())
                return;
            var editWindow = new OrderEditWindow(null, _currentUser);
            if (editWindow.ShowDialog() == true)
            {
                LoadOrders();
                MessageHelper.ShowInformation("Список заказов обновлен");
            }
        }
        private bool IsEditWindowOpen()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is OrderEditWindow)
                {
                    MessageHelper.ShowWarning("Окно редактирования уже открыто");
                    window.Activate();
                    return true;
                }
            }
            return false;
        }

        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || _currentUser.RoleId != 1)
            {
                MessageHelper.ShowError("Доступ запрещен! Только администратор может редактировать заказы.");
                return;
            }
            var menuItem = sender as MenuItem;
            var order = menuItem?.Tag as Order;
            if (order == null) return;
            if (IsEditWindowOpen()) return;
            var editWindow = new OrderEditWindow(order, _currentUser);
            if (editWindow.ShowDialog() == true)
            {
                LoadOrders();
                MessageHelper.ShowInformation("Список заказов обновлен");
            }
        }
        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null || _currentUser.RoleId != 1)
            {
                MessageHelper.ShowError("Доступ запрещен! Только администратор может редактировать заказы.");
                return;
            }
            var menuItem = sender as MenuItem;
            var order = menuItem?.Tag as Order;
            if(order == null) return;
            var result = MessageBox.Show($"Вы уверены, что хотите удалить заказ #{order.Id}?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    _db.Order.Remove(order);
                    _db.SaveChanges();
                    LoadOrders();
                    MessageHelper.ShowInformation("Заказ удален");
                }
                catch (Exception ex)
                {
                    MessageHelper.ShowError($"Ошибка при удалении заказа: {ex.Message}");
                }
            }
        }

        private void OrderList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (_currentUser == null || _currentUser.RoleId != 1)
            {
                MessageHelper.ShowError("Доступ запрещен! Только администратор может редактировать заказы.");
                return;
            }
            var selectedOrder = OrderList.SelectedItem as Order;
            if (selectedOrder == null) return;
            if(IsEditWindowOpen()) return;
            var editWindow = new OrderEditWindow(selectedOrder, _currentUser);
            if (editWindow.ShowDialog() == true)
            {
                LoadOrders();
                MessageHelper.ShowInformation("Список заказов обновлен");
            }
        }
    }
}
