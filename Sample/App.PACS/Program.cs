using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using App.PACS.Model;

namespace App.PACS
{
    class Program
    {
        static void Main(string[] args)
        {
            MefBootstrap bootstrap = new MefBootstrap();
            bootstrap.Configure();

            DicomServiceManager.Instance.StartService();

            Console.WriteLine("Press any key to exit");
            Console.Read();
        }
    }
}
