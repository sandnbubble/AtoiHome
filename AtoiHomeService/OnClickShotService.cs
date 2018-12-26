using System;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using AtoiHome;
using log4net;
using log4net.Core;
using log4net.Layout.Pattern;

namespace AtoiHomeService
{
    public enum ServiceState
    {
        SERVICE_STOPPED = 0x00000001,
        SERVICE_START_PENDING = 0x00000002,
        SERVICE_STOP_PENDING = 0x00000003,
        SERVICE_RUNNING = 0x00000004,
        SERVICE_CONTINUE_PENDING = 0x00000005,
        SERVICE_PAUSE_PENDING = 0x00000006,
        SERVICE_PAUSED = 0x00000007,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ServiceStatus
    {
        public int dwServiceType;
        public ServiceState dwCurrentState;
        public int dwControlsAccepted;
        public int dwWin32ExitCode;
        public int dwServiceSpecificExitCode;
        public int dwCheckPoint;
        public int dwWaitHint;
    };


    public partial class OneClickShotService : ServiceBase
    {
        public static ILog log = LogManager.GetLogger(typeof(OneClickShotService));

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        ServiceHostManager OneClickShotServiceHostManager = new ServiceHostManager();

        public OneClickShotService()
        {
            try
            {
                InitializeComponent();

                eventLogForWin = new System.Diagnostics.EventLog();
                if (!System.Diagnostics.EventLog.SourceExists("AtoiHomeServiceSource"))
                {
                    System.Diagnostics.EventLog.CreateEventSource(
                        "AtoiHomeServiceSource", "AtoiHomeServiceLog");
                }
                eventLogForWin.Source = "AtoiHomeServiceSource";
                eventLogForWin.Log = "AtoiHomeServiceLog";
            }
            catch (Exception ex)
            {
                log.DebugFormat(ex.Message.ToString());
            }
        }

        protected override void OnStart(string[] args)
        {
            log.Info("Started - AtoiHomeService");
            eventLogForWin.WriteEntry("In OnStart");
            try
            {
                // Update the service state to Start Pending.
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
                serviceStatus.dwWaitHint = 100000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                OneClickShotServiceHostManager.StartService();

                // Update the service state to Running.
                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception e)
            {
                OneClickShotServiceHostManager.StopService();
                eventLogForWin.WriteEntry(e.Message);
                log.Info("Error :" + e.Message);
            }
        }

        protected override void OnStop()
        {
            log.Info("Stoped - AtoiHomeService");
            eventLogForWin.WriteEntry("In OnStop");

            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            OneClickShotServiceHostManager.StopService();
            // Update the service state to Running.

            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }
    }
}
