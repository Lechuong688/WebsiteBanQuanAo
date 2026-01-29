using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;


namespace Data.Repository
{
    public interface IDatabaseSql
    {
        //string GetConnectByDomain(string domain);
        Task<SqlConnection> GetConnect();
        Task<DataTable> ExecuteTable(string sql);
        Task<int> ExecuteNonQuery(string sql);
        Task<int> ExecuteScalar(string sql);
        Task<DataTable> ExecuteProcTable(string procName, List<SqlParameter> lstParam);
        Task<DataSet> ExecuteProcDataSet(string procName, List<SqlParameter> lstParam);
        List<Dictionary<string, object>> ParseTableToDictionary(DataTable dt);
        Task<int> ExecuteProcNonQuery(string procName, List<SqlParameter> lstParam);
        Task<IList<T>> ExecuteProcToList<T>(string procName, List<SqlParameter> lstParam);
        Task<string> ExecuteProcToStringJson(string procName, List<SqlParameter> lstParam);
        string ConvertDataTabletoJson(DataTable dt);
        List<T> ConvertDataTableToList<T>(DataTable dt);
        IList<T> DataTableToList<T>(DataTable table);
        IList<T> ConvertToList<T>(DataTable dt);
        Task<string> ExecuteProcToJson(string procName, List<SqlParameter> lstParam);
        SqlParameter CreateSqlParameter(object value, string name);
        List<SqlParameter> ConvertClassToSqlParameter<T>(T dt);
        List<SqlParameter> ConvertClassToSqlParameterNull<T>(T dt);
        DataTable ToUserDefinedDataTable<T>(T dt);
        DataTable ConvertToCustomUserDefinedDataTable<T>(IEnumerable<T> values) where T : class;
        Task<IList<T>> ExecuteProcXmlToList<T>(string procName, List<SqlParameter> lstParam);
        Task<T> ExecuteProcXmlToObject<T>(string procName, List<SqlParameter> lstParam) where T : new();
    }
}
