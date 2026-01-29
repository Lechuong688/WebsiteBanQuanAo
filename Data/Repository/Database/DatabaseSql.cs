using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace Data.Repository
{
    public class DatabaseSql : IDatabaseSql
    {
        private readonly ILogger _logger;
        private string _connectStringDb;
        private readonly IConfiguration _configuration;
        public DatabaseSql(ILogger<DatabaseSql> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            _connectStringDb = _configuration["ConnectionStrings:ConnectdDb"];
        }
        //public string GetConnectByDomain(string domain)
        //{
        //    return "Data Source=.;Initial Catalog=" + domain + ";User ID=sa;Password=sk1234;Pooling=False;Connect Timeout=30";
        //}
        #region Create connection
        /// <summary>
        /// Lấy kết nối đến DB
        /// </summary>
        /// <param name="_connectStringDb">
        /// kết nối db
        /// </param>
        /// <returns></returns>
        public async Task<SqlConnection> GetConnect()
        {
            SqlConnection con = new SqlConnection(_connectStringDb);
            if (con.State == ConnectionState.Closed) await con.OpenAsync();
            return con;
        }
        #endregion

        #region Execute Table for query
        public async Task<DataTable> ExecuteTable(string sql)
        {
            var con = await GetConnect();
            SqlCommand cmd = new SqlCommand(sql, con)
            {
                CommandType = CommandType.Text
            };
            SqlDataAdapter da = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();
            try
            {
                da.Fill(dt);
                return dt;
            }
            catch (Exception ex)
            {
                _logger.LogError("ERROR ExecuteTable: " + sql + ": " + ex.Message);
                return null;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }
        #endregion

        #region Execute Non Query for query
        public async Task<int> ExecuteNonQuery(string sql)
        {
            var con = await GetConnect();
            SqlCommand cmd = new SqlCommand(sql, con)
            {
                CommandType = CommandType.Text
            };
            try
            {
                return await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError("ERROR ExecuteNonQuery: " + sql + ": " + ex.Message);
                return 0;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }
        #endregion

        #region Execute Scalar for query
        public async Task<int> ExecuteScalar(string sql)
        {
            var con = await GetConnect();
            SqlCommand cmd = new SqlCommand(sql, con)
            {
                CommandType = CommandType.Text
            };
            try
            {
                return (Int32)(await cmd.ExecuteScalarAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError("ERROR ExecuteScalar: " + sql + ": " + ex.Message);
                return 0;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }
        #endregion

        #region Execute Table for Store Procedure
        public async Task<DataTable> ExecuteProcTable(string procName, List<SqlParameter> lstParam)
        {
            var con = await GetConnect();
            var dt = new DataTable();
            try
            {
                using (var cmd = new SqlCommand(procName, con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = con.ConnectionTimeout;
                    if (lstParam.Count > 0)
                        foreach (var param in lstParam)
                        {
                            SqlParameter p = new SqlParameter()
                            {
                                ParameterName = param.ParameterName,
                                DbType = param.DbType,
                                SqlDbType = param.SqlDbType,
                                Direction = param.Direction,
                                IsNullable = param.IsNullable,
                                Value = param.Value,
                                Size = param.Size,
                                Precision = param.Precision,
                                Scale = param.Scale,
                            };
                            cmd.Parameters.Add(p);
                        }
                    var dtAdapter = new SqlDataAdapter(cmd);
                    dtAdapter.Fill(dt);
                    var paramOutput = lstParam.Where(x => x.Direction == ParameterDirection.Output || x.Direction == ParameterDirection.InputOutput);
                    if (paramOutput.Count() > 0)
                    {
                        foreach (var item in paramOutput)
                        {
                            item.Value = cmd.Parameters[item.ParameterName].Value;
                        }
                    }
                    return dt;
                }
            }
            catch (Exception ex)
            {
                string paramStr = "PARAM: ";
                if (lstParam.Count > 0)
                {
                    foreach (var param in lstParam)
                    {
                        paramStr = paramStr + param.ParameterName + ":" + (JsonConvert.SerializeObject(param?.Value ?? ""));
                    }
                }
                _logger.LogError("ERROR ExecuteProcTable: " + procName + ": " + paramStr + " " + ex.Message);
                return new DataTable();
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }
        #endregion

        #region Execute DataSet for Store Procedure
        public async Task<DataSet> ExecuteProcDataSet(string procName, List<SqlParameter> lstParam)
        {
            var con = await GetConnect();
            var ds = new DataSet();
            try
            {
                using (var cmd = new SqlCommand(procName, con))
                {
                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = con.ConnectionTimeout;
                    if (lstParam.Count > 0)
                        foreach (var param in lstParam)
                        {
                            cmd.Parameters.Add(param);
                        }
                    var dtAdapter = new SqlDataAdapter(cmd);
                    dtAdapter.Fill(ds);
                    return ds;
                }
            }
            catch (Exception ex)
            {
                string paramStr = "PARAM: ";
                if (lstParam.Count > 0)
                {
                    foreach (var param in lstParam)
                    {
                        paramStr = paramStr + param.ParameterName + ":" + (JsonConvert.SerializeObject(param?.Value ?? ""));
                    }
                }
                _logger.LogError("ERROR ExecuteProcDataSet: " + procName + ": " + paramStr + " " + ex.Message);
                return null;
            }
            finally
            {
                con.Close();
                con.Dispose();
            }
        }
        #endregion

        #region Parse Datatable To Dictionary
        public List<Dictionary<string, object>> ParseTableToDictionary(DataTable dt)
        {
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            return rows;
        }

        #endregion

        #region Execute Non Query for Store Procedure
        public async Task<int> ExecuteProcNonQuery(string procName, List<SqlParameter> lstParam)
        {
            var con = await GetConnect();
            try
            {
                //_logger.LogInformation("============================================================");
                //_logger.LogInformation("PROC_NAME_TOLIST: " + procName + "_" + DateTime.Now.ToString() + ": STRAT");
                using (var cmd = new SqlCommand(procName, con))
                {
                    //int CommandTimeout = _configuration.GetSection("SqlCommand:CommandTimeout").Get<int>();
                    cmd.CommandTimeout = 0;
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (lstParam.Count > 0)
                        foreach (var param in lstParam)
                        {
                            cmd.Parameters.Add(param);
                        }
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException ex)
            {
                string paramStr = "PARAM: ";
                if (lstParam.Count > 0)
                {
                    foreach (var param in lstParam)
                    {
                        paramStr = paramStr + param.ParameterName + ":" + (JsonConvert.SerializeObject(param?.Value ?? ""));
                    }
                }
                _logger.LogError("ERROR ExecuteProcNonQuery: " + procName + ": " + paramStr + " " + ex.Message);
                return 0;
            }
            finally
            {
                //_logger.LogInformation("PROC_NAME_TOLIST: " + procName + "_" + DateTime.Now.ToString() + ": END");
                //_logger.LogInformation("============================================================");
                con.Close();
                con.Dispose();
            }
        }
        #endregion

        #region Execute Store Procedure And Convert Result to List<T>

        public List<string> InvalidJsonElements;

        public async Task<IList<T>> ExecuteProcToList<T>(string procName, List<SqlParameter> lstParam)
        {
            var dt = new DataTable();

            IList<T> objectsList = new List<T>();
            try
            {
                //_logger.LogInformation("============================================================");
                //_logger.LogInformation("PROC_NAME_TOLIST: " + procName +"_" + DateTime.Now.ToString() + ": STRAT");
                //Execute procedure and get table result
                dt = await ExecuteProcTable(procName, lstParam);

                if (dt.Rows.Count == 0) return objectsList;
                else
                {
                    if (dt.Columns.Contains("ErrorMessage") && dt.Rows[0]["ErrorMessage"].ToString() != "")
                        return objectsList;
                }
                //objectsList = ConvertToList<T>(dt);
                //Covert datatable to json string
                string jsonString = JsonConvert.SerializeObject(ParseTableToDictionary(dt));
                objectsList = JsonConvert.DeserializeObject<List<T>>(jsonString);

                ////Convert jsonString to List<T>
                //InvalidJsonElements = null;
                //var array = JArray.Parse(jsonString);

                //foreach (var item in array)
                //{
                //    try
                //    {
                //        //Map single json in array to object<T>.
                //        var itemMapped = item.ToObject<T>();

                //        objectsList.Add(itemMapped);
                //    }
                //    catch (Exception ex)
                //    {
                //        InvalidJsonElements = InvalidJsonElements ?? new List<string>();
                //        InvalidJsonElements.Add(item.ToString());
                //        _logger.LogError("ERROR ExecuteProcToList: " + procName + ": Map single json in array to object: " + ex.Message);
                //    }
                //}
                //_logger.LogInformation("PROC_NAME_TOLIST: " + procName + "_" + DateTime.Now.ToString() + ": END");
                //_logger.LogInformation("============================================================");
                return objectsList;
            }
            catch (Exception ex)
            {
                string paramStr = "PARAM: ";
                if (lstParam.Count > 0)
                {
                    foreach (var param in lstParam)
                    {
                        paramStr = paramStr + param.ParameterName + ":" + (JsonConvert.SerializeObject(param?.Value ?? ""));
                    }
                }
                _logger.LogError("ERROR ExecuteProcToList: " + procName + ": " + paramStr + " " + ex.Message);
                return null;
            }
        }

        public async Task<IList<T>> ExecuteProcToList2<T>(string procName, List<SqlParameter> lstParam)
        {
            var dt = new DataTable();
            IList<T> objectsList = new List<T>();
            try
            {
                //Execute procedure and get table result
                dt = await ExecuteProcTable(procName, lstParam);

                if (dt.Rows.Count == 0) return objectsList;
                objectsList = DataTableToList<T>(dt);

                return objectsList;
            }
            catch (Exception ex)
            {
                string paramStr = "PARAM: ";
                if (lstParam.Count > 0)
                {
                    foreach (var param in lstParam)
                    {
                        paramStr = paramStr + param.ParameterName + ":" + (JsonConvert.SerializeObject(param?.Value ?? ""));
                    }
                }
                _logger.LogError("ERROR ExecuteProcToList2: " + procName + ": " + paramStr + " " + ex.Message);
                return null;
            }
        }
        public async Task<string> ExecuteProcToStringJson(string procName, List<SqlParameter> lstParam)
        {
            var dt = new DataTable();
            try
            {
                //Execute procedure and get table result
                dt = await ExecuteProcTable(procName, lstParam);
                //var stringXml = new StringBuilder("");
                //if (dt.Rows.Count == 0) return null;

                //stringXml.Append("<ListData>");
                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    stringXml.Append(dt.Rows[i][0]);
                //}
                //stringXml.Append("</ListData>");

                //XmlDocument doc = new XmlDocument();
                //doc.LoadXml(stringXml.ToString());
                string json = JsonConvert.SerializeObject(dt);
                return json;
            }
            catch (Exception ex)
            {
                string paramStr = "PARAM: ";
                if (lstParam.Count > 0)
                {
                    foreach (var param in lstParam)
                    {
                        paramStr = paramStr + param.ParameterName + ":" + (JsonConvert.SerializeObject(param?.Value ?? ""));
                    }
                }
                _logger.LogError("ERROR ExecuteProcToStringJson: " + procName + ": " + paramStr + " " + ex.Message);
                return null;
            }
        }

        public async Task<IList<T>> ExecuteProcXmlToList<T>(string procName, List<SqlParameter> lstParam)
        {
            List<T> reval = new List<T>();

            var dt = new DataTable();
            try
            {
                //_logger.LogInformation("============================================================");
                //_logger.LogInformation("PROC_NAME_TOLISTXML: " + procName + "_" + DateTime.Now.ToString() + ": STRAT");
                //Execute procedure and get table result
                dt = await ExecuteProcTable(procName, lstParam);
                var stringXml = new StringBuilder("");
                if (dt.Rows.Count == 0) return new List<T>();
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    stringXml.Append(dt.Rows[i][0]);
                }
                XmlDocument doc = new XmlDocument();
                string val = stringXml.ToString();
                doc.LoadXml(val);
                string json = JsonConvert.SerializeXmlNode(doc);
                var nds = doc.GetElementsByTagName(typeof(T).Name);
                foreach (var item in nds)
                {
                    var jsNode = JsonConvert.SerializeObject(item);
                    var resultDynamic = JsonConvert.DeserializeObject<dynamic>(jsNode);
                    if (resultDynamic != null)
                    {
                        var listDynamic = resultDynamic[typeof(T).Name];
                        var jsonData = JsonConvert.SerializeObject(listDynamic);
                        var data = JsonConvert.DeserializeObject<T>(jsonData);
                        reval.Add(data);
                    }
                }
            }
            catch (Exception ex)
            {
                string paramStr = "PARAM: ";
                if (lstParam.Count > 0)
                {
                    foreach (var param in lstParam)
                    {
                        paramStr = paramStr + param.ParameterName + ":" + (JsonConvert.SerializeObject(param?.Value ?? ""));
                    }
                }
                _logger.LogError("ERROR ExecuteProcXmlToList: " + procName + ": " + paramStr + " " + ex.Message);
                return null;
            }
            //_logger.LogInformation("PROC_NAME_TOLISTXML: " + procName + "_" + DateTime.Now.ToString() + ": END");
            //_logger.LogInformation("============================================================");
            return reval;
        }
        public async Task<T> ExecuteProcXmlToObject<T>(string procName, List<SqlParameter> lstParam) where T : new()
        {
            T reval = new T();

            var dt = new DataTable();
            try
            {
                //_logger.LogInformation("============================================================");
                //_logger.LogInformation("PROC_NAME_XML_TO_OBJECT: " + procName + "_" + DateTime.Now.ToString() + ": STRAT");
                //Execute procedure and get table result
                dt = await ExecuteProcTable(procName, lstParam);
                var stringXml = new StringBuilder("");
                if (dt.Rows.Count == 0) return reval;
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    stringXml.Append(dt.Rows[i][0]);
                }
                XmlDocument doc = new XmlDocument();
                string val = stringXml.ToString();
                doc.LoadXml(val);
                string json = JsonConvert.SerializeXmlNode(doc);
                var nds = doc.GetElementsByTagName(typeof(T).Name);

                if (nds.Count > 0)
                {
                    var jsNode = JsonConvert.SerializeObject(nds[0]);
                    var resultDynamic = JsonConvert.DeserializeObject<dynamic>(jsNode);
                    if (resultDynamic != null)
                    {
                        var listDynamic = resultDynamic[typeof(T).Name];
                        var jsonData = JsonConvert.SerializeObject(listDynamic);
                        reval = JsonConvert.DeserializeObject<T>(jsonData);
                    }
                }

            }
            catch (Exception ex)
            {
                string paramStr = "PARAM: ";
                if (lstParam.Count > 0)
                {
                    foreach (var param in lstParam)
                    {
                        paramStr = paramStr + param.ParameterName + ":" + (JsonConvert.SerializeObject(param?.Value ?? ""));
                    }
                }
                _logger.LogError("ERROR ExecuteProcXmlToObject: " + procName + ": " + paramStr + " " + ex.Message);
                return reval;
            }
            //_logger.LogInformation("PROC_NAME_XML_TO_OBJECT: " + procName + "_" + DateTime.Now.ToString() + ": END");
            //_logger.LogInformation("============================================================");
            return reval;
        }

        #endregion

        #region Convert DataTable to Json
        public string ConvertDataTabletoJson(DataTable dt)
        {
            List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();
            Dictionary<string, object> row;
            foreach (DataRow dr in dt.Rows)
            {
                row = new Dictionary<string, object>();
                foreach (DataColumn col in dt.Columns)
                {
                    row.Add(col.ColumnName, dr[col]);
                }
                rows.Add(row);
            }
            return JsonConvert.SerializeObject(rows);
        }
        #endregion

        #region Convert DataTable to List<T>
        public List<T> ConvertDataTableToList<T>(DataTable dt)
        {
            var objectsList = new List<T>();

            if (dt.Rows.Count == 0) return objectsList;
            //objectsList = ConvertToList<T>(dt);
            //Covert datatable to json string
            string jsonString = JsonConvert.SerializeObject(ParseTableToDictionary(dt));

            //Convert jsonString to List<T>
            InvalidJsonElements = null;
            var array = JArray.Parse(jsonString);

            foreach (var item in array)
            {
                try
                {
                    //Map single json in array to object<T>.
                    var itemMapped = item.ToObject<T>();

                    objectsList.Add(itemMapped);
                }
                catch (Exception ex)
                {
                    InvalidJsonElements = InvalidJsonElements ?? new List<string>();
                    InvalidJsonElements.Add(item.ToString());
                }
            }

            return objectsList;
        }

        //  Converts DataTable To List
        //  DataTable dtTable = GetEmployeeDataTable();
        //  List<Employee> employeeList = dtTable.DataTableToList<Employee>();

        public IList<T> DataTableToList<T>(DataTable table)
        {
            try
            {
                List<T> list = new List<T>();

                foreach (var row in table.AsEnumerable())
                {
                    T obj = Activator.CreateInstance<T>();

                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        try
                        {
                            var value = row[prop.Name];
                            System.Type t = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                            object safeValue = (value == null) ? null : Convert.ChangeType(value, t);
                            PropertyInfo propertyInfo = obj.GetType().GetProperty(prop.Name);
                            propertyInfo.SetValue(obj, safeValue, null);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }

                    list.Add(obj);
                }

                return list;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Convert To List<T>
        public IList<T> ConvertToList<T>(DataTable dt)
        {
            var columnNames = dt.Columns.Cast<DataColumn>()
                .Select(c => c.ColumnName)
                .ToList();
            var properties = typeof(T).GetProperties();
            return dt.AsEnumerable().Select(row =>
            {
                var objT = Activator.CreateInstance<T>();
                foreach (var property in properties)
                {
                    System.Type t = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    if (columnNames.Contains(property.Name))
                    {
                        object safeValue = (row[property.Name] == null) ? null : Convert.ChangeType(row[property.Name], t);
                        if (safeValue == null)
                        {
                            continue;
                        }
                        property.SetValue(objT, safeValue);
                    }
                }
                return objT;
            }).ToList();
        }
        #endregion
        #region Execute proc table and convert to Json
        public async Task<string> ExecuteProcToJson(string procName, List<SqlParameter> lstParam)
        {
            try
            {
                DataTable dt = await ExecuteProcTable(procName, lstParam);
                var json = ConvertDataTabletoJson(dt);
                return json;
            }
            catch (Exception ex)
            {
                string paramStr = "PARAM: ";
                if (lstParam.Count > 0)
                {
                    foreach (var param in lstParam)
                    {
                        paramStr = paramStr + param.ParameterName + ":" + (JsonConvert.SerializeObject(param?.Value ?? ""));
                    }
                }
                _logger.LogError("ERROR ExecuteProcToJson: " + procName + ": " + paramStr + " " + ex.Message);
                return null;
            }

        }
        #endregion

        #region Convert class to SqlParameter
        public SqlParameter CreateSqlParameter(object value, string name)
        {
            var param = new SqlParameter
            {
                ParameterName = @"" + name + "",
                Value = value
            };
            return param;
        }

        public List<SqlParameter> ConvertClassToSqlParameter<T>(T dt)
        {
            var listParam = new List<SqlParameter>();
            try
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                foreach (PropertyInfo pro in properties)
                {
                    Object value = pro.GetValue(dt, null);
                    if (value != null)
                    {
                        listParam.Add(new SqlParameter(@"" + pro.Name + "", value));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("ERROR ConvertClassToSqlParameter Convert T to List<SqlParameter> error: " + ex.Message);
            }

            return listParam;
        }


        public List<SqlParameter> ConvertClassToSqlParameterNull<T>(T dt)
        {
            var listParam = new List<SqlParameter>();
            try
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                foreach (var pro in properties)
                {
                    Object value = pro.GetValue(dt, null);
                    if (value != null)
                        listParam.Add(new SqlParameter(@"" + pro.Name + "", value));
                    else
                        listParam.Add(new SqlParameter(@"" + pro.Name + "", DBNull.Value));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("ERROR ConvertClassToSqlParameterNull Convert T to List<SqlParameter> error: " + ex.Message);
            }

            return listParam;
        }

        public DataTable ToUserDefinedDataTable<T>(T dt)
        {
            var table = new DataTable();
            var values = new List<T>();
            values.Add(dt);
            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            if (values != null)
            {
                foreach (var value in values)
                {
                    if (value != null)
                    {
                        var newRow = table.NewRow();
                        foreach (var prop in properties)
                        {
                            if (table.Columns.Contains(prop.Name))
                                newRow[prop.Name] = prop.GetValue(value, null) ?? DBNull.Value;
                        }
                        table.Rows.Add(newRow);
                    }
                }
            }
            return table;
        }
        public DataTable ConvertToCustomUserDefinedDataTable<T>(IEnumerable<T> values) where T : class
        {
            var table = new DataTable();

            var properties = typeof(T).GetProperties();
            foreach (var prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);

            if (values != null)
            {
                foreach (var value in values)
                {
                    if (value != null)
                    {
                        var newRow = table.NewRow();
                        foreach (var prop in properties)
                        {
                            if (table.Columns.Contains(prop.Name))
                                newRow[prop.Name] = prop.GetValue(value, null) ?? DBNull.Value;
                        }
                        table.Rows.Add(newRow);
                    }
                }
            }
            return table;
        }

        #endregion
    }
}
