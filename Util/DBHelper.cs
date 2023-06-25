using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
 

namespace WDKisStockToZKH.Util
{
    public class DBHelper
    {
        //正式、测试



        //tring mcon = "Data Source=127.0.0.1;Initial Catalog=AIS20190517131855;uid=sa;pwd=!QAZxsw2;";

        //定义这个类要使用的全局变量
        private SqlConnection conn;
        private SqlCommand cmd;
        private SqlDataReader dr;
        private SqlDataAdapter adapter;


        /// <summary>
        /// 当前库
        /// </summary>
        public DBHelper()
        {
            //正式
            //2022-11-4修改ip地址
            string constr = "Data Source=.;Initial Catalog=AIS20170120225638;User ID=sa;Password=123456; Integrated Security=False;Persist Security Info=False;Max Pool Size=1024";
            //string constr = "Data Source=.;Initial Catalog=ais_k3cloud_v7_wyindustry_20230406_45d344;User ID=sa;Password=123456; Integrated Security=False;Persist Security Info=False;Max Pool Size=1024";
            //string constr = "Data Source=192.168.31.151;Initial Catalog=AIS20230407100933;User ID=sa;Password=123abc!; Integrated Security=False;Persist Security Info=False;Max Pool Size=1024";


            //string constr = "Data Source=.;Initial Catalog=AIS20211215152445;User ID=sa;Password=123456; Integrated Security=False;Persist Security Info=False;Max Pool Size=1024";
            //string constr = "Data Source=192.168.1.200;Initial Catalog=AIS20211215152445;User ID=sa;Password=123abc!; Integrated Security=False;Persist Security Info=False;Max Pool Size=1024";
            //string constr = "Data Source=127.0.0.1;Initial Catalog=AIS20220215142129;User ID=sa;Password=123abc!; Integrated Security=False;Persist Security Info=False;Max Pool Size=1024";

            //string constr = "Data Source=127.0.0.1;Initial Catalog=AIS20210927133850;User ID=sa;Password=123456789; Integrated Security=False;Persist Security Info=False;Max Pool Size=1024";
            //测试
            //string constr = "Data Source=127.0.0.1;Initial Catalog=AIS20210923070720;User ID=sa;Password=!QAZxsw2; Integrated Security=False;Persist Security Info=False;Max Pool Size=1024";


            conn = new SqlConnection(constr);

        }
        /// <summary>
        /// 第二数据库
        /// </summary> 
        public DBHelper(int zc)
        {
            string constr = "Data Source=127.0.0.1;Initial Catalog=AIS20190517131855;User ID=sa;Password=!QAZxsw2; Integrated Security=False;Persist Security Info=False;Max Pool Size=1024";

            conn = new SqlConnection(constr);

        }


        public SqlConnection GetConn()
        {
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            return conn;
        }

        /// <summary>
        /// 创建一个 SQL 参数，主要实现SqlParameter[] 参数列表
        /// </summary>
        /// <param name="parameterName">参数名</param>
        /// <param name="dbType">类型</param>
        /// <param name="value">值</param>
        /// <returns>返回创建完成的参数</returns>
        public SqlParameter CreateParameter(string parameterName, SqlDbType dbType, object value)
        {
            parameterName = parameterName.Replace("'", "").Replace(";", "").Replace("--", "");
            SqlParameter result = new SqlParameter(parameterName, dbType);
            result.Value = value;
            return result;
        }

        /// <summary>
        /// 单向操作，主要用于（增加，删除，修改）,返回受影响的行数
        /// </summary>
        /// <param name="cmdTxt">安全的sql语句（string.format）</param>
        /// <returns></returns>
        public int GetExcuteNonQuery(string cmdTxt, out string errormsg)
        {
            string outmsg = "OK";
            int result = GetExcuteNonQuery(cmdTxt, out outmsg, null);

            errormsg = outmsg;
            return result;


        }
        /// <summary>
        /// 带参数化的　主要用于（增加，删除，修改）,返回受影响的行数
        /// </summary>
        /// <param name="cmdTxt">带参数列表的sql语句</param>
        /// <param name="pars">要传入的参数列表</param>
        /// <returns></returns>
        public int GetExcuteNonQuery(string cmdTxt, out string errormsg, params SqlParameter[] pars)
        {
            int result = 0;
            try
            {
                cmd = new SqlCommand(cmdTxt, GetConn());
                if (pars != null)
                {
                    if (pars.Length > 0)
                    {
                        for (int i = 0; i < pars.Length; i++)
                        {

                            if (pars[i].Value != null)
                            {

                                if (pars[i].Direction.ToString() != "Output")
                                {
                                    if (pars[i].SqlDbType == SqlDbType.NVarChar || pars[i].SqlDbType == SqlDbType.VarChar || pars[i].SqlDbType == SqlDbType.Text)
                                    {
                                        if (pars[i].Value.ToString() != "")
                                        {
                                            if (pars[i].Value.ToString().IndexOf("'") > -1 || pars[i].Value.ToString().IndexOf(";") > -1 || pars[i].Value.ToString().IndexOf("--") > -1)
                                            {
                                                pars[i].Value = pars[i].Value.ToString().Replace("'", " ").Replace(";", " ").Replace("--", " ");
                                            }
                                        }
                                    }
                                }

                            }

                        }
                    }
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(pars);
                }
                errormsg = "OK";
                result = cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                
                conn.Close();
                errormsg = ex.ToString();
                WriteLog(cmdTxt, "", ex.ToString(), pars);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return result;
        }

        /// <summary>
        /// 对连接执行 Transact-SQL 语句或者存储过程并返回受影响的行数
        /// </summary>
        /// <param name="cmdText">SQL 语句或者存储过程名称</param>
        /// <param name="pars">参数</param>
        /// <returns>受影响的行数</returns>
        public int GetExcuteNonQuery(string cmdTxt, CommandType cmdtype, out string errormsg, params SqlParameter[] pars)
        {
            int result = 0;

            try
            {
                cmd = new SqlCommand(cmdTxt, GetConn());
                cmd.CommandType = cmdtype;
                if (pars != null)
                {
                    if (pars.Length > 0)
                    {
                        for (int i = 0; i < pars.Length; i++)
                        {

                            if (pars[i].Value != null)
                            {

                                if (pars[i].Direction.ToString() != "Output")
                                {

                                    if (pars[i].SqlDbType == SqlDbType.NVarChar || pars[i].SqlDbType == SqlDbType.VarChar || pars[i].SqlDbType == SqlDbType.Text)
                                    {
                                        if (pars[i].Value.ToString() != "")
                                        {
                                            if (pars[i].Value.ToString().IndexOf("'") > -1 || pars[i].Value.ToString().IndexOf(";") > -1 || pars[i].Value.ToString().IndexOf("--") > -1)
                                            {
                                                pars[i].Value = pars[i].Value.ToString().Replace("'", " ").Replace(";", " ").Replace("--", " ");
                                            }
                                        }
                                    }
                                }

                            }

                        }
                    }
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(pars);
                }

                errormsg = "OK";
                result = cmd.ExecuteNonQuery();

            }
            catch (Exception ex)
            {
                conn.Close();
                WriteLog(cmdTxt, cmdtype.ToString(), ex.ToString(), pars);
                errormsg = ex.ToString();
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return result;

        }
        /// <summary>
        /// 用于执行储过程（返回单表数据）
        /// </summary>
        /// <param name="procname">存储过程名称</param>
        /// <param name="plist"></param>
        /// <returns>DataTable</returns>
        public DataTable ExecProcDataTable(string procname, List<SqlParameter> plist)
        {
            cmd = new SqlCommand(procname, GetConn());
            //设置类型为存储过程
            cmd.CommandType = CommandType.StoredProcedure;

            if (plist != null && plist.Count != 0)
            {
                cmd.Parameters.AddRange(plist.ToArray());
            }
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable();

            cmd.ExecuteNonQuery();
            return dt;

        }

        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        /// <typeparam name="T">返回的类型</typeparam>
        /// <param name="cmdText">SQL 语句</param>
        /// <param name="pars">参数列表</param>
        /// <returns>结果集中第一行的第一列或空引用</returns>
        public T ExecuteScalar<T>(string cmdText, params SqlParameter[] pars)
        {
            using (SqlCommand cmd = new SqlCommand(cmdText, GetConn()))
            {
                if (pars != null)
                {
                    if (pars.Length > 0)
                    {
                        for (int i = 0; i < pars.Length; i++)
                        {

                            if (pars[i].Value != null)
                            {

                                if (pars[i].Direction.ToString() != "Output")
                                {

                                    if (pars[i].SqlDbType == SqlDbType.NVarChar || pars[i].SqlDbType == SqlDbType.VarChar || pars[i].SqlDbType == SqlDbType.Text)
                                    {
                                        if (pars[i].Value.ToString() != "")
                                        {
                                            if (pars[i].Value.ToString().IndexOf("'") > -1 || pars[i].Value.ToString().IndexOf(";") > -1 || pars[i].Value.ToString().IndexOf("--") > -1)
                                            {
                                                pars[i].Value = pars[i].Value.ToString().Replace("'", " ").Replace(";", " ").Replace("--", " ");
                                            }
                                        }
                                    }
                                }

                            }

                        }
                    }
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(pars);
                }
                T result = (T)cmd.ExecuteScalar();
                conn.Close();
                return result;
            }
        }
        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        /// <typeparam name="T">参数类型[范型]</typeparam>
        /// <param name="cmdText">sql语句</param>
        /// <returns>返回T类型</returns>
        public T ExecuteScalar<T>(string cmdText)
        {
            return ExecuteScalar<T>(cmdText, null);
        }
        /// <summary>
        /// 执行查询，并返回查询所返回的结果集中第一行的第一列。忽略其他列或行。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmdTxt">SQL 语句或者存储过程名称</param>
        /// <param name="cmdtype">决定是存储过程还是sql语句</param>
        /// <param name="pars">参数列表</param>
        /// <returns>返回T类型</returns>
        public T ExecuteScalar<T>(string cmdTxt, CommandType cmdtype, params SqlParameter[] pars)
        {
            using (cmd = new SqlCommand(cmdTxt, GetConn()))
            {
                cmd.CommandType = cmdtype;
                if (pars != null)
                {
                    if (pars.Length > 0)
                    {
                        for (int i = 0; i < pars.Length; i++)
                        {

                            if (pars[i].Value != null)
                            {

                                if (pars[i].Direction.ToString() != "Output")
                                {

                                    if (pars[i].SqlDbType == SqlDbType.NVarChar || pars[i].SqlDbType == SqlDbType.VarChar || pars[i].SqlDbType == SqlDbType.Text)
                                    {
                                        if (pars[i].Value.ToString() != "")
                                        {
                                            if (pars[i].Value.ToString().IndexOf("'") > -1 || pars[i].Value.ToString().IndexOf(";") > -1 || pars[i].Value.ToString().IndexOf("--") > -1)
                                            {
                                                pars[i].Value = pars[i].Value.ToString().Replace("'", " ").Replace(";", " ").Replace("--", " ");
                                            }
                                        }
                                    }
                                }
                            }

                        }
                    }
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(pars);
                }
                T result = (T)cmd.ExecuteScalar();
                conn.Close();

                return result;
            }

        }
        /// <summary>
        /// 将 cmdText 发送到 System.Data.SqlClient.SqlCommand.Connection，并使用 System.Data.CommandBehavior 值之一生成一个 DataReader
        /// </summary>
        /// <param name="cmdTxt">安全的sql语句（string.format）</param>
        /// <returns>一个 DataReader 对象</returns>
        public SqlDataReader GetDataReader(string cmdTxt)
        {
            return GetDataReader(cmdTxt, null);
        }
        /// <summary>
        /// 将 cmdText 发送到 System.Data.SqlClient.SqlCommand.Connection，并使用 System.Data.CommandBehavior 值之一生成一个 DataReader
        /// </summary>
        /// <param name="cmdTxt">安全的sql语句（string.format）</param>
        /// <param name="pars">参数</param>
        /// <returns>一个 DataReader 对象</returns>
        public SqlDataReader GetDataReader(string cmdTxt, params SqlParameter[] pars)
        {
            using (cmd = new SqlCommand(cmdTxt, GetConn()))
            {
                if (pars != null)
                {
                    if (pars.Length > 0)
                    {
                        for (int i = 0; i < pars.Length; i++)
                        {

                            if (pars[i].Value != null)
                            {

                                if (pars[i].Direction.ToString() != "Output")
                                {

                                    if (pars[i].SqlDbType == SqlDbType.NVarChar || pars[i].SqlDbType == SqlDbType.VarChar || pars[i].SqlDbType == SqlDbType.Text)
                                    {
                                        if (pars[i].Value.ToString() != "")
                                        {
                                            if (pars[i].Value.ToString().IndexOf("'") > -1 || pars[i].Value.ToString().IndexOf(";") > -1 || pars[i].Value.ToString().IndexOf("--") > -1)
                                            {
                                                pars[i].Value = pars[i].Value.ToString().Replace("'", " ").Replace(";", " ").Replace("--", " ");
                                            }
                                        }
                                    }
                                }

                            }

                        }
                    }
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(pars);
                }
                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return dr;
            }
        }
        /// <summary>
        ///  将 cmdText 发送到 System.Data.SqlClient.SqlCommand.Connection，并使用 System.Data.CommandBehavior 值之一生成一个 DataReader
        /// </summary>
        /// <param name="cmdTxt">存储过程名称或者sql语句</param>
        /// <param name="cmdtype">决定是存储过程类型还是sql语句</param>
        /// <param name="pars">参数列表</param>
        /// <returns>返回一个DataReader对象</returns>
        public SqlDataReader GetDataReader(string cmdTxt, CommandType cmdtype, params SqlParameter[] pars)
        {
            using (cmd = new SqlCommand(cmdTxt, GetConn()))
            {
                cmd.CommandType = cmdtype;
                if (pars != null)
                {
                    if (pars.Length > 0)
                    {
                        for (int i = 0; i < pars.Length; i++)
                        {

                            if (pars[i].Value != null)
                            {

                                if (pars[i].Direction.ToString() != "Output")
                                {

                                    if (pars[i].SqlDbType == SqlDbType.NVarChar || pars[i].SqlDbType == SqlDbType.VarChar || pars[i].SqlDbType == SqlDbType.Text)
                                    {
                                        if (pars[i].Value.ToString() != "")
                                        {
                                            if (pars[i].Value.ToString().IndexOf("'") > -1 || pars[i].Value.ToString().IndexOf(";") > -1 || pars[i].Value.ToString().IndexOf("--") > -1)
                                            {
                                                pars[i].Value = pars[i].Value.ToString().Replace("'", " ").Replace(";", " ").Replace("--", " ");
                                            }
                                        }
                                    }
                                }

                            }

                        }
                    }
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(pars);
                }
                dr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                return dr;
            }

        }
        /// <summary>
        /// 做数据绑定显示作用，一般绑定的是数据查看控件
        /// </summary>
        /// <param name="cmdTxt">sql语句</param>
        /// <param name="tableName">要绑定显示的具体表名</param>
        /// <returns>返回一个数据表</returns>
        public DataTable GetFillData(string cmdTxt)
        {
            return GetFillData(cmdTxt, null);
        }
        /// <summary>
        /// 序列化
        /// </summary>
        /// <param name="cmdTxt"></param>
        /// <returns></returns>
        public T GetFillData<T>(string cmdTxt)
        {
            
            DataTable dataTable = new DataTable();
         
            
            dataTable = GetFillData(cmdTxt, null);

            
            string json = "";
            try
            {

                json = JsonConvert.SerializeObject(dataTable);
                LogHelper.Info(json);
            }
            catch (Exception ex)
            {
                LogHelper.Error($"{ex.Message}",new Exception("序列化出问题"));
                
            }
            try
            {
                return JsonConvert.DeserializeObject<T>(json);

            }
            catch (Exception ex)
            {
                LogHelper.Error($"{ex.Message}", new Exception("序列化出问题"));

            }
            return JsonConvert.DeserializeObject<T>(json);



        }
        /// <summary>
        /// 做数据绑定显示作用，一般绑定的是数据查看控件
        /// </summary>
        /// <param name="cmdTxt">带参数的sql语句</param>
        /// <param name="pars">参数列表</param>
        /// <returns>返回是一个数据表</returns>


        public DataSet GetFillDataDouble(string cmdTxt)
        {
            DataSet ds = new DataSet();
            DataTable table = new DataTable();

            try
            {
                cmd = new SqlCommand(cmdTxt, GetConn());

                using (adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(ds);
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                WriteLog(cmdTxt, "", ex.ToString());
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {

                    conn.Close();
                }
            }
            return ds;
        }



        public DataTable GetFillData(string cmdTxt, params SqlParameter[] pars)
        {
            DataSet ds = new DataSet();
            DataTable table = new DataTable();
           
            try
            {
                cmd = new SqlCommand(cmdTxt, GetConn());
              
                if (pars != null)
                {
                    if (pars.Length > 0)
                    {
                        for (int i = 0; i < pars.Length; i++)
                        {

                            if (pars[i].Value != null)
                            {

                                if (pars[i].Direction.ToString() != "Output")
                                {

                                    if (pars[i].SqlDbType == SqlDbType.NVarChar || pars[i].SqlDbType == SqlDbType.VarChar || pars[i].SqlDbType == SqlDbType.Text)
                                    {
                                        if (pars[i].Value.ToString() != "")
                                        {
                                            if (pars[i].Value.ToString().IndexOf("'") > -1 || pars[i].Value.ToString().IndexOf(";") > -1 || pars[i].Value.ToString().IndexOf("--") > -1)
                                            {
                                                pars[i].Value = pars[i].Value.ToString().Replace("'", " ").Replace(";", " ").Replace("--", " ");
                                            }
                                        }
                                    }
                                }

                            }

                        }
                    }
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(pars);
                    cmd.CommandTimeout = 90;
                }
               
                using (adapter = new SqlDataAdapter(cmd))
                {
                    
                    adapter.Fill(ds);
                    table = ds.Tables[0];
                   
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                LogHelper.Error($"{ex.Message},{cmdTxt}", new Exception("查询SQL有问题"));
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {

                    conn.Close();
                }
            }
         
            return table;
        }
        /// <summary>
        /// 做数据绑定显示作用，一般绑定的是数据查看控件
        /// </summary>
        /// <param name="cmdTxt">存储过程名称或者sql语句</param>
        /// <param name="cmdtype">决定是存储过程类型还是sql语句</param>
        /// <param name="pars">参数列表</param>
        /// <returns>返回一个DataTable</returns>
        public DataTable GetFillData(string cmdTxt, CommandType cmdtype, params SqlParameter[] pars)
        {
            DataSet ds = new DataSet();
            DataTable table = new DataTable();
            try
            {
                cmd = new SqlCommand(cmdTxt, GetConn());
                cmd.CommandType = cmdtype;
                if (pars != null)
                {
                    if (pars.Length > 0)
                    {
                        for (int i = 0; i < pars.Length; i++)
                        {

                            if (pars[i].Value != null)
                            {

                                if (pars[i].Direction.ToString() != "Output")
                                {

                                    if (pars[i].SqlDbType == SqlDbType.NVarChar || pars[i].SqlDbType == SqlDbType.VarChar || pars[i].SqlDbType == SqlDbType.Text)
                                    {
                                        if (pars[i].Value.ToString() != "")
                                        {
                                            if (pars[i].Value.ToString().IndexOf("'") > -1 || pars[i].Value.ToString().IndexOf(";") > -1 || pars[i].Value.ToString().IndexOf("--") > -1)
                                            {
                                                pars[i].Value = pars[i].Value.ToString().Replace("'", " ").Replace(";", " ").Replace("--", " ");
                                            }
                                        }
                                    }
                                }

                            }

                        }
                    }
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(pars);
                    cmd.CommandTimeout = 90;
                }
                using (adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(ds);
                    table = ds.Tables[0];
                }
            }
            catch (Exception ex)
            {
                conn.Close();
                WriteLog(cmdTxt, cmdtype.ToString(), ex.ToString(), pars);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return table;
        }

        /// <summary>
        /// 分页数据绑定显示
        /// </summary>
        /// <param name="cmdTxt">string.format格式化sql语句,格式如:"select top {0} * from books where typeid not in (select top {1} id from books order by typeid) order by typeid"总记录数 TotalRecordCount总记录数通过executescalar获取</param>
        /// <param name="pageSize">设置的分页数大小,默认为10</param>
        /// <param name="currentIndex">当前页的索引,通常是通过querystring获取.如:string currentIndex = Request.QueryString["id"] ?? "1"</param>
        /// <returns>返回当前页的数据显示</returns>
        public DataTable GetFillData(string cmdTxt, int pageSize, int currentIndex)
        {
            DataTable dt = new DataTable();
            using (adapter = new SqlDataAdapter(string.Format(cmdTxt, pageSize, pageSize * (currentIndex - 1)), GetConn()))
            {
                adapter.Fill(dt);
            }
            return dt;
        }


        /// <summary>
        /// 对连接执行 Transact-SQL 语句或者存储过程并返回受影响的行数
        /// </summary>
        /// <param name="cmdText">SQL 语句或者存储过程名称</param>
        /// <param name="pars">参数</param>
        /// <returns>受影响的行数</returns>
        public int GetExcuteNonQueryAnother(string cmdTxt, CommandType cmdtype, params SqlParameter[] pars)
        {
            SqlConnection conn = new SqlConnection("Data Source=10.4.253.15;Initial Catalog=AirSystem;User ID=sa;Password=yinling4210A; Integrated Security=False;Persist Security Info=False;Max Pool Size=3000");
            int result = 0;
            try
            {
                if (conn.State == ConnectionState.Closed)
                {
                    conn.Open();
                }
                SqlCommand cmd = new SqlCommand(cmdTxt, conn);
                cmd.CommandType = cmdtype;
                if (pars != null)
                {
                    if (pars.Length > 0)
                    {
                        for (int i = 0; i < pars.Length; i++)
                        {

                            if (pars[i].Value != null)
                            {

                                if (pars[i].Direction.ToString() != "Output")
                                {

                                    if (pars[i].SqlDbType == SqlDbType.NVarChar || pars[i].SqlDbType == SqlDbType.VarChar || pars[i].SqlDbType == SqlDbType.Text)
                                    {
                                        if (pars[i].Value.ToString() != "")
                                        {
                                            if (pars[i].Value.ToString().IndexOf("'") > -1 || pars[i].Value.ToString().IndexOf(";") > -1 || pars[i].Value.ToString().IndexOf("--") > -1)
                                            {
                                                pars[i].Value = pars[i].Value.ToString().Replace("'", " ").Replace(";", " ").Replace("--", " ");
                                            }
                                        }
                                    }
                                }

                            }

                        }
                    }
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddRange(pars);
                }
                result = cmd.ExecuteNonQuery();
                conn.Close();
            }
            catch (Exception ex)
            {
                conn.Close();
                WriteLog(cmdTxt, cmdtype.ToString(), ex.ToString(), pars);
            }
            finally
            {
                conn.Close();
            }
            return result;
        }
        /// 2015-05-14 陈博 同条件下 查数据 和 返回结果集条数
        /// <summary>
        ///  同条件下 查数据 和 返回结果集条数
        /// </summary>
        /// <param name="cmdTxt">存储过程名称或者sql语句</param>
        /// <param name="cmdtype">决定是存储过程类型还是sql语句</param>
        /// <param name="pars">参数列表</param>
        /// <returns>返回一个DataSet</returns>
        public DataSet GetFillDataSet(string cmdTxt, CommandType cmdtype, params SqlParameter[] pars)
        {
            DataSet ds = new DataSet();

            try
            {
                cmd = new SqlCommand(cmdTxt, GetConn());
                cmd.CommandType = cmdtype;
                if (pars != null)
                    cmd.Parameters.AddRange(pars);
                cmd.CommandTimeout = 90;
                using (adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(ds);

                }
            }
            catch (Exception ex)
            {
                conn.Close();
                WriteLog(cmdTxt, cmdtype.ToString(), ex.ToString(), pars);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return ds;
        }

        public void WriteLog(string cmdTxt, string cmdtype, string errmes, params SqlParameter[] pars)
        {
            string mess = string.Empty;
            mess += "cmdTxt:   " + cmdTxt + "\r\n";
            mess += "cmdtype:   " + cmdtype + "\r\n";
            mess += "params:   \r\n";
            if (pars != null)
            {
                for (int i = 0; i < pars.Length; i++)
                {
                    if (pars[i].Direction.ToString() != "Output")
                    {
                        mess += pars[i].ParameterName.ToString() + ":" + pars[i].Value.ToString() + "\r\n";
                    }
                }
            }
            mess += "\r\n";
            mess += "ERROR MES:   " + errmes + " \r\n\r\n=======================\r\n\r\n";
            StreamWriter sw = null;
            try
            {
                //string logpath = AppDomain.CurrentDomain.BaseDirectory + "\\LOG\\";
                string logpath = @"D:\MyProjcetLog\";
                //string logpath = @"E:\logs\";
                string time25 = DateTime.Now.ToString("yyyy年MM月dd日 hh时mm分ss秒ffffff毫秒");
                sw = new StreamWriter(logpath + "DBHelper_Error_" + DateTime.Now.ToString("yyyyMMdd") + ".txt", true, Encoding.Default);
                sw.WriteLine(time25 + " " + mess);
                sw.Close();
            }
            catch (Exception ex)
            {
                if (sw != null) { sw.Close(); sw.Dispose(); }
                throw ex;
            }
            finally
            {
                sw.Close(); sw.Dispose();
            }
        }
    }
}
