using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FadePlus.Model
{
    public class LongField : Field
    {
        private long Value;

        public LongField(string name, int startPosition, bool isSub, bool isNullable)
            : base(typeof(long), sizeof(long), name, startPosition, isSub, isNullable) { }

        public override void ReadValue(FileStream dataStream)
        {
            base.ReadValue(dataStream);

            // data buffer
            byte[] buffer = new byte[size];

            // Read our data into the buffer
            dataStream.Read(buffer, 0, size);

            // Check Endian
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            Value = BitConverter.ToInt64(buffer, 0);
        }

        public override bool Equals(string query)
        {
            long hold;
            if (Int64.TryParse(query, out hold))
                return (hold == Value);
            else return false;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
