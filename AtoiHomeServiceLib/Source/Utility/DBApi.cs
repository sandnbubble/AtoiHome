using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#if _EXTERNAL_DB
using MySql.Data.MySqlClient;
#else

#endif

namespace AtoiHomeServiceLib.Source.Utility
{
    public class DBApi
    {
        public DBApi()
        {
        }

        #region _EXTERNAL_DB
#if _EXTERNAL_MARIADB
        static MySqlConnection connection;
        static MySqlCommand command;


        public static bool Connect(string strConnection)
        {
            try
            {
                connection = new MySqlConnection(strConnection);
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool ValidateUser(string strID, string strPassword)
        {
            bool bRet = false;
            try
            {
                command = connection.CreateCommand();
                string strQuery = string.Format("select userid from userinfo where userid='{0}' and password='{1}'", strID, strPassword);
                command.CommandText = strQuery;
                connection.Open();
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();

                if (Reader.Read())
                {
                    bRet = true;
                }
                connection.Close();
                return bRet;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static string Query(string strQuery)
        {
            try
            {
                command = connection.CreateCommand();
                command.CommandText = strQuery;
                connection.Open();
                MySqlDataReader Reader;
                Reader = command.ExecuteReader();
                StringBuilder sb = new StringBuilder();
                while (Reader.Read())
                {
                    string thisrow = "";
                    for (int i = 0; i < Reader.FieldCount; i++)
                        thisrow += Reader.GetValue(i).ToString();
                    sb.AppendLine(thisrow);
                }
                connection.Close();
                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
#endif
        #endregion
#if _EXTERNAL_MSSQLDB
        /// <summary>
        /// Insert uploadimage infomation to database
        /// </summary>
        /// <param name="strQuery"></param>
        /// <returns></returns>
        public static bool ValidateUser(string strEmail, string strPassword)
        {
            try
            {
                SqlConnection myConnection = new SqlConnection("Data Source=ATOI\\ATOIHOMEDBSERVER; Persist Security Info = False; User ID = sa; Password = gksrmf; Initial Catalog = AtoiHomeWeb");
                SqlCommand myCommand = myConnection.CreateCommand();
                string strQuery = string.Format("select email from AspNetUsers where email='{0}'", strEmail);
                myCommand.CommandText = strQuery;
                myConnection.Open();
                SqlDataReader Reader = myCommand.ExecuteReader();
                bool bRet = false;
                if (Reader.Read())
                {
                    bRet = true;
                }
                else
                    bRet = false;
                Reader.Close();
                myConnection.Close();
                return bRet;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
#endif
    }
}
