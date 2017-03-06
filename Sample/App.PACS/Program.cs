using System;
using uDicom.Common;
using uDicom.Log.Log4net;

namespace App.PACS
{
    class Program
    {
        static void Main(string[] args)
        {
            MefBootstrap bootstrap = new MefBootstrap();
            bootstrap.Configure();

            LogManager.GetLog = Log4NetLogManager.GetLogger;

            DicomServiceManager.Instance.StartService();

            Console.WriteLine("Press any key to exit");
            Console.Read();
        }
    }
}
