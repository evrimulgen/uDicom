using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using App.PACS.Model;
using uDicom.Log.Log4net;
using UIH.Dicom.Log;

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
