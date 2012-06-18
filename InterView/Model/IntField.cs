using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FadePlus.Model
{
    public class IntField : Field
    {
        private int Value;

        public IntField(string name, int startPosition, bool isSub, bool isNullable)
            : base(typeof(int), sizeof(int), name, startPosition, isSub, isNullable)
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

            Value = BitConverter.ToInt32(buffer, 0);
        }

        public override bool Equals(string query)
        {
            int hold;
            if (Int32.TryParse(query, out hold))
                return (hold == Value);
            else return false;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
