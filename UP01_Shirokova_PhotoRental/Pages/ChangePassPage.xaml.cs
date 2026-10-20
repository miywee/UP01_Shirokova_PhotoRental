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
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        private void SavePassword_Click(
            object sender, RoutedEventArgs e)
        {
            if (!Core.CurrentUserId.HasValue)
            {
                MessageBox.Show(
                    "Сначала войдите в аккаунт!");
                return;
            }

            string oldPassword = OldPasswordBox.Password;
            string newPassword = NewPasswordBox.Password;
            string confirm = ConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(oldPassword) ||
                string.IsNullOrEmpty(newPassword) ||
                string.IsNullOrEmpty(confirm))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            if (newPassword.Length < 6 ||
                newPassword.Length > 128 ||
                !Regex.IsMatch(newPassword,
                    @"^[A-Za-z0-9!@#$%^&*_-]+$") ||
                !Regex.IsMatch(newPassword, @"[0-9]"))
            {
                MessageBox.Show(
                    "Новый пароль должен содержать " +
                    "от 6 до 128 символов, английские " +
                    "буквы и хотя бы одну цифру.");
                return;
            }

            if (newPassword != confirm)
            {
                MessageBox.Show(
                    "Новые пароли не совпадают!");
                return;
            }

            try
            {
                int userId = Core.CurrentUserId.Value;

                var user = Core.context.Users
                    .FirstOrDefault(x => x.ID == userId);

                if (user == null)
                {
                    MessageBox.Show(
                        "Пользователь не найден!");
                    return;
                }

                if (!PasswordHelper.VerifyPassword(
                    oldPassword, user.PasswordHash))
                {
                    MessageBox.Show(
                        "Текущий пароль указан неверно!");
                    return;
                }

                if (oldPassword == newPassword)
                {
                    MessageBox.Show(
                        "Новый пароль должен отличаться от старого!");
                    return;
                }

                user.PasswordHash =
                    PasswordHelper.HashPassword(newPassword);

                Core.context.SaveChanges();

                MessageBox.Show(
                    "Пароль успешно изменён!",
                    "PhotoRent",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                OldPasswordBox.Clear();
                NewPasswordBox.Clear();
                ConfirmPasswordBox.Clear();

                GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка изменения пароля:\n" +
                    ex.GetBaseException().Message);
            }
        }

        private void BackButton_Click(
            object sender, RoutedEventArgs e)
        {
            GoBack();
        }

        private void GoBack()
        {
            if (NavigationService != null &&
                NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                NavigationService?.Navigate(
                    new AuthPage());
            }
        }
    }
}
