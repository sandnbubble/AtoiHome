using Microsoft.Win32;
using System;

namespace AtoiHomeServiceLib.Source.Utility
{
    public class Utils
    {
        /// <summary>
        /// 레지스트리의 WOW6432Note에서 파일업로드 폴더 이름을 얻어온다.
        /// 64비트 코드로 변경해야함.
        /// </summary>
        /// <param name="strKey"></param>
        /// <returns></returns>
        public static string getRegValue(string RegKey, string NameOfValue)
        {
            try
            {
                using (var view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                                        RegistryView.Registry32))
                {
                    using (var clsid32 = view32.OpenSubKey(@"Software\\" + RegKey, false))
                    {
                        foreach (string name in clsid32.GetValueNames())
                        {
                            if (name.Equals(NameOfValue))
                                return clsid32.GetValue(name).ToString();
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static bool setRegValue(string RegKey, string NameOfValue, string Value)
        {
            try
            {
                using (var view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                                        RegistryView.Registry32))
                {
                    using (var clsid32 = view32.OpenSubKey(@"Software\\" + RegKey, true))
                    {
                        foreach (string name in clsid32.GetValueNames())
                        {
                            if (name.Equals(NameOfValue))
                            {
                                clsid32.SetValue(NameOfValue, Value);
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
