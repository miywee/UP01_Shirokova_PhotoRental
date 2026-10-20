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
using System.Data.Entity;

namespace UP01_Shirokova_PhotoRental.Pages
{
    public partial class RentalsPage : Page
    {
        public RentalsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(
            object sender, RoutedEventArgs e)
        {
            if (!Core.CurrentUserId.HasValue ||
                !Core.context.Users.Any(x =>
                    x.ID == Core.CurrentUserId.Value &&
                    x.Role == "Admin"))
            {
                MessageBox.Show("Доступ запрещён!");
                NavigationService?.Navigate(new AuthPage());
                return;
            }

            EquipmentCombo.ItemsSource =
                Core.context.Equipment
                    .OrderBy(x => x.Name).ToList();

            RenterCombo.ItemsSource =
                Core.context.Users
                    .OrderBy(x => x.FIO).ToList();

            EquipmentCombo.SelectedIndex = 0;
            RenterCombo.SelectedIndex = 0;

            StartDatePicker.SelectedDate = DateTime.Today;
            EndDatePicker.SelectedDate =
                DateTime.Today.AddDays(1);

            LoadRentals();
        }

        private void LoadRentals()
        {
            RentalsGrid.ItemsSource =
                Core.context.Rentals
                    .Include(x => x.Equipment)
                    .Include(x => x.Users)
                    .OrderBy(x => x.ID)
                    .ToList();
        }

        private void AddButton_Click(
            object sender, RoutedEventArgs e)
        {
            Equipment equipment =
                EquipmentCombo.SelectedItem as Equipment;

            Users renter =
                RenterCombo.SelectedItem as Users;

            if (equipment == null || renter == null ||
                !StartDatePicker.SelectedDate.HasValue ||
                !EndDatePicker.SelectedDate.HasValue)
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            DateTime start =
                StartDatePicker.SelectedDate.Value;

            DateTime end =
                EndDatePicker.SelectedDate.Value;

            if (end < start)
            {
                MessageBox.Show(
                    "Дата окончания не может быть раньше начала!");
                return;
            }

            if (!equipment.IsAvailable)
            {
                MessageBox.Show(
                    "Выбранная техника недоступна!");
                return;
            }

            int days = (end - start).Days + 1;

            try
            {
                Rentals rental = new Rentals
                {
                    EquipmentID = equipment.ID,
                    RenterID = renter.ID,
                    StartDate = start,
                    EndDate = end,
                    TotalPrice = equipment.PricePerDay * days,
                    Status = "Pending",
                    CreatedAt = DateTime.Now
                };

                Core.context.Rentals.Add(rental);
                Core.context.SaveChanges();

                MessageBox.Show(
                    "Заявка успешно создана!",
                    "PhotoRent");

                LoadRentals();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка создания заявки:\n" +
                    ex.GetBaseException().Message);
            }
        }

        private void SaveButton_Click(
            object sender, RoutedEventArgs e)
        {
            try
            {
                if (!RentalsGrid.CommitEdit(
                    DataGridEditingUnit.Cell, true) ||
                    !RentalsGrid.CommitEdit(
                    DataGridEditingUnit.Row, true))
                {
                    MessageBox.Show(
                        "Исправьте данные в редактируемой строке!");
                    return;
                }

                var rentals =
                    Core.context.Rentals.ToList();

                foreach (var rental in rentals)
                {
                    if (rental.EndDate < rental.StartDate ||
                        rental.TotalPrice <= 0)
                    {
                        MessageBox.Show(
                            "Проверьте даты и стоимость заявок!");
                        return;
                    }

                    if (rental.Status != "Pending" &&
                        rental.Status != "Completed" &&
                        rental.Status != "Cancelled")
                    {
                        MessageBox.Show(
                            "Допустимые статусы: " +
                            "Pending, Completed, Cancelled");
                        return;
                    }
                }

                Core.context.SaveChanges();

                MessageBox.Show(
                    "Изменения сохранены!",
                    "PhotoRent");

                LoadRentals();
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
            Rentals selected =
                RentalsGrid.SelectedItem as Rentals;

            if (selected == null)
            {
                MessageBox.Show(
                    "Выберите заявку для удаления!");
                return;
            }

            if (MessageBox.Show(
                "Удалить заявку №" + selected.ID + "?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning)
                != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                Core.context.Rentals.Remove(selected);
                Core.context.SaveChanges();

                MessageBox.Show("Заявка удалена!");
                LoadRentals();
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
            LoadRentals();
        }

        private void BackButton_Click(
            object sender, RoutedEventArgs e)
        {
            if (NavigationService != null &&
                NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}