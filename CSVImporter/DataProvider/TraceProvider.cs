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
        private NpgsqlConnection connection;

        public TraceProvider()
        {
            connection = new NpgsqlConnection(GetConnectionString());
        }

        public async Task SaveBatchTraceDataAsync(List<TraceData> batch)
        {
            var sb = new StringBuilder();
            using (var conn = new NpgsqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    int paramIndex = 0;
                    for (int i = 0; i < batch.Count; i++)
                    {
                        sb.Append($@"INSERT INTO tracedata(traceheaderid, recordid, tracevalue)
                        VALUES(@TraceHeaderId{i}, @RecordId{i}, @TraceValue{i});");
                        cmd.Parameters.Add($"TraceHeaderId{i}", NpgsqlTypes.NpgsqlDbType.Bigint);
                        cmd.Parameters[paramIndex].Value = batch[i].TraceHeaderId;
                        paramIndex++;
                        cmd.Parameters.Add($"RecordId{i}", NpgsqlTypes.NpgsqlDbType.Bigint);
                        cmd.Parameters[paramIndex].Value = batch[i].RecordId;
                        paramIndex++;
                        cmd.Parameters.Add($"TraceValue{i}", NpgsqlTypes.NpgsqlDbType.Numeric);
                        if (batch[i].TraceValue != null)
                            cmd.Parameters[paramIndex].Value = batch[i].TraceValue;
                        else
                            cmd.Parameters[paramIndex].Value = DBNull.Value;
                        paramIndex++;
                        //cmd.Parameters.AddWithValue($"TraceHeaderId{i}", batch[i].TraceHeaderId);
                        //cmd.Parameters.AddWithValue($"RecordId{i}", batch[i].RecordId);
                        //cmd.Parameters.AddWithValue($"TraceValue{i}", batch[i].TraceValue);
                    }
                    cmd.CommandText = sb.ToString();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return;
        }

        public void SaveBatchTraceData(List<TraceData> batch)
        {
            var sb = new StringBuilder();
            using (var conn = new NpgsqlConnection(GetConnectionString()))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    int paramIndex = 0;
                    for (int i = 0; i < batch.Count; i++)
                    {
                        sb.Append($@"INSERT INTO tracedata(traceheaderid, recordid, tracevalue)
                        VALUES(@TraceHeaderId{i}, @RecordId{i}, @TraceValue{i});");
                        cmd.Parameters.Add($"TraceHeaderId{i}", NpgsqlTypes.NpgsqlDbType.Bigint);
                        cmd.Parameters[paramIndex].Value = batch[i].TraceHeaderId;
                        paramIndex++;
                        cmd.Parameters.Add($"RecordId{i}", NpgsqlTypes.NpgsqlDbType.Bigint);
                        cmd.Parameters[paramIndex].Value = batch[i].RecordId;
                        paramIndex++;
                        cmd.Parameters.Add($"TraceValue{i}", NpgsqlTypes.NpgsqlDbType.Numeric);
                        if (batch[i].TraceValue != null)
                            cmd.Parameters[paramIndex].Value = batch[i].TraceValue;
                        else
                            cmd.Parameters[paramIndex].Value = DBNull.Value;
                        paramIndex++;
                        //cmd.Parameters.AddWithValue($"TraceHeaderId{i}", batch[i].TraceHeaderId);
                        //cmd.Parameters.AddWithValue($"RecordId{i}", batch[i].RecordId);
                        //cmd.Parameters.AddWithValue($"TraceValue{i}", batch[i].TraceValue);
                    }
                    cmd.CommandText = sb.ToString();
                    cmd.ExecuteNonQuery();
                }
            }

            return;
        }

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
                var sql = @"INSERT INTO tracedata(traceheaderid, recordid, tracevalue)
                    VALUES(@TraceHeaderId, @RecordId, @TraceValue)
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

                return traceDate;
            }
        }

        public async Task<TraceHeader> SaveTraceHeaderAsync(TraceHeader header)
        {
            using (IDbConnection connection = new NpgsqlConnection(GetConnectionString()))
            {
                var sql = @"INSERT INTO traceheader(filedataid, tracename, blocksize, tracedate, vunit, hresolution, hoffset, hunit)
                            VALUES(@FileDataId, @TraceName, @BlockSize, @TraceDate, @VUnit, @HResolution, @HOffSet, @HUnit)
                            RETURNING traceheaderid;";

                header.TraceHeaderId = await connection.ExecuteScalarAsync<int>(sql, header);

                return header;
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

        public async Task<VUnitData> SaveVUnitDataAsync(VUnitData vunit)
        {
            using (IDbConnection connection = new NpgsqlConnection(GetConnectionString()))
            {
                var sql = @"INSERT INTO vunitdata(filedataid, tracename, vunit)
                            VALUES(@FileDataId, @TraceName, @VUnit);
                            RETURNING tracetimeid;";

                vunit.VUnitDataId = await connection.ExecuteScalarAsync<int>(sql, vunit);
                return vunit;
            }
        }

        private string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["ElectricalTrace"].ConnectionString;
        }
    }
}
