using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
using Path = System.IO.Path;
using Microsoft.Office.Interop.Excel;
using Window = System.Windows.Window;
using Range = Microsoft.Office.Interop.Excel.Range;
using System.Runtime.InteropServices;
using System.Reflection.Metadata;
using System.Reflection;

namespace TestReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog opf = new OpenFileDialog();
            opf.Filter = "csv File|*.csv|xlsx File|*.xlsx";
            opf.ShowDialog();
            if (Path.GetExtension(opf.FileName) == ".csv")
            {
                ShowData.ItemsSource = LoadCSV(opf.FileName);
            }
            else if (Path.GetExtension(opf.FileName) == ".xlsx")
            {
                ShowData.ItemsSource = LoadXLSX(opf.FileName);
            }
        }

        public ObservableCollection<Record> LoadXLSX (string filename)
        {
            ObservableCollection<Record> records = new ObservableCollection<Record>();
            Microsoft.Office.Interop.Excel.Application _excelApp = new Microsoft.Office.Interop.Excel.Application();
            _excelApp.Visible = false;
            Workbook workbook = _excelApp.Workbooks.Open(filename);
            Worksheet worksheet = (Worksheet)workbook.Worksheets[1];
            Range excelRange = worksheet.UsedRange;
            object[,] valueArray = (object[,])excelRange.get_Value(XlRangeValueDataType.xlRangeValueDefault);
            for (int row = 2; row<=worksheet.UsedRange.Rows.Count; row++)
            {
                Record rec = new Record();
                rec.Name = valueArray[row,1].ToString();
                rec.Distance = double.Parse(valueArray[row, 2].ToString());
                rec.Angle = double.Parse(valueArray[row, 3].ToString());
                rec.Width = double.Parse(valueArray[row, 4].ToString());
                rec.Height = double.Parse(valueArray[row, 5].ToString());
                rec.IsDefect = valueArray[row, 6].ToString();
                records.Add(rec);
            }
            workbook.Close(false);
            Marshal.ReleaseComObject(workbook);
            _excelApp.Quit();
            Marshal.FinalReleaseComObject(_excelApp);
            return records;
        }
        public ObservableCollection<Record> LoadCSV(string filename)
        {
            ObservableCollection<Record> records = new ObservableCollection<Record>();
            string[] lines = File.ReadAllLines(filename);
            lines = lines.Skip(1).ToArray();
            foreach (string line in lines)
            {
                string[] elem = line.Split(';');
                Record rec = new Record();
                rec.Name = elem[0];
                rec.Distance = double.Parse(elem[1]);
                rec.Angle = double.Parse(elem[2]);
                rec.Width = double.Parse(elem[3]);
                rec.Height = double.Parse(elem[4]);
                rec.IsDefect = elem[5];
                records.Add(rec);
            }
            return records;
        }


        private void SendInfo(object sender, SelectionChangedEventArgs e)
        {
            int scale = 10;
            Record record = ShowData.SelectedItem as Record;
            if (record == null)
            {
                InfoText.Text = "";
            }
            else
            {
                ShowImage.Children.Clear();
                InfoText.Text = "Name:" + record.Name + "\n\nDistance:" + record.Distance + "\n\nAngle:" + record.Angle
                + "\n\nWidth:" + record.Width + "\n\nHeight:" + record.Height + "\n\nIsDefect:" + record.IsDefect;
                System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                rect.Stroke = new SolidColorBrush(Colors.Red);
                rect.Fill = new SolidColorBrush(Colors.Red);
                rect.StrokeThickness = 2;
                rect.Width = record.Height * scale;
                rect.Height = record.Width * scale;
                Canvas.SetTop(rect, (record.Angle - record.Height) * scale);
                Canvas.SetLeft(rect, (record.Distance - record.Width) * scale);
                ShowImage.Children.Add(rect);
            }

        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog spf = new SaveFileDialog();
            spf.Filter = "csv File|*.csv|xlsx File|*.xlsx";
            spf.ShowDialog();
            if (Path.GetExtension(spf.FileName) == ".csv")
            {
                SaveCSV(spf.FileName);
            }
            else if (Path.GetExtension(spf.FileName) == ".xlsx")
            {
                SaveXLSX(spf.FileName);
            }
        }

        public void SaveXLSX(string filename)
        {
            string[] colname = new string[] { "A", "B", "C", "D", "E", "F" };
            ObservableCollection<Record> records = (ObservableCollection<Record>)ShowData.Items.SourceCollection;
            Microsoft.Office.Interop.Excel.Application _excelApp = new Microsoft.Office.Interop.Excel.Application();
            _excelApp.Visible = false;
            Workbook workbook = _excelApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
            Worksheet worksheet = (Worksheet)workbook.Sheets[1];
            Range Header = worksheet.Range["A1", "F1"];
            string[] heads = { "Name", "Distance", "Angle", "Width", "Height", "IsDefect" };
            Header.Value2 = heads;
            for (int row = 2; row <= records.Count+1; row++)
            {
                object[] elem = { records[row-2].Name, records[row-2].Distance, records[row-2].Angle, records[row - 2].Width, records[row-2].Height, records[row-2].IsDefect };
                Range Name = worksheet.Range["A" + row,"F" + row];
                Name.Value2 = elem;
                //Range Distance = worksheet.Range["B" + row, "B" + row];
                //Distance.Value2 = records[row - 1].Distance;
                //Range Angle = worksheet.Range["C" + row, "C" + row];
               // Angle.Value2 = records[row - 1].Name;
                //Range Width = worksheet.Range["D" + row, "D" + row];
               // Width.Value2 = records[row - 1].Name;
               // Range Height = worksheet.Range["E" + row, "A" + row];
               // Height.Value2 = records[row - 1].Name;
               // Range IsDefect = worksheet.Range["A" + row, "A" + row];
               // IsDefect.Value2 = records[row - 1].Name;
            }
            workbook.SaveAs(filename, XlFileFormat.xlOpenXMLWorkbook, Missing.Value,Missing.Value, false, false, XlSaveAsAccessMode.xlNoChange,
                XlSaveConflictResolution.xlUserResolution, true,Missing.Value, Missing.Value, Missing.Value);
            workbook.Close(false);
            Marshal.ReleaseComObject(workbook);
            _excelApp.Quit();
            Marshal.FinalReleaseComObject(_excelApp);
        }
        public void SaveCSV(string filename)
        {
            ObservableCollection<Record> records = (ObservableCollection<Record>)ShowData.Items.SourceCollection;
            string result = "Name;Distance;Angle;Width;Height;IsDefect\n";
            for (int i = 0; i < records.Count; i++)
            {
                result = result + records[i].Name + ';' + records[i].Distance + ';' + records[i].Angle + ';' + 
                    records[i].Width + ';' + records[i].Height + ';' + records[i].IsDefect+"\n";
            }
            System.IO.File.WriteAllText(filename, result, System.Text.Encoding.UTF8);

        }
    }
    public class Record
    {
        public string Name { get; set; }
        public double Distance { get; set; }
        public double Angle { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string IsDefect { get; set; }

    }
}
