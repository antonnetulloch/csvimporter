using Akka.Actor;
using CSVImporter.DataModel;
using CSVImporter.Message;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVImporter.Actor
{
    public class TraceDataParserActor : ReceiveActor
    {
        public TraceDataParserActor()
        {
            Receive<ParseTraceDataCommand>(parseLine =>
            {
                var elements = parseLine.Line.Split(',');
                if (elements[0].Trim('"') == "")
                {
                    for (int i = 1; i < elements.Length; i++)
                    {
                        string elementData = elements[i].Trim('"');
                        if (elementData != "")
                        {
                            decimal? value = null;
                            if (decimal.TryParse(elementData, NumberStyles.Float, CultureInfo.InvariantCulture, out decimal result))
                                value = result;

                            var trace = new TraceData
                            {
                                TraceHeaderId = parseLine.TraceHeaderIds[i - 1],
                                RecordId = parseLine.RecordIndex,
                                TraceValue = value
                            };
                            Context.ActorSelection(parseLine.TraceDataActorPath).Tell(new SaveTraceCommand { Size = parseLine.LineSize, TraceData = trace });
                            //try
                            //{
                            //    traceProvider.SaveTraceDataAsync(trace);
                            //    UpdateProgressCounter(lineSize);
                            //}
                            //catch(Exception ex)
                            //{
                            //    UpdateErrorMessage(ex.Message);
                            //}
                        }
                    }
                }
            });
        }
    }
}
