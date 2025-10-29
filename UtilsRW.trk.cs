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
          throw new Exception("WriteITPointsArrToFile Impossible lock mutex [" + synchroName + "]");
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
            throw new Exception("WriteITPointsArrToFile impossible create folder [" + folder + "] -- " + ex2.Message);
          }
          try
          {
            Utils.WriteITPointsArrToFile(filePath, itpArr, null, append);
          }
          catch (Exception ex2)
          {
            throw new Exception("WriteITPointsArrToFile impossible create file [" + filePath + "] -- " + ex2.Message);
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
        throw new Exception("ReadByttesArrFromFile Impossible lock mutex [" + synchroName + "]");
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
          throw new Exception("ReadITPointsArrFromFile Impossible lock mutex [" + synchroName + "]");
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
        var floatArr = new object[] { (long)0, p.t_lat, p.t_lon, p.ServSecFrom2000, p.t_alt, p.t_pow, p.t_vel };
        listList.Add(floatArr);
      }
      else
      {
        if (pPrev.SecFrom2000 == p.SecFrom2000)
        {
          skipCnt++; continue;
        }
        pPrev = p;
        var floatArr = new object[] { (long)(p.SecFrom2000 - dt0), p.t_lat, p.t_lon, p.ServSecFrom2000, p.t_alt, p.t_pow, p.t_vel };
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
        var floatArr = new object[] { (long)0, p.t_lat, p.t_lon, p.t_ServSecFrom2000, p.t_alt, p.t_pow, p.t_vel, p.t_imei };
        listList.Add(floatArr);
      }
      else
      {
        if (pPrev.t_imei != p.t_imei)
        {
          pPrev = p;
          var floatArr = new object[] { (long)(p.t_SecFrom2000 - dt0), p.t_lat, p.t_lon, p.t_ServSecFrom2000, p.t_alt, p.t_pow, p.t_vel, p.t_imei };
          listList.Add(floatArr);
        }
        else
        {
          if (pPrev.t_SecFrom2000 == p.t_SecFrom2000)
          {
            skipCnt++; continue;
          }
          pPrev = p;
          var floatArr = new object[] { (long)(p.t_SecFrom2000 - dt0), p.t_lat, p.t_lon, p.t_ServSecFrom2000, p.t_alt, p.t_pow, p.t_vel };
          listList.Add(floatArr);
        }
      }
    }
    return listList;
  }
}
