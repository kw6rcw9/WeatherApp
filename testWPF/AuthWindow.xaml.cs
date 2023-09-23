using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using testWPF.Models;

namespace testWPF
{
    /// <summary>
    /// Логика взаимодействия для AuthWindow.xaml
    /// </summary>
    public partial class AuthWindow : Window
    {
        
        public AuthWindow()
        {
            InitializeComponent();
            
        }

        private void UserAuth_Click(object sender, RoutedEventArgs e)
        {
            string login = UserLoginField.Text.Trim();
            string password = UserPasswordField.Password.Trim();
            if (login.Equals("")  || password.Length < 3)
            {
                MessageBox.Show("Вы что-то ввели неверно");
                return;
            }
            User authUser = null;
            using (AppDbContext db = new AppDbContext())
            {
                authUser = db.Users.Where(user => user.Login == login && user.Password == Hash(password)).FirstOrDefault();
            }
            if (authUser == null)
                MessageBox.Show("Такого пользователя не существует");
            else
            {
                UserLoginField.Text = "";         
                UserPasswordField.Password = "";
                UserAuth.Content = "Готово";
            }

        }
        private string Hash(string input)
        {
            byte[] temp = Encoding.UTF8.GetBytes(input);
            using (SHA1Managed sha1 = new SHA1Managed())
            {
                var hash = sha1.ComputeHash(temp);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
