using System;
using System.Collections.Generic;
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
#if _EXTERNAL_DB
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
    }
}
