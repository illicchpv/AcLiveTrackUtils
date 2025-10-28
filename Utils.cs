using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

//namespace TrackerUtils
//{
public delegate void LogWriteDelegateType(string msg, int type); // type 0 - at 10 - log 20- warn 30- err 40- except
public delegate void ReadTTrackListFromRaw_CallBack(List<TPoint> tpl);
public delegate void ReadTTrackListFromRawTLP_CallBack(TPointRec[] tpr);
//// tst
//public delegate void functionDelegateType(int x);


public partial class Utils
{
    public static int WaitOneInterval = 60 * 1000; // 60 sec // Utils.WaitOneInterval

    private static Random rnd = new Random();
    private static object _UtilsLocker = new object();
    private static object _ReadWriteLock = new object();

    public static LogWriteDelegateType WriteCallBack = null;  // Utils.WriteCallBack
                                                              //// tst
                                                              //public static functionDelegateType myCallBack = null;
                                                              //public static void doLog(string msg, functionDelegateType func)  // Utils.doLog
                                                              //{
                                                              //    func(5);
                                                              //    if(myCallBack != null)
                                                              //        myCallBack(3);
                                                              //}

    public static void Write(string msg)
    {
        lock (_UtilsLocker)
        {
            Console.Write(msg);
        }

    }
    public static void WriteLine(string msg)
    {
        lock (_UtilsLocker)
        {
            Console.Write(msg + "\r\n");
        }

    }

        // <add key="MinLogType" value="30" />        <!-- type 0 - at 10 - log 20- warn [30]- err 40- except -->
        private static int? _MinLogType = null;
        public static int MinLogTypeDefault = 1;
        public static int MinLogType // Utils.MinLogType
        { 
            get
            {
                if (_MinLogType == null)
                { // type 0 - at 10 - log 20- warn 30- err 40- except
                    return MinLogTypeDefault;
                }
                return (int)_MinLogType;
            }
        set
        {
            _MinLogType = value;
        }
    }

    private static void WriteLine(string msg, int type)  // Utils.WriteLine
    {
        if(type >= MinLogType && WriteCallBack == null)
        {
            Debugger.Log(50, null, msg + "\r\n");
        }
        Write(msg, type);
    }
    private static void Write(string msg, int type)  // Utils.WriteLine
    {
        if(type >= MinLogType)
        {
            if (WriteCallBack == null)
            {
                lock (_UtilsLocker)
                {
                    Console.Write(msg);
                }
            }
            else
            {
                WriteCallBack(msg, type);
            }
        }
    }

    internal static string WriteLogAt(string msg)  // Utils.WriteAt
    {
        var m = "tuLogAt -- " + msg;
        WriteLine(m, 0);
        return m;
    }
    internal static string WriteLog(string msg)  // Utils.WriteLog
    {
        var m = "tuLog -- " + msg;
        WriteLine(m, 10);
        return m;
    }
    internal static string WriteWarning(string msg)  // Utils.WriteWarning
    {
        var m = "tuWarning -- " + msg;
        WriteLine(m, 20);
        return m;
    }
    internal static string WriteError(string msg)  // Utils.WriteError
    {
        var m = "tuError -- " + msg;
        WriteLine(m, 30);
        return m;
    }
    internal static string WriteException(string msg, Exception ex)  // Utils.WriteException
    {
        var m = "tuException -- " + msg + (ex == null ? "" : ("[" + ex.Message + "]"));
        WriteLine(m, 40);
        return m;
    }

    public static string RandomString(int length)  // Utils.RandomString
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        lock (_UtilsLocker)
        {
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[rnd.Next(s.Length)]).ToArray());
        }
    }
    public static int getRnd(int from, int to)  // Utils.getRnd
    {
        lock (_UtilsLocker)
        {
            return rnd.Next(from, to);
        }
    }

    #region ---------------------- utils for binary file read\write
    public static byte[] StructureToByteArray<T>(T str, int cnt = 1) where T : struct
    {
        int size = Marshal.SizeOf(str) * cnt;
        byte[] arr = new byte[size];
        IntPtr ptr = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(str, ptr, true);
        Marshal.Copy(ptr, arr, 0, size);
        Marshal.FreeHGlobal(ptr);
        return (byte[])arr;
    }
    //private byte[] getBytes(ITPoint aux)
    //{
    //    int length = Marshal.SizeOf(aux);
    //    IntPtr ptr = Marshal.AllocHGlobal(length);
    //    byte[] myBuffer = new byte[length];

    //    Marshal.StructureToPtr(aux, ptr, true);
    //    Marshal.Copy(ptr, myBuffer, 0, length);
    //    Marshal.FreeHGlobal(ptr);

    //    return myBuffer;
    //}
    public static unsafe T ByteArrayToStructure<T>(byte[] bytes) //where T : struct  // Utils
    {
        fixed (byte* ptr = &bytes[0])
        {
            return (T)Marshal.PtrToStructure((IntPtr)ptr, typeof(T));
            //Marshal.StructureToPtr()
        }
    }
    //private static unsafe ITPoint[] ConvertToITPointArr(byte[] data, int structCount)
    //{
    //    int structSize = Utils.SizeOfITP;

    //    var arr = new ITPoint[structCount];
    //    fixed (byte* pData = &data[0])
    //    {
    //        IntPtr ptrDest = Marshal.AllocHGlobal(structSize * structCount);
    //        Marshal.StructureToPtr(arr[0], ptrDest, true);
    //        Marshal.Copy(data, ptrDest, 0, structSize * structCount);

    //        for (int i = 0; i < structCount; i++)
    //        {
    //            arr[i] = *(ITPoint*)(pData + (structSize * i));
    //        }
    //    }
    //    return arr;
    //}
    public static unsafe ITPoint[] Convert(byte[] buffer)  // Utils.Convert
    {
        ITPoint[] v = new ITPoint[buffer.Length / Utils.SizeOfITP];
        fixed (byte* pBuffer = buffer)
        {
            for (int i = 0; i < v.Length; i++)
                v[i] = ((ITPoint*)pBuffer)[i];
        }
        return v;
    }
    public static byte[] Convert(ITPoint[] itpArr)  // unsafe Utils.Convert
    {
        var sz = Utils.SizeOfITP;
        byte[] buffer = new byte[sz * itpArr.Length];
        for (int i = 0; i < itpArr.Length; i++)
        {
            var b = StructureToByteArray<ITPoint>(itpArr[i]);
            b.CopyTo(buffer, i * sz);
        }
        return buffer;
    }
    public static byte[] Convert(ITPoint itp)  // unsafe Utils.Convert
    {
        var sz = Utils.SizeOfITP;
        byte[] buffer = new byte[Utils.SizeOfITP];
        var b = StructureToByteArray<ITPoint>(itp);
        b.CopyTo(buffer, 0);
        return buffer;
    }
    private static int? _SizeOfITP = null;
    public static int SizeOfITP // Utils.SizeOfITP
    {
        get
        {
            if(_SizeOfITP == null)
                _SizeOfITP = Marshal.SizeOf(typeof(ITPoint));
            return (int)_SizeOfITP;
        }
    }

    private static int? _SizeOfTPR = null;
    public static int SizeOfTPR // Utils.SizeOfTPR
    {
        get
        {
            if(_SizeOfTPR == null)
                _SizeOfTPR = Marshal.SizeOf(typeof(TPointRec));
            return (int)_SizeOfTPR;
        }
    }
    public static unsafe TPointRec[] ConvertTPR(byte[] buffer)  // Utils.ConvertTPR
    {
        TPointRec[] v = new TPointRec[buffer.Length / Utils.SizeOfTPR];
        fixed (byte* pBuffer = buffer)
        {
            for (int i = 0; i < v.Length; i++)
                v[i] = ((TPointRec*)pBuffer)[i];
        }
        return v;
    }
    public static byte[] ConvertTPR(TPointRec[] tprArr)  // unsafe Utils.ConvertTPR
    {
        var sz = Utils.SizeOfTPR;
        byte[] buffer = new byte[sz * tprArr.Length];
        for (int i = 0; i < tprArr.Length; i++)
        {
            var b = StructureToByteArray<TPointRec>(tprArr[i]);
            b.CopyTo(buffer, i * sz);
        }
        return buffer;
    }
    //public static byte[] ConvertItps(ITPointS itps)  // unsafe Utils.ConvertItps
    //{
    //    var sz = (Marshal.SizeOf(typeof(ITPointS)));
    //    byte[] buffer = new byte[sz];
    //    var b = StructureToByteArray<ITPointS>(itps);
    //    b.CopyTo(buffer, 0);
    //    return buffer;
    //}
    //public static unsafe ITPointS[] ConvertItps(byte[] buffer)  // Utils.ConvertItps
    //{
    //    ITPointS[] v = new ITPointS[buffer.Length / (Marshal.SizeOf(typeof(ITPointS)))];
    //    fixed (byte* pBuffer = buffer)
    //    {
    //        for (int i = 0; i < v.Length; i++)
    //            v[i] = ((ITPointS*)pBuffer)[i];
    //    }
    //    return v;
    //}
    #endregion ---------------------- utils for binary file read\write

    #region ---------------------- binary file read\write
    public static void WriteToBinaryFile<T>(string filePath, T objectToWrite, string synchroName, bool append = false)  // Utils.WriteToBinaryFile
    {
        if (string.IsNullOrEmpty(synchroName))
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write))
            {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, objectToWrite);
            }
        }
        else
        {
            using (var mutex = new Mutex(false, synchroName))
            {
                // acquire the mutex (or timeout after 1 seconds)
                var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
                if (!st)
                    throw new Exception("WriteToBinaryFile Imposible lock mutex [" + synchroName + "]");
                try
                {
                    using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create, FileAccess.Write))
                    {
                        var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        binaryFormatter.Serialize(stream, objectToWrite);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("WriteToBinaryFile mutex4 [" + synchroName + "] created and: " + ex.Message);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }

        //lock (_ReadWriteLock)
        //{
        //    using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
        //    {
        //        var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //        binaryFormatter.Serialize(stream, objectToWrite);
        //    }
        //}
    }

    public static T ReadFromBinaryFile<T>(string filePath, ref long poz, string synchroName, Type type = null)  // Utils.ReadFromBinaryFile
    {
        if (string.IsNullOrEmpty(synchroName))
        {
            using (Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                if (poz >= stream.Length)
                    return default(T);
                stream.Position = poz;
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var r = (T)binaryFormatter.Deserialize(stream);
                poz = stream.Position;
                return r;
            }
        }
        else
        {
            using (var mutex = new Mutex(false, synchroName))
            {
                // acquire the mutex (or timeout after 1 seconds)
                var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
                if (!st)
                    throw new Exception("ReadFromBinaryFile Imposible lock mutex [" + synchroName + "]");
                try
                {
                    using (Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read))
                    {
                        if (poz >= stream.Length)
                            return default(T);
                        stream.Position = poz;
                        var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        // неудачная попытка зачитать файл сохранённый в другой программе
                        //if (type != null)
                        //{
                        //    binaryFormatter.Binder = new PreMergeToMergedDeserializationBinder() { type = type };
                        //}
                        var r = (T)binaryFormatter.Deserialize(stream);
                        poz = stream.Position;
                        return r;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("ReadFromBinaryFile mutex5 [" + synchroName + "] created and: " + ex.Message);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
        //lock (_ReadWriteLock)
        //{
        //    using (Stream stream = File.Open(filePath, FileMode.Open))
        //    {
        //        if (poz >= stream.Length)
        //            return default(T);
        //        stream.Position = poz;
        //        var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
        //        // неудачная попытка зачитать файл сохранённый в другой программе
        //        //if (type != null)
        //        //{
        //        //    binaryFormatter.Binder = new PreMergeToMergedDeserializationBinder() { type = type };
        //        //}
        //        var r = (T)binaryFormatter.Deserialize(stream);
        //        poz = stream.Position;
        //        return r;
        //    }
        //}
    }
    // неудачная попытка зачитать файл сохранённый в другой программе
    //sealed class PreMergeToMergedDeserializationBinder : SerializationBinder
    //{
    //    public Type type = null;
    //    public string fmt = null;
    //    public override Type BindToType(string assemblyName, string typeName)
    //    {
    //        return type;

    //        Type typeToDeserialize = null;

    //        // For each assemblyName/typeName that you want to deserialize to
    //        // a different type, set typeToDeserialize to the desired type.
    //        String exeAssembly = Assembly.GetExecutingAssembly().FullName;

    //        // The following line of code returns the type.
    //        //typeToDeserialize = Type.GetType(fmt);
    //        //typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
    //        //    typeName, exeAssembly));

    //        return typeToDeserialize;
    //    }
    //}
    #endregion ---------------------- binary file read\write

    public static string fixedStr(int nom, int len)  // Utils.fixedStr
    {
        return nom.ToString().PadLeft(len, '0');
    }

    public static string GetConfigStr(string key, string defaultVal) // Utils.GetConfigStr
    {
        string v = ConfigurationManager.AppSettings[key];
        if (string.IsNullOrWhiteSpace(v))
            return defaultVal;
        return v;
    }
    public static long GetConfigLong(string key, long defaultVal) // Utils.GetConfigLong
    {
        string v = ConfigurationManager.AppSettings[key];
        if (string.IsNullOrWhiteSpace(v))
            return defaultVal;
        v = v.Trim();
        long i = defaultVal;
        if (!long.TryParse(v, out i))
            return defaultVal;
        else
            return i;
    }
    public static bool GetConfigBool(string key, bool defaultVal) // Utils.GetConfigBool
    {
        var vs = GetConfigStr(key, "0").ToLower();
        return (vs == "1" || vs == "true" || vs == "yes") ? true : false;
    }

    public static unsafe void Copy_all(ref TPointRec from, ref TPointRec to) // Utils.Copy_imei
    {
        fixed (byte* bf = from.imei_buf)
        {
            fixed(byte* bt = to.imei_buf)
            {
                for (int i=0; i<TPoint.TPointRecSize(); i++)
                    bt[i] = bf[i];
            }
        }
    }
    public static unsafe void Copy_imei(ref TPointRec from, ref TPointRec to) // Utils.Copy_imei
    {
        fixed (byte* bf = from.imei_buf)
        {
            fixed(byte* bt = to.imei_buf)
            {
                for (int i=0; i<TPoint.TPointImeiMaxLen2; i++)
                    bt[i] = bf[i];
            }
        }
    }
    public static unsafe void Set_imei(ref TPointRec r, string imei) // Utils.Set_imei
    {
        var bytes = Encoding.UTF8.GetBytes(imei);
        var len = Math.Min(TPoint.TPointImeiMaxLen, bytes.Length);

        fixed (byte* b = r.imei_buf)
        {
            for (int i=0; i<len; i++)
                b[i] = bytes[i];
            b[TPoint.TPointImeiMaxLen] = 0;
            b[TPoint.TPointImeiMaxLen1] = (byte)len;
        }
    }
    public static unsafe string Get_imei(ref TPointRec r) // Utils.Get_imei
    {
        byte[] bytes;
        fixed (byte* b = r.imei_buf)
        {
            var len = (int)b[TPoint.TPointImeiMaxLen1];
            bytes = new byte[len];
            for (int i=0; i<len; i++)
                bytes[i] = b[i];
        }
        return Encoding.UTF8.GetString(bytes);

        //byte *ptr = r.imei_buf;
        //for (int i=0; i<len; i++)
        //{
        //    arr[i] = ptr[i];
        //}
        //var decodeString = Encoding.UTF8.GetString(arr);
        //return decodeString;

        //Marshal.Copy((IntPtr)r.imei_buf, arr, 0, len);
        //var decodeString = Encoding.UTF8.GetString(arr);
        //return decodeString;
    }

    public static void checkUtilsLog()
    {
        Utils.WriteLogAt("checkUtilsLog WriteLogAt");
        Utils.WriteLog("checkUtilsLog WriteLog");
        Utils.WriteWarning("checkUtilsLog WriteWarning"); 
        Utils.WriteError("checkUtilsLog WriteError");
        try
        {
            throw new Exception("test Exception");
        }catch(Exception ex)
        {
            Utils.WriteException("checkUtilsLog WriteException", ex);
        }

    }
}
