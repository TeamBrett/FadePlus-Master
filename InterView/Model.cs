//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.Globalization;
//using System.IO;

//namespace FadePlus
//{
//    class Model
//    {
//        //public class Record : List<Field>
//        //{
//        //    public int RecLength { get { return reclen; } }
//        //    int reclen;
//        //    public bool ShowEnds { get; set; }
//        //    public bool ShowAscii { get; set; }
//        //    public string LeftEnd { get; set; }
//        //    public string RightEnd { get; set; }
//        //    public bool doubleSAKS2long { get; set; }

//        //    public Record Copy()
//        //    {
//        //        Record newrecord = new Record();
//        //        Field[] fields = new Field[this.Count];

//        //        this.CopyTo(fields);

//        //        foreach (var item in fields)
//        //        {
//        //            newrecord.Add(item);
//        //        }

//        //        newrecord.reclen = RecLength;

//        //        return newrecord;
//        //    }

//        //    public Field AddField(string name, int startPos, string typeToken, int size)
//        //    {
//        //        Field newField;
//        //        switch (typeToken.ToUpper())
//        //        {
//        //            case "CHAR":
//        //                newField = new CharField(size, name, startPos);
//        //                break;
//        //            case "VARCHAR":
//        //                newField = new VarCharField(size, name, startPos);
//        //                break;
//        //            case "SMALLINT":
//        //                newField = new ShortField(name, startPos);
//        //                break;
//        //            case "INTEGER":
//        //                newField = new IntField(name, startPos);
//        //                break;
//        //            case "INT":
//        //                newField = new IntField(name, startPos);
//        //                break;
//        //            case "DOUBLE":
//        //                newField = new DoubleField(name, startPos);
//        //                break;
//        //            case "LONG":
//        //                newField = new LongField(name, startPos);
//        //                break;
//        //            case "BIGINT":
//        //                newField = new LongField(name, startPos);
//        //                break;
//        //            case "EBCDIC":
//        //                newField = new EBCDICField(size, name, startPos);
//        //                break;
//        //            case "DATE":
//        //                newField = new DateTimeField(size, name, startPos);
//        //                break;
//        //            default:
//        //                return null;
//        //        }
//        //        reclen += newField.Size;
//        //        this.Add(newField);
//        //        return newField;
//        //    }

//        //    public string ToString(int NamePad, int ValuePad)
//        //    {
//        //        StringBuilder sb = new StringBuilder();
//        //        int valuePad = 0;
//        //        int namePad = 0;

//        //        foreach (var field in this)
//        //        {
//        //            if (field.Name.Length > namePad) namePad = field.Name.Length;
//        //            if (ShowEnds)
//        //            {
//        //                if ((LeftEnd.Length + RightEnd.Length + field.ToString().Length) > valuePad)
//        //                    valuePad = LeftEnd.Length + RightEnd.Length + field.ToString().Length;
//        //            }
//        //            else
//        //            {
//        //                if ((field.ToString().Length) > valuePad)
//        //                    valuePad = field.ToString().Length;
//        //            }
//        //        }

//        //        foreach (var field in this)
//        //        {
//        //            string v;
//        //            if (ShowEnds)
//        //                v = String.Format("{0}{1}{2}", LeftEnd, field.ToString(), RightEnd);
//        //            else v = field.ToString();

//        //            sb.AppendFormat("{0,-" + NamePad + "} {1,-" + ValuePad + "}{2,-900}\n",
//        //                                field.Name,
//        //                                v,
//        //                                ShowAscii ? GenerateASCII(field.ToString()) : ""
//        //                                );
//        //        }
//        //        return sb.ToString();
//        //    }

//        //    public string TemplateToString()
//        //    {
//        //        StringBuilder sb = new StringBuilder(500);

//        //        int i = 1;
//        //        foreach (var field in this)
//        //        {
//        //            sb.AppendFormat(" {0,4} {1,25} {2,7} {3,4}\n", i++, field.Name, field.type.Name, field.Size);
//        //        }
//        //        return sb.ToString();
//        //    }

//        //    private static string GenerateASCII(string ivalue)
//        //    {
//        //        // this'll make it quicker
//        //        StringBuilder nvalue = new StringBuilder(1000);
//        //        nvalue.Append("\t\t");
//        //        foreach (var c in ivalue)
//        //        {
//        //            nvalue.Append(" ");
//        //            nvalue.Append(((int)c).ToString().PadLeft(3));
//        //        }
//        //        return nvalue.ToString();
//        //    }
//        //}

//        //public class DataFile
//        //{
//        //    public string filename;
//        //    public long filesize;
//        //    public int Id;
//        //    public int RecordCount;
//        //    public FileStream dataStream;
//        //}

//        //#region Field Types

//        //public abstract class Field
//        //{
//        //    public Type type { get; set; }
//        //    public int Size { get; set; }
//        //    public string Name { get; set; }
//        //    public int startPosition { get; set; }
//        //    public bool isNullable { get; set; }

//        //    public abstract void ReadValue(FileStream dataStream);// { }

//        //    public abstract override string ToString();

//        //    public abstract bool Equals(string query);

//        //    public Field(Type typ, int length, string name, int startPosition)
//        //    {
//        //        this.type = typ;
//        //        this.Size = length;
//        //        this.Name = name;
//        //        this.startPosition = startPosition;
//        //    }
//        //}

//        //public class CharField : Field
//        //{
//        //    private string Value;

//        //    public CharField(int length, string name, int startPosition)
//        //        : base(typeof(string), length, name, startPosition)
//        //    {
//        //    }

//        //    public override void ReadValue(FileStream dataStream)
//        //    {
//        //        // data buffer
//        //        byte[] buffer = new byte[Size];

//        //        // Read our data into the buffer
//        //        dataStream.Read(buffer, 0, Size);
//        //        Value = System.Text.ASCIIEncoding.ASCII.GetString(buffer);
//        //    }

//        //    public override bool Equals(string query)
//        //    {
//        //        return (query.Trim() == Value.Trim());
//        //    }

//        //    public override string ToString()
//        //    {
//        //        return Value;
//        //    }
//        //}
//        //public class ShortField : Field
//        //{
//        //    private short Value;

//        //    public ShortField(string name, int startPosition)
//        //        : base(typeof(short), sizeof(short), name, startPosition)
//        //    {

//        //    }

//        //    public override void ReadValue(FileStream dataStream)
//        //    {
//        //        // data buffer
//        //        byte[] buffer = new byte[Size];

//        //        // Read our data into the buffer
//        //        dataStream.Read(buffer, 0, Size);

//        //        // Check Endian
//        //        if (BitConverter.IsLittleEndian)
//        //            Array.Reverse(buffer);

//        //        Value = BitConverter.ToInt16(buffer, 0);
//        //    }

//        //    public override bool Equals(string query)
//        //    {
//        //        short hold;
//        //        if (Int16.TryParse(query, out hold))
//        //            return (hold == Value);
//        //        else return false;
//        //    }

//        //    public override string ToString()
//        //    {
//        //        return Value.ToString();
//        //    }
//        //}
//        //public class IntField : Field
//        //{
//        //    private int Value;

//        //    public IntField(string name, int startPosition)
//        //        : base(typeof(int), sizeof(int), name, startPosition)
//        //    {
//        //    }

//        //    public override void ReadValue(FileStream dataStream)
//        //    {
//        //        // data buffer
//        //        byte[] buffer = new byte[Size];

//        //        // Read our data into the buffer
//        //        dataStream.Read(buffer, 0, Size);

//        //        // Check Endian
//        //        if (BitConverter.IsLittleEndian)
//        //            Array.Reverse(buffer);

//        //        Value = BitConverter.ToInt32(buffer, 0);
//        //    }

//        //    public override bool Equals(string query)
//        //    {
//        //        int hold;
//        //        if (Int32.TryParse(query, out hold))
//        //            return (hold == Value);
//        //        else return false;
//        //    }

//        //    public override string ToString()
//        //    {
//        //        return Value.ToString();
//        //    }
//        //}
//        public class DoubleField : Field
//        {
//            private double Value;
//            //public bool AsLong = false;

//            public DoubleField(string name, int startPosition)
//                : base(typeof(double), sizeof(double), name, startPosition) { }

//            public override void ReadValue(FileStream dataStream)
//            {
//                // data buffer
//                byte[] buffer = new byte[Size];

//                // Read our data into the buffer
//                dataStream.Read(buffer, 0, Size);

//                // Check Endian
//                if (BitConverter.IsLittleEndian)
//                    Array.Reverse(buffer);

//                //if (AsLong)
//                //    Value = (long)BitConverter.ToDouble(buffer, 0);
//                //else
//                Value = BitConverter.ToDouble(buffer, 0);
//            }

//            public override bool Equals(string query)
//            {
//                //if (AsLong)
//                //{
//                double hold;
//                if (Double.TryParse(query, out hold))
//                    return (hold == Value);
//                else return false;
//                //}
//                //else
//                //{
//                //    long hold;
//                //    if (Int64.TryParse(query, out hold))
//                //        return (hold == Value);
//                //    else return false;
//                //}
//            }

//            public override string ToString()
//            {
//                return Value.ToString();
//            }
//        }
//        public class LongField : Field
//        {
//            private long Value;

//            public LongField(string name, int startPosition)
//                : base(typeof(long), sizeof(long), name, startPosition) { }

//            public override void ReadValue(FileStream dataStream)
//            {
//                // data buffer
//                byte[] buffer = new byte[Size];

//                // Read our data into the buffer
//                dataStream.Read(buffer, 0, Size);

//                // Check Endian
//                if (BitConverter.IsLittleEndian)
//                    Array.Reverse(buffer);

//                Value = BitConverter.ToInt64(buffer, 0);
//            }

//            public override bool Equals(string query)
//            {
//                long hold;
//                if (Int64.TryParse(query, out hold))
//                    return (hold == Value);
//                else return false;
//            }

//            public override string ToString()
//            {
//                return Value.ToString();
//            }
//        }
//        public class VarCharField : Field
//        {
//            private string Value;

//            public VarCharField(int length, string name, int startPosition) : base(typeof(string), length, name, startPosition) { }

//            public override void ReadValue(FileStream dataStream)
//            {
//                // data buffer
//                byte[] buffer = new byte[sizeof(short)];

//                // Read our data into the buffer
//                dataStream.Read(buffer, 0, sizeof(short));

//                // Check Endian
//                if (BitConverter.IsLittleEndian)
//                    Array.Reverse(buffer);

//                short first2 = BitConverter.ToInt16(buffer, 0);
//                if (first2 < 0)
//                    throw new Exception("Bad VARCHAR read");

//                buffer = new byte[first2];

//                dataStream.Read(buffer, 0, first2);

//                Value = System.Text.ASCIIEncoding.ASCII.GetString(buffer);
//            }

//            public override bool Equals(string query)
//            {
//                return (query.Trim() == Value.Trim());
//            }

//            public override string ToString()
//            {
//                return Value;
//            }
//        }
//        public class EBCDICField : Field
//        {
//            private string Value;

//            public EBCDICField(int length, string name, int startPosition) : base(typeof(string), length, name, startPosition) { }

//            public override void ReadValue(FileStream dataStream)
//            {
//                // data buffer
//                byte[] buffer = new byte[Size];

//                dataStream.Read(buffer, 0, Size);

//                Value = JonSkeet.Ebcdic.EbcdicEncoding.GetEncoding("EBCDIC-US").GetString(buffer);
//            }

//            public override bool Equals(string query)
//            {
//                return (query.Trim() == Value.Trim());
//            }

//            public override string ToString()
//            {
//                return Value;
//            }
//        }
//        public class DateTimeField : Field
//        {
//            private DateTime Value;
//            string readformatter = "yyyyMMddHHmmss";

//            public DateTimeField(int length, string name, int startPosition) : base(typeof(string), length, name, startPosition) { }

//            public override void ReadValue(FileStream dataStream)
//            {
//                // data buffer
//                byte[] buffer = new byte[Size];

//                dataStream.Read(buffer, 0, Size);

//                // Get the Date format provider
//                DateTimeFormatInfo dtfi = new CultureInfo(CultureInfo.CurrentCulture.LCID, false).DateTimeFormat;

//                // get the string value
//                string date = System.Text.ASCIIEncoding.ASCII.GetString(buffer);

//                // Parse it
//                try { Value = DateTime.ParseExact(date, readformatter, dtfi); }
//                catch { }

//            }

//            public override bool Equals(string query)
//            {
//                DateTimeFormatInfo dtfi = new CultureInfo(CultureInfo.CurrentCulture.LCID, false).DateTimeFormat;
//                return (DateTime.ParseExact(query.Trim(), readformatter, dtfi) == Value);
//            }

//            public override string ToString()
//            {
//                return Value.ToString("MM/DD/YYY");
//            }
//        }

//        #endregion
//    }
//}
