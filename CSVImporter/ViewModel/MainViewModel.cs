using Akka.Actor;
using Akka.Routing;
using CSVImporter.Actor;
using CSVImporter.DataModel;
using CSVImporter.DataProvider;
using CSVImporter.Message;
using CSVImporter.Utility;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace CSVImporter.ViewModel
{
    public class MainViewModel : ViewModelBase, IUpdateProgress
    {
        private ActorSystem actorSystem;
        private string traceDataActorPath;
        private string parserActorPath;
        private HeaderDataParser parser;
        public ITraceProvider traceProvider { get; set; }

        public MainViewModel()
        {
            ProgressBarIsVisible = false;
            traceProvider = new TraceProvider();
            parser = new HeaderDataParser();

            actorSystem = ActorSystem.Create("trace-data-system");
            var props = Props.Create(() => new TraceDataPersistenceActor(this)).WithRouter(new SmallestMailboxPool(1));
            var actor = actorSystem.ActorOf(props, "traceData");
            traceDataActorPath = actor.Path.ToStringWithAddress();
            var parserProps = Props.Create(() => new TraceDataParserActor()).WithRouter(new SmallestMailboxPool(10));
            var parserActor = actorSystem.ActorOf(parserProps, "parseTraceData");
            parserActorPath = parserActor.Path.ToStringWithAddress();
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

        private long totalLines;

        public long TotalLines
        {
            get { return totalLines; }
            set
            {
                totalLines = value;
                RaisePropertyChanged(nameof(TotalLines));
            }
        }

        private long linesProcessed;

        public long LinesProcessed
        {
            get { return linesProcessed; }
            set
            {
                linesProcessed = value;
                RaisePropertyChanged(nameof(LinesProcessed));
            }
        }

        private long totalTraceEntries;

        public long TotalTraceEntries
        {
            get { return totalTraceEntries; }
            set
            {
                totalTraceEntries = value;
                RaisePropertyChanged(nameof(TotalTraceEntries));
            }
        }

        private long traceEntriesProcessed;

        public long TraceEntriesProcessed
        {
            get { return traceEntriesProcessed; }
            set
            {
                traceEntriesProcessed = value;
                RaisePropertyChanged(nameof(TraceEntriesProcessed));
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
                                long linesSum = 0;
                                var sb = new StringBuilder();
                                foreach (var fileName in dialog.FileNames)
                                {
                                    var size = new FileInfo(fileName).Length;
                                    total += size;
                                    sb.AppendLine($"{fileName} - {size} bytes;");

                                    var lines = File.ReadLines(fileName).LongCount();
                                    sb.AppendLine($"{fileName} - {lines};");
                                    linesSum += lines;
                                }
                                SizeImported = 0;
                                TotalToImport = total;
                                TotalLines = linesSum;

                                Message = sb.ToString();

                                foreach (var fileName in dialog.FileNames)
                                {
                                    ThreadPool.QueueUserWorkItem(async o => await ReadFile(fileName));
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
            
            var fileData = new FileData { FileName = filePath, ImportedDate = DateTime.Now };
            List<string> traceNames = null;
            List<DateTime> traceDates = null;
            List<int> blockSizes = null;
            List<string> vUnits = null;
            List<decimal> hResolutions = null;
            List<decimal> hOffsets = null;
            List<string> hUnits = null;
            List<long> traceHeaderIds = new List<long>();
            int recordIndex = 0;

            var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using (StreamReader file = new StreamReader(fileStream))
            {
                while ((line = file.ReadLine()) != null)
                {
                    var lineSize = Encoding.ASCII.GetByteCount(line);
                    if (line.Trim() != "") index++;
                    switch (index)
                    {
                        case 1:
                            fileData.Model = parser.ParseModel(line);
                            SizeImported += lineSize;
                            break;
                        case 2:
                            fileData.BlockNumber = parser.ParseBlockNumber(line);
                            await traceProvider.SaveFileDataAsync(fileData);
                            SizeImported += lineSize;
                            break;
                        case 3:
                            traceNames = parser.ParseTraceNames(line);
                            if (TotalTraceEntries <= TotalLines)
                            {
                                TotalTraceEntries = TotalLines * traceNames.Count();
                                Message = $"{Message}{Environment.NewLine}Total Trace Entries: {TotalTraceEntries}";
                            }
                            SizeImported += lineSize;
                            break;
                        case 4:
                            blockSizes = parser.ParseBlockSizes(line);
                            SizeImported += lineSize;
                            break;
                        case 5:
                            traceDates = parser.ParseTraceDates(line);
                            SizeImported += lineSize;
                            break;
                        case 6:
                            //SaveTraceDateAndTime(line, fileData.FileDataId, traceNames, traceDates);
                            //SaveTraceTime(line, fileData.FileDataId, traceNames);
                            SizeImported += lineSize;
                            break;
                        case 7:
                            vUnits = parser.ParseVUnits(line);
                            SizeImported += lineSize;
                            break;
                        case 8:
                            hResolutions = parser.ParseHResolution(line);
                            SizeImported += lineSize;
                            break;
                        case 9:
                            hOffsets = parser.ParseHOffset(line);
                            SizeImported += lineSize;
                            break;
                        case 10:
                            hUnits = parser.ParseHUnits(line);
                            traceHeaderIds = await SaveTraceHeaderAsync(fileData.FileDataId, traceNames, blockSizes, traceDates, vUnits,
                                hResolutions, hOffsets, hUnits);
                            SizeImported += lineSize;
                            break;
                        default:
                            recordIndex++;
                            SaveTraceData(line, fileData.FileDataId, recordIndex, traceHeaderIds, lineSize);
                            break;
                    }

                    counter++;
                    LinesProcessed++;
                }
                actorSystem.ActorSelection(traceDataActorPath).Tell(new CompleteProcessingCommand());
            }
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

        private void SaveTraceDate(string line, int fileDataId, string[] traceNames)
        {
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "Date")
            {
                for (int i = 1; i < elements.Length; i++)
                {
                    if (elements[i].Trim('"') != "")
                    {
                        traceProvider.SaveTraceDateAsync(new TraceDate
                        {
                            FileDataId = fileDataId,
                            TraceName = traceNames[i],
                            FileTraceDate = DateTime.ParseExact(elements[i], "yyyy/MM/dd", CultureInfo.InvariantCulture)
                        });
                    }
                }
            }
        }

        private void SaveTraceDateAndTime(string line, int fileDataId, string[] traceNames, string[] traceDates)
        {
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "Time")
            {
                for (int i = 1; i < elements.Length; i++)
                {
                    string fullDate = $"{traceDates[i]} {elements[i]}";
                    if (elements[i].Trim('"') != "")
                    {
                        traceProvider.SaveTraceDateAsync(new TraceDate
                        {
                            FileDataId = fileDataId,
                            TraceName = traceNames[i],
                            FileTraceDate = DateTime.ParseExact(fullDate, "yyyy/MM/dd HH:mm:ss", new CultureInfo("en-US"))
                        });
                    }
                }
            }
        }

        private void SaveTraceTime(string line, int fileDataId, string[] traceNames)
        {
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "Time")
            {
                for (int i = 1; i < elements.Length; i++)
                {
                    if (elements[i].Trim('"') != "")
                    {
                        traceProvider.SaveTraceTimeAsync(new TraceTime
                        {
                            FileDataId = fileDataId,
                            TraceName = traceNames[i],
                            FileTraceTime = DateTime.ParseExact(elements[i], "HH:mm:ss", new CultureInfo("en-US"))
                        });
                    }
                }
            }
        }

        private async Task<List<long>> SaveTraceHeaderAsync(int fileDataId, List<string> traceNames, List<int> blockSizes, List<DateTime> traceDates,
            List<string> vUnits, List<decimal> hResolutions, List<decimal> hOffsets, List<string> hUnits)
        {
            List<long> ids = new List<long>();
            for (int i = 0; i < traceNames.Count; i++)
            {
                var header = await traceProvider.SaveTraceHeaderAsync(new TraceHeader
                {
                    FileDataId = fileDataId,
                    TraceName = traceNames[i],
                    BlockSize = blockSizes[i],
                    TraceDate = traceDates[i],
                    VUnit = vUnits[i],
                    HResolution = hResolutions[i],
                    HOffSet = hOffsets[i],
                    HUnit = hUnits[i]
                });
                ids.Add(header.TraceHeaderId);
            }
            return ids;
        }

        private void SaveTraceData(string line, int fileDataId, int recordIndex, List<long> traceHeaderIds, int lineSize)
        {
            actorSystem.ActorSelection(parserActorPath).Tell(new ParseTraceDataCommand
            {
                Line = line,
                RecordIndex = recordIndex,
                TraceHeaderIds = traceHeaderIds,
                LineSize = lineSize,
                TraceDataActorPath = traceDataActorPath
            });
        }

        public void UpdateProgressCounter(int processed, int batchTotal)
        {
            SizeImported += processed;
            TraceEntriesProcessed += batchTotal;
            //LinesProcessed = LinesProcessed + batchTotal;
        }

        public void UpdateErrorMessage(string error)
        {
            FailureCount += 1;
            ErrorMessage = $"{ErrorMessage}{Environment.NewLine}{error}";
        }
    }
}