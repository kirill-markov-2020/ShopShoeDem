using System;
using System.Linq;
using System.Windows;
using ShopShoe.Db;
using ShopShoe.Helpers;
using Microsoft.Win32;
using System.IO;
using System.Windows.Media.Imaging;
using System.Text;

namespace ShopShoe
{
    public partial class ProductEditWindow : Window
    {
        private ShopShoeDbEntities _db = new ShopShoeDbEntities();
        private Product _editingProduct;
        private string _selectedPhotoPath;
        private string _originalPhotoPath;

        public ProductEditWindow(Product product = null)
        {
            InitializeComponent();
            _editingProduct = product;
            LoadComboBoxes();

            if (product != null)
            {
                Title = "Редактирование товара";
                LoadProductData();
                _originalPhotoPath = product.Photo;
            }
            else
            {
                Title = "Добавление товара";
                IdTextBox.Visibility = Visibility.Collapsed;
                _originalPhotoPath = null;
            }
        }

        private void LoadComboBoxes()
        {
            CategoryComboBox.ItemsSource = _db.Category.ToList();
            ProducerComboBox.ItemsSource = _db.Producer.ToList();
            SupplierComboBox.ItemsSource = _db.Supplier.ToList();
            UnitComboBox.ItemsSource = _db.Unit.ToList();
        }

        private void LoadProductData()
        {
            IdTextBox.Text = _editingProduct.Id.ToString();
            NameTextBox.Text = _editingProduct.Name;
            ArticleTextBox.Text = _editingProduct.Article;
            DescriptionTextBox.Text = _editingProduct.Description;
            PriceTextBox.Text = _editingProduct.Price.ToString();
            AmountTextBox.Text = _editingProduct.AmountStock.ToString();
            DiscountTextBox.Text = _editingProduct.Discount.ToString();
            _selectedPhotoPath = _editingProduct.Photo;
            _originalPhotoPath = _editingProduct.Photo;

            var image = LoadImageFromPath(_selectedPhotoPath);
            PhotoPreview.Source = image != null ? image : GetDefaultImage();

            CategoryComboBox.SelectedValue = _editingProduct.CategoryId;
            ProducerComboBox.SelectedValue = _editingProduct.ProducerId;
            SupplierComboBox.SelectedValue = _editingProduct.SupplierId;
            UnitComboBox.SelectedValue = _editingProduct.UnitId;
        }

        private BitmapImage LoadImageFromPath(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path))
                    return null;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(path, UriKind.RelativeOrAbsolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private BitmapImage GetDefaultImage()
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri("res/picture.png", UriKind.Relative);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

       private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
{
    OpenFileDialog openDialog = new OpenFileDialog();
    openDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";

    if (openDialog.ShowDialog() == true)
    {
        _selectedPhotoPath = openDialog.FileName;
        var image = LoadImageFromPath(_selectedPhotoPath);
        PhotoPreview.Source = image != null ? image : GetDefaultImage();
        MessageHelper.ShowInformation($"Выбрано фото: {System.IO.Path.GetFileName(_selectedPhotoPath)}");
    }
}

        private bool ValidateFields()
        {
            var errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
                errors.AppendLine("Введите наименование товара!");

            if (string.IsNullOrWhiteSpace(ArticleTextBox.Text) || ArticleTextBox.Text.Length != 6)
                errors.AppendLine("Артикул должен содержать ровно 6 символов!");

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price < 0)
                errors.AppendLine("Цена должна быть положительным числом!");

            if (!int.TryParse(AmountTextBox.Text, out int amount) || amount < 0)
                errors.AppendLine("Количество должно быть целым неотрицательным числом!");

            if (!decimal.TryParse(DiscountTextBox.Text, out decimal discount) || discount < 0 || discount > 100)
                errors.AppendLine("Скидка должна быть числом от 0 до 100!");

            if (CategoryComboBox.SelectedValue == null)
                errors.AppendLine("Категория не выбрана!");
            if (ProducerComboBox.SelectedValue == null)
                errors.AppendLine("Производитель не выбран!");
            if (SupplierComboBox.SelectedValue == null)
                errors.AppendLine("Поставщик не выбран!");
            if (UnitComboBox.SelectedValue == null)
                errors.AppendLine("Единица измерения не выбрана!");

            if (errors.Length > 0)
            {
                MessageHelper.ShowError(errors.ToString());
                return false;
            }
            return true;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateFields())
                    return;

                string savedPhotoPath = _selectedPhotoPath;

                if (!string.IsNullOrEmpty(_selectedPhotoPath) && _selectedPhotoPath != _editingProduct?.Photo)
                {
                    savedPhotoPath = CopyPhotoToProject(_selectedPhotoPath);
                    if (_editingProduct != null && !string.IsNullOrEmpty(_originalPhotoPath) && File.Exists(_originalPhotoPath))
                    {
                        File.Delete(_originalPhotoPath);
                    }
                }

                if (_editingProduct == null)
                {
                    Product newProduct = new Product();
                    newProduct.Name = NameTextBox.Text;
                    newProduct.Article = ArticleTextBox.Text;
                    newProduct.Description = DescriptionTextBox.Text;
                    newProduct.Price = decimal.Parse(PriceTextBox.Text);
                    newProduct.AmountStock = int.Parse(AmountTextBox.Text);
                    newProduct.Discount = decimal.Parse(DiscountTextBox.Text);
                    newProduct.CategoryId = (int)CategoryComboBox.SelectedValue;
                    newProduct.ProducerId = (int)ProducerComboBox.SelectedValue;
                    newProduct.SupplierId = (int)SupplierComboBox.SelectedValue;
                    newProduct.UnitId = (int)UnitComboBox.SelectedValue;
                    newProduct.Photo = savedPhotoPath;

                    _db.Product.Add(newProduct);
                    _db.SaveChanges();
                    MessageHelper.ShowInformation("Товар успешно добавлен!");
                }
                else
                {
                    _editingProduct.Name = NameTextBox.Text;
                    _editingProduct.Article = ArticleTextBox.Text;
                    _editingProduct.Description = DescriptionTextBox.Text;
                    _editingProduct.Price = decimal.Parse(PriceTextBox.Text);
                    _editingProduct.AmountStock = int.Parse(AmountTextBox.Text);
                    _editingProduct.Discount = decimal.Parse(DiscountTextBox.Text);
                    _editingProduct.CategoryId = (int)CategoryComboBox.SelectedValue;
                    _editingProduct.ProducerId = (int)ProducerComboBox.SelectedValue;
                    _editingProduct.SupplierId = (int)SupplierComboBox.SelectedValue;
                    _editingProduct.UnitId = (int)UnitComboBox.SelectedValue;
                    _editingProduct.Photo = savedPhotoPath;

                    _db.SaveChanges();
                    MessageHelper.ShowInformation("Товар успешно обновлен!");
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }

        private string CopyPhotoToProject(string sourcePath)
        {
            string targetDir = AppDomain.CurrentDomain.BaseDirectory + "res/";
            if (!Directory.Exists(targetDir))
                Directory.CreateDirectory(targetDir);
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(sourcePath);
            string targetPath = Path.Combine(targetDir, fileName);
            File.Copy(sourcePath, targetPath, true);
            return targetPath;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}