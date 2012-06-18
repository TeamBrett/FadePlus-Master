using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.IO;

namespace FadePlus.Model
{
    public class DateTimeField : Field
    {
        private DateTime Value;
        public string DateFormat = "yyyyMMddHHmmss";

        public DateTimeField(int length, string name, int startPosition, bool isSub, bool isNullable) 
            : base(typeof(string), length, name, startPosition, isSub, isNullable) { }

        public override void ReadValue(FileStream dataStream)
        {
            base.ReadValue(dataStream);

            // data buffer
            byte[] buffer = new byte[size];

            dataStream.Read(buffer, 0, size);

            // Get the Date format provider
            DateTimeFormatInfo dtfi = new CultureInfo(CultureInfo.CurrentCulture.LCID, false).DateTimeFormat;

            // get the string value
            string date = System.Text.ASCIIEncoding.ASCII.GetString(buffer);

            // Parse it
            try { Value = DateTime.ParseExact(date, "yyyyMMddHHmmss", dtfi); }
            catch { }

        }

        public override bool Equals(string query)
        {
            DateTimeFormatInfo dtfi = new CultureInfo(CultureInfo.CurrentCulture.LCID, false).DateTimeFormat;
            return (DateTime.ParseExact(query.Trim(), DateFormat, dtfi) == Value);
        }

        public override string ToString()
        {
            return Value.ToString(DateFormat);
        }
    }
}

