﻿using System;
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
using System.Net.Http;
using Newtonsoft.Json.Linq;
using testWPF.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using System.Net;
using Microsoft.Win32;

namespace testWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string API_KEY = "07d1480f18fa6b47d8b43d4817eb60b3";
        AppDbContext _db;
        public MainWindow()
        {
            InitializeComponent();
            UserListUpdate();
            MainScreen.IsChecked = true;
            SetDefaultSize.IsSelected = true;

            if (!File.Exists("user.xml"))
            {
                ShowAuthWindow();
            }
           
            XmlSerializer xml = new XmlSerializer(typeof(AuthUser));
            using (FileStream file = new FileStream("user.xml", FileMode.Open))
            {
                AuthUser authUser = (AuthUser)xml.Deserialize(file);
                UserNameLabel.Content = authUser.Login;
            }
            if (UserNameLabel.Content.ToString() != "admin".ToLower())
            {
                UserListScreen.Visibility = Visibility.Hidden;
            }

        }

        private void UserListUpdate()
        {
            ObservableCollection<User> items = new ObservableCollection<User>();
            _db = new AppDbContext();
            List<User> list = _db.Users.Select(x => x).ToList();
            foreach (User user in list)
            {

                items.Add(user);

            }
            UsersListBox.ItemsSource = items;
        }

        private void ShowAuthWindow()
        {
            Hide();
            AuthWindow window = new AuthWindow();
            window.Show();
            Close();
        }

        private async void GetWeatherButton_Click(object sender, RoutedEventArgs e)
        {
            string city = UserCityTextBox.Text.Trim();
            if(city.Length < 2)
            {
                MessageBox.Show("Укажите ваш город");
                return;
            }

            try
            {
                string data = await GetWeather(city);
                var json = JObject.Parse(data);
                string temp = json["main"]["temp"].ToString();
                WeatherResults.Content = $"В городе {city} {Math.Round(Convert.ToSingle(temp))}°";

        }
            catch(WebException ex)
            {
                
                MessageBox.Show(ex.Message);
                WeatherResults.Content = "";
            }



}

        private async Task<string> GetWeather(string city)
        {
            HttpClient client = new HttpClient();
            string url = $"https://api.openweathermap.org/data/2.5/weather?q={city}&appid={API_KEY}&units=metric";

            string response = await client.GetStringAsync(url);

            return response;
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            string objName = ((RadioButton)sender).Name;
           StackPanel[] panels ={ MainScreenPanel,  CabinetScreenPanel, NotesScreenPanel, UserListPanel };
            
            foreach (StackPanel panel in panels)
                panel.Visibility = Visibility.Hidden;
            switch (objName)
            {
                case "MainScreen":
                    MainScreenPanel.Visibility = Visibility.Visible;
                    break;
                case "UserListScreen":
                    UserListPanel.Visibility = Visibility.Visible;
                    break;
                case "CabinetScreen":
                    CabinetScreenPanel.Visibility = Visibility.Visible;
                    break;
                case "NotesScreen":
                    NotesScreenPanel.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void DeleteUserButton_Click(object sender, RoutedEventArgs e)
        {
            string login = UserLoginCheckField.Text.Trim();
           
            if (login.Equals(""))
            {
                MessageBox.Show("Вы что-то ввели неверно");
                return;
            }
            User user = null;
            
                user = _db.Users.Where(x => x.Login == login ).FirstOrDefault();
                if (user == null)
                    MessageBox.Show("Такого пользователя не существует");
                else
                {
                    _db.Users.Remove(user);
                    _db.SaveChanges();
                    MessageBox.Show("Пользователь удален из базы данных");
                    UserLoginCheckField.Text = "";
                    DeleteUserButton.Content = "Удалили";
                UserListUpdate();
                }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            File.Delete("user.xml");
            ShowAuthWindow();
        }

        private void UserChangeButton_Click(object sender, RoutedEventArgs e)
        {
            string login = UserLogin.Text.Trim();
            string email = UserEmail.Text.Trim();
            if (login.Equals("") || !email.Contains("@"))
            {
                MessageBox.Show("вы что-то ввели неверно");
                return;
            }

            AppDbContext db = new AppDbContext();
            int countUsers = db.Users.Count(el => el.Login == login);
            if(countUsers != 0 && !login.Equals(UserNameLabel.Content))
            {
                MessageBox.Show("Такой логин уже занят");
                return;
            }
            User user = db.Users.FirstOrDefault(x => x.Login == UserNameLabel.Content.ToString());
            user.Email = email;
            user.Login = login;
            db.SaveChanges();
            UserNameLabel.Content = login;
            UserChangeButton.Content = "Готово";
            AuthUser auth = new AuthUser(login, email);
            XmlSerializer xml = new XmlSerializer(typeof(AuthUser));
            using (FileStream file = new FileStream("user.xml", FileMode.Create))
            {
                xml.Serialize(file, auth);
            }
            UserListUpdate();
        }

        private void MenuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.ShowDialog();
            bool isFolder = (bool)openFileDialog.ShowDialog();

            if (isFolder)
            {
                using(Stream stream = File.Open(openFileDialog.FileName, FileMode.Open))
                {
                    using(StreamReader reader = new StreamReader(stream))
                    {
                        UserNotesTextBox.Text = reader.ReadToEnd();
                    }

                }
            }

        }

        private void MenuSaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveTextToFile();


        }

        private void TimesNewRomanSetText_Click(object sender, RoutedEventArgs e)
        {
            UserNotesTextBox.FontFamily = new FontFamily("Times New Roman");
            VerdanaSetText.IsChecked = false;
        }

        private void VerdanaSetText_Click(object sender, RoutedEventArgs e)
        {
            UserNotesTextBox.FontFamily = new FontFamily("Verdana");
            TimesNewRomanSetText.IsChecked = false;
        }

        private void SelectFontSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem comboBox = (ComboBoxItem)SelectFontSize.SelectedItem;
            int fontSize = Convert.ToInt32(comboBox.Tag);
            UserNotesTextBox.FontSize = fontSize;
        }

        private void MenuNewFile_Click(object sender, RoutedEventArgs e)
        {
            if (UserNotesTextBox.Text.Trim().Equals(""))
                return;
            SaveTextToFile();
            UserNotesTextBox.Text = "";
        }

        private void SaveTextToFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            bool isFolder = (bool)saveFileDialog.ShowDialog();

            if (isFolder)
            {
                using (Stream file = File.Open(saveFileDialog.FileName, FileMode.OpenOrCreate))
                {
                    using (StreamWriter writer = new StreamWriter(file))
                    {
                        writer.Write(UserNotesTextBox.Text);
                    }
                }
            }
        }
    }
}
