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

//public partial class Utils
//{
//    internal static int itpsSz = (Marshal.SizeOf(typeof(ITPointS)));
//    internal static long secInHour = 60 * 60;

//    public static void _WriteITPointsArrToFileS(string filePath, long baseHour2000, ITPoint[] itpArr)  // Utils._WriteITPointsArrToFileS
//    {
//        var baseSec2000 = LongTime.HourLongToSecLong(baseHour2000);
//        using (Stream stream = File.Open(filePath, FileMode.OpenOrCreate))
//        {
//            var itps = new ITPointS();
//            foreach (var itp in itpArr)
//            {
//                var i = itp.SecFrom2000 - baseSec2000;
//                if (i < 0 || i >= secInHour)
//                {
//                    WriteError("WriteITPointsArrToFileS -- bad records in:" + filePath + " baseHour2000=" + baseHour2000);
//                    continue;
//                }
//                itps.t_alt = itp.t_alt;
//                itps.t_lat = itp.t_lat;
//                itps.t_lon = itp.t_lon;
//                //itps.t_vel = itp.t_vel;

//                var btArr = Utils.ConvertItps(itps);
//                stream.Position = itpsSz * i;
//                stream.Write(btArr, 0, btArr.Length);
//            }
//        }
//    }

//    public static ITPoint[] ReadITPointsArrFromTrkFileS(string filePath, long baseHour2000, string synchroName)
//    {
//        ITPoint[] itpArr = null;
//            using (var mutex = new Mutex(false, synchroName))
//            {
//                // acquire the mutex (or timeout after 1 seconds)
//                var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
//                if (!st)
//                    throw new Exception("Imposible lock mutexTrk8");
//                try
//                {
//                    using (Stream stream = File.Open(filePath, FileMode.Open))
//                    {
//                        if (stream.Length == 0)
//                            return null;
//                        var bbuff = new byte[stream.Length];
//                        stream.Read(bbuff, 0, (int)stream.Length);
//                        itpArr = Utils.Convert(bbuff);
//                    }
//                }
//                catch (Exception ex)
//                {
//                    throw new Exception("mutexTrk82 [" + synchroName + "] created and: " + ex.Message);
//                }
//                finally
//                {
//                    mutex.ReleaseMutex();
//                }
//            }

//        return null;
//    }

//    public static ITPoint[] _ReadITPointsArrFromFileS(string filePath, long baseHour2000)  // Utils._ReadITPointsArrFromFileS
//    {
//        var baseSec2000 = LongTime.HourLongToSecLong(baseHour2000);
//        ITPointS[] itpsArr = null;
//        using (Stream stream = File.Open(filePath, FileMode.Open))
//        {
//            var bsz = stream.Length;
//            var bbuff = new byte[bsz];
//            stream.Read(bbuff, 0, (int)bsz);
//            itpsArr = Utils.ConvertItps(bbuff);
//        }
//        var itpList = new List<ITPoint>();
//        for (var i = 0; i < itpsArr.Length; i++)
//        {
//            if (itpsArr[i].t_lat == 0 && itpsArr[i].t_lon == 0)
//                continue; // empty place
//            var itp = new ITPoint();
//            itp.SecFrom2000 = baseSec2000 + i;
//            itp.t_alt = itpsArr[i].t_alt;
//            itp.t_lat = itpsArr[i].t_lat;
//            itp.t_lon = itpsArr[i].t_lon;
//            //itp.t_vel = itpsArr[i].t_vel;
//            itpList.Add(itp);
//        }
//        return itpList.ToArray();
//    }
//}

public partial class Utils
{
    // ITPoints file read\write (TRK files)
    public static void WriteITPointsArrToFile(string filePath, ITPoint[] itpArr, string synchroName, bool append = true)  // Utils.WriteITPointsArrToFile
    {
        Utils.WriteLogAt("WriteITPointsArrToFile");

        if (string.IsNullOrEmpty(synchroName))
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write))
            {
                var bArr = Utils.Convert(itpArr);
                stream.Write(bArr, 0, bArr.Length);
            }
        }
        else
        {
            using (var mutex = new Mutex(false, synchroName))
            {
                // acquire the mutex (or timeout after 1 seconds)
                var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
                if (!st)
                    throw new Exception("WriteITPointsArrToFile Imposible lock mutex [" + synchroName + "]");
                try
                {
                    using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write))
                    {
                        var bArr = Utils.Convert(itpArr);
                        stream.Write(bArr, 0, bArr.Length);
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    // the directory did not exist. Create this and try again
                    var folder = Path.GetDirectoryName(filePath);
                    try
                    {
                        Directory.CreateDirectory(folder);
                    }
                    catch (Exception ex2)
                    {
                        throw new Exception("WriteITPointsArrToFile imposible create folder [" + folder + "] -- " + ex2.Message);
                    }
                    try
                    {
                        Utils.WriteITPointsArrToFile(filePath, itpArr, null, append);
                    }
                    catch (Exception ex2)
                    {
                        throw new Exception("WriteITPointsArrToFile imposible create file [" + filePath + "] -- " + ex2.Message);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("WriteITPointsArrToFile mutex6 [" + synchroName + "] created and: " + ex.Message);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }
    public static byte[] ReadByttesArrFromFile(string filePath, string synchroName)
    {
        Utils.WriteLogAt("ReadByttesArrFromFile");

        byte[] bbuff = null;
        using (var mutex = new Mutex(false, synchroName))
        {
            // acquire the mutex (Utils.WaitOneInterval timeout after 1 seconds)
            var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
            if (!st)
                throw new Exception("ReadByttesArrFromFile Imposible lock mutex [" + synchroName + "]");
            try
            {
                using (Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                {
                    var bsz = stream.Length;
                    bbuff = new byte[bsz];
                    stream.Read(bbuff, 0, (int)bsz);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("ReadByttesArrFromFile mutex8 [" + synchroName + "] created and: " + ex.Message);
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        return bbuff;
    }
    public static ITPoint[] ReadITPointsArrFromFile(string filePath, string synchroName)
    {
        Utils.WriteLogAt("ReadITPointsArrFromFile");

        ITPoint[] itpArr = null;
        if (string.IsNullOrEmpty(synchroName))
        {
            using (Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                var bsz = stream.Length;
                var bbuff = new byte[bsz];
                stream.Read(bbuff, 0, (int)bsz);
                itpArr = Utils.Convert(bbuff);
            }
        }
        else
        {
            using (var mutex = new Mutex(false, synchroName))
            {
                // acquire the mutex (Utils.WaitOneInterval timeout after 1 seconds)
                var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
                if (!st)
                    throw new Exception("ReadITPointsArrFromFile Imposible lock mutex [" + synchroName + "]");
                try
                {
                    using (Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read)) // , FileAccess.Read, FileShare.ReadWrite
                    {
                        var bsz = stream.Length;
                        var bbuff = new byte[bsz];
                        stream.Read(bbuff, 0, (int)bsz);
                        itpArr = Utils.Convert(bbuff);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("ReadITPointsArrFromFile mutex8 [" + synchroName + "] created and: " + ex.Message);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
        return itpArr;
    }
    public static List<object> getPointsListArr(ITPoint[] rez, ref long dt0, ref double? maxDouble, ref double? minDouble)
    {
        maxDouble = null; minDouble = null;
        var listList = new List<object>();
        var r = rez.OrderBy(o => o.SecFrom2000).ToList();

        ITPoint pPrev = default(ITPoint);
        var isFirst = true;
        var skipCnt = 0;
        dt0 = 0;
        foreach (var p in r)
        {
            if (isFirst)
            {
                isFirst = false;
                pPrev = p; dt0 = p.SecFrom2000;
                var floatArr = new object[] {(long)0,                      p.t_lat,    p.t_lon, p.ServSecFrom2000, p.t_alt, p.t_pow, p.t_vel };
                listList.Add(floatArr);
            }
            else
            {
                if (pPrev.SecFrom2000 == p.SecFrom2000)
                {
                    skipCnt++; continue;
                }
                pPrev = p; 
                var floatArr = new object[] {(long)(p.SecFrom2000 - dt0),  p.t_lat,    p.t_lon, p.ServSecFrom2000, p.t_alt, p.t_pow, p.t_vel };
                listList.Add(floatArr);
            }
        }
        return listList;
    }
    public static List<object> getTPointsListArr(List<TPoint> rez, ref long dt0, ref double? maxDouble, ref double? minDouble)
    {
        maxDouble = null; minDouble = null;
        var listList = new List<object>();
        var r = rez.OrderBy(o => o.t_imei).ThenBy(o => o.t_SecFrom2000);

        TPoint pPrev = default(TPoint);
        var isFirst = true;
        var skipCnt = 0;
        dt0 = 0;
        foreach (var p in r)
        {
            if (isFirst)
            {
                isFirst = false;
                pPrev = p; dt0 = p.t_SecFrom2000;
                var floatArr = new object[]     {(long)0,                       p.t_lat,  p.t_lon, p.t_ServSecFrom2000, p.t_alt, p.t_pow, p.t_vel, p.t_imei};
                listList.Add(floatArr);
            }
            else
            {
                if (pPrev.t_imei != p.t_imei)
                {
                    pPrev = p;
                    var floatArr = new object[] {(long)(p.t_SecFrom2000 - dt0), p.t_lat,  p.t_lon, p.t_ServSecFrom2000, p.t_alt, p.t_pow, p.t_vel, p.t_imei};
                    listList.Add(floatArr);
                }else
                {
                    if (pPrev.t_SecFrom2000 == p.t_SecFrom2000)
                    {
                        skipCnt++; continue;
                    }
                    pPrev = p; 
                    var floatArr = new object[] {(long)(p.t_SecFrom2000 - dt0), p.t_lat,  p.t_lon, p.t_ServSecFrom2000, p.t_alt, p.t_pow, p.t_vel};
                    listList.Add(floatArr);
                }
            }
        }
        return listList;
    }
#if MOREgetPointsList
    public static List<object> getTPointsListArr(TPoint[] rez, ref long dt0, ref double? maxDouble, ref double? minDouble)
    {
        maxDouble = null; minDouble = null;
        var listList = new List<object>();
        var r = rez.OrderBy(o => o.t_imei).ThenBy(o => o.t_SecFrom2000);

        TPoint pPrev = default(TPoint);
        var isFirst = true;
        var skipCnt = 0;
        dt0 = 0;
        foreach (var p in r)
        {
            if (isFirst)
            {
                isFirst = false;
                pPrev = p; dt0 = p.t_SecFrom2000;
                var floatArr = new object[]     {(long)0,                      p.t_lat,    p.t_lon, p.t_ServSecFrom2000, p.t_alt, p.t_pow, p.t_imei};
                listList.Add(floatArr);
            }
            else
            {
                if (pPrev.t_imei != p.t_imei)
                {
                    pPrev = p;
                    var floatArr = new object[] {(long)(p.t_SecFrom2000 - dt0),p.t_lat,    p.t_lon, p.t_ServSecFrom2000, p.t_alt, p.t_pow, p.t_imei};
                    listList.Add(floatArr);
                }else
                {
                    if (pPrev.t_SecFrom2000 == p.t_SecFrom2000)
                    {
                        skipCnt++; continue;
                    }
                    pPrev = p; 
                    var floatArr = new object[] {(long)(p.t_SecFrom2000 - dt0),  p.t_lat,    p.t_lon, p.t_ServSecFrom2000, p.t_alt, p.t_pow};
                    listList.Add(floatArr);
                }
            }
        }
        return listList;
    }
#endif //MOREgetPointsList

    //public static void WriteITPointsToFile(string filePath, List<ITPoint> itpList, string synchroName, bool append = true)  // Utils.WriteITPointsToFile
    //{
    //    if (string.IsNullOrEmpty(synchroName))
    //    {
    //        using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
    //        {
    //            foreach (var ipt in itpList)
    //            {
    //                var b = StructureToByteArray<ITPoint>(ipt);
    //                stream.Write(b, 0, b.Length);
    //            }
    //        }
    //    }
    //    else
    //    {
    //        using (var mutex = new Mutex(false, synchroName))
    //        {
    //            // acquire the mutex (or timeout after 1 seconds)
    //            var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
    //            if(!st)
    //                throw new Exception("Imposible lock mutex7");
    //            try
    //            {
    //                using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
    //                {
    //                    foreach (var ipt in itpList)
    //                    {
    //                        var b = StructureToByteArray<ITPoint>(ipt);
    //                        stream.Write(b, 0, b.Length);
    //                    }
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                throw new Exception("mutex7 [" + synchroName + "] created and: " + ex.Message);
    //            }
    //            finally
    //            {
    //                mutex.ReleaseMutex();
    //            }
    //        }
    //    }
    //}
    //public static List<ITPoint> ReadITPointsFromFile(string filePath, ref long poz, string synchroName)  // Utils.ReadITPointsFromFile
    //{
    //    if (string.IsNullOrEmpty(synchroName))
    //    {
    //        using (Stream stream = File.Open(filePath, FileMode.Open))
    //        {
    //            if (poz >= stream.Length)
    //                return null;
    //            stream.Position = poz;
    //
    //            var itpList = new List<ITPoint>();
    //            var sz = Marshal.SizeOf(typeof(ITPoint));
    //            do
    //            {
    //                var buff = new byte[sz];
    //                stream.Read(buff, 0, sz);
    //                var itp = Utils.ByteArrayToStructure<ITPoint>(buff);
    //                itpList.Add(itp);
    //            } while (stream.Position < stream.Length);
    //            return itpList;
    //        }
    //    }
    //    else
    //    {
    //        List<ITPoint> itpList = null;
    //        using (var mutex = new Mutex(false, synchroName))
    //        {
    //            // acquire the mutex (or timeout after 1 seconds)
    //            var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
    //            if(!st)
    //                throw new Exception("Imposible lock mutex");
    //            try
    //            {
    //                using (Stream stream = File.Open(filePath, FileMode.Open))
    //                {
    //                    if (poz >= stream.Length)
    //                        return null;
    //                    stream.Position = poz;
    //
    //                    var sz = Marshal.SizeOf(typeof(ITPoint));
    //                    do
    //                    {
    //                        var buff = new byte[sz];
    //                        stream.Read(buff, 0, sz);
    //                        var itp = Utils.ByteArrayToStructure<ITPoint>(buff);
    //                        itpList.Add(itp);
    //                    } while (stream.Position < stream.Length);
    //                }
    //            }
    //            catch (Exception ex)
    //            {
    //                throw new Exception("mutex9 [" + synchroName + "] created and: " + ex.Message);
    //            }
    //            finally
    //            {
    //                mutex.ReleaseMutex();
    //            }
    //        }
    //        return itpList;
    //    }
    //}
}
