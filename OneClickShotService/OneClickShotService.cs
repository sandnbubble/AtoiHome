using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace OneClickShotService
{
    public partial class OneClickShotService : ServiceBase
    {
        public OneClickShotService()
        {
            InitializeComponent();
            eventLogWinService = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("OneClickShotSource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "OneClickShotSource", "OneClickShotLog");
            }
            eventLogWinService.Source = "MySource";
            eventLogWinService.Log = "MyNewLog";
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
