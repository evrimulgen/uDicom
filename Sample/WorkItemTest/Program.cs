using System;
using uDicom.Common;
using uDicom.Log.Log4net;
using uDicom.WorkItemService;
using uDicom.WorkItemService.Common;
using uDicom.WorkItemService.Interface;
using WorkItemTest.DicomSend;
using WorkItemTest.Model;

namespace WorkItemTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Bootstrap bootstrap = new Bootstrap();
            bootstrap.Configure();

            LogManager.GetLog = Log4NetLogManager.GetLogger;

            using (var ctx = new WorkItemContext())
            {
                ctx.Database.CreateIfNotExists();

                for (int i = 0; i < 100; i++)
                {
                    var item = new WorkItem
                    {
                        Priority = WorkItemPriorityEnum.High,
                        ScheduledTime = DateTime.Now.AddSeconds(i * 1),
                        Status = WorkItemStatusEnum.Pending,
                        Type = "DicomSend",
                        Request = new DicomSendRequest(), 
                        Progress = new DicomSendProgress()
                        
                    };
                    ctx.WorkItems.Add(item);
                }

                ctx.SaveChanges();
            }

            Console.WriteLine("Start WorkItem Processor!");

            WorkItemProcessor.CreateProcessor(5, 5, "workItem");
            WorkItemProcessor.Instance.Run();

            Console.WriteLine("Press any key to exit!");
            Console.Read();
        }
    }
}
