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
    /// Логика взаимодействия для OrderEditWindow.xaml
    /// </summary>
    public partial class OrderEditWindow : Window
    {
        private Order _editingOrder;
        private ShopShoeDbEntities _db = new ShopShoeDbEntities();
        public OrderEditWindow(Order order = null, User currentUser = null)
        {

            InitializeComponent();
            _editingOrder = order;
            LoadCombo();
            if (order != null)
            {
                Title = "Редактирование заказа";
                LoadOrderData();
            }
            else
            {
                Title = "Добавление заказа";
                IdTextBox.Visibility = Visibility.Collapsed;
                IdTextBlock.Visibility = Visibility.Collapsed;
            }
            
        }
        private void LoadCombo()
        {
            StatusComboBox.ItemsSource = _db.Status.ToList();
            PickUpPointComboBox.ItemsSource = _db.PickUpPoint.ToList();
            UserComboBox.ItemsSource = _db.User.ToList();
        }
        private void LoadOrderData()
        {
            IdTextBox.Text = _editingOrder.Id.ToString();
            ArticleTextBox.Text = _editingOrder.Article;

            StatusComboBox.SelectedValue = _editingOrder.StatusId;
            PickUpPointComboBox.SelectedValue = _editingOrder.PickUpPointId;
            UserComboBox.SelectedValue = _editingOrder.UserId;
            OrderDatePicker.SelectedDate = _editingOrder.OrderDate;
            DeliveryDatePicker.SelectedDate = _editingOrder.DeliveryDate;
        }
        private bool ValidateFields()
        {
            var errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(ArticleTextBox.Text))
                errors.AppendLine("Введите артикул заказа!");

            if (StatusComboBox.SelectedValue == null)
                errors.AppendLine("Выберите статус заказа!");

            if (PickUpPointComboBox.SelectedValue == null)
                errors.AppendLine("Выберите пункт выдачи!");

            if (OrderDatePicker.SelectedDate == null)
                errors.AppendLine("Выберите дату заказа!");
            else
            {
                DateTime orderDate = OrderDatePicker.SelectedDate.Value;
                if (DeliveryDatePicker.SelectedDate != null && DeliveryDatePicker.SelectedDate.Value < orderDate)
                    errors.AppendLine("Дата доставки не может быть раньше даты заказа!");
            }

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

                Order order = _editingOrder ?? new Order();
                order.Article = ArticleTextBox.Text;
                order.StatusId = (int)StatusComboBox.SelectedValue;
                order.PickUpPointId = (int)PickUpPointComboBox.SelectedValue;
                order.OrderDate = OrderDatePicker.SelectedDate.Value;
                order.DeliveryDate = DeliveryDatePicker.SelectedDate ?? DateTime.Now;
                order.UserId = (int)UserComboBox.SelectedValue;
                if (_editingOrder == null)
                {
                    _db.Order.Add(order);
                }

                _db.SaveChanges();

                string message = _editingOrder == null ? "Заказ успешно добавлен!" : "Заказ успешно обновлен!";
                MessageHelper.ShowInformation(message);
                DialogResult = true;
                Close(); 
            }
            catch (Exception ex)
            {
                MessageHelper.ShowError($"Ошибка сохранения: {ex.Message}");
            }
        }
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
    }
}
