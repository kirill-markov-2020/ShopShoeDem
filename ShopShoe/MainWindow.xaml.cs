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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ShopShoe
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ShopShoeDbEntities _db = new ShopShoeDbEntities();
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

            var user = _db.User.Where(u => u.Login == LoginTextBox.Text && u.Password == PasswordBox.Password).FirstOrDefault();
            if (user == null)
            {
                MessageHelper.ShowError("Неверный логин или пароль");
                return;
            }
            CurrentSession.CurrentUser = user;
            
            new ProductWindow(user).Show();
            Close();
            
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e)
        {
            new OrderWindow().Show();
            Close();
        }
    }
}
