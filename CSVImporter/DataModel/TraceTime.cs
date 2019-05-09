using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVImporter.DataModel
{
    public class TraceTime
    {
        public int TraceTimeId { get; set; }

        public int FileDataId { get; set; }

        public int TraceName { get; set; }

        public DateTime FileTraceTime { get; set; }
    }
}
