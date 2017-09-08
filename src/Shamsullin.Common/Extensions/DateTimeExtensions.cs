using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Shamsullin.Common.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime EndOfYear(this DateTime date)
        {
            return new DateTime(date.Year, 12, 31, 0, 0, 0);
        }

        public static DateTime EndOfMonth(this DateTime date)
        {
            return date.AddMonths(1).StartOfMonth().AddDays(-1).EndOfDay();
        }

        public static DateTime EndOfWeek(this DateTime date)
        {
            var delta = 7 - (int) date.DayOfWeek;
            return date.AddDays(delta).EndOfDay();
        }

        public static DateTime EndOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59);
        }

        public static DateTime StartOfYear(this DateTime date)
        {
            return new DateTime(date.Year, 1, 1, 0, 0, 0);
        }

        public static DateTime StartOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1, 0, 0, 0);
        }

        public static DateTime StartOfWeek(this DateTime date)
        {
            var delta = 1 - (int) date.DayOfWeek;
            if (delta > 0) delta -= 7;
            return date.AddDays(delta).Date;
        }

        public static IEnumerable<DateTime> DaysBetween(this DateTime date1, DateTime date2)
        {
            for (var date = new[] {date1, date2}.Min(); date <= new[] {date1, date2}.Max(); date = date.AddDays(1))
                yield return date.Date;
        }

        public static IEnumerable<DateTime> WeeksBetween(this DateTime date1, DateTime date2)
        {
            for (var date = new[] {date1, date2}.Min().StartOfWeek();
                date <= new[] {date1, date2}.Max();
                date = date.AddDays(7))
                yield return date.StartOfWeek();
        }

        public static IEnumerable<DateTime> MonthsBetween(this DateTime date1, DateTime date2)
        {
            for (var date = new[] {date1, date2}.Min().StartOfMonth();
                date <= new[] {date1, date2}.Max();
                date = date.AddMonths(1))
                yield return date.StartOfMonth();
        }

        public static DateTime WithoutMilliseconds(this DateTime date)
        {
            return DateTime.Parse(date.ToString("dd.MM.yyyy HH:mm:ss"));
        }

        public static DateTime StartOfDay(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        }

        public static string ToFilenameString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd_HH.mm.ss.fff");
        }

        public static string Year(this DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.Year.ToString() : string.Empty;
        }

        public static string ToShortDateString(this DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ToShortDateString() : string.Empty;
        }

        public static string ToString(this DateTime? dateTime, string format)
        {
            return dateTime.HasValue ? dateTime.Value.ToString(format) : string.Empty;
        }

        public static DateTime? EndOfDay(this DateTime? date)
        {
            return date.HasValue ? date.Value.EndOfDay() : (DateTime?) null;
        }

        public static bool IsToday(this DateTime dateTime)
        {
            return dateTime.Date == DateTime.Now.Date;
        }

        public static bool IsYesterday(this DateTime dateTime)
        {
            return dateTime.Date == DateTime.Now.Date.AddDays(-1);
        }

        public static bool IsDayBeforeYesterday(this DateTime dateTime)
        {
            return dateTime.Date == DateTime.Now.Date.AddDays(-2);
        }

        public static bool IsEarly(this DateTime dateTime)
        {
            return dateTime.Date < DateTime.Now.Date.AddDays(-2);
        }

        public static bool IsToday(this DateTime? dateTime)
        {
            return dateTime.HasValue && dateTime.Value.Date == DateTime.Now.Date;
        }

        public static bool IsYesterday(this DateTime? dateTime)
        {
            return dateTime.HasValue && dateTime.Value.Date == DateTime.Now.Date.AddDays(-1);
        }

        public static bool IsDayBeforeYesterday(this DateTime? dateTime)
        {
            return dateTime.HasValue && dateTime.Value.Date == DateTime.Now.Date.AddDays(-2);
        }

        public static bool IsEarly(this DateTime? dateTime)
        {
            return dateTime.HasValue && dateTime.Value.Date < DateTime.Now.Date.AddDays(-2);
        }

        public static bool Between(this DateTime dateTime, DateTime startDate, DateTime endDate)
        {
            return dateTime >= startDate && dateTime <= endDate;
        }

        public static bool IsValidSqlDate(this DateTime dateTime)
        {
            return dateTime.Between(new DateTime(1753, 1, 1), new DateTime(9999, 12, 31));
        }

        public static string ToDatabaseString(this DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public static long ToUnixTime(this DateTime dateTime)
        {
            return (long) (DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
        }

        public static float Seconds(this Stopwatch sw)
        {
            return sw.ElapsedMilliseconds/1000f;
        }
    }
}