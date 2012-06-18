using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FadePlus.Model
{
    public class FillerField : Field
    {
        private byte[] Value;

        public FillerField(int size, string name, int startPosition, bool isSub, bool isNullable)
            : base(typeof(object), size, name, startPosition, isSub, isNullable)
        {
        }

        public override void ReadValue(FileStream dataStream)
        {
            // data buffer
//            byte[] buffer = new byte[Size];

            // Read our data into the buffer
//            dataStream.Read(buffer, 0, Size);

            // Check Endian
//            if (BitConverter.IsLittleEndian)
  //              Array.Reverse(buffer);

//            Value = BitConverter.ToInt32(buffer, 0);
//            Value = buffer;
        }

        public override bool Equals(string query)
        {
//            Value.Equals(new BinaryReader(StringReader(query).arr
//            int hold;
//            if (Int32.TryParse(query, out hold))
//                return (hold == Value);
//            else return false;
            return false;
        }

        public override string ToString()
        {
            return "";
//            return Value.ToString();
        }
    }
}
