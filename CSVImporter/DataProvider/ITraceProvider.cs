using CSVImporter.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVImporter.DataProvider
{
    public interface ITraceProvider
    {
        Task<FileData> SaveFileDataAsync(FileData file);
        Task<BlockData> SaveBlockDataAsync(BlockData block);
        Task<TraceDate> SaveTraceDateAsync(TraceDate traceDate);
        Task<TraceTime> SaveTraceTimeAsync(TraceTime traceTime);
        Task<TraceData> SaveTraceDataAsync(TraceData traceData);
        Task SaveBatchTraceDataAsync(List<TraceData> traceData);
        void SaveBatchTraceData(List<TraceData> batch);
        Task<TraceHeader> SaveTraceHeaderAsync(TraceHeader header);
    }
}
