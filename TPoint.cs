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

[Serializable]
public class TPoint
{
    public const int TPointImeiMaxLen = 18; // TPoint.TPointImeiMaxLen
    public const int TPointImeiMaxLen1 = 19; // TPoint.TPointImeiMaxLen1
    public const int TPointImeiMaxLen2 = 20; // TPoint.TPointImeiMaxLen2

    public static int TPointRecSize() // TPoint.TPointRecSize()
    {
        return Marshal.SizeOf(new TPointRec());
    }

    public TPoint(string imei)
    {
        t_imei = imei;
    }

    public string t_imei;
    public long t_SecFrom2000; // from 2000 utc
    
    public long t_ServSecFrom2000; // on Server sec from 2000 utc

    public float t_lat = 0;
    public float t_lon = 0;
    public float t_alt = 0;
    public short t_pow = 0; //new
    public float t_vel = -1;

    public DateTime get_t_date() { return LongTime.Sec2000ToDateTime(t_SecFrom2000); }

    public DateTime get_serv_t_date() { return LongTime.Sec2000ToDateTime(t_ServSecFrom2000); }

    public static TPoint newTPointTest(string imei, DateTime dt) // используется ConsoleTrackers.csproj // не используется в AnHttpSvc.csproj
    {
        var tp = new TPoint(imei)
        {
            t_SecFrom2000 = LongTime.DateToSecLong(dt),
            t_ServSecFrom2000 = LongTime.DateToSecLong(DateTime.UtcNow),
            //t_date = dt,
            t_lat = 55.5F + (float)(Utils.getRnd(0, 10)/100000.0), //55.48275F
            t_lon = 37.3F + (float)(Utils.getRnd(0, 10)/100000.0), //37.26511F
            t_alt = 0,
            //t_vel = 0,
            t_pow = 100,
        };
        return tp;
    }
    // Utils.getRnd

}

[StructLayout(LayoutKind.Explicit, Pack = 1, CharSet = CharSet.Unicode)]
public unsafe struct TPointRec // unsafe
{
[FieldOffset(0)]
    public fixed byte imei_buf[20];
[FieldOffset(20)]
    public long SecFrom2000;
[FieldOffset(28)]
    public long ServSecFrom2000;//
[FieldOffset(36)]
    public float t_lat;
[FieldOffset(40)]
    public float t_lon;
[FieldOffset(44)]
    public float t_alt;
[FieldOffset(48)]
    public short t_pow; //new
[FieldOffset(50)]
    public float t_vel; //new

[FieldOffset(0)]
    public long long1;
[FieldOffset(8)]
    public long long2;
[FieldOffset(16)]
    public long long3;
[FieldOffset(24)]
    public long long4;
}


