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
using System.Security.Cryptography;

namespace UP01_Shirokova_PhotoRental.Pages
{

    public partial class AuthPage : Page
    {
        private static int failedAttempts = 0;


        public AuthPage()
        {
            InitializeComponent();

            if (failedAttempts >= 3)
            {
                ShowCaptcha();
            }

        }

        private void ShowCaptcha()
        {
            CaptchaPanel.Visibility = Visibility.Visible;

            LoginButton.IsEnabled = false;

            CaptchaInput.Clear();

            GenerateCaptcha();
        }

        private void GenerateCaptcha()
        {
            const string symbols =
                "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

            byte[] bytes = new byte[6];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }

            char[] result = new char[6];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = symbols[bytes[i] % symbols.Length];
            }

            CaptchaText.Text = new string(result);
        }

        private void CheckCaptcha_Click(
            object sender, RoutedEventArgs e)
        {
            string entered = CaptchaInput.Text.Trim();

            if (string.Equals(
                entered,
                CaptchaText.Text,
                StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "Капча пройдена успешно! Можете войти.",
                    "PhotoRent");

                failedAttempts = 0;

                CaptchaPanel.Visibility = Visibility.Collapsed;
                LoginButton.IsEnabled = true;

                CaptchaInput.Clear();
                PasswordInput.Clear();
            }
            else
            {
                MessageBox.Show(
                    "Неверная капча! Попробуйте ещё раз.",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);

                CaptchaInput.Clear();
                GenerateCaptcha();
            }
        }

        private void RefreshCaptcha_Click(
            object sender, RoutedEventArgs e)
        {
            CaptchaInput.Clear();
            GenerateCaptcha();
        }



        private void RegisterButton_Click(
            object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }

        private void LoginButton_Click(
            object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordInput.Password;

            if (string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show(
                    "Введите логин и пароль!",
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            try
            {
                var user = Core.context.Users
                    .FirstOrDefault(u => u.Login == login);

                bool isValid = user != null &&
                    PasswordHelper.VerifyPassword(
                        password, user.PasswordHash);


                if (!isValid)
                {
                    failedAttempts++;

                    PasswordInput.Clear();

                    MessageBox.Show(
                        "Неверный логин или пароль!",
                        "Ошибка авторизации",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);

                    if (failedAttempts >= 3)
                    {
                        ShowCaptcha();
                    }

                    return;
                }


                failedAttempts = 0;
                Core.CurrentUserId = user.ID;

                MessageBox.Show(
                    "Вход выполнен успешно!\n" +
                    "Пользователь: " + user.FIO + "\n" +
                    "Роль: " + user.Role,
                    "PhotoRent",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);


                if (user.Role == "Admin")
                {
                    NavigationService?.Navigate(
                        new AdminPage());
                }
                else if (user.Role == "User")
                {
                    NavigationService?.Navigate(
                        new UserPage());
                }
                else
                {
                    MessageBox.Show(
                        "Неизвестная роль пользователя!");
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка обращения к базе данных:\n" +
                    ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
