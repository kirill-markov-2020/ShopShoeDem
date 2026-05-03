using ShopShoe.Db;
using ShopShoe.Helpers;
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
        private List<Product> _products = new List<Product>();
        private ShopShoeDbEntities _db = new ShopShoeDbEntities(); 
        public ProductWindow(User user)
        {
            InitializeComponent();
            FIO.Text = $"{user.Surname} {user.Name} {user.Patronymic}".Trim();
            LoadProducts();
            LoadData();
        }
        public ProductWindow()
        {
            InitializeComponent();
            LoadProducts();
            LoadData();
        }

        public void LoadProducts()
        {
            _products = _db.Product.ToList();
            ProductList.ItemsSource = _products;
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

            if (!string.IsNullOrWhiteSpace(search.TrimEnd()))
            {
                query = query.Where(p => (p.Name != null && p.Name.ToLower().Contains(search.ToLower())) ||
                (p.Description != null && p.Description.ToLower().Contains(search.ToLower())) ||
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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            new ProductEditWindow().Show();
            Close();

        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ProductList.SelectedItem is Product selectedProduct)
            {
                new ProductEditWindow(selectedProduct).Show();
                Close();
            }
        }
    }
}
