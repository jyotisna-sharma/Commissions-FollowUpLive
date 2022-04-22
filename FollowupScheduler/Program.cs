using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace FollowupScheduler
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
//# if debug
            vinod objVin = new vinod();
            objVin.runService();
           
//#else
//            ServiceBase[] ServicesToRun;
//            ServicesToRun = new ServiceBase[] 
//            { 
//                new vinod() 
//            };
//            ServiceBase.Run(ServicesToRun);
//# endif
        }
    }
}
