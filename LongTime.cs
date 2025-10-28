using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


public partial class LongTime
{
    public const long SecondsInHour = 3600;// LongTime.SecondsInHour

    private static DateTime timeUtc0000 = (new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    private static DateTime timeUtcJS0000 = (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));

    ////private static DateTime timeUtc0000 = new DateTime(1970, 1, 1).ToUniversalTime();
    //private static DateTime timeUtc0000 = TimeZoneInfo.ConvertTimeToUtc(
    //    DateTime.ParseExact("2000-01-01 00:00:00", "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)
    //    , TimeZoneInfo.Utc
    //);
    //private static DateTime timeUtc111 = new DateTime(111, DateTimeKind.Utc);
    ////private static DateTime timeUtc00001 = new DateTime(2000, 1, 1).ToUniversalTime();

    public static long DateToHourLong(DateTime dt)        // LongTime.DateToHourLong
    {
        dt = dt.ToUniversalTime();
        return (long)(((dt - timeUtc0000).TotalSeconds) / SecondsInHour);
    }
    public static long DateToHourLongStart(DateTime dt)        // LongTime.DateToHourLongStart
    {
        dt = dt.ToUniversalTime();
        long v = (long)((dt - timeUtc0000).TotalSeconds) / SecondsInHour;
        return v; //(v * SecondsInHour);
    }
    public static long DateToMinutesLong(DateTime dt)        // LongTime.DateToMinutesLong
    {
        dt = dt.ToUniversalTime();
        return (long)(((dt - timeUtc0000).TotalSeconds) / 60);
    }
    public static long SecLongToHourLong(long val)        // LongTime.SecLongToHourLong
    {
        return (long)(val / SecondsInHour);
    }
    public static long? SecLongToHourLong(long? val)        // LongTime.SecLongToHourLong
    {

        return val == null ? (long?)null : (long)(val / SecondsInHour);
    }
    public static long HourLongToSecLong(long val)        // LongTime.HourLongToSecLong
    {
        return (long)(val * SecondsInHour);
    }
    public static long DateToSecLong(DateTime dt)        // LongTime.DateToSecLong
    {
        dt = dt.ToUniversalTime();
        return (long)((dt - timeUtc0000).TotalSeconds);
    }
    public static long? DateToSecLong(DateTime? dt)        // LongTime.DateToSecLong
    {
        if(dt == null) return null;
        dt = ((DateTime)dt).ToUniversalTime();
        return (long)((((DateTime)dt) - timeUtc0000).TotalSeconds);
    }
    // JS
    public static long? DateToSecJS(DateTime? dt)        // LongTime.DateToSecJS
    {
        if(dt == null) return null;
        dt = ((DateTime)dt).ToUniversalTime();
        return (long)((((DateTime)dt) - timeUtcJS0000).TotalSeconds);
    }
    public static DateTime SecJSToDateTime(long v)        // LongTime.SecJSToDateTime
    {
        return (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(v); // ????? .ToUniversalTime()
    }
    // JS

    public static string LongToHourStr(long val)        // LongTime.LongToHourStr
    {
        return val.ToString().PadLeft(9, '0');
    }
    public static long HourStrToHourLong(string hour)        // LongTime.HourStrToHourLong
    {
        return Convert.ToInt64(hour);
    }
    public static DateTime Hour2000ToDateTime(long v)        // LongTime.Hour2000ToDateTime
    {
        return (new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddHours(v); // ????? .ToUniversalTime()
    }
    public static DateTime Minutes2000ToDateTime(long v)        // LongTime.Hour2000ToDateTime
    {
        return (new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMinutes(v); // ????? .ToUniversalTime()
    }
    public static DateTime LtToDateTime(long v)        // LongTime.LtToDateTime
    {
        return (new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(v); // ????? .ToUniversalTime()
    }
    public static DateTime Sec2000ToDateTime(long v)        // LongTime.Sec2000ToDateTime
    {
        return (new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddSeconds(v); // ????? .ToUniversalTime()
    }
    public static DateTime? CompactUtcStrToDateTime(string v)
    {
        if(string.IsNullOrEmpty(v))
            return null;
        TimeZone localZone = TimeZone.CurrentTimeZone;
        DateTime d = DateTime.ParseExact(v, "yyyyMMddHHmmss", CultureInfo.CurrentCulture);
        var dt = TimeZoneInfo.ConvertTimeToUtc(DateTime.ParseExact(v, "yyyyMMddHHmmss", CultureInfo.InvariantCulture), TimeZoneInfo.Utc);
        return dt;
    }

    public static string Hour2000ToTrkPath(long v)      // LongTime.Hour2000ToTrkPath(v)
    {
        var dt = Hour2000ToDateTime(v);
        return dt.Year.ToString().PadLeft(4, '0')
            + "\\" + dt.DayOfYear.ToString().PadLeft(3, '0')
            + "\\" + v.ToString().PadLeft(9, '0') 
            + ".trk";
        //return (v / 1000).ToString().PadLeft(9, '0') 
        //    + "\\" + (v / 100).ToString().PadLeft(9, '0') 
        //    + "\\" + v.ToString().PadLeft(9, '0') 
        //    + ".trk";
    }
    public static string Hour2000ToStrPath(long v)      // LongTime.Hour2000ToTrkPath(v)
    {
        var dt = Hour2000ToDateTime(v);
        return dt.Year.ToString().PadLeft(4, '0')
            + "\\" + dt.Month.ToString().PadLeft(2, '0')
            + "\\" + dt.Day.ToString().PadLeft(3, '0')
            + "\\" + v.ToString().PadLeft(9, '0') 
            + ".trk";
    }
    public static string Hour2000ToAzurePath(long v)      // LongTime.Hour2000ToAzurePath(v)
    {
        return LongTime.LongToHourStr(v);
    }

    // LongTime.JavaScriptStringDateToDateTime("2024-03-16T01:11:22.000Z");
    public static DateTime JavaScriptStringDateToDateTime(string dtStr)
    { // sample: dtStr - "2024-03-16T01:11:22.000Z"
        var f = "yyyy-MM-ddTHH:mm:ss.FFFZ";
        return DateTime.ParseExact(dtStr, f, null);
    }
}

//public partial class LongTime
//{
//    long _LongTimeSeconds = 0; // new DateTime(2000,1,1)
//    public LongTime(DateTime dt)
//    {
//        dt = dt.ToUniversalTime();
//        if (dt < timeUtc0000)
//            throw new Exception("LongTime out off range");
//        _LongTimeSeconds = (long)(dt - timeUtc0000).TotalSeconds;
//    }

//    public string ToSecStr()
//    {
//        return _LongTimeSeconds.ToString().PadLeft(13, '0');
//    }
//    public string ToMinuteStr()
//    {
//        return ToMinuteFrom2000().ToString().PadLeft(11, '0');
//    }
//    public string ToHourStr()
//    {
//        return LongToHourStr(ToHourFrom2000());
//        //return ToHourFrom2000().ToString().PadLeft(9, '0');
//    }

//    public long ToSecFrom2000()
//    {
//        return _LongTimeSeconds;
//    }
//    public long ToMinuteFrom2000()
//    {
//        return _LongTimeSeconds / (60);
//    }
//    public long ToHourFrom2000()
//    {
//        return _LongTimeSeconds / (SecondsInHour);
//    }
//    public void AddSec(long secCnt)
//    {
//        _LongTimeSeconds += secCnt;
//    }
//}

