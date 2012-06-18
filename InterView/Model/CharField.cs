using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FadePlus.Model
{

    public class CharField : Field
    {
        private string Value;

        public CharField(int length, string name, int startPosition, bool isSub, bool isNullable)
            : base(typeof(string), length, name, startPosition, isSub, isNullable)
        {
        }

        public override void ReadValue(FileStream dataStream)
        {
            base.ReadValue(dataStream);

            // data buffer
            byte[] buffer = new byte[size];

            // Read our data into the buffer
            dataStream.Read(buffer, 0, size);
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
