using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FadePlus.Model
{
    public class Record : List<Field>
    {
        public int RecLength 
        { 
            get
            {
                return reclen;
                //int totlen = reclen;
                //int templen = 0;
                //foreach (var template in SubTemplates)
                //{
                //    int sublen = template.Value.RecLength;

                //    // If it's the first cycle, set the templen for comparison later
                //    if (templen == 0)
                //        templen = sublen;
                //    else
                //    {
                //        // If the subtemplate length isn't the same as other templates, error
                //        if (templen != sublen)
                //        {
                //            throw new Exception("Subtemplates must still equal a total record length");
                //        }
                //        else templen = sublen;
                //    }
                //    totlen += sublen;
                //}
                
                //return totlen; 
            } 
        }
        
        int reclen;

        public bool ShowEnds { get { return showends; } set { showends = value; UpdateSubTemplateOptions(); } }
        private bool showends = false;

        public bool ShowAscii { get { return showascii; } set { showascii = value; UpdateSubTemplateOptions(); } }
        private bool showascii = false;

        public string LeftEnd { get { return leftend; } set { leftend = value; UpdateSubTemplateOptions() ;} }
        private string leftend = "[";

        public string RightEnd { get { return rightend; } set { rightend = value; UpdateSubTemplateOptions(); } }
        private string rightend = "]";

        public string DateFormat { get { return dateformat; } 
            set 
            { 
                dateformat = value; 
                UpdateSubTemplateOptions();
                foreach (var field in this)
                {
                    if(field.GetType() == typeof(DateTimeField))
                        ((DateTimeField)field).DateFormat = this.dateformat;
                }
            } 
        }
        private string dateformat = "yyyyMMddHHmmss";

        public bool doubleSAKS2long { get { return doublesaks2long; } set { doublesaks2long = value; UpdateSubTemplateOptions(); } }
        private bool doublesaks2long = false;
        
        string currentTemplate = String.Empty;
        private Dictionary<string, Record> SubTemplates = new Dictionary<string,Record>(0);
        
        public Record Copy()
        {
            Record newrecord = new Record();
            Field[] fields = new Field[this.Count];

            this.CopyTo(fields);

            foreach (var item in fields)
            {
                newrecord.Add(item);
            }

            newrecord.reclen = reclen;

            CopyTemplateOption(newrecord);
            
            return newrecord;
        }
        
        private void CopyTemplateOption(Record v)
        {
            v.ShowAscii = this.ShowAscii;
            v.ShowEnds = this.ShowEnds;
            v.LeftEnd = this.LeftEnd;
            v.RightEnd = this.RightEnd;
            v.doubleSAKS2long = this.doubleSAKS2long;
            v.DateFormat = this.dateformat;
        }

        private void UpdateSubTemplateOptions()
        {
            foreach (var template in SubTemplates)
	        {
                CopyTemplateOption(template.Value);
                //Record v = template.Value;
                //v.ShowAscii = this.ShowAscii;
                //v.ShowEnds = this.ShowEnds;
                //v.LeftEnd = this.LeftEnd;
                //v.RightEnd = this.RightEnd;
                //v.doubleSAKS2long = this.doubleSAKS2long;
	        }
        }

        public Field AddField(string name, int startPos, string typeToken, int size, bool isSub, bool isNullable)
        {
            Field newField;
            switch (typeToken.ToUpper())
            {
                case "CHAR":
                    newField = new CharField(size, name, startPos, isSub, isNullable);
                    break;
                case "VARCHAR":
                    newField = new VarCharField(size, name, startPos, isSub, isNullable);
                    break;
                case "SMALLINT":
                    newField = new ShortField(name, startPos, isSub, isNullable);
                    break;
                case "INTEGER":
                    newField = new IntField(name, startPos, isSub, isNullable);
                    break;
                case "INT":
                    newField = new IntField(name, startPos, isSub, isNullable);
                    break;
                case "DOUBLE":
                    newField = new DoubleField(name, startPos, isSub, isNullable);
                    break;
                case "LONG":
                    newField = new LongField(name, startPos, isSub, isNullable);
                    break;
                case "BIGINT":
                    newField = new LongField(name, startPos, isSub, isNullable);
                    break;
                case "EBCDIC":
                    newField = new EBCDICField(size, name, startPos, isSub, isNullable);
                    break;
                case "DATE":
                    newField = new DateTimeField(size, name, startPos, isSub, isNullable);
                    ((DateTimeField)newField).DateFormat = this.dateformat;
                    break;
                case "FILLER":
                    newField = new FillerField(size, name, startPos, isSub, isNullable);
                    break;
                default:
                    return null;
            }

            //if (isSub) NewSubTemplate(newField.ToString());
            
            if (SubTemplates.Count ==0)
            {
                this.Add(newField);
                if (isNullable)
                    reclen += newField.size + 1;
                else
                    reclen += newField.size;
            }
            else
                SubTemplates[currentTemplate].Add(newField);
            return newField;
        }

        public void NewSubTemplate(string indValue)
        {
            SubTemplates.Add(indValue, new Record());
            currentTemplate = indValue;
        }

        public string ToString(int NamePad, int ValuePad, bool ascii=false)
        {
            StringBuilder sb = new StringBuilder();
            int valuePad = 0;
            int namePad = 0;

            // Calculate formatting
            foreach (var field in this)
            {
                // Name Padding
                if (field.Name.Length > namePad) namePad = field.Name.Length;
                
                if (ShowEnds)
                {
                    // if actual value and optional ends is bigger than the provided value, override
                    if ((LeftEnd.Length + RightEnd.Length + field.ToString().Length) > valuePad)
                        valuePad = LeftEnd.Length + RightEnd.Length + field.ToString().Length;
                }
                else
                {
                    // Override if value length greater than provided padding
                    if ((field.ToString().Length) > valuePad)
                        valuePad = field.ToString().Length;
                }
            }

            // Add each field to the report
            if (ascii == false)
            {
                foreach (var field in this)
                {
                    string v;
                    if (ShowEnds)
                        v = String.Format("{0}{1}{2}", LeftEnd, field.ToString(), RightEnd);
                    else v = field.ToString();

                    sb.AppendFormat("{0,-" + NamePad + "} {1,-" + ValuePad + "}{2,-900}\n",
                                        field.Name,
                                        v,
                                        ShowAscii ? GenerateASCII(field.ToString()) : ""
                                        );
                }
            }
            else
            {
                bool first = true;
                foreach (var field in this)
                {
                    if (first)
                        sb.AppendFormat("{0}", field.ToString());
                    else
                        sb.AppendFormat(",{0}", field.ToString());
                    first = false;
                }
            }
            return sb.ToString();
        }

        public string TemplateToString()
        {
            StringBuilder sb = new StringBuilder(500);

            int i = 1;
            foreach (var field in this)
            {
                sb.AppendFormat(" {0,4} {1,25} {2,7} {3,4}\n", i++, field.Name, field.GetType().Name, field.size);
            }
            return sb.ToString();
        }

        private static string GenerateASCII(string ivalue)
        {
            // this'll make it quicker
            StringBuilder nvalue = new StringBuilder(1000);
            nvalue.Append("\t\t");
            foreach (var c in ivalue)
            {
                nvalue.Append(" ");
                nvalue.Append(((int)c).ToString().PadLeft(3));
            }
            return nvalue.ToString();
        }

        public Record ReadRow(FileStream dataStream) { return ReadRow(dataStream, this); }
        public Record ReadRow(FileStream dataStream, Record r)
        {
            // Make a new record with all the options of this one but none of the fields
            Record rec = new Record();
            CopyTemplateOption(rec);

            foreach (var field in r)
            {
                // Technically we should never hit filler field because the subtemplates take it up
                if(field.GetType() == typeof(FillerField))
                    throw new Exception("Filler field needs to be filled with a template, use chars if the file fills with text");

                // Get the next value in the stream
                field.ReadValue(dataStream);

                // Add it to the record
                rec.Add(field);

                // If the current field is an indicator for a following template
                if (field.SubTemplateIndicator == true)
                {
                    // Keep track of the last field written
                    Field lastField = null;
                    do
                    {
                        // Read the subtemplate, we use the instance's copy of read row
                        // and pass the sub template because the subtemplate array is flat
                        // and not heirarchical.  All templates at any level exist in this
                        // single instance
                        Record sub = ReadRow(dataStream, rec);
                        foreach (var subfield in sub)
                        {
                            lastField = subfield;
                            this.Add(subfield);
                        }
                    }
                    while(lastField.SubTemplateIndicator == true);
                }
            }
            return rec;
        }

        private Record ReadSubTemplate(FileStream dataStream, Field field)
        {
            Record sub;
            if (field.SubTemplateIndicator == true)
            {
                sub = SubTemplates[field.ToString()].ReadRow(dataStream);
                foreach (var subfield in sub)
                {
                    this.Add(subfield);
                }
                return sub;
            }
            else return null;
        }
        public List<SearchField> GetSearchFields()
        {
            // this probably needs to go in the package to the combobox
            Dictionary<string, Field> indfields = new Dictionary<string, Field>();

            List<SearchField> sfs = new List<SearchField>();

            foreach (var field in this)
            {
                if (field.SubTemplateIndicator)
                    indfields.Add(String.Empty, field);
                sfs.Add( new SearchField(field.Name, null, field));
            }

            foreach (var rec in SubTemplates)
            {
                foreach (var field in rec.Value)
	            {
                    if (field.SubTemplateIndicator)
                        indfields.Add(rec.Key, field);
                    sfs.Add(new SearchField(rec.Key + ": " + field.Name, rec.Key, field));
                }
            }

            foreach (var s in sfs)
                s.indFields = indfields;

            return sfs;

            // add all the top level fields

                // get the indicator fields
                
                // Add all the indicator

                // <Field indicatorField, string indicatorValue, Field searcField> {searchValue}
                // List<Dictionary<string indicatorValue, Field indcatorField>>
                // List<Dictionary<string searchValue, Field searchField
                // This will be one list, ordered , search at the end

                // 1
                // Goto beginning of next record (positionincurrentrecord = (currentposition in stream % reclength)
                //                               (Seek for (record length - psitionincurrentrecord))
                // if its the end of the file
                //      and its the first time on this start file
                //          switch the indicator
                //      and if its not the first time on this file
                //          return no find
                //      and there is only 1 file
                //          go to the beginning of the file 
                //      and a next file exists
                //          rewind the query stream
                //          go to the next file
                //          go to the beginning
                //      and a first file exists
                //          rewind the query string
                //          goto the first file
                //          goto the beginnging
                // endif
                // 
                // foreach field
                //      seek to field start position
                //      read field
                //      if match for list value && there is another field in the list
                //          continue to next field; (seek for (nextFieldStart - thisFieldStart + thisFieldLength))
                //      if match for list value && no more in the list
                //          return found;(whatever that number is)
                //      if !match for list value 
                //          goto 1
                //
        }
    }

    public class SearchField
    {
        public string DisplayText { get; set; }
        public string Key { get; set; }
        public Field searchField {get;set;}
        public SearchField(string displaytext, string key, Field search)
        { this.DisplayText = displaytext; this.Key = key; this.searchField = search; }
                public string Query { set; get; }

        public Dictionary<string, Field> indFields { get; set; }
        
        public SearchField()
        {
            this.indFields = new Dictionary<string, Field>();
        }
        public SearchField(Dictionary<string, Field> templateIndicatorFields)
        {
            this.indFields = templateIndicatorFields;
        }
    }
}

