using System;
using uDicom.Common;
using uDicom.Log.Log4net;
using uDicom.WorkItemService.ShredHost;

namespace WorkItemServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrap bootstrap = new Bootstrap();
            bootstrap.Configure();

            LogManager.GetLog = Log4NetLogManager.GetLogger;

            ShredHost.Start();

            Console.WriteLine("Press any key to exit!");
            Console.Read();

            ShredHost.Stop();

        }
    }
}
