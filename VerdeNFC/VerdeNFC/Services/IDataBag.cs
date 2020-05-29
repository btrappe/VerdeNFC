using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace VerdeNFC.Services
{
    public interface IDataBag
    {
        byte [] GetData();
        void SetData(byte[] data);
    }
}
