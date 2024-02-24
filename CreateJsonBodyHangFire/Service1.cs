using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace CreateJsonBodyHangFire
{
    public partial class Service1 : ServiceBase
    {
        private IScheduler _scheduler;

        public void onDEBUG()
        {
            MyJob job = new MyJob();
            job.mainExecute();
        }

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            string CronExpression = ConfigurationManager.AppSettings["CronExpression"];
            string TimeZone_ = ConfigurationManager.AppSettings["TimeZone_"];
            string windowsZoneId = TZConvert.RailsToWindows(TimeZone_);
            TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(windowsZoneId);

            // Configure the Quartz scheduler
            ISchedulerFactory schedulerFactory = new StdSchedulerFactory();
            _scheduler = schedulerFactory.GetScheduler().Result;

            string id = Guid.NewGuid().ToString();

            IJobDetail job = JobBuilder.Create<MyJob>()
                .WithIdentity("J" + id, "JG" + id)
                .Build();

            // Create and configure the trigger to run every day at 11 PM
            /*ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("MyTrigger", "MyGroup")
                .StartAt(DateBuilder.TodayAt(23, 0, 0))  // 11 PM
                .WithSimpleSchedule(x => x
                    .WithIntervalInHours(24)  // Every 24 hours
                    .RepeatForever())
                .Build();*/

            ITrigger trigger = null;
            trigger = TriggerBuilder.Create()
                    .WithIdentity("T" + id, "TG" + id)
                    .StartNow()
                    .WithCronSchedule(CronExpression, x => x.InTimeZone(timeZoneInfo))
                    //.WithCronSchedule(CronExpression)
                    .Build();

            /*ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity("MyTrigger", "MyGroup")
            .StartAt(DateBuilder.EvenMinuteDate(DateTimeOffset.UtcNow.AddMinutes(1)))  // Run on the next minute
            .WithSimpleSchedule(x => x
                .WithIntervalInMinutes(1)  // Every 1 minute
                .RepeatForever())
            .Build();*/

            // Schedule the job with the trigger
            _scheduler.ScheduleJob(job, trigger);

            // Start the scheduler
            _scheduler.Start();
        }

        protected override void OnStop()
        {
            // Shutdown the scheduler when the service is stopped
            _scheduler?.Shutdown();
        }
    }
}
