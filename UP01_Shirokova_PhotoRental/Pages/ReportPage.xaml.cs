using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
using Excel = Microsoft.Office.Interop.Excel;
using Word = Microsoft.Office.Interop.Word;

namespace UP01_Shirokova_PhotoRental.Pages
{
    public partial class ReportPage : Page
    {
        public ReportPage()
        {
            InitializeComponent();
        }

        private class RentalRow
        {
            public int ID { get; set; }
            public string EquipmentName { get; set; }
            public string Category { get; set; }
            public string RenterName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public string Status { get; set; }
            public decimal TotalPrice { get; set; }
        }

        private List<RentalRow> GetReportData()
        {
            var rentals = Core.context.Rentals.ToList();
            var equipment = Core.context.Equipment.ToList();
            var users = Core.context.Users.ToList();

            var result =
                (from r in rentals
                 join e in equipment
                     on r.EquipmentID equals e.ID
                 join u in users
                     on r.RenterID equals u.ID
                 orderby r.ID
                 select new RentalRow
                 {
                     ID = r.ID,
                     EquipmentName = e.Name,
                     Category = e.Category,
                     RenterName = u.FIO,
                     StartDate = r.StartDate,
                     EndDate = r.EndDate,
                     Status = r.Status,
                     TotalPrice = r.TotalPrice
                 }).ToList();

            return result;
        }

        private SaveFileDialog CreateSaveDialog(
            string extension, string filter)
        {
            return new SaveFileDialog
            {
                Title = "Сохранить отчёт PhotoRent",
                FileName = "PhotoRent_Report_" +
                    DateTime.Now.ToString("yyyyMMdd_HHmm"),
                DefaultExt = extension,
                Filter = filter,
                AddExtension = true,
                OverwritePrompt = true
            };
        }

        private void WordButton_Click(
            object sender, RoutedEventArgs e)
        {
            ExportWordOrPdf(false);
        }

        private void PdfButton_Click(
            object sender, RoutedEventArgs e)
        {
            ExportWordOrPdf(true);
        }

        private void ExportWordOrPdf(bool pdf)
        {
            var dialog = pdf
                ? CreateSaveDialog(
                    ".pdf", "PDF (*.pdf)|*.pdf")
                : CreateSaveDialog(
                    ".docx", "Word (*.docx)|*.docx");

            if (dialog.ShowDialog() != true)
                return;

            Word.Application app = null;
            Word.Document document = null;
            Word.Table table = null;

            try
            {
                var rows = GetReportData();

                app = new Word.Application();
                app.Visible = false;

                document = app.Documents.Add();

                document.PageSetup.Orientation =
                    Word.WdOrientation.wdOrientLandscape;

                document.Content.Text =
                    "ОТЧЁТ ПО АРЕНДЕ ФОТО- И ВИДЕОТЕХНИКИ\r\n" +
                    "PhotoRent\r\n" +
                    "Дата формирования: " +
                    DateTime.Now.ToString("dd.MM.yyyy HH:mm") +
                    "\r\n\r\n";

                Word.Range range = document.Range(
                    document.Content.End - 1,
                    document.Content.End - 1);

                table = document.Tables.Add(
                    range, rows.Count + 1, 8);

                table.Borders.Enable = 1;

                table.AutoFitBehavior(
                    Word.WdAutoFitBehavior.wdAutoFitWindow);

                string[] headers =
                {
                    "ID",
                    "Техника",
                    "Категория",
                    "Арендатор",
                    "Начало",
                    "Конец",
                    "Статус",
                    "Стоимость"
                };

                for (int c = 0; c < headers.Length; c++)
                {
                    table.Cell(1, c + 1).Range.Text =
                        headers[c];
                }

                table.Rows[1].Range.Bold = 1;

                for (int i = 0; i < rows.Count; i++)
                {
                    var r = rows[i];
                    int n = i + 2;

                    table.Cell(n, 1).Range.Text =
                        r.ID.ToString();
                    table.Cell(n, 2).Range.Text =
                        r.EquipmentName;
                    table.Cell(n, 3).Range.Text =
                        r.Category;
                    table.Cell(n, 4).Range.Text =
                        r.RenterName;
                    table.Cell(n, 5).Range.Text =
                        r.StartDate.ToString("dd.MM.yyyy");
                    table.Cell(n, 6).Range.Text =
                        r.EndDate.ToString("dd.MM.yyyy");
                    table.Cell(n, 7).Range.Text =
                        r.Status;
                    table.Cell(n, 8).Range.Text =
                        r.TotalPrice.ToString("N2");
                }

                if (pdf)
                {
                    document.ExportAsFixedFormat(
                        dialog.FileName,
                        Word.WdExportFormat.wdExportFormatPDF);
                }
                else
                {
                    document.SaveAs2(
                        dialog.FileName,
                        Word.WdSaveFormat.wdFormatXMLDocument);
                }

                MessageBox.Show(
                    "Отчёт успешно сохранён!\n" +
                    dialog.FileName,
                    "PhotoRent");
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка экспорта:\n" +
                    ex.GetBaseException().Message);
            }
            finally
            {
                if (document != null)
                {
                    try
                    {
                        document.Close(
                            Word.WdSaveOptions.wdDoNotSaveChanges);
                    }
                    catch { }
                }

                if (app != null)
                {
                    try { app.Quit(); }
                    catch { }
                }

                ReleaseOfficeObject(table);
                ReleaseOfficeObject(document);
                ReleaseOfficeObject(app);
            }
        }

        private void ExcelButton_Click(
            object sender, RoutedEventArgs e)
        {
            var dialog = CreateSaveDialog(
                ".xlsx", "Excel (*.xlsx)|*.xlsx");

            if (dialog.ShowDialog() != true)
                return;

            Excel.Application app = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet sheet = null;

            try
            {
                var rows = GetReportData();

                app = new Excel.Application();
                app.Visible = false;
                app.DisplayAlerts = false;

                workbook = app.Workbooks.Add();

                sheet = (Excel.Worksheet)
                    workbook.Worksheets[1];

                sheet.Name = "Заявки на аренду";

                object[,] values =
                    new object[rows.Count + 1, 8];

                string[] headers =
                {
                    "ID",
                    "Техника",
                    "Категория",
                    "Арендатор",
                    "Начало",
                    "Конец",
                    "Статус",
                    "Стоимость"
                };

                for (int c = 0; c < 8; c++)
                    values[0, c] = headers[c];

                for (int i = 0; i < rows.Count; i++)
                {
                    var r = rows[i];

                    values[i + 1, 0] = r.ID;
                    values[i + 1, 1] = r.EquipmentName;
                    values[i + 1, 2] = r.Category;
                    values[i + 1, 3] = r.RenterName;
                    values[i + 1, 4] =
                        r.StartDate.ToString("dd.MM.yyyy");
                    values[i + 1, 5] =
                        r.EndDate.ToString("dd.MM.yyyy");
                    values[i + 1, 6] = r.Status;
                    values[i + 1, 7] =
                        (double)r.TotalPrice;
                }

                Excel.Range dataRange = sheet.Range[
                    "A1", "H" + (rows.Count + 1)];

                dataRange.Value2 = values;

                Excel.Range headerRange =
                    sheet.Range["A1", "H1"];

                headerRange.Font.Bold = true;

                Excel.Range columns =
                    (Excel.Range)sheet.Columns;

                columns.AutoFit();

                workbook.SaveAs(
                    dialog.FileName,
                    Excel.XlFileFormat.xlOpenXMLWorkbook);

                MessageBox.Show(
                    "Excel-отчёт успешно сохранён!\n" +
                    dialog.FileName,
                    "PhotoRent");

                ReleaseOfficeObject(columns);
                ReleaseOfficeObject(headerRange);
                ReleaseOfficeObject(dataRange);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка экспорта в Excel:\n" +
                    ex.GetBaseException().Message);
            }
            finally
            {
                if (workbook != null)
                {
                    try { workbook.Close(false); }
                    catch { }
                }

                if (app != null)
                {
                    try { app.Quit(); }
                    catch { }
                }

                ReleaseOfficeObject(sheet);
                ReleaseOfficeObject(workbook);
                ReleaseOfficeObject(app);
            }
        }

        private void ReleaseOfficeObject(object obj)
        {
            if (obj != null && Marshal.IsComObject(obj))
            {
                Marshal.ReleaseComObject(obj);
            }
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
