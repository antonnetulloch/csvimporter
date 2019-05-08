using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System.IO;
using System.Text;
using System.Threading;

namespace CSVImporter.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            ProgressBarIsVisible = false;
        }

        private RelayCommand importCommand;

        private bool progressBarIsVisible;

        public bool ProgressBarIsVisible
        {
            get { return progressBarIsVisible; }
            set
            {
                progressBarIsVisible = value;
                RaisePropertyChanged(nameof(ProgressBarIsVisible));
            }
        }

        private long totalToImport;

        public long TotalToImport
        {
            get { return totalToImport; }
            set
            {
                totalToImport = value;
                RaisePropertyChanged(nameof(TotalToImport));
            }
        }

        private long sizeImported;

        public long SizeImported
        {
            get { return sizeImported; }
            set
            {
                sizeImported = value;
                RaisePropertyChanged(nameof(SizeImported));
            }
        }

        private string message;

        public string Message
        {
            get { return message; }
            set
            {
                message = value;
                RaisePropertyChanged(nameof(Message));
            }
        }

        public RelayCommand ImportCommand
        {
            get
            {
                return importCommand ??
                    (importCommand = new RelayCommand(
                        () =>
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
                                SizeImported = 0;
                                TotalToImport = total;

                                Message = sb.ToString();

                                foreach (var fileName in dialog.FileNames)
                                {
                                    //var task = Task.Run(() => ReadFile(fileName));
                                    ThreadPool.QueueUserWorkItem(o => ReadFile(fileName));
                                    //task.Wait();
                                }
                            }
                            else
                                Message = "Failed to open file";
                        })
                    );
            }
        }

        private void ReadFile(string filePath)
        {
            //Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    prgFileImport.Visibility = Visibility.Visible;
            //}));
            ProgressBarIsVisible = true;

            long counter = 0;
            string line;
            StreamReader file = new StreamReader(filePath);
            while ((line = file.ReadLine()) != null)
            {
                counter++;
                var lineSize = Encoding.ASCII.GetByteCount(line);
                SizeImported += lineSize;
                if (counter == 1000)
                {
                    //Dispatcher.BeginInvoke((Action)(() =>
                    //{
                    //    prgFileImport.Value = importedSize;
                    //    txtImportedSize.Text = importedSize.ToString();
                    //}));
                    counter = 0;
                }
            }
            //Dispatcher.BeginInvoke((Action)(() =>
            //{
            //    prgFileImport.Visibility = Visibility.Hidden;
            //}));
        }
    }
}