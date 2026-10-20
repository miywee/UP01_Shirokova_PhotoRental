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
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();

        }

        private void Page_Loaded(
            object sender, RoutedEventArgs e)
        {
            AvailabilityComboBox.SelectedIndex = 0;
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

                EquipmentGrid.ItemsSource =
                    query.OrderBy(x => x.Name).ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка загрузки:\n" +
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

        private void AddButton_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                int ownerId = Core.context.Users
                    .Select(x => x.ID)
                    .FirstOrDefault();

                if (ownerId == 0)
                {
                    MessageBox.Show(
                        "Сначала необходимо создать пользователя!");
                    return;
                }

                Equipment equipment = new Equipment
                {
                    Name = "Новая техника",
                    Category = "Другое",
                    Description = "Введите описание",
                    PricePerDay = 1000,
                    IsAvailable = true,
                    OwnerID = ownerId
                };

                Core.context.Equipment.Add(equipment);
                Core.context.SaveChanges();

                MessageBox.Show(
                    "Техника добавлена! " +
                    "Выделите строку и измените данные.");

                LoadEquipment();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка добавления:\n" +
                    ex.GetBaseException().Message);
            }
        }

        private void SaveButton_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                EquipmentGrid.CommitEdit(
                    DataGridEditingUnit.Cell, true);

                EquipmentGrid.CommitEdit(
                    DataGridEditingUnit.Row, true);

                var equipment = Core.context.Equipment.ToList();

                if (equipment.Any(x =>
                    string.IsNullOrWhiteSpace(x.Name) ||
                    string.IsNullOrWhiteSpace(x.Category) ||
                    x.Name.Length > 150 ||
                    x.Category.Length > 80 ||
                    x.PricePerDay <= 0))
                {
                    MessageBox.Show(
                        "Проверьте название, категорию " +
                        "и цену техники!");
                    return;
                }

                Core.context.SaveChanges();

                MessageBox.Show(
                    "Изменения успешно сохранены!",
                    "PhotoRent");

                LoadEquipment();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка сохранения:\n" +
                    ex.GetBaseException().Message);
            }
        }

        private void DeleteButton_Click(
            object sender, RoutedEventArgs e)
        {
            Equipment selected =
                EquipmentGrid.SelectedItem as Equipment;

            if (selected == null)
            {
                MessageBox.Show(
                    "Выберите технику для удаления!");
                return;
            }

            var result = MessageBox.Show(
                "Удалить технику \"" + selected.Name +
                "\" и все связанные с ней заявки?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var rentals = Core.context.Rentals
                    .Where(x => x.EquipmentID == selected.ID)
                    .ToList();

                Core.context.Rentals.RemoveRange(rentals);

                Core.context.Equipment.Remove(selected);

                Core.context.SaveChanges();

                MessageBox.Show(
                    "Техника и связанные заявки удалены!");

                LoadEquipment();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка удаления:\n" +
                    ex.GetBaseException().Message);
            }
        }

        private void RefreshButton_Click(
            object sender, RoutedEventArgs e)
        {
            LoadEquipment();
        }

        private void CatalogButton_Click(
            object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new UserPage());
        }

        private void LogoutButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Выйти из аккаунта?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question)
                == MessageBoxResult.Yes)
            {
                Core.CurrentUserId = null;
                NavigationService?.Navigate(new AuthPage());
            }
        }

        private void DiagramButton_Click(
            object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(
                new DiagramPage());
        }


        private void ReportButton_Click(
            object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(
                new ReportPage());
        }


        private void ChangePassword_Click(
            object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(
                new ChangePassPage());
        }


        private void RentalsButton_Click(
            object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(
                new RentalsPage());
        }


    }
}
