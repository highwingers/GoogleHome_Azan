﻿using Google.Cast.ClassLibrary.Service.Quartz;
using Google.Cast.Data;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Google.Cast.ClassLibrary.Service.Muslimsalat
{
    public class PrayerSetup<T, TOnce> : IPrayerSetup<T, TOnce> where T:IJob
                                                                where TOnce:IJob
    {
        private IDal _dal;
        public PrayerSetup()
        {
            _dal = new Dal();
        }

        public bool SetUp(string _url, string cron, string player)
        {
   
            // Get Prayer Timing
            PrayerResponse result = GetPrayerTimes(_url);

            //Create Schedule Object
            var sched = new Google.Cast.ClassLibrary.Service.Quartz.Scheduler();

            // Delete Exisiting Jobs and Create a Daily Schedule for Daily Prayer Download
            JobCleanUp(sched, cron);

            //Schedule all Prayers
            SchedulePrayers(result, sched, _dal.GetPlayer());

            return true;
        }

        private static PrayerResponse GetPrayerTimes(string url)
        {
            var p = new Google.Cast.ClassLibrary.Service.Muslimsalat.Muslimsalat();
            var result = p.GetPrayers(url);
            return result;
        }

        private static void JobCleanUp(Scheduler a, string cronSchedule) 
        {
            // Delete Exisiting jobs if ANY
            a.DeleteJobs();
            // Get Today azan Timing --- ONCE a day execute
            a.CreateJobOnce<TOnce>("DailyUpdate", System.DateTime.Now, cronSchedule);
        }

        private static void SchedulePrayers(PrayerResponse result, Scheduler a, string player)
        {

            //DateTime demo = SetPrayerTime(DateTime.Now.ToString("yyyy-MM-dd"), System.Convert.ToString( System.DateTime.Now.TimeOfDay), "demo", a, player);

            DateTime fjr = SetPrayerTime(result.items[0].date_for, result.items[0].fajr, "fajar", a, player);
            DateTime duhur = SetPrayerTime(result.items[0].date_for, result.items[0].dhuhr, "duhur", a, player);
            DateTime asar = SetPrayerTime(result.items[0].date_for, result.items[0].asr, "asar", a, player);
            DateTime magrib = SetPrayerTime(result.items[0].date_for, result.items[0].maghrib, "magrib", a, player);
            DateTime isha = SetPrayerTime(result.items[0].date_for, result.items[0].isha, "isha", a, player);
        }

        private static DateTime SetPrayerTime(string _date, string _time, string prayerName, Scheduler a, string player)
        {
            DateTime dateOnly = DateTime.Parse(_date);

            TimeSpan timeOnly = DateTime.Parse(_time).TimeOfDay;

            DateTime setTime = dateOnly + timeOnly;

            a.CreateJob<T>(prayerName, setTime, player);
            
            return setTime;
        }
    }
}
