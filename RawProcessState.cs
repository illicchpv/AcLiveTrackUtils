using System;
using System.IO;
using System.Threading;

[Serializable]
public class RawProcessState
{
  private string _rootRaw = null;
  private const string synchroName = "Raw2TrkConfig";

  public RawProcessState(string fn, string rootRaw)
  {
    curStateFileName = fn;
    _rootRaw = rootRaw;
  }
  public string curStateFileName;

  private string _workRawFileName = null;
  public string workRawFileName
  {
    get { return _workRawFileName; }
    set
    {
      _workRawFileName = value; /* positionInWorkFile = 0; */
    }
  }

  public long positionInWorkFile { get; set; }

  public string workRawFileFullName { get { return workRawFileName == null ? null : _rootRaw + workRawFileName + ".raw"; } }  //  + Program.flnRaw

  public string[] GetRawFiles(string rootRaw) // , string flnRaw
  {
    string[] nfArr = Directory.GetFiles(rootRaw, "*.raw"); // flnRaw + 
    if (nfArr.Length > 0)
    {
      for (var i = 0; i < nfArr.Length; i++)
      {
        nfArr[i] = nfArr[i].ToLower().Replace(_rootRaw, "").Replace(".raw", ""); // .Replace(Program.flnTrk, "")
      }
      Array.Sort(nfArr, StringComparer.InvariantCulture);
    }
    return nfArr;
  }
  public string[] GetRawFilesAndFirstFileForProcess(string rootRaw) // , string flnRaw
  { // get first file from flnRaw folder for processing
    string[] nfArr = GetRawFiles(rootRaw); // , flnRaw
    if (nfArr.Length > 0)
    {
      workRawFileName = nfArr[0].ToLower();
      positionInWorkFile = 0;
    }
    return nfArr;
  }
  public void Save()
  {
    using (var mutex = new Mutex(false, synchroName))
    {
      // acquire the mutex (or timeout after 1 seconds)
      var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
      if (!st)
        throw new Exception("Raw2TrkConfig.Save Impossible lock mutex [" + synchroName + "]");
      try
      {
        Utils.WriteToBinaryFile<RawProcessState>(curStateFileName, this
            , synchroName: null
            , append: false
            );
      }
      catch (Exception ex)
      {
        throw new Exception("Raw2TrkConfig.Save  mutex[" + synchroName + "] created, locked but: " + ex.Message);
      }
      finally
      {
        mutex.ReleaseMutex();
      }
    }
  }
  public void Load()
  {
    using (var mutex = new Mutex(false, synchroName))
    {
      // acquire the mutex (or timeout after 1 seconds)
      var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
      if (!st)
        throw new Exception("Raw2TrkConfig.Load Impossible lock mutex [" + synchroName + "]");
      try
      {
        long poz = 0;
        var rst = Utils.ReadFromBinaryFile<RawProcessState>(curStateFileName
            , ref poz
            , null
            );
        if (rst != null)
        {
          this.workRawFileName = rst.workRawFileName;
          this.positionInWorkFile = rst.positionInWorkFile;
        }
      }
      catch (Exception ex)
      {
        throw new Exception("Raw2TrkConfig.Load  mutex[" + synchroName + "] created, locked but: " + ex.Message);
      }
      finally
      {
        mutex.ReleaseMutex();
      }
    }
  }
}

