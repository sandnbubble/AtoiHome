using System.ServiceProcess;

namespace AtoiHomeService
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            try
            {
                ServicesToRun = new ServiceBase[]
                {
                new OneClickShotService()
                };
                ServiceBase.Run(ServicesToRun);
            }
            catch (System.Exception ex)
            {
                ;
            }
        }
    }
}
