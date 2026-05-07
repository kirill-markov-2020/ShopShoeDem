using ShopShoe.Db;
using ShopShoe.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ShopShoe
{
    /// <summary>
    /// Логика взаимодействия для ProductWindow.xaml
    /// </summary>
    public partial class ProductWindow : Window
    {
        private List<Product> _products = new List<Product>();
        private ShopShoeDbEntities _db = new ShopShoeDbEntities();
        private User _currentUser;
        public ProductWindow(User user = null)
        {
            InitializeComponent();
            _currentUser = user;
            LoadUI();
            if (user != null)
                FIO.Text = $"{user.Surname} {user.Name} {user.Patronymic}".Trim();
            else
                FIO.Text = "Гость";

            LoadProducts();
            LoadData();

            if (_currentUser == null || _currentUser.RoleId != 1) 
                AddProductButton.Visibility = Visibility.Collapsed;
        }

        public void LoadProducts()
        {
            _products = _db.Product.ToList();
            ProductList.ItemsSource = _products;
            ApplyFilter();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }

        public void LoadData()
        {
            var filters = new List<string>();
            filters.Add("Все поставщики");
            filters.AddRange(_db.Supplier.Select(x => x.Name).ToList());
            
            FilterCombobox.ItemsSource = filters;

        }

        private void LoadUI()
        {
            int roleId = _currentUser?.RoleId ?? 3;

            AddProductButton.Visibility = Visibility.Collapsed;
            SortingCombobox.Visibility = Visibility.Collapsed;
            FilterCombobox.Visibility = Visibility.Collapsed;
            SearchTextBox.Visibility = Visibility.Collapsed;
            OrderButton.Visibility = Visibility.Collapsed;


            switch (roleId)
            {
                case 1:
                    AddProductButton.Visibility = Visibility.Visible;
                    SortingCombobox.Visibility = Visibility.Visible;
                    FilterCombobox.Visibility = Visibility.Visible;
                    SearchTextBox.Visibility = Visibility.Visible;
                    OrderButton.Visibility = Visibility.Visible;
                    break;

                case 2:
                    SortingCombobox.Visibility = Visibility.Visible;
                    FilterCombobox.Visibility = Visibility.Visible;
                    SearchTextBox.Visibility = Visibility.Visible;
                    OrderButton.Visibility = Visibility.Visible;

                    break;

                case 3:
                    break;
            }
        }

        private void SortingCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void FilterCombobox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilter();

        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();

        }
        public void ApplyFilter()
        {
            if (SearchTextBox == null || FilterCombobox == null || SortingCombobox == null)
                return ;
            var query = _products.AsEnumerable();

            string search = SearchTextBox.Text ?? "";
            string filter = FilterCombobox.SelectedItem as string ?? "Все поставщики";
            int sort = SortingCombobox.SelectedIndex;

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => (p.Name != null && p.Name.ToLower().Contains(search.ToLower())) ||
                (p.Description != null && p.Description.ToLower().Contains(search.ToLower())) ||
                (p.Category != null && p.Category.Name.ToLower().Contains(search.ToLower())) ||
                (p.Unit != null && p.Unit.Name.ToLower().Contains(search.ToLower())) ||
                (p.Producer != null && p.Producer.Name.ToLower().Contains(search.ToLower())) ||
                (p.Supplier != null && p.Supplier.Name.ToLower().Contains(search.ToLower())));
            }
            if (filter != "Все поставщики")
            {
                query = query.Where(p => p.Supplier != null && p.Supplier.Name == filter);
            }

            if (sort == 1)
            {
                query = query.OrderByDescending(p => p.AmountStock);
            }
            else if (sort == 2)
            {
                query = query.OrderBy(p => p.AmountStock);
            }
            ProductList.ItemsSource = query.ToList();

        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditWindowOpen())
                return;
            var editWindow = new ProductEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadProducts();
                MessageHelper.ShowInformation("Список товаров обновлен");
            }
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var product = menuItem?.Tag as Product;
            if (product == null)
                return;
            if (_currentUser != null && _currentUser.RoleId == 1)
            {
                var selectedProduct = ProductList.SelectedItem as Product;
                if (selectedProduct == null)
                    return;
                if (IsEditWindowOpen())
                    return;
                var editWindow = new ProductEditWindow(product);
                if (editWindow.ShowDialog() == true)
                {
                    LoadProducts();
                    MessageHelper.ShowInformation("Список товаров обновлен");
                }
            }
            else
            {
                MessageHelper.ShowError("Вы не администратор!");
            }

        }

        private void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var product = menuItem?.Tag as Product;
            if (product == null)
                return;
            if (_currentUser != null && _currentUser.RoleId == 1)
            {
                bool isInOrder = _db.OrderItem.Any(oi => oi.ProductId == product.Id);
                if (isInOrder)
                {
                    MessageHelper.ShowError($"Товар {product.Name} присутствует в заказах!");
                    return;
                }
                var result = MessageBox.Show($"Удалить товар {product.Name}?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if(result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if(!string.IsNullOrEmpty(product.Photo) && File.Exists(product.Photo)) 
                            File.Delete(product.Photo);
                        _db.Product.Remove(product);
                        _db.SaveChanges();
                        LoadProducts();
                        MessageHelper.ShowInformation("Удалено");

                    }
                    catch (Exception ex)
                    {
                        MessageHelper.ShowError(ex.Message);
                    }
                }
            }
            else
            {
                MessageHelper.ShowError("Вы не администратор!");
            }
        }


        private void ProductList_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if(_currentUser != null && _currentUser.RoleId == 1)
            {
                var selectedProduct = ProductList.SelectedItem as Product;
                if(selectedProduct == null) 
                    return;
                if (IsEditWindowOpen())
                    return;
                var editWindow = new ProductEditWindow(selectedProduct);
                if (editWindow.ShowDialog() == true)
                {
                    LoadProducts();
                    MessageHelper.ShowInformation("Список товаров обновлен");
                }
            }
        }
        private bool IsEditWindowOpen()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is ProductEditWindow)
                {
                    MessageHelper.ShowWarning("Окно редактирования уже открыто");
                    window.Activate();
                    return true;
                }
            }
            return false;
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            OrderWindow orderWindow = new OrderWindow();
            orderWindow.Show();
            Close();
        }
    }
}
