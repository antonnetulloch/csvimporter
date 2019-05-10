using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVImporter.DataModel
{
    public class TraceHeader
    {
        public int TraceHeaderId { get; set; }
        public int FileDataId { get; set; }
        public string TraceName { get; set; }
        public int BlockSize { get; set; }
        public DateTime TraceDate { get; set; }
        public string VUnit { get; set; }
        public decimal HResolution { get; set; }
        public decimal HOffSet { get; set; }
        public string HUnit { get; set; }
    }
}
