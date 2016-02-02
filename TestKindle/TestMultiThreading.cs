using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KindleSpider.Common;

namespace TestKindle
{
    public class TestMultiThreading
    {
        public static void Test()
        {
            MultiThreadingWorker thWork = new MultiThreadingWorker();
            thWork.threadCount = 20;
            thWork.workContent = new WorkContent(WorkFun);
            for (int i = 0; i < 100; i++)
                thWork.AddWork(i);
            thWork.Start();
        }

        private static void WorkFun(object obj)
        {
            Console.WriteLine("执行内容：" + obj.ToString());
        }
    }
}
