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

namespace UP01_Shirokova_PhotoRental.Pages
{
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(
            object sender, RoutedEventArgs e)
        {
            AvailabilityComboBox.SelectedIndex = 0;
            SortComboBox.SelectedIndex = 0;
            LoadEquipment();
        }

        private void LoadEquipment()
        {
            try
            {
                var query = Core.context.Equipment.AsQueryable();

                string search = SearchTextBox.Text
                    .Trim().ToLower();

                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        x.Name.ToLower().Contains(search) ||
                        x.Category.ToLower().Contains(search));
                }

                if (AvailabilityComboBox.SelectedIndex == 1)
                {
                    query = query.Where(x => x.IsAvailable);
                }
                else if (AvailabilityComboBox.SelectedIndex == 2)
                {
                    query = query.Where(x => !x.IsAvailable);
                }

                switch (SortComboBox.SelectedIndex)
                {
                    case 1:
                        query = query.OrderBy(x => x.PricePerDay);
                        break;

                    case 2:
                        query = query.OrderByDescending(
                            x => x.PricePerDay);
                        break;

                    default:
                        query = query.OrderBy(x => x.Name);
                        break;
                }

                EquipmentGrid.ItemsSource = query.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки техники:\n" +
                    ex.GetBaseException().Message);
            }
        }

        private void FilterChanged(
            object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                LoadEquipment();
            }
        }


        private void LogoutButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Вы действительно хотите выйти из аккаунта?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                Core.CurrentUserId = null;
                NavigationService?.Navigate(new AuthPage());
            }
        }


        private void ChangePassword_Click(
            object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(
                new ChangePassPage());
        }

    }
}
