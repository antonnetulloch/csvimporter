using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace CSVImporter
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

        private long importedSize = 0;

        private void BtnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "CSV Files (*.csv)|*.csv";
            dialog.Multiselect = true;
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                long total = 0;
                var sb = new StringBuilder();
                foreach (var fileName in dialog.FileNames)
                {
                    var size = new FileInfo(fileName).Length;
                    total += size;
                    sb.AppendLine($"{fileName} - {size} bytes;");
                }
                importedSize = 0;
                prgFileImport.Value = importedSize;
                prgFileImport.Maximum = total;
                txtEditor.Text = sb.ToString();

                foreach(var fileName in dialog.FileNames)
                {
                    //var task = Task.Run(() => ReadFile(fileName));
                    ThreadPool.QueueUserWorkItem(o => ReadFile(fileName));
                    //task.Wait();
                }
            }
            else
                txtEditor.Text = "Failed to open file";
        }

        private void ReadFile(string filePath)
        {
            Dispatcher.BeginInvoke((Action)(() => 
            {
                prgFileImport.Visibility = Visibility.Visible;
            }));

            long counter = 0;
            string line;
            StreamReader file = new StreamReader(filePath);
            while((line = file.ReadLine()) != null)
            {
                counter++;
                var lineSize = System.Text.ASCIIEncoding.ASCII.GetByteCount(line);
                importedSize += lineSize;
                if(counter == 1000)
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        prgFileImport.Value = importedSize;
                        txtImportedSize.Text = importedSize.ToString();
                    }));
                    counter = 0;
                }
            }
            Dispatcher.BeginInvoke((Action)(() =>
            {
                prgFileImport.Visibility = Visibility.Hidden;
            }));
        }

        public void SetVisibility(Visibility visibility)
        {
            prgFileImport.Visibility = visibility;
        }
    }
}
