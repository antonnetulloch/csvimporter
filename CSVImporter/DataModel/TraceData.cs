using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVImporter.DataModel
{
    public class TraceData
    {
        public long TraceDataId { get; set; }
        public int FileDataId { get; set; }
        public long RecordId { get; set; }
        public string TraceName { get; set; }
        public string TraceValue { get; set; }
    }
}
