using System;
using System.Data.SqlClient;


namespace AtoiHomeServiceLib.Source.Utility
{
    public class DBApi
    {
        public DBApi()
        {
        }
        /// <summary>
        /// Insert uploadimage infomation to database using MSSQL SERVER
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
    }
}
