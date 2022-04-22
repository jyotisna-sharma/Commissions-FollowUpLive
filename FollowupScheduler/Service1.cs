using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.IO; 

namespace FollowupScheduler
{
    public partial class vinod : ServiceBase
    {
        public vinod()
        {
            InitializeComponent();
        }

        private System.Timers.Timer timeToExecute = new System.Timers.Timer();

        protected override void OnStart(string[] args)
        {
            timeToExecute.Start();
            //Run on 1 hours 
            timeToExecute.Interval = 24 * 30 * 60 * 1000;
            //15 min
            //timeToExecute.Interval = 900000;
            timeToExecute.Elapsed += new System.Timers.ElapsedEventHandler(timeToExecute_Elapsed);
        }

        void timeToExecute_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            runService();
        }

        protected override void OnStop()
        {
        }

        public void runService()
        {
            MyAgencyVault.FollowUpProcess.FollowUpService.FollowUpProc();
        }

    }
}
