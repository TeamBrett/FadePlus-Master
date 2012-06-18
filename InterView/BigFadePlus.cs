using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using FadePlus.Model;

namespace FadePlus
{
    public delegate void RecordChangeEventHandler(object sender, int currentRecord, string record);
    public delegate void WarningEventHandler(object sender, string warning);

    public class FadePlus
    {
        List<DataFile> DataFiles;
        public string TemplateFileName;
        public Record RecordTemplate;
        public bool ShowAscii { get; set; }
        public bool ShowEnds { get; set; }
        public string LeftEnd { get; set; }
        private string recText;
        public string RightEnd { get; set; }
        public int CurrentRecord
        {
            get { return cr; }
            set
            {
                cr = value;
                if(RecordChange != null)
                    RecordChange(this, value, recText);
            }
        }
        private int cr = 1;
        public string DateFormat = "yyyyMMddHHmmss";
        public int RecordCount { get { return recCount; } }
        private int recCount = 0;        
        //public bool ConvertDoubleSaksToLong 
        //{
        //    get { return convertSaks;}
        //    set 
        //    {   
        //        if (RecordTemplate != null)
        //                foreach (var f in RecordTemplate)
        //                    if (f.Name.ToUpper().Contains("SAK") && f.type == typeof(double))
        //                        ((DoubleField)f).AsLong = value;
        //        convertSaks = value;
        //    }
        //}
        //private bool convertSaks = false;

        public event WarningEventHandler Warning;
        public event RecordChangeEventHandler RecordChange;

        #region Constructors

        public FadePlus(string templateFileName)
        {
            ShowAscii = false;
            ShowEnds = false;
            LeftEnd = "[";
            RightEnd = "]";
            //ConvertDoubleSaksToLong = false;
            
            if (!LoadTemplate(templateFileName))
                return;
        }
        public FadePlus(string templateFileName, string dataFileName)
        {
            // Check file size against record length
            if (File.Exists(templateFileName))
                if (!LoadTemplate(templateFileName))
                    return;

            AddDataFile(dataFileName);
        }
        public FadePlus(string templateFileName, List<string> dataFileNames)
            : this(templateFileName)
        {
            // Check file size against record length
            if (File.Exists(templateFileName))
                if (!LoadTemplate(templateFileName))
                    return;

            // Add the data files
            foreach (var filename in dataFileNames)
                AddDataFile(filename);
        }

        #endregion
        
        public void Refresh()
        { GetRecord(CurrentRecord); }

        public string GetRecord(int recNum, bool ascii=false)
        {
            string filename = "";
            int fileStartPosition = 0;

            // preflight checks
            if (recNum < 1 || RecordTemplate == null)
                return "";

            StringBuilder recReport = new StringBuilder(1000);

            #region Get File

            // Get the file we need to read from
            int rec = 0;
            foreach (var file in DataFiles)
            {
                // if the number we were given is beyond the count of this file
                if (recNum > (file.RecordCount + rec))
                {
                    // Count the number of records in we are
                    rec += file.RecordCount;
                }
                // otherwise this is our file
                else
                {
                    filename = file.filename;
                    fileStartPosition = recNum - rec;
                    break;
                }
            }

            if (filename == "")
            {
                Warning(this,"Couldn't find record" + recNum.ToString());
                return null;
            }

            #endregion

            // Get our filestream
            FileStream dataStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            // Position the file stream
            long recpos = (fileStartPosition - 1) * RecordTemplate.RecLength;
            // Current Field
            Record newrec = RecordTemplate.Copy();

            try
            {
                //// Go to the record
                dataStream.Seek(recpos, SeekOrigin.Begin);

                //// Get the fields
                //foreach (var field in newrec)
                //    field.ReadValue(dataStream);
                newrec.ReadRow(dataStream);
            }catch(Exception e) { throw e; }

            // Tag the ends of the values if requested
            
            if (ShowEnds)
            {
                newrec.ShowEnds = ShowEnds;
                newrec.LeftEnd = LeftEnd;
                newrec.RightEnd = RightEnd;
            }

            // Generate the ascii values if requested
            if (ShowAscii)
            {
                newrec.ShowAscii = ShowAscii;
            }

            newrec.DateFormat = this.DateFormat;

            // set the current record and return it
            recText = newrec.ToString(30, 125, ascii);
            CurrentRecord = recNum;
            return recText;
        }

        private FileStream queryDataStream;
        public long Search(SearchField sf)
        {
            #region preflight checks
            if (RecordTemplate == null && DataFiles != null && DataFiles.Count > 0)
                return -1;
            int startfile = 0;

            // Create a new stream if one doesn't exit.
            if (queryDataStream == null)
                queryDataStream = DataFiles[0].dataStream;
            else
            {
                // Find the datafile our query stream belongs to
                foreach (var file in DataFiles)
                    if (file.dataStream == queryDataStream)
                        break;
                    else startfile++;
            }
            #endregion

            // some shortcuts
            int reclen = RecordTemplate.RecLength;
            FileStream stream = queryDataStream;

            #region Logic
            /*
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
            // foreach indfield
            //      seek to indfield start position
            //      read indfield
            //      if match for ind value && there is another field in the list
            //          continue to next field; (seek for (nextFieldStart - thisFieldStart + thisFieldLength))
            //      if match for list value && no more in the list
            //          test query
            //          return found or break to next record;(whatever that number is)
            //      if !match for list value 
            //          goto 1
            // Otherwise
             *      seek to the search field
             *      read the value
             *      test the value
             *      break if no match
             *      return record id if found
             */
            #endregion

            #region new
            // loops for each record in the file.
            for (int currfile = -1; currfile < DataFiles.Count; )
            {
                #region handle DataFile and sets us at the beginning of a record for reading
                // Figure out the beginning of the next whole record
                if (stream.Position != 0)
                {
                    long currRecBegin = reclen - (stream.Position % reclen);
                    stream.Seek((currRecBegin), SeekOrigin.Current);
                }

                // Checks to see if we're at the end
                // get the next page if we're at the end
                // return if we've cycled back to where we started
                if (stream.Position + sf.searchField.Size >= stream.Length)
                {
                    if (currfile == startfile) return -1;
                    else if (DataFiles.Count == 1)
                    {
                        currfile = startfile;
                        stream.Seek(0, SeekOrigin.Begin);
                    }
                    else
                    {
                        if (currfile == -1)
                            currfile = startfile;

                        if (DataFiles.Count > currfile)
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            stream = DataFiles[currfile++].dataStream;
                            stream.Seek(0, SeekOrigin.Begin);
                        }
                        else
                        {
                            stream.Seek(0, SeekOrigin.Begin);
                            stream = DataFiles[0].dataStream;
                            currfile = 0;
                            stream.Seek(0, SeekOrigin.Begin);
                        }
                    }
                }
                #endregion

                #region Search the Record

                if (sf.indFields.Count != 0)
                {
                    int f = 1;
                    int laststart = 0;
                    int lastlength = 0;

                    foreach (var ifield in sf.indFields)
                    {
                        stream.Seek((ifield.Value.startPosition - laststart + lastlength), SeekOrigin.Current);

                        ifield.Value.ReadValue(stream);

                        if (ifield.Value.ToString() == sf.Key && f < sf.indFields.Count)
                        {
                            laststart = ifield.Value.startPosition;
                            lastlength = ifield.Value.Size;
                            continue;
                        }
                        if (ifield.Value.ToString() == sf.Key && f == sf.indFields.Count)
                        {
                            stream.Seek((sf.searchField.startPosition - ifield.Value.startPosition + ifield.Value.Size), SeekOrigin.Current);
                            sf.searchField.ReadValue(stream);
                            if (sf.searchField.ToString() == sf.Query)
                            {
                                int numrecs = 0;
                                for (int z = 0; z < currfile; z++)
                                    numrecs += DataFiles[z].RecordCount;

                                // beginning of the found record         //recs in file  /files before this one   //because the lookup is based on real numbers
                                return ((stream.Position - sf.searchField.startPosition) / reclen) + numrecs + 1;
                            }
                            else break;
                        }
                        if (ifield.Value.ToString() != sf.Key)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    currfile = 0;
                    if(sf.searchField.isNullable)
                        stream.Seek((sf.searchField.startPosition - 1), SeekOrigin.Current);
                    else
                        stream.Seek((sf.searchField.startPosition), SeekOrigin.Current);

                    sf.searchField.ReadValue(stream);

                    if (sf.searchField.Equals(sf.Query))
                    {
                        int numrecs = 0;

                        for (int z = 0; z < currfile; z++)
                            numrecs += DataFiles[z].RecordCount;

                        return ((stream.Position - sf.searchField.startPosition) / reclen) + numrecs + 1;
                    }
                    else continue;
                }
                #endregion
            }
            return -1;
            #endregion

            #region old
            //// Figure out the beginning of the next whole record
            //if(stream.Position != 0)
            //    currRecBegin = reclen - (stream.Position % reclen);

            ////// Set it back a record length minus our field length so when we 
            ////if(currRecBegin > (reclen - field.Size))
            ////    currRecBegin = currRecBegin - (reclen - field.Size);

            //// Set to the beginning of the next record search field
            //queryDataStream.Seek((currRecBegin + field.startPosition), SeekOrigin.Current);

            //bool cycled = false;
            //for (int i = page; i < DataFiles.Count; i++)
            //{
            //    do
            //    {
            //        field.ReadValue(queryDataStream);
            //        if (field.Equals(query))
            //          // beginning of the found record                    //recs in file                  /files before this one    //because the lookup is based on real numbers
            //            return ((stream.Position - field.startPosition) / reclen) + (i * reclen)                      + 1;
            //        // Read the next record if there is room
            //        if (stream.Position < reclen + stream.Length)
            //            stream.Seek(reclen - field.Size, SeekOrigin.Current);
            //        else break;
            //    } while (true) ;
                
            //    // If we hit the end of the file and need a new page
            //    if (page == ++i)
            //    {
            //        if(cycled == true)
            //            return -1;

            //        stream.Seek(0, SeekOrigin.Begin);
            //        i--;
            //        cycled = true;
            //        continue;
            //    }
            //    if (i == DataFiles.Count && page != 0 && i + 1 != page)
            //    {
            //        // reset our current stream
            //        stream.Seek(0, SeekOrigin.Begin);
            //        // Get a new stream
            //        stream = DataFiles[i + 1].dataStream;
            //        // set it to zero
            //        stream.Seek(field.startPosition, SeekOrigin.Begin);

            //        continue;
            //    }
            //}
            #endregion
        }

        static public ParseResults ValidateTemplate(string filename)
        {
            return ParseTemplate(filename);
            //if (pr.success == false)
            //    return false;
            //else
            //    return true;
        }
        public bool LoadTemplate(string filename)
        {
            ParseResults pr = ParseTemplate(filename);
            if(pr.success == false)
                Warning(this, pr.Error);
            RecordTemplate = pr.record;
            TemplateFileName = filename;
            return true;
        }
        
        public struct ParseResults
        {
            public Record record;
            public string Error;
            public bool success;
        }
        static private ParseResults ParseTemplate(string filename)
        {
            /*
             * MSIS_ID                        at   0 for  20 type CHAR
             * ADJ_IND                        at  20 for   2 type SMALLINT
             * TOS                            at  22 for   2 type SMALLINT
             * TYPE_CLM                       at  24 for   2 type SMALLINT
             * DATE_PAID                      at  26 for   6 type VARCHAR
             * AMT_PAID                       at  32 for   4 type INTEGER  NULL
             * BEG_DOS                        at  36 for   4 type INTEGER  NOTNULL
             * END_DOS                        at  40 for   8 type LONG
             * PROV_ID                        at  42 for  14 type DATETIME "YYYYMMHH24MMSI" NULL
             * DOLLA_AMOUNT                   at  56 for   1 type CHAR SUB_IND
             * SUB_FILLER                     at  57 for 150 type FILLER
             * 
             * SUB J
             * DATE_PAID                      at  57 for   6 type VARCHAR
             * AMT_PAID                       at  63 for   4 type INTEGER  NULL
             * BLARB_INDICATOR                at  67 for   4 type INTEGER SUB_IND
             * SUB_FILLER                     at  71 for  75 type FILLER
             * 
             * SUB 45
             * ADJ_IND                        at  57 for   2 type SMALLINT
             * TOS                            at  59 for   2 type SMALLINT
             * TYPE_CLM                       at  61 for   2 type SMALLINT
             * 
             * Indicators
             * must be at the end and followed by filler to reach full length
             * 
             * 
            Number of Rows: 1190205
            */
            ParseResults pr = new ParseResults();

            if (filename == String.Empty)
            {
                pr.success = false;
                pr.Error = "\n Empty File Name \n";
                return pr;
            }

            string dotout;
            
            using (StreamReader reader = new StreamReader(filename))
            {
                try
                {
                    dotout = reader.ReadToEnd();
                }
                catch (OutOfMemoryException e)
                {
                    pr.Error = "File too big to be a template";
                    pr.success = false;
                    return pr;
                }
            }

            string[] tokens;
            int ihold;

            // {FieldName} at {position} for {size} type {datatype} {DATEFORMAT} {NULL/NOTNULL}
            //      1       2     3       4    5     6       7        Optional      Optional
            int numTokens = 7;

            // Split DotOut file on spaces
            tokens = dotout.Split(new char[] { ' ', '\r', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            // To hold each field as described in the DotOut
            Record rec = new Record();

            // Process each token
            for (int i = 0; (i + 7) <= tokens.GetLength(0); )
            {
                // Field Name
                Field newField;
                string name;
                int start;
                int size;
                string type;
                bool isTemplateIndicator = false;

                // Name
                name = tokens[i++];

                // Expected "at" token
                if (tokens[i++].ToUpper() != "AT")
                {
                    pr.Error = (("Expected \"at\" on line " +
                                    (rec.Count + 1).ToString() +
                                    " field " +
                                    (rec.Count - ((i - 1) * numTokens)).ToString()) + "\n");
                    pr.success = false;
                    return pr;
                }
                // Try to parse the position
                if (!int.TryParse(tokens[i++], out ihold))
                {
                    pr.Error = ((@"Unparsable Position on Line " +
                                            (rec.Count + 1).ToString() +
                                            " field " +
                                            (rec.Count - ((i - 1) * numTokens)).ToString()) + "\n");
                    pr.success = false;
                    return pr;
                }
                else start = ihold;

                // Expected "for" token
                if (tokens[i++].ToUpper() != "FOR")
                {
                    pr.Error = (("Expected \"for\" on line " +
                                            (rec.Count + 1).ToString() +
                                            " field " +
                                            (rec.Count - ((i - 1) * numTokens)).ToString()) + "\n");
                    pr.success = false;
                    return pr;
                }
                // Try to parse the size
                if (!int.TryParse(tokens[i++], out ihold))
                {
                    pr.Error = ((@"Unparsable Field Size on Line " +
                                            (rec.Count + 1).ToString() +
                                            " Field " +
                                            (rec.Count - ((i - 1) * numTokens)).ToString()) + "\n");
                    pr.success = false;
                    return pr;
                }
                size = ihold;

                // Expected "type" token
                if (tokens[i++].ToUpper() != "TYPE")
                {
                    pr.Error = (("Expected \"type\" on line " +
                                            (rec.Count + 1).ToString() +
                                            " field " +
                                            (rec.Count - ((i - 1) * numTokens)).ToString()) + "\n");
                    pr.success = false;
                    return pr;
                }

                // Get the type
                type = tokens[i++];

                if (type.ToUpper() == "DATE")
                {
                    // counts the date formatter which follows date
                    // oracle's date format is incomprehensible to our parsers
                    // technically this should be hidden in the datefield object
                    // but it's too abstracted behind the field object to make it 
                    // owrth it.  besides this counts as a token
                    i++;
                }
                
                bool isNullable = false;
                if (tokens.Length > i)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        if (tokens[i].ToUpper() == "SUB_IND")
                        {
                            isTemplateIndicator = true;
                            i++;
                        }
                        // Optional Null paramter
                        if (tokens[i] == "NULL")
                        {
                            isNullable = true;
                            i++;
                        }
                        if (tokens[i] == "NOTNULL")
                        {
                            isNullable = false;
                            i++;
                        }                
                    }
                }

                // We have our parameters add our new field
                newField = rec.AddField(name, start, type, size, isTemplateIndicator, isNullable);

                if (tokens.Length > i + 1)
                {
                    if (tokens[i].ToUpper() == "SUB")
                    {
                        i++;                    // Count the "SUB" token
                        rec.NewSubTemplate(tokens[i++]);   // start a new template & count the token
                    }                // Check for optional rownumber extra bit
                }
                if (tokens.Length > i + 2)
                {
                    if (tokens[i] == "Number" && tokens[i + 1] == "of" && tokens[i + 2] == "Rows:")
                    {
                        i += 3;
                    }
                }
            }
            pr.record = rec;
            pr.success = true;
            // We've made our template
            return pr;
        }

        public bool AddDataFile(string filename)
        {
            if(RecordTemplate == null)
                return false;

            FileInfo fi = new FileInfo(filename);
            DataFile df = new DataFile();

            if (!File.Exists(filename))
            {
                Warning(this,"\nData file \"" + filename + "\" does not exist\n");
                return false;
            }

            df.filename = filename;
            df.filesize = fi.Length;

            if (RecordTemplate.RecLength > 0)
            {
                df.RecordCount = (int)(fi.Length / RecordTemplate.RecLength);
                this.recCount += df.RecordCount;
            }
            else
                return false;

            if (fi.Length % RecordTemplate.RecLength != 0)
            {
                if(Warning != null)
                    Warning(this,"\nData file contains partial records or you have the wrong data/mask file.");
                return false;
            }

            df.dataStream = new FileStream(filename, FileMode.Open, FileAccess.Read);

            if (this.DataFiles == null)
                this.DataFiles = new List<DataFile>(1);

            this.DataFiles.Add(df);

            return true;
        }
        
    }
}
