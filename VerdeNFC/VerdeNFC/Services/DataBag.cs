using System;
using System.Collections.Generic;
using System.Text;

namespace VerdeNFC.Services
{
    public class DataBag : IDataBag
    {
        byte[] _mem;

        public DataBag()
        {
            _mem = new byte[80];
        }

        public byte [] GetData()
        {
            return _mem;
        }

        public void SetData(byte[] data)
        {
            _mem=data;
        }

    }
}
