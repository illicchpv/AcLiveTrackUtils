using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class ImeiTPoint
{
    //public DateTime t_date = DateTime.MinValue;
    //public long SecFrom2000 = 0;
    //public float t_lat = 0;
    //public float t_lon = 0;
    //public float t_alt = 0;
    //public float t_vel = -1;
    //public Int16 t_nPowr = -1;

    //public static ImeiTPoint newImeiTPoint(long secFrom2000)
    //{
    //    n++;
    //    var tp = new ImeiTPoint()
    //    {
    //        SecFrom2000 = secFrom2000,
    //        t_lat = n,
    //        t_lon = 0,
    //        t_alt = 0,
    //        t_vel = 0,
    //        t_nPowr = 100,
    //    };
    //    return tp;
    //}

    //private static long n = 0;
    //public static ITPoint newITPoint(long secFrom2000)
    //{
    //    n++;
    //    var tp = new ITPoint()
    //    {
    //        SecFrom2000 = secFrom2000,
    //        t_lat = n,
    //        t_lon = 0,
    //        t_alt = 0,
    //        t_vel = 0,
    //        t_nPowr = 100,
    //    };
    //    return tp;
    //}
    public static ITPoint newITPoint(TPoint tp) // используется! в ActiveLife2.csproj // не используется в ActiveLife2.csproj
    {
        var itp = new ITPoint()
        {
            SecFrom2000 = LongTime.DateToSecLong(tp.get_t_date()),
            ServSecFrom2000 = LongTime.DateToSecLong(tp.get_serv_t_date()),
            t_lat = tp.t_lat,
            t_lon = tp.t_lon,
            t_alt = tp.t_alt,
            t_pow = tp.t_pow,
            t_vel = tp.t_vel,
        };
        return itp;
    }
    public static ITPoint newITPoint(TPointRec tpr) // используется! в ActiveLife2.csproj // не используется в ActiveLife2.csproj
    {
        var itp = new ITPoint()
        {
            SecFrom2000 = tpr.SecFrom2000, //LongTime.DateToSecLong(tp.get_t_date()),
            ServSecFrom2000 = tpr.ServSecFrom2000, //LongTime.DateToSecLong(tp.get_serv_t_date()),
            t_lat = tpr.t_lat,
            t_lon = tpr.t_lon,
            t_alt = tpr.t_alt,
            t_pow = tpr.t_pow,
            t_vel = tpr.t_vel,
        };
        return itp;
    }

    public static int ITPointSize() // ImeiTPoint.ITPointSize()
    {
        return Marshal.SizeOf(new ITPoint());
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 1)] // , Pack = 1, CharSet = CharSet.Unicode
public struct ITPoint
{
    public long SecFrom2000;    // 8b
    public long ServSecFrom2000;// 8b
    public float t_lat;         // 4b
    public float t_lon;         // 4b
    public float t_alt;         // 4b
    public short t_pow; //new   // 2
    public float t_vel;         // 4b
}

public class ImeiTPointArrRez
{
    public string jsonRez { get; set; }
    public int ptsCount { get; set; }
    public long dt0 { get; set; }
    public double? maxDouble { get; set; }
    public double? minDouble { get; set; }
}
