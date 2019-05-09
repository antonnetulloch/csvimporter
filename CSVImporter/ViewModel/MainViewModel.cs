using Akka.Actor;
using Akka.Routing;
using CSVImporter.Actor;
using CSVImporter.DataModel;
using CSVImporter.DataProvider;
using CSVImporter.Message;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSVImporter.ViewModel
{
    public class MainViewModel : ViewModelBase, IUpdateProgress
    {
        private ActorSystem actorSystem;
        private string traceDataActorPath;
        public ITraceProvider traceProvider { get; set; }

        public MainViewModel()
        {
            ProgressBarIsVisible = false;
            traceProvider = new TraceProvider();

            actorSystem = ActorSystem.Create("trace-data-system");
            var props = Props.Create(() => new TraceDataPersistenceActor(this)).WithRouter(new RoundRobinPool(10));
            var actor = actorSystem.ActorOf(props, "traceData");
            traceDataActorPath = actor.Path.ToStringWithAddress();
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

        private int failureCount;

        public int FailureCount
        {
            get { return failureCount; }
            set
            {
                failureCount = value;
                RaisePropertyChanged(nameof(FailureCount));
            }
        }

        private string errorMessage;

        public string ErrorMessage
        {
            get { return errorMessage; }
            set
            {
                errorMessage = value;
                RaisePropertyChanged(nameof(ErrorMessage));
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
                                    ThreadPool.QueueUserWorkItem(async o => await ReadFile(fileName));
                                    //task.Wait();
                                }
                            }
                            else
                                Message = "Failed to open file";
                        })
                    );
            }
        }

        private async Task ReadFile(string filePath)
        {
            ProgressBarIsVisible = true;

            int index = 0;
            long counter = 0;
            string line;
            StreamReader file = new StreamReader(filePath);
            var fileData = new FileData { FileName = filePath, ImportedDate = DateTime.Now };
            string[] traceNames = null;
            int recordIndex = 0;

            while ((line = file.ReadLine()) != null)
            {
                var lineSize = Encoding.ASCII.GetByteCount(line);
                if (line.Trim() != "") index++;
                if (index == 1)
                {
                    fileData.Model = ParseModel(line);
                    SizeImported += lineSize;
                }
                else if (index == 2)
                {
                    fileData.BlockNumber = ParseBlockNumber(line);
                    await traceProvider.SaveFileDataAsync(fileData);
                    SizeImported += lineSize;
                }
                else if(index == 3)
                {
                    traceNames = ParseTraceNames(line);
                    SizeImported += lineSize;
                }
                else if(index == 4)
                {
                    SaveBlockData(line, fileData.FileDataId, traceNames);
                    SizeImported += lineSize;
                }
                else if(index > 10)
                {
                    recordIndex++;
                    SaveTraceData(line, fileData.FileDataId, recordIndex, traceNames, lineSize);
                }
                
                if (counter < 20)
                    Message = $"{Message}{Environment.NewLine}{line}";

                counter++;
                
                
                if (counter == 10000)
                {
                    counter = 0;
                }
            }
        }

        private string ParseModel(string line)
        {
            var model = "";
            var elements = line.Split(',');
            if(elements[0].Trim('"') == "Model")
                model = elements[1].Trim('"');
            return model;
        }

        private int ParseBlockNumber(string line)
        {
            var blockNumber = "0";
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "BlockNumber")
                blockNumber = elements[1].Trim('"');
            return int.Parse(blockNumber);
        }

        private string[] ParseTraceNames(string line)
        {
            List<string> traceNames = new List<string>();
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "TraceName")
            {
                for (int i = 0; i < elements.Length; i++)
                {
                    traceNames.Add(elements[i].Trim('"'));
                }
            }
            return traceNames.ToArray();
        }

        private void SaveBlockData(string line, int fileDataId, string[] traceNames)
        {
            var elements = line.Split(',');
            if(elements[0].Trim('"') == "BlockSize")
            {
                for (int i = 1; i < elements.Length; i++)
                {
                    if (elements[i].Trim('"') != "")
                    {
                        traceProvider.SaveBlockDataAsync(new BlockData
                        {
                            FileDataId = fileDataId,
                            TraceName = traceNames[i],
                            BlockSize = int.Parse(elements[i])
                        });
                    }
                }
            }
        }

        private void SaveTraceData(string line, int fileDataId, int recordIndex, string[] traceNames, int lineSize)
        {
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "")
            {
                for (int i = 1; i < elements.Length; i++)
                {
                    if (elements[i].Trim('"') != "")
                    {
                        var trace = new TraceData
                        {
                            FileDataId = fileDataId,
                            RecordId = recordIndex,
                            TraceName = traceNames[i],
                            TraceValue = elements[i]
                        };
                        actorSystem.ActorSelection(traceDataActorPath).Tell(new SaveTraceCommand { Size = lineSize, TraceData = trace });
                        //traceProvider.SaveTraceDataAsync(trace);
                    }
                }
            }
        }

        public void UpdateProgressCounter(int processed)
        {
            SizeImported += processed;
        }

        public void UpdateErrorMessage(string error)
        {
            FailureCount += 1;
            ErrorMessage = $"{ErrorMessage}{Environment.NewLine}{error}";
        }
    }
}