using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVImporter
{
    public interface IUpdateProgress
    {
        void UpdateProgressCounter(int processed);

        void UpdateErrorMessage(string error);
    }
}
