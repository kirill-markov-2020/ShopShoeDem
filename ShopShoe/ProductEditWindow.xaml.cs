using Microsoft.Win32;
using ShopShoe.Db;
using ShopShoe.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Path = System.IO.Path;
using System.Data.Entity.Migrations;

namespace ShopShoe
{
    /// <summary>
    /// Логика взаимодействия для ProductEditWindow.xaml
    /// </summary>
    public partial class ProductEditWindow : Window
    {
        private ShopShoeDbEntities _db = new ShopShoeDbEntities();
        private Product _currentProduct;
        private ImageHelper ImageHelper = new ImageHelper();
        private string _photoPath = null;
        public ProductEditWindow()
        {
            InitializeComponent();
            _currentProduct = null;
            Title = "Добавление товара";
            IdTextBox.Visibility = Visibility.Collapsed;
            IdTextBlock.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
            LoadComboBoxes();

            
        }
        public ProductEditWindow(Product product)
        {
            InitializeComponent();
            _currentProduct = product;
            IdTextBox.Visibility = Visibility.Visible;
            IdTextBlock.Visibility = Visibility.Visible;
            Title = $"Редактирование товара {product.Name} ({product.Article})";
            LoadComboBoxes();
            FillFields();
        }
        public void LoadComboBoxes()
        {
            CategoryComboBox.ItemsSource = _db.Category.ToList();
            UnitComboBox.ItemsSource = _db.Unit.ToList();
            SupplierComboBox.ItemsSource = _db.Supplier.ToList();
            ProducerComboBox.ItemsSource = _db.Producer.ToList();
        }
        public void FillFields()
        {
            if (_currentProduct == null) return;
            IdTextBox.Text = _currentProduct.Id.ToString();
            ArticleTextBox.Text = _currentProduct.Article;
            NameTextBox.Text = _currentProduct.Name;
            DescriptionTextBox.Text = _currentProduct.Description;
            PriceTextBox.Text = _currentProduct.Price.ToString();
            AmountStockTextBox.Text = _currentProduct.AmountStock.ToString();
            DiscountTextBox.Text = _currentProduct.Discount.ToString();
            CategoryComboBox.SelectedIndex = _currentProduct.CategoryId - 1;
            UnitComboBox.SelectedIndex = _currentProduct.UnitId - 1;
            SupplierComboBox.SelectedIndex = _currentProduct.SupplierId - 1;
            ProducerComboBox.SelectedIndex = _currentProduct.ProducerId - 1;
            LoadProductImage(_currentProduct.Photo);
        }
        public void LoadProductImage(string PhotoName)
        {
            string ImagePath;
            if (!string.IsNullOrEmpty(PhotoName))
            {
                ImagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PhotoName);
                if (File.Exists(ImagePath))
                {
                    ProductImage.Source = new BitmapImage(new Uri(ImagePath));
                    return;
                }
            }
        }
        

        private void PhotoButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Изображения|*.png;*.jpg;*.jpeg;*.bmp";

            if (dialog.ShowDialog() == true)
            {
                BitmapImage img = new BitmapImage();

                img.BeginInit();
                img.UriSource = new Uri(dialog.FileName);
                img.CacheOption = BitmapCacheOption.OnLoad;
                img.EndInit();

                if (img.PixelWidth > 300 || img.PixelHeight > 200)
                {
                    MessageHelper.ShowError("Размер изображения не должен превышать 300x200");
                    return;
                }

                _photoPath = dialog.FileName;
                ProductImage.Source = img;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var errors = new StringBuilder();
            if (string.IsNullOrWhiteSpace(ArticleTextBox.Text))
                errors.AppendLine("Поле артикул не заполнено!");
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                errors.AppendLine("Поле с наименованием не заполнено!");
            if (!decimal.TryParse(PriceTextBox.Text, out var price) || price < 0)
                errors.AppendLine("Поле цены не заполнено или заполнено неправильно!");
            if (!decimal.TryParse(DiscountTextBox.Text, out var discount) || discount < 0 || discount > 100)
                errors.AppendLine("Поле скидки не заполнено или заполнено неправильно!");
            if (!int.TryParse(AmountStockTextBox.Text, out var amount) || amount < 0)
                errors.AppendLine("Поле c количеством на складе не заполнено или заполнено неправильно!");
            if (CategoryComboBox.SelectedItem == null)
                errors.AppendLine("Категория товара не выбрана!");
            if (ProducerComboBox.SelectedItem == null)
                errors.AppendLine("Производитель товара не выбран!");
            if (SupplierComboBox.SelectedItem == null)
                errors.AppendLine("Поставщик товара не выбрана!");
            if (UnitComboBox.SelectedItem == null)
                errors.AppendLine("Единица измерения товара не выбрана!");
            if (errors.Length > 0)
            {
                MessageHelper.ShowError(errors.ToString());
                return;
            }
            try
            {

                if (_currentProduct == null)
                {
                    _currentProduct = new Product();
                    _db.Product.Add(_currentProduct);
                }
                _currentProduct.Article = ArticleTextBox.Text;
                _currentProduct.Name = NameTextBox.Text;
                _currentProduct.Description = DescriptionTextBox.Text;
                _currentProduct.Price = price;
                _currentProduct.Discount = discount;
                _currentProduct.AmountStock = amount;
                _currentProduct.CategoryId = (CategoryComboBox.SelectedItem as Category).Id;
                _currentProduct.ProducerId = (ProducerComboBox.SelectedItem as Producer).Id;
                _currentProduct.SupplierId = (SupplierComboBox.SelectedItem as Supplier).Id;
                _currentProduct.UnitId = (UnitComboBox.SelectedItem as Unit).Id;
                if (!string.IsNullOrEmpty(_photoPath))
                {
                    
                    string fileName = Path.GetFileName(_photoPath);
                    string destPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "res", fileName);
                    File.Copy(_photoPath, destPath, true);
                    _currentProduct.Photo = "res/" + fileName;
                }
                _db.Product.AddOrUpdate(_currentProduct);


                _db.SaveChanges();

                MessageHelper.ShowInformation("Товар сохранён!");
                GoBack();
            }
            catch (Exception ex)
            {
                
                MessageHelper.ShowError(ex.Message);
            }


        }

        private void GoBack()
        {
            new ProductWindow().Show();
            Close();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var itemsInOrder = _db.OrderItem.Select(op => op.ProductId).ToList();
            if (!itemsInOrder.Contains(_currentProduct.Id))
            {
                var productToDelete = _db.Product.FirstOrDefault(p => p.Id == _currentProduct.Id);

                _db.Product.Remove(productToDelete);
                _db.SaveChanges();
                MessageHelper.ShowInformation($"Товар {_currentProduct.Name} был удален!");
                GoBack();

            }
            else
                MessageHelper.ShowError($"Данный товар присутствует в заказе!");
            
           
        }
    }
}
