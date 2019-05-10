using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVImporter.Utility
{
    public class HeaderDataParser
    {
        public string ParseModel(string line)
        {
            var model = "";
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "Model")
                model = elements[1].Trim('"');
            return model;
        }

        public int ParseBlockNumber(string line)
        {
            var blockNumber = "0";
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "BlockNumber")
                blockNumber = elements[1].Trim('"');
            return int.Parse(blockNumber);
        }

        public List<DateTime> ParseTraceDates(string line)
        {
            List<DateTime> traceDates = new List<DateTime>();
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "Date")
            {
                for (int i = 1; i < elements.Length; i++)
                {
                    if (elements[i].Trim('"') != "")
                        traceDates.Add(DateTime.ParseExact(elements[i].Trim('"'), "yyyy/MM/dd", null));
                }
            }
            return traceDates;
        }

        public List<string> ParseTraceNames(string line)
        {
            List<string> traceNames = new List<string>();
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "TraceName")
            {
                for (int i = 1; i < elements.Length; i++)
                {
                    if (elements[i].Trim('"') != "")
                        traceNames.Add(elements[i].Trim('"'));
                }
            }
            return traceNames;
        }

        public List<int> ParseBlockSizes(string line)
        {
            List<int> sizes = new List<int>();
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "BlockSize")
            {
                for (int i = 1; i < elements.Length; i++)
                {
                    int result = 0;
                    int.TryParse(elements[i].Trim('"'), out result);
                    sizes.Add(result);
                }
            }
            return sizes;
        }

        public List<string> ParseVUnits(string line)
        {
            List<string> vUnits = new List<string>();
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "VUnit")
            {
                for (int i = 1; i < elements.Length; i++)
                {
                    if (elements[i].Trim('"') != "")
                        vUnits.Add(elements[i].Trim('"'));
                }
            }
            return vUnits;
        }

        public List<decimal> ParseHResolution(string line)
        {
            List<decimal> hResolutions = new List<decimal>();
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "HResolution")
            {
                for (int i = 1; i < elements.Length; i++)
                {
                    if (elements[i].Trim('"') != "")
                        hResolutions.Add(decimal.Parse(elements[i].Trim('"'), System.Globalization.NumberStyles.Float));
                }
            }
            return hResolutions;
        }

        public List<decimal> ParseHOffset(string line)
        {
            List<decimal> hOffsets = new List<decimal>();
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "HOffset")
            {
                for (int i = 1; i < elements.Length; i++)
                {
                    if (elements[i].Trim('"') != "")
                        hOffsets.Add(decimal.Parse(elements[i].Trim('"'), System.Globalization.NumberStyles.Float));
                }
            }
            return hOffsets;
        }

        public List<string> ParseHUnits(string line)
        {
            List<string> hUnits = new List<string>();
            var elements = line.Split(',');
            if (elements[0].Trim('"') == "HUnit")
            {
                for (int i = 1; i < elements.Length; i++)
                {
                    if (elements[i].Trim('"') != "")
                        hUnits.Add(elements[i].Trim('"'));
                }
            }
            return hUnits;
        }
    }
}
