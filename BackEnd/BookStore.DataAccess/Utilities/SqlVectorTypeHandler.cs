using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlTypes;
using System.Data;

namespace BookStore.DataAccess.Utilities
{
    /// <summary>
    /// Custom Dapper Type Handler for EF Core 10 / SQL Server 2025 native Vectors.
    /// Dapper does not know how to map the new SqlVector struct out-of-the-box. 
    /// This class bridges the gap, teaching Dapper how to pass vectors to the underlying ADO.NET engine.
    /// </summary>
    /// <remarks>
    /// DEMO NOTE: To make this work globally, you must register it once at application startup (e.g., in Program.cs):
    /// SqlMapper.AddTypeHandler(new SqlVectorTypeHandler());
    /// </remarks>
    public class SqlVectorTypeHandler : SqlMapper.TypeHandler<SqlVector<float>>
    {
        /// <summary>
        /// Invoked when Dapper sends data TO the database.
        /// Takes our C# SqlVector and safely packages it into the SQL Parameter.
        /// </summary>
        public override void SetValue(IDbDataParameter parameter, SqlVector<float> value)
        {
            // ADO.NET's modern Microsoft.Data.SqlClient natively supports SqlVector.
            // By casting to SqlParameter, we bypass Dapper's default string/binary mapping 
            // and let ADO.NET serialize the vector directly via the TDS protocol for maximum performance.
            ((SqlParameter)parameter).Value = value;
        }

        /// <summary>
        /// Invoked when Dapper reads data FROM the database.
        /// Takes the raw object returned by the SqlDataReader and parses it back into our C# struct.
        /// </summary>
        public override SqlVector<float> Parse(object value)
        {
            // If the database returns NULL, Dapper handles it before calling Parse().
            // Therefore, we can safely cast the boxed object back to the strongly-typed SqlVector.

            // Add custom parsing logic here if you ever need to transform the vector BACK from 
            // a legacy binary format, but for native SQL Server vectors, a direct cast works perfectly.
            return (SqlVector<float>)value;
        }
    }
}