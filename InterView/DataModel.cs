//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.IO;
//using System.Globalization;

//namespace FadePlus
//{
//    public class Mask
//    {
//        // To hold each field as described in the DotOut
//        public List<RecordField> fields { get; set; }
//        public int FieldCount { get; set; }
//        public int RecordLength { get; set; }
//        public int maxNameLength
//        {
//            get
//            {
//                int m = 0;
//                foreach (var field in fields)
//                    if ((field.Name).Trim().Length > m)
//                        m = field.Name.Length;
//                return m;
//            }
//        }
//        public Mask()
//        {
//            fields = new List<RecordField>();
//            FieldCount = 0;
//            RecordLength = 0;
//            this.MaxRecLength = 30;
//        }
//        public int oracount { get; set; }
//        public int MaxRecLength { get; set; }
//        public bool IsEBCDIC { get; set; }

//        public override string ToString()
//        {
//            StringBuilder sb = new StringBuilder(500);

//            foreach (var field in this.fields)
//            {
//                sb.AppendFormat(" {0,4} {1,25} {2,7} {3,4}\n", field.Id, field.Name, field.DataType, field.Size);
//            }
//            return sb.ToString();
//        }
//    }

//    public enum FieldType { CHAR, SMALLINT, INTEGER, DOUBLE, LONG, EBCDIC, DATE, VARCHAR };

//    public class RecordField
//    {

//        public string Name { get; set; }
//        public int Start { get; set; }
//        public int Size { get; set; }
//        public FieldType DataType { get; set; }
//        public long Id { get; set; }
//        public bool CanNull { get; set; }
//        public int Padding { get; set; }
//        public int Longest { get; set; }

//        public RecordField(string fieldName)
//        {
//            this.Id = 0;
//            this.Name = fieldName;
//            this.Start = 0;
//            this.Size = 0;
//            this.DataType = FieldType.CHAR;
//            this.Longest = 0;
//        }

//        public override bool Equals(object obj)
//        {
//            return base.Equals(obj);
//        }

//        public string Format { get; set; }
//    }

//}
