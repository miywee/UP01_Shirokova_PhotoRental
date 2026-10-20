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
    public partial class DiagramPage : Page
    {
        public DiagramPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(
            object sender, RoutedEventArgs e)
        {
            LoadChart();
        }

        private void LoadChart()
        {
            try
            {
                ChartPanel.Children.Clear();

                var rentals = Core.context.Rentals.ToList();
                var equipment = Core.context.Equipment.ToList();

                var statistics =
                    (from r in rentals
                     join eq in equipment
                     on r.EquipmentID equals eq.ID
                     where r.Status == "Completed"
                     group r by eq.Category into g
                     orderby g.Key
                     select new
                     {
                         Category = g.Key,
                         Count = g.Count(),
                         Total = g.Sum(x => x.TotalPrice)
                     }).ToList();

                int totalCount =
                    statistics.Sum(x => x.Count);

                decimal totalSum =
                    statistics.Sum(x => x.Total);

                SummaryText.Text =
                    "Завершённых аренд: " + totalCount +
                    "\nОбщая стоимость: " +
                    totalSum.ToString("N2") + " руб.";

                if (statistics.Count == 0)
                {
                    ChartPanel.Children.Add(
                        new TextBlock
                        {
                            Text = "Нет данных для диаграммы."
                        });

                    return;
                }

                decimal maxValue =
                    statistics.Max(x => x.Total);

                foreach (var item in statistics)
                {
                    TextBlock label = new TextBlock
                    {
                        Text = item.Category + " — " +
                               item.Total.ToString("N2") +
                               " руб. (" + item.Count +
                               " аренд)",
                        FontSize = 14,
                        Margin = new Thickness(0, 15, 0, 6)
                    };

                    ChartPanel.Children.Add(label);

                    double barWidth = maxValue > 0
                        ? (double)(item.Total / maxValue) * 500
                        : 2;

                    Border bar = new Border
                    {
                        Width = Math.Max(2, barWidth),
                        Height = 28,
                        CornerRadius = new CornerRadius(5),
                        HorizontalAlignment =
                            HorizontalAlignment.Left
                    };

                    bar.SetResourceReference(
                        Border.BackgroundProperty,
                        "AccentBrush");

                    ChartPanel.Children.Add(bar);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка построения диаграммы:\n" +
                    ex.GetBaseException().Message);
            }
        }

        private void RefreshButton_Click(
            object sender, RoutedEventArgs e)
        {
            LoadChart();
        }

        private void BackButton_Click(
            object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}
