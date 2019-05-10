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
        public long TraceHeaderId { get; set; }
        public long RecordId { get; set; }
        public decimal? TraceValue { get; set; }
    }
}
