
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace UP01_Shirokova_PhotoRental
{
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();

            ApplyTheme(Properties.Settings.Default.AppTheme);
            StartClock();

            MainFrame.Navigate(new Pages.AuthPage());
        }

        private void StartClock()
        {
            if (timer != null)
                return;

            DateTimeNow.Text =
                DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);

            timer.Tick += (s, e) =>
            {
                DateTimeNow.Text =
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss");
            };

            timer.Start();
        }

        private void Window_Loaded(
            object sender, RoutedEventArgs e)
        {
            StartClock();
        }

        private void BackButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private void MainFrame_Navigated(
            object sender, NavigationEventArgs e)
        {
            Page page = e.Content as Page;

            PageTitle.Text = page != null
                ? page.Title
                : "PhotoRent";
        }

        private void ThemeButton_Click(
            object sender, RoutedEventArgs e)
        {
            string newTheme =
                Properties.Settings.Default.AppTheme == "Dark"
                ? "Light"
                : "Dark";

            ApplyTheme(newTheme);

            Properties.Settings.Default.AppTheme = newTheme;
            Properties.Settings.Default.Save();
        }

        private void ApplyTheme(string theme)
        {
            string dictionaryFile = theme == "Dark"
                ? "CustomDictionary.xaml"
                : "Dictionary.xaml";

            var dictionary = new ResourceDictionary
            {
                Source = new Uri(
                    "pack://application:,,,/" + dictionaryFile,
                    UriKind.Absolute)
            };

            var dictionaries =
                Application.Current.Resources.MergedDictionaries;

            if (dictionaries.Count > 0)
            {
                dictionaries[0] = dictionary;
            }
            else
            {
                dictionaries.Add(dictionary);
            }

            ThemeButton.Content = theme == "Dark"
                ? "Светлая тема"
                : "Тёмная тема";
        }

        private void Window_Closing(
            object sender, CancelEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы действительно хотите выйти из приложения?",
                "Подтверждение выхода",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            else
            {
                timer?.Stop();
            }
        }
    }
}
