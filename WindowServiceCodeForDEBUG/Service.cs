using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace WindowServiceCodeForDEBUG
{
    class Service : ServiceBase
    {

        protected override void OnStart(string[] args)
        {
            Console.WriteLine(DateTime.Now);
            Thread lotMonitor = new Thread(Program.LotMonitor);
            Thread messageMonitor = new Thread(Program.SendMessages);

            lotMonitor.Start();
            messageMonitor.Start();
        }

        protected override void OnStop()
        {
            base.OnStop();
        }

        protected override void OnPause()
        {
            base.OnPause();
        }
    }
}
