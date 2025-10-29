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
  // === ReadBlockFromRawFile
  public static TPointRec[] ReadTPRFromRawFile(string filePath, ref long poz, string synchroName)  // Utils.ReadTPRFromRawFile
  {
    Utils.WriteLogAt("ReadTPRFromRawFile");
    TPointRec[] tprArr = null;

    using (var mutex = new Mutex(false, synchroName))
    {
      var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
      if (!st)
        throw new Exception("ReadTPRFromRawFile Impossible lock mutex [" + synchroName + "]");
      try
      {
        using (Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
        {
          if (poz >= stream.Length)
            return null;
          stream.Position = poz;
          var bsz = stream.Length - poz;
          var bbuff = new byte[bsz];
          stream.Read(bbuff, 0, (int)bsz);
          tprArr = Utils.ConvertTPR(bbuff);
          poz = stream.Position;
        }
      }
      catch (Exception ex)
      {
        throw new Exception("ReadTPRFromRawFile mutex18 [" + synchroName + "] created and: " + ex.Message);
      }
      finally
      {
        mutex.ReleaseMutex();
      }
    }
    return tprArr;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="filePath"></param>
  /// <param name="poz"></param>
  /// <param name="synchroName"></param>
  /// <param name="callBack"></param>
  /// <returns></returns>
  public static int? ReadTTrackListFromRaw(string filePath  // Utils.ReadTTrackListFromRaw
      , ref long poz
      , string synchroName
      , ReadTTrackListFromRawTLP_CallBack callBack
      )
  {
    Utils.WriteLogAt("ReadTTrackListFromRaw");

    if (string.IsNullOrEmpty(synchroName))
      throw new Exception("ReadTTrackListFromRaw illegal parameter: synchroName");
    if (string.IsNullOrEmpty(synchroName))
      throw new Exception("ReadTTrackListFromRaw illegal parameter: callBack");

    var tprArr = Utils.ReadTPRFromRawFile(filePath, ref poz, synchroName);
    if (tprArr == null || tprArr.Length == 0)
      return null;
    if (callBack != null)
      callBack(tprArr);
    return 1;
  }

  public static void WriteArrToRawFile(string filePath, TPointRec[] tprArr, string synchroName)
  {
    Utils.WriteLogAt("WriteArrToRawFile");
    using (var mutex = new Mutex(false, synchroName))
    {
      // acquire the mutex (or timeout after 1 seconds)
      var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
      if (!st)
        throw new Exception("WriteArrToRawFile Impossible lock mutex [" + synchroName + "]");
      try
      {
        using (Stream stream = File.Open(filePath, FileMode.Append, FileAccess.Write))
        {
          var bArr = Utils.ConvertTPR(tprArr);
          stream.Write(bArr, 0, bArr.Length);
        }
      }
      catch (Exception ex)
      {
        throw new Exception("WriteArrToRawFile mutex1 [" + synchroName + "] created and: " + ex.Message);
      }
      finally
      {
        mutex.ReleaseMutex();
      }
    }
  }
}
