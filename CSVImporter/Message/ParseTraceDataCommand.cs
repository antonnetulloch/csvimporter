using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVImporter.Message
{
    public class ParseTraceDataCommand
    {
        public string Line { get; set; }
        public int RecordIndex { get; set; }
        public List<long> TraceHeaderIds { get; set; }
        public int LineSize { get; set; }
        public string TraceDataActorPath { get; set; }
    }
}
