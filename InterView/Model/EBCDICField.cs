using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FadePlus.Model
{
    public class EBCDICField : Field
    {
        private string Value;

        public EBCDICField(int length, string name, int startPosition, bool isSub, bool isNullable) 
            : base(typeof(string), length, name, startPosition, isSub, isNullable) { }

        public override void ReadValue(FileStream dataStream)
        {
            // data buffer
            byte[] buffer = new byte[size];

            dataStream.Read(buffer, 0, size);

            Value = JonSkeet.Ebcdic.EbcdicEncoding.GetEncoding("EBCDIC-US").GetString(buffer);
        }

        public override bool Equals(string query)
        {
            return (query.Trim() == Value.Trim());
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
