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

public class RawFilePointer
{
    public long fNom;
    public long poz;

    public RawFilePointer()
    { }
    public RawFilePointer(RawFilePointer rfp)
    { fNom = rfp.fNom; poz = rfp.poz; }

    public static int Compare(RawFilePointer rfp1, RawFilePointer rfp2) // RawFilePointer.Compare
    {
        var v = rfp1.fNom - rfp2.fNom;
        if (v == 0)
            v = rfp1.poz - rfp2.poz;
        return (int)v;
    }
}
