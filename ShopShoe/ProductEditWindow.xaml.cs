using Microsoft.Win32;
using ShopShoe.Db;
using ShopShoe.Helpers;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;

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

                Product product = _editingProduct ?? new Product();

                product.Name = NameTextBox.Text;
                product.Article = ArticleTextBox.Text;
                product.Description = DescriptionTextBox.Text;
                product.Price = decimal.Parse(PriceTextBox.Text);
                product.AmountStock = int.Parse(AmountTextBox.Text);
                product.Discount = decimal.Parse(DiscountTextBox.Text);
                product.CategoryId = (int)CategoryComboBox.SelectedValue;
                product.ProducerId = (int)ProducerComboBox.SelectedValue;
                product.SupplierId = (int)SupplierComboBox.SelectedValue;
                product.UnitId = (int)UnitComboBox.SelectedValue;

                string savedPhotoPath = _selectedPhotoPath;

                if (!string.IsNullOrEmpty(_selectedPhotoPath) && _selectedPhotoPath != _editingProduct?.Photo)
                {
                    savedPhotoPath = CopyPhotoToProject(_selectedPhotoPath);
                    if (_editingProduct != null && !string.IsNullOrEmpty(_originalPhotoPath) && File.Exists(_originalPhotoPath))
                    {
                        File.Delete(_originalPhotoPath);
                    }
                }
                product.Photo = savedPhotoPath;

                if (_editingProduct == null)
                {
                    _db.Product.Add(product);
                }

                _db.SaveChanges();

                string message = _editingProduct == null ? "Товар успешно добавлен!" : "Товар успешно обновлен!";
                MessageHelper.ShowInformation(message);

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
            ResizeImage(sourcePath, targetPath, 300, 200);
            return targetPath;
        }
        private void ResizeImage(string sourcePath, string targetPath, int maxWidth, int maxHeight)
        {
            using (var srcImage = Image.FromFile(sourcePath))
            {
                int newWidth, newHeight;

                if (srcImage.Width > srcImage.Height)
                {
                    newWidth = maxWidth;
                    newHeight = (int)((double)srcImage.Height / srcImage.Width * maxWidth);
                }
                else
                {
                    newHeight = maxHeight;
                    newWidth = (int)((double)srcImage.Width / srcImage.Height * maxHeight);
                }

                using (var destImage = new Bitmap(newWidth, newHeight))
                {
                    using (var graphics = Graphics.FromImage(destImage))
                    {
                        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                        graphics.DrawImage(srcImage, 0, 0, newWidth, newHeight);
                    }
                    destImage.Save(targetPath, ImageFormat.Jpeg);
                }
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}