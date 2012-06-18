using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FadePlus.Model
{
    public class VarCharField : Field
    {
        private string Value;

        public VarCharField(int length, string name, int startPosition, bool isSub, bool isNullable) 
            : base(typeof(string), length, name, startPosition, isSub, isNullable) { }

        public override void ReadValue(FileStream dataStream)
        {
            // null
            base.ReadValue(dataStream);
            // data buffer
            byte[] buffer = new byte[sizeof(short)];

            // Read our data into the buffer
            dataStream.Read(buffer, 0, sizeof(short));

            // Check Endian
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            short first2 = BitConverter.ToInt16(buffer, 0);
            if (first2 < 0)
                throw new Exception("Bad VARCHAR read");
            
            buffer = new byte[first2];

            dataStream.Read(buffer, 0, first2);

            dataStream.Seek(this.size - first2 - sizeof(short), SeekOrigin.Current);
            
            Value = System.Text.ASCIIEncoding.ASCII.GetString(buffer);
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
