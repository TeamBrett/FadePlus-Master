using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FadePlus.Model
{
    public class DoubleField : Field
    {
        private double Value;
        //public bool AsLong = false;

        public DoubleField(string name, int startPosition, bool isSub, bool isNullable)
            : base(typeof(double), sizeof(double), name, startPosition, isSub, isNullable) { }

        public override void ReadValue(FileStream dataStream)
        {
            base.ReadValue(dataStream);

            // data buffer
            byte[] buffer = new byte[sizeof(double)];

            // Read our data into the buffer
            dataStream.Read(buffer, 0, sizeof(double));

            // Check Endian
            if (BitConverter.IsLittleEndian)
                Array.Reverse(buffer);

            //if (AsLong)
            //    Value = (long)BitConverter.ToDouble(buffer, 0);
            //else
            Value = BitConverter.ToDouble(buffer, 0);
        }

        public override bool Equals(string query)
        {
            //if (AsLong)
            //{
            double hold;
            if (Double.TryParse(query, out hold))
                return (hold == Value);
            else return false;
            //}
            //else
            //{
            //    long hold;
            //    if (Int64.TryParse(query, out hold))
            //        return (hold == Value);
            //    else return false;
            //}
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
