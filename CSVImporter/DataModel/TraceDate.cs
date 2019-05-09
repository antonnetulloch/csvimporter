using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVImporter.DataModel
{
    public class TraceDate
    {
        public int TraceDateId { get; set; }

        public int FileDataId { get; set; }

        public string TraceName { get; set; }

        public DateTime FileTraceDate { get; set; }
    }
}
