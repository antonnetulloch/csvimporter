using CSVImporter.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVImporter.Message
{
    public class SaveTraceCommand
    {
        public TraceData TraceData { get; set; }
        public int Size { get; set; }
    }
}
