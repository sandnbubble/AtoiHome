using log4net;
using log4net.Core;
using log4net.Layout.Pattern;
using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Activation;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]

namespace AtoiHome
{

    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Multiple)]

    class Program
    {
        public static ILog log = LogManager.GetLogger("Program");

        static void Main(string[] args)
        {
            ServiceHostManager TextTransferServiceHostManager = new ServiceHostManager();

            log.Info("Start WCF atoihome service!!");
            try
            {
                TextTransferServiceHostManager.StartService();
                Console.ReadKey();
                TextTransferServiceHostManager.StopService();

            }
            catch (Exception e)
            {
                Program.log.DebugFormat(e.StackTrace);
            }
        }
    }

    /// <summary>
    /// 로그를 color로 출력
    /// </summary>
    public class ColoredMessageConverter : PatternLayoutConverter
    {
        protected override void Convert(TextWriter writer, LoggingEvent loggingEvent)
        {
            string color = "";
            switch (loggingEvent.Level.Name)
            {
                case "DEBUG":
                    color = "green";
                    break;
                case "WARN":
                case "INFO":
                    color = "white";
                    break;
                case "ERROR":
                    color = "pink";
                    break;
                case "FATAL":
                    color = "red";
                    break;
            }
            string logToRender = string.Format(" <p style='color:{0}'>{1}</p>", color, loggingEvent.RenderedMessage);
            //Add logToRender to file
            writer.Write(logToRender);
        }
    }
}

