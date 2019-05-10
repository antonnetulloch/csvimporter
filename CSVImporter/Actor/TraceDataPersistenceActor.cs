using Akka.Actor;
using CSVImporter.DataModel;
using CSVImporter.DataProvider;
using CSVImporter.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVImporter.Actor
{
    public class TraceDataPersistenceActor : ReceiveActor
    {
        private ITraceProvider traceProvider;
        private IUpdateProgress progressUpdater;
        private int totalSize;
        private List<TraceData> traces;

        public TraceDataPersistenceActor(IUpdateProgress updater)
        {
            progressUpdater = updater;
            traceProvider = new TraceProvider();
            traces = new List<TraceData>();

            Receive<SaveTraceCommand>(async trace =>
            {
                int batchTotal = 100;
                try
                {
                    if(traces.Count < batchTotal)
                    {
                        traces.Add(trace.TraceData);
                        totalSize += trace.Size;
                    }
                    else
                    {
                        await traceProvider.SaveBatchTraceDataAsync(traces);
                        //traceProvider.SaveBatchTraceData(traces);
                        progressUpdater.UpdateProgressCounter(totalSize, batchTotal);
                        traces = new List<TraceData>();
                        totalSize = 0;
                    }
                    
                }
                catch (Exception ex)
                {
                    traces = new List<TraceData>();
                    totalSize = 0;
                    progressUpdater.UpdateErrorMessage(ex.Message);
                }
            });

            Receive<CompleteProcessingCommand>(async (c) =>
            {
                await traceProvider.SaveBatchTraceDataAsync(traces);
                progressUpdater.UpdateProgressCounter(totalSize, traces.Count());
                traces = new List<TraceData>();
                totalSize = 0;
            });
        }
    }
}
