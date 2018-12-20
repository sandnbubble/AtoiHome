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


    public partial class TextTransferService: ServiceBase
    {
        public static ILog log = LogManager.GetLogger(typeof(TextTransferService));

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);

        ServiceHostManager TextTransferServiceHostManager = new ServiceHostManager();

        public TextTransferService()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("MySource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "TrextTransferService", "DebugLog");
            }
            eventLog1.Source = "TrextTransferService";
            eventLog1.Log = "DebugLog";
        }

        protected override void OnStart(string[] args)
        {
            log.Info("Started - AtoiHomeService");
            eventLog1.WriteEntry("In OnStart");
            try
            {
                // Update the service state to Start Pending.
                ServiceStatus serviceStatus = new ServiceStatus();
                serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
                serviceStatus.dwWaitHint = 100000;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);

                TextTransferServiceHostManager.StartService();

                // Update the service state to Running.
                serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
                SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            }
            catch (Exception e)
            {
                TextTransferServiceHostManager.StopService();
                eventLog1.WriteEntry(e.Message);
                log.Info("Error :" + e.Message);
            }
        }

        protected override void OnStop()
        {
            log.Info("Stoped - AtoiHomeService");
            eventLog1.WriteEntry("In OnStop");

            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOP_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            TextTransferServiceHostManager.StopService();
            // Update the service state to Running.

            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }
    }
}
