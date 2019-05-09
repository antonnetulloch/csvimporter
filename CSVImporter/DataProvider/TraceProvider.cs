using CSVImporter.DataModel;
using Npgsql;
using Dapper;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSVImporter.DataProvider
{
    public class TraceProvider : ITraceProvider
    {
        public async Task<BlockData> SaveBlockDataAsync(BlockData block)
        {
            using (IDbConnection connection = new NpgsqlConnection(GetConnectionString()))
            {
                var sql = @"INSERT INTO blockdata(filedataid, tracename, blocksize)
                            VALUES(@FileDataId, @TraceName, @BlockSize)
                            RETURNING blockdataid;";

                block.BlockDataId = await connection.ExecuteScalarAsync<int>(sql, block);
                return block;
            }
        }

        public async Task<FileData> SaveFileDataAsync(FileData file)
        {
            using (IDbConnection connection = new NpgsqlConnection(GetConnectionString()))
            {
                var sql = @"INSERT INTO filedata(filename, model, blocknumber, importeddate)
                            VALUES(@FileName, @Model, @BlockNumber, @ImportedDate)
                            RETURNING filedataid;";

                file.FileDataId = await connection.ExecuteScalarAsync<int>(sql, file);
                return file;
            }
        }

        public async Task<TraceData> SaveTraceDataAsync(TraceData traceData)
        {
            using (IDbConnection connection = new NpgsqlConnection(GetConnectionString()))
            {
                var sql = @"INSERT INTO tracedata(filedataid, recordid, tracename, tracevalue)
                        VALUES(@FileDataId, @RecordId, @TraceName, @TraceValue)
                        RETURNING tracedataid";

                traceData.TraceDataId = await connection.ExecuteScalarAsync<int>(sql, traceData);
                return traceData;
            }
            
        }

        public async Task<TraceDate> SaveTraceDateAsync(TraceDate traceDate)
        {
            using (IDbConnection connection = new NpgsqlConnection(GetConnectionString()))
            {
                var sql = @"INSERT INTO tracedate(filedataid, tracename, filetracedate)
                            VALUES(@FileDataId, @TraceName, @FileTraceDate)
                            RETURNING tracedateid;";

                traceDate.TraceDateId = await connection.ExecuteScalarAsync<int>(sql, traceDate);

                connection.Close();
                connection.Dispose();

                return traceDate;
            }
        }

        public async Task<TraceTime> SaveTraceTimeAsync(TraceTime traceTime)
        {
            using (IDbConnection connection = new NpgsqlConnection(GetConnectionString()))
            {
                var sql = @"INSERT INTO tracetime(filedataid, tracename, filetracetime)
                            VALUES(@FileDataId, @TraceName, @FileTraceTime);
                            RETURNING tracetimeid;";

                traceTime.TraceTimeId = await connection.ExecuteScalarAsync<int>(sql, traceTime);
                return traceTime;
            }
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["ElectricalTrace"].ConnectionString;
        }
    }
}
