using Microsoft.Win32;
using System;
using System.Linq;
using System.ServiceProcess;
using System.Windows;

namespace AtoiHomeManager.Source.Utils
{

    public class Utility
    {
        public class TaskArgs
        {
            public string ServiceName { get; set; }
            public int Duration { get; set; }

            public TaskArgs(string ServiceName, int Duration)
            {
                this.ServiceName = ServiceName;
                this.Duration = Duration;
            }
        }

        public static Rect GetWorkingArea()
        {
            System.Windows.Forms.Screen secondaryScreen = System.Windows.Forms.Screen.AllScreens.Where(s => !s.Primary).FirstOrDefault();

            if (secondaryScreen == null)
            {
                return new Rect(0, 0, (int)SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
            }
            else
            {
                int left = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => !x.Primary).WorkingArea.Left;
                int top = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => !x.Primary).WorkingArea.Top;
                int width = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => !x.Primary).WorkingArea.Width;
                int height = System.Windows.Forms.Screen.AllScreens.FirstOrDefault(x => !x.Primary).WorkingArea.Height;
                Rect WorkingArea = new Rect(left, top, width, height);
                return WorkingArea;
            }
        }

        public static bool GetServiceStatus(string strServicename)
        {
            try
            {
                ServiceController[] services = ServiceController.GetServices();
                foreach (ServiceController service in services)
                {
                    if (service.ServiceName == strServicename)
                    {
                        switch (service.Status)
                        {
                            case ServiceControllerStatus.Running:
                                return true;
                            default:
                                return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }

        public static void StartService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void StopService(string serviceName, int timeoutMilliseconds)
        {
            ServiceController service = new ServiceController(serviceName);
            try
            {
                TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void RestartService(TaskArgs args)
        {
            ServiceController service = new ServiceController(args.ServiceName);
            try
            {
                int millisec1 = Environment.TickCount;
                TimeSpan timeout = TimeSpan.FromMilliseconds(args.Duration);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

                // count the rest of the timeout
                int millisec2 = Environment.TickCount;
                timeout = TimeSpan.FromMilliseconds(args.Duration - (millisec2 - millisec1));

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
