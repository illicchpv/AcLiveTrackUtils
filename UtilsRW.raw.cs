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
            if(!st)
                throw new Exception("ReadTPRFromRawFile Imposible lock mutex [" + synchroName + "]");
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
        if(tprArr == null || tprArr.Length == 0)
            return null;
        if(callBack != null)
            callBack(tprArr);
        return 1;

        //int? readCount = null;
        //using (var mutex = new Mutex(false, synchroName))
        //{
        //    var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
        //    if(!st)
        //        throw new Exception("ReadTTrackListFromRaw Imposible lock mutex [" + synchroName + "]");
        //    try
        //    {
        //        using (Stream stream = File.Open(filePath, FileMode.Open))
        //        {
        //            if (poz >= stream.Length)
        //                return readCount;
        //            stream.Position = poz;

        //            readCount = 0;
        //            do
        //            {
        //                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

        //                List<TPoint> r = null;
        //                try
        //                {
        //                    r = (List<TPoint>)binaryFormatter.Deserialize(stream);
        //                }catch (Exception ex2)
        //                {
        //                    Utils.WriteException("ReadTTrackListFromRaw Deserialize",ex2);
        //                    poz = stream.Length;
        //                    return null;
        //                }
        //                if (r.Count > 0)
        //                {
        //                    readCount = 1;
        //                    callBack(r);
        //                }
        //                poz = stream.Position;

        //            } while (stream.Position < stream.Length);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //readCount = null;
        //        throw new Exception("ReadTTrackListFromRaw mutex2 [" + synchroName + "] created and: " + ex.Message);
        //    }
        //    finally
        //    {
        //        mutex.ReleaseMutex();
        //    }
        //}
        //return readCount;
    }

    public static void WriteArrToRawFile(string filePath, TPointRec[] tprArr, string synchroName)
    {
        Utils.WriteLogAt("WriteArrToRawFile");
        using (var mutex = new Mutex(false, synchroName))
        {
            // acquire the mutex (or timeout after 1 seconds)
            var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
            if(!st)
                throw new Exception("WriteArrToRawFile Imposible lock mutex [" + synchroName + "]");
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

#if false
#region // old version - System.Runtime.Serialization
    public static void WriteToRawFile(string filePath, List<TPoint> objectToWrite, string synchroName, bool append = false)  // Utils.WriteToBinaryFile
    {
        Utils.WriteLogAt("WriteToRawFile");

        if (string.IsNullOrEmpty(synchroName))
        {
            using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
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
                if(!st)
                    throw new Exception("WriteToRawFile Imposible lock mutex [" + synchroName + "]");
                try
                {
                    using (Stream stream = File.Open(filePath, append ? FileMode.Append : FileMode.Create))
                    {
                        var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        binaryFormatter.Serialize(stream, objectToWrite);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("WriteToRawFile mutex1 [" + synchroName + "] created and: " + ex.Message);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }
    
    /// <summary>
    /// return null если больше нечего читать или ошибка чтения
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="poz"></param>
    /// <param name="synchroName"></param>
    /// <param name="callBack"></param>
    /// <returns></returns>
    public static int? ReadTTrackListFromRaw(string filePath  // Utils.ReadTTrackListFromRaw
        , ref long poz
        , string synchroName
        , ReadTTrackListFromRaw_CallBack callBack
        )
    {
        Utils.WriteLogAt("ReadTTrackListFromRaw");

        if (string.IsNullOrEmpty(synchroName))
            throw new Exception("ReadTTrackListFromRaw illegal parameter: synchroName");
        if (string.IsNullOrEmpty(synchroName))
            throw new Exception("ReadTTrackListFromRaw illegal parameter: callBack");

        int? readCount = null;
        using (var mutex = new Mutex(false, synchroName))
        {
            var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
            if(!st)
                throw new Exception("ReadTTrackListFromRaw Imposible lock mutex [" + synchroName + "]");
            try
            {
                using (Stream stream = File.Open(filePath, FileMode.Open))
                {
                    if (poz >= stream.Length)
                        return readCount;
                    stream.Position = poz;

                    readCount = 0;
                    do
                    {
                        var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

                        List<TPoint> r = null;
                        try
                        {
                            r = (List<TPoint>)binaryFormatter.Deserialize(stream);
                        }catch (Exception ex2)
                        {
                            Utils.WriteException("ReadTTrackListFromRaw Deserialize",ex2);
                            poz = stream.Length;
                            return null;
                        }
                        if (r.Count > 0)
                        {
                            readCount = 1;
                            callBack(r);
                        }
                        poz = stream.Position;

                        //var r = (List<TPoint>)binaryFormatter.Deserialize(stream);
                        //if (r.Count > 0)
                        //{
                        //    readCount = 1;
                        //    callBack(r);
                        //}
                        //poz = stream.Position;
                    } while (stream.Position < stream.Length);
                }
            }
            catch (Exception ex)
            {
                //readCount = null;
                throw new Exception("ReadTTrackListFromRaw mutex2 [" + synchroName + "] created and: " + ex.Message);

                //var p = poz;
                //using (Stream stream2 = File.Open(filePath, FileMode.Open))
                //{
                //    List<TPoint> r = null;
                //    do
                //    {
                //        p++;
                //        try
                //        {
                //            stream2.Position = p;
                //            var bf = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                //            r = (List<TPoint>)bf.Deserialize(stream2);
                //        }
                //        catch (Exception ex2)
                //        {
                //        }
                //    }while(r == null && p < stream2.Length);
                //    poz = p;
                //    p=0;
                //}
            }
            finally
            {
                mutex.ReleaseMutex();
            }
        }
        return readCount;
    }

    public static List<TPoint> ReadBlockFromRawFile(string filePath, ref long poz, string synchroName, Type type = null)  // Utils.ReadFromBinaryFile
    {
        Utils.WriteLogAt("ReadBlockFromRawFile");

        if (string.IsNullOrEmpty(synchroName))
        {
            using (Stream stream = File.Open(filePath, FileMode.Open))
            {
                if (poz >= stream.Length)
                    return null;
                stream.Position = poz;
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                var r = (List<TPoint>)binaryFormatter.Deserialize(stream);
                poz = stream.Position;
                return r;
            }
        }
        else
        {
            using (var mutex = new Mutex(false, synchroName))
            {
                var st = mutex.WaitOne(Utils.WaitOneInterval, false); // ();
                if(!st)
                    throw new Exception("ReadBlockFromRawFile Imposible lock mutex [" + synchroName + "]");
                try
                {
                    using (Stream stream = File.Open(filePath, FileMode.Open))
                    {
                        if (poz >= stream.Length)
                            return null;
                        stream.Position = poz;
                        var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                        List<TPoint> r = null;
                        try
                        {
                            r = (List<TPoint>)binaryFormatter.Deserialize(stream);
                        }catch (Exception ex2)
                        {
                            Utils.WriteException("ReadBlockFromRawFile Deserialize",ex2);
                            poz = stream.Length;
                            return r;
                        }
                        poz = stream.Position;
                        return r;
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("ReadBlockFromRawFile mutex3 [" + synchroName + "] created and: " + ex.Message);
                }
                finally
                {
                    mutex.ReleaseMutex();
                }
            }
        }
    }

#endregion  // old version - System.Runtime.Serialization
#endif

}
