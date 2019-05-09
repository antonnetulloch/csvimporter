using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVImporter.DataModel
{
    public class FileData
    {
        public int FileDataId { get; set; }
        public string FileName { get; set; }
        public string Model { get; set; }
        public int BlockNumber { get; set; }
        public DateTime ImportedDate { get; set; }
    }
}
