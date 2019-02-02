using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // The code provided will print ‘Hello World’ to the console.
            // Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.
            try
            {
                Console.WriteLine(getRegValue("atoihome", "UploadPath"));
                setRegValue("atoihome", "UploadPath", "d:/uploadfile/");
                Console.WriteLine("Changed uploadpath to {0}", getRegValue("atoihome", "UploadPath"));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            Console.ReadKey();

            // Go to http://aka.ms/dotnet-get-started-console to continue learning how to build a console app! 
        }

        public static string getRegValue (string RegKey, string NameOfValue)
        {
            try
            {
                using (var view32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,
                                        RegistryView.Registry32))
                {
                    using (var clsid32 = view32.OpenSubKey(@"Software\\"+RegKey, false))
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
