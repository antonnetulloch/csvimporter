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

        public TraceDataPersistenceActor(IUpdateProgress updater)
        {
            progressUpdater = updater;
            traceProvider = new TraceProvider();

            Receive<SaveTraceCommand>(async trace =>
            {
                try
                {
                    var traceData = await traceProvider.SaveTraceDataAsync(trace.TraceData);
                    if (traceData.TraceDataId > 0)
                        progressUpdater.UpdateProgressCounter(trace.Size);
                }
                catch (Exception ex)
                {
                    progressUpdater.UpdateErrorMessage(ex.ToString());
                }
            });
        }
    }
}
