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
using System.Net.Http;
using Newtonsoft.Json.Linq;

namespace testWPF
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string API_KEY = "07d1480f18fa6b47d8b43d4817eb60b3";
        public MainWindow()
        {
            InitializeComponent();
            MainScreen.IsChecked = true;
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
            catch(HttpRequestException ex)
            {
                MessageBox.Show("Укажите верный город");
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
            string objName = ((RadioButton)sender).ToString();
            StackPanel[] panels = { MainScreenPanel };
            foreach (StackPanel panel in panels)
                panel.Visibility = Visibility.Hidden;
            switch (objName)
            {
                case "MainScreen":
                    MainScreenPanel.Visibility = Visibility.Visible;
                    break;
            }
        }
    }
}
