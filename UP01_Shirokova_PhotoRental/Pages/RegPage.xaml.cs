using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace UP01_Shirokova_PhotoRental.Pages
{

    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(
            object sender, RoutedEventArgs e)
        {
            string fio = FIOTextBox.Text.Trim();
            string login = LoginTextBox.Text.Trim();
            string password = PasswordInput.Password;
            string confirmPassword =
                ConfirmPasswordInput.Password;

            if (string.IsNullOrWhiteSpace(fio) ||
                string.IsNullOrWhiteSpace(login) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            if (fio.Length < 5 ||
                fio.Length > 150 ||
                !Regex.IsMatch(fio, @"^[\p{L}\s-]+$"))
            {
                MessageBox.Show(
                    "Укажите корректное ФИО!");
                return;
            }

            if (!Regex.IsMatch(
                login, @"^[A-Za-z0-9._-]{3,80}$"))
            {
                MessageBox.Show(
                    "Логин должен содержать от 3 до 80 " +
                    "символов: английские буквы, цифры, " +
                    "точку, дефис или подчёркивание.");
                return;
            }
            if (password.Length < 6 ||
                password.Length > 128)
            {
                MessageBox.Show(
                    "Пароль должен содержать " +
                    "от 6 до 128 символов!");
                return;
            }

            if (!Regex.IsMatch(
                password, @"^[A-Za-z0-9!@#$%^&*_-]+$"))
            {
                MessageBox.Show(
                    "Используйте английские буквы, " +
                    "цифры и допустимые символы!");
                return;
            }

            if (!Regex.IsMatch(password, @"[0-9]"))
            {
                MessageBox.Show(
                    "Пароль должен содержать хотя бы одну цифру!");
                return;
            }

            if (password != confirmPassword)
            {
                MessageBox.Show(
                    "Пароли не совпадают!");
                return;
            }

            try
            {
                bool exists = Core.context.Users
                    .Any(u => u.Login == login);

                if (exists)
                {
                    MessageBox.Show(
                        "Пользователь с таким логином уже существует!");
                    return;
                }

                var newUser = new Users
                {
                    FIO = fio,
                    Login = login,
                    PasswordHash =
                        PasswordHelper.HashPassword(password),
                    Role = "User"
                };

                Core.context.Users.Add(newUser);
                Core.context.SaveChanges();

                MessageBox.Show(
                    "Регистрация прошла успешно!",
                    "PhotoRent",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                NavigationService?.Navigate(new AuthPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка регистрации:\n" +
                    ex.GetBaseException().Message);
            }
        }

        private void BackButton_Click(
            object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AuthPage());
        }
    }
}
