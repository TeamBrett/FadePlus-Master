using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FadePlus.Model
{
    public class ShortField : Field
    {
        private short Value;

        public ShortField(string name, int startPosition, bool isSub, bool isNullable)
            : base(typeof(short), sizeof(short), name, startPosition, isSub, isNullable)
        {

        }

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

            Value = BitConverter.ToInt16(buffer, 0);
        }

        public override bool Equals(string query)
        {
            short hold;
            if (Int16.TryParse(query, out hold))
                return (hold == Value);
            else return false;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
