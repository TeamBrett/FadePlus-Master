//using System;
//using System.Collections.Generic;
//using System.Text;
//using System.IO;
//using System.Globalization;

//namespace SmallFadePlus
//{
//    public delegate void RecordChangeEventHandler(object sender, int currentRecord, string record);   
//    public delegate void WarningEventHandler(object sender, string warning);

//    class FadePlus
//    {
//        #region Properties & Such
//        public bool IsEBCDIC { get; set; }
//        public bool ShowAscii { get; set; }
//        public bool ShowEnds { get; set; }
//        public bool TrimVarChar { get; set; }
//        public bool SubStringSearch { get; set; }
//        public bool StripNullsFromVarChars { get; set; }
//        public bool ConvertDoubleSaksToLong { get; set; }
//        public string LeftEnd { get; set; }
//        private string recText; 
//        public string RightEnd { get; set; }
//        public string DateFormat = "yyyyMMddHHmmss";
//        public bool DataIsLoaded = false;
//        public List<string> Warnings = new List<string>();
//        private int cr = 1;
//        public event RecordChangeEventHandler RecordChange;
//        public int CurrentRecord 
//        {
//            get { return cr; } 
//            set {
//                cr = value; 
//                RecordChange(this, value, recText);
//            } 
//        }
//        public Mask mask { get; set; }

//        Dictionary<int, Dictionary<int, object>> Data;
//        public int RecordCount { get; set; }

//        public FadePlus()
//        {
//            IsEBCDIC = false;
//            ShowAscii = false;
//            ShowEnds = false;
//            TrimVarChar = false;
//            SubStringSearch = false;
//            StripNullsFromVarChars = false;
//            ConvertDoubleSaksToLong = false;
//            LeftEnd = "[";
//            RightEnd = "]";
//            DateFormat = "yyyyMMddHHmmss";
//            Data = new Dictionary<int, Dictionary<int, object>>();
//        }
//        #endregion

//        #region Actions

//        public string Refresh()
//        {
//            return GotoRecord(CurrentRecord);
//        }

//        public string GotoRecord(int recNum)
//        {
//            string value = "";
//            string ivalue = "";
//            int maxvar = 30;

//            if (recNum > RecordCount)
//                recNum = RecordCount;
//            int recIndex = recNum - 1;
            
//            // preflight checks
//            if (DataIsLoaded == false || recNum < 1 || mask == null )
//                return "";

//            StringBuilder sb = new StringBuilder(((int)mask.MaxRecLength * 2));



//            // For each field
//            for (int field = 0; field < mask.FieldCount; field++)
//            {
//                // Fill in the name of the field
//                sb.Append(mask.fields[field].Name.PadRight((int)(mask.maxNameLength + 10)));

//                // Get our values.
//                switch (mask.fields[field].DataType)
//                {
//                    case FieldType.CHAR:
//                        ivalue = (string)Data[recIndex][field];
//                        break;
//                    case FieldType.VARCHAR:
//                        ivalue = (string)Data[recIndex][field];
//                        // remove nulls if so desired
//                        if (StripNullsFromVarChars)
//                        {
//                            ivalue = Denullify(ivalue);
//                        }
//                        // trim
//                        if (TrimVarChar)
//                        {
//                            ivalue.Trim();
//                            maxvar = ivalue.Length;
//                        }
//                        break;
//                    case FieldType.SMALLINT:
//                        ivalue = ((short)Data[recIndex][field]).ToString();
//                        break;
//                    case FieldType.INTEGER:
//                        ivalue = ((int)Data[recIndex][field]).ToString();
//                        break;
//                    case FieldType.DOUBLE:
//                        ivalue = ((double)Data[recIndex][field]).ToString();
//                        break;
//                    case FieldType.LONG:
//                        ivalue = ((long)Data[recIndex][field]).ToString();
//                        break;
//                    case FieldType.EBCDIC:
//                        ivalue = (string)Data[recIndex][field];
//                        break;
//                    case FieldType.DATE:
//                        ivalue = ((DateTime)Data[recIndex][field]).ToString(DateFormat);//"yyyyMMddHHmmss");// (mask.fields[field].Format);
//                        break;
//                    default:
//                        Warnings.Add("\nUnrecognized Type \n");
//                        break;
//                }

//                // Tag the ends of the values if requested
//                if (ShowEnds)
//                    value = LeftEnd + ivalue + RightEnd;
//                else value = ivalue;

//                // Adjust the padding if we're trimming varchars.  Get the largest value of the trimmed varcahr fields
//                // and the sizes of all the others.
//                if (TrimVarChar)
//                    sb.Append(value.PadLeft(maxvar + LeftEnd.Length + RightEnd.Length));
//                // And if we're not trimming varchars
//                else
//                    sb.Append(value.PadLeft((int)mask.MaxRecLength + LeftEnd.Length + RightEnd.Length));

//                // Generate the ascii values if requested
//                if (ShowAscii)
//                {
//                    // this'll make it quicker
//                    StringBuilder nvalue = new StringBuilder(1000);
//                    nvalue.Append("\t\t");
//                    foreach (var c in ivalue)
//                    {
//                        nvalue.Append(" ");
//                        nvalue.Append(((int)c).ToString().PadLeft(3));
//                    }
//                    sb.Append(nvalue.ToString());
//                }
//                sb.Append("\n");
//            }

//            // set the current record and return it
//            recText = sb.ToString();
//            CurrentRecord = recNum;
//            return recText;
//        }

//        private static string Denullify(string ivalue)
//        {
//            // temp array we can work with
//            char[] tc = ivalue.ToCharArray();
//            // go through everything and replace the \0
//            for (int c = 0; c < tc.Length; c++)
//                tc[c] = tc[c] == '\0' ? ' ' : tc[c];
//            // go back to our string
//            ivalue = new string(tc);//.ToString();
//            return ivalue;
//        }

//        public bool LoadRecords(string DataFileName)
//        {
//            try
//            {
//                // Set up DataFile
//                int currField = 0;
//                int record = 0;

//                #region EBCDIC
//                // God help us if this actually works.
//                if (IsEBCDIC == true)
//                {
//                    using (FileStream dataStream = new FileStream(DataFileName, FileMode.Open))
//                    {
//                        //JonSkeet.Ebcdic.EbcdicEncoding.GetEncoding("EBCDIC-US")
//                        Data = new Dictionary<int, Dictionary<int, object>>((int)RecordCount);

//                        for (record = 0; record < RecordCount; record++)
//                        {
//                            // Add an entry into the data dictionary for our new row & fields
//                            Data.Add(record, new Dictionary<int, object>((int)mask.FieldCount));

//                            // foreach field
//                            for (currField = 0; currField < mask.FieldCount; currField++)
//                            {
//                                try
//                                {
//                                    byte[] buffer = new byte[mask.fields[currField].Size];
//                                    dataStream.Read(buffer, 0, mask.fields[currField].Size);

//                                    Data[record][currField] = JonSkeet.Ebcdic.EbcdicEncoding.ASCII.GetString(buffer);

//                                }
//                                catch { return false; }
//                            }
//                        }
//                    }
//                }
//                else
//                {
//                #endregion
//                    // Open our file stream
//                    using (FileStream dataStream = new FileStream(DataFileName, FileMode.Open))
//                    {
//                        // Create a new data dictionary that holds other dictionaries(Data=table, []=rows, [][]=fields)
//                        Data = new Dictionary<int, Dictionary<int, object>>((int)RecordCount);

//                        for (record = 0; record < RecordCount; record++)
//                        {
//                            // Add an entry into the data dictionary for our new row(record) & fields(new dictionary)
//                            Data.Add(record, new Dictionary<int, object>((int)mask.FieldCount));

//                            if (dataStream.Position % mask.RecordLength != 0)
//                                throw new Exception();

//                            // foreach field
//                            for (currField = 0; currField < mask.FieldCount; currField++)
//                            {
//                                // Current Field
//                                RecordField field = mask.fields[currField];

//                                byte[] buffer;

//                                #region Bad

//                                #endregion

//                                #region VarChar init bytes
//                                if (field.DataType == FieldType.VARCHAR)
//                                {
//                                    buffer = new byte[sizeof(short)];
//                                    // Read the first two bytes into a short
//                                    short first2;

//                                    // Read our data into the buffer
//                                    dataStream.Read(buffer, 0, sizeof(short));
                                    
//                                    // reverse if endian
//                                    if (BitConverter.IsLittleEndian)
//                                            Array.Reverse(buffer);

//                                    // Get our short
//                                    first2 = BitConverter.ToInt16(buffer, 0);
//                                    try
//                                    {
//                                        // New buffer for the text
//                                        buffer = new byte[first2];
//                                    }
//                                    catch (OverflowException e) { Warnings.Add("\nBad read on varchar field size bytes. Record " + (record + 1).ToString() + " Field " + (field.Id + 1).ToString() + "\n"); }
//                                    // Read in the value
//                                    dataStream.Read(buffer, 0, first2);

//                                    // Calculate the slack the varchar didn't use
//                                    long slack = (field.Size - (first2 + sizeof(short)));

//                                    // Move ahead
//                                    dataStream.Seek(slack,SeekOrigin.Current);
//                                }
//                                #endregion
//                                else
//                                {
//                                    // data buffer
//                                    buffer = new byte[field.Size];

//                                    // Read our data into the buffer
//                                    dataStream.Read(buffer, 0, field.Size);
//                                }

//                                // Get the value
//                                switch (field.DataType)
//                                {
//                                    case FieldType.CHAR:
//                                        Data[record][currField] = System.Text.ASCIIEncoding.ASCII.GetString(buffer);
//                                        break;                                   // UTF7,ASCII, I'm not 100% what is goin' on here.
//                                    case FieldType.VARCHAR:
//                                        Data[record][currField] = System.Text.ASCIIEncoding.ASCII.GetString(buffer);

//                                        // Fill in our padding for the trimmed fields
//                                        if (field.Longest < ((string)Data[record][currField]).Length)
//                                            field.Longest = ((string)Data[record][currField]).Length;
//                                        break;
//                                    case FieldType.SMALLINT:
//                                        // Check Endian
//                                        if (BitConverter.IsLittleEndian)
//                                            Array.Reverse(buffer);
//                                        Data[record][currField] = BitConverter.ToInt16(buffer, 0);
//                                        break;
//                                    case FieldType.INTEGER:
//                                        // Check Endian
//                                        if (BitConverter.IsLittleEndian)
//                                            Array.Reverse(buffer);
//                                        Data[record][currField] = BitConverter.ToInt32(buffer, 0);
//                                        break;
//                                    case FieldType.DOUBLE:
//                                        // Check Endian
//                                        if (BitConverter.IsLittleEndian)
//                                            Array.Reverse(buffer);
//                                        Data[record][currField] = BitConverter.ToDouble(buffer, 0);
//                                        break;
//                                    case FieldType.LONG:
//                                        // Check Endian
//                                        if (BitConverter.IsLittleEndian)
//                                            Array.Reverse(buffer);
//                                        Data[record][currField] = (long)BitConverter.ToDouble(buffer, 0);
//                                        break;
//                                    case FieldType.EBCDIC:
//                                        Warnings.Add("\n There shouldn't be an EBCDIC field in this file!\n");
//                                        break;
//                                    case FieldType.DATE:
//                                        // Get the Date format provider
//                                        DateTimeFormatInfo dtfi = new CultureInfo(CultureInfo.CurrentCulture.LCID, false).DateTimeFormat;

//                                        // get the string value
//                                        string date = System.Text.ASCIIEncoding.ASCII.GetString(buffer);

//                                        // Parse it
//                                        try { Data[record][currField] = DateTime.ParseExact(date, @"yyyyMMddHHmmss", dtfi); }
//                                        catch { Warnings.Add("Couldn't parse the date"); }

//                                        break;
//                                    default:
//                                        break;
//                                }
//                            }
//                        }
//                    }
//                }
//            }
//            catch (OutOfMemoryException)
//            {
//                Warnings.Add("\n Your file is too large I can't get it all into memory");
//                UnloadData();
//                return false;
//            }
//            catch (Exception e)
//            {
//                Warnings.Add("\n There was a problem reading the file.  Please check the warnings here and check you have the correct files selected." + e.Message);
//            }
//            GotoRecord(CurrentRecord);
//            return true;
//        }

//        public void UnloadData()
//        {
//            Data.Clear();
//        }

//        public bool LoadTemplate(Mask newMask)
//        {
//            this.mask = newMask;
//            return true;
//        }

//        public Mask ParseDotOut(string filename)
//        {
//            /*
//            MSIS_ID                        at   0 for  20 type CHAR
//            ADJ_IND                        at  20 for   2 type SMALLINT
//            TOS                            at  22 for   2 type SMALLINT
//            TYPE_CLM                       at  24 for   2 type SMALLINT
//            DATE_PAID                      at  26 for   4 type INTEGER
//            AMT_PAID                       at  30 for   4 type INTEGER
//            BEG_DOS                        at  34 for   4 type INTEGER
//            END_DOS                        at  38 for   4 type INTEGER
//            PROV_ID                        at  42 for  12 type CHAR
//            AMT_CHG                        at  54 for   4 type INTEGER
//            TPL_AMT                        at  58 for   4 type INTEGER
//            PROG_TYPE                      at  62 for   2 type SMALLINT
//            PLAN_ID                        at  64 for  12 type CHAR
//            QTY                            at  76 for   4 type INTEGER
//            MEDI_DED                       at  80 for   4 type INTEGER
//            MEDI_COIN                      at  84 for   4 type INTEGER
//            DIAG_1                         at  88 for   8 type CHAR
//            DIAG_2                         at  96 for   8 type CHAR
//            POS                            at 104 for   2 type SMALLINT
//            SPECIALTY                      at 106 for   4 type CHAR
//            SVC_CDE                        at 110 for   8 type CHAR
//            SVC_CDE_FLAG                   at 118 for   2 type SMALLINT
//            SVC_CDE_MOD                    at 120 for   2 type CHAR
//            UB92_REV_CDE                   at 122 for   2 type SMALLINT
//            PROV_ID_SVC                    at 124 for  12 type CHAR
//            NATIONAL_PROV_ID               at 136 for  12 type CHAR
//            PROVIDER_TAXONOMY              at 148 for  12 type CHAR
//            ICN_ORIG                       at 160 for  21 type CHAR
//            NUM_DTL                        at 181 for   2 type SMALLINT
//            ICN_ADJ                        at 183 for  21 type CHAR
//            NUM_DTL_ADJ                    at 204 for   2 type SMALLINT
//            SAK_CLAIM                      at 206 for   8 type DOUBLE
//            FFYQ                           at 214 for   5 type CHAR
//            Number of Rows: 1190205
//            */
//            using (StreamReader reader = new StreamReader(filename))
//            {
//                string dotout = reader.ReadToEnd();
//                string[] tokens;
//                int ihold;

//                // {FieldName} at {position} for {size} type {datatype}
//                //      1       2     3       4    5     6       7
//                int numTokens = 7;

//                // Split DotOut file on spaces
//                tokens = dotout.Split(new char[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

//                // To hold each field as described in the DotOut
//                Mask mask = new Mask();
//                IsEBCDIC = true;  // We assume it is and prove it wrong.

//                // Process each token
//                for (int i = 0; (i + 7) <= tokens.GetLength(0); mask.FieldCount++)
//                {
//                    // Field Name
//                    RecordField newfld = new RecordField(tokens[i++]);

//                    // Expected "at" token
//                    if (tokens[i++].ToUpper() != "AT")
//                    {
//                        Warnings.Add(("Expected \"at\" on line " +
//                                        (mask.FieldCount + 1).ToString() +
//                                        " field " +
//                                        (mask.FieldCount - ((i - 1) * numTokens)).ToString()) + "\n");
//                        return null;
//                    }
//                    // Try to parse the position
//                    if (!int.TryParse(tokens[i++], out ihold))
//                    {
//                        Warnings.Add((@"Unparsable Position on Line " +
//                                             (mask.FieldCount + 1).ToString() +
//                                             " field " +
//                                             (mask.FieldCount - ((i - 1) * numTokens)).ToString()) + "\n");
//                        return null;
//                    }
//                    else newfld.Start = ihold;

//                    // Expected "for" token
//                    if (tokens[i++].ToUpper() != "FOR")
//                    {
//                        Warnings.Add(("Expected \"for\" on line " +
//                                              (mask.FieldCount + 1).ToString() +
//                                              " field " +
//                                              (mask.FieldCount - ((i - 1) * numTokens)).ToString()) + "\n");
//                        return null;
//                    }
//                    // Try to parse the size
//                    if (!int.TryParse(tokens[i++], out ihold))
//                    {
//                        Warnings.Add((@"Unparsable Field Size on Line " +
//                                              (mask.FieldCount + 1).ToString() +
//                                              " Field " +
//                                              (mask.FieldCount - ((i - 1) * numTokens)).ToString()) + "\n");
//                        return null;
//                    }
//                    newfld.Size = ihold;

//                    // Expected "type" token
//                    if (tokens[i++].ToUpper() != "TYPE")
//                    {
//                        Warnings.Add(("Expected \"type\" on line " +
//                                              (mask.FieldCount + 1).ToString() +
//                                              " field " +
//                                              (mask.FieldCount - ((i - 1) * numTokens)).ToString()) + "\n");
//                        return null;
//                    }

//                    // Datatype
//                    switch (tokens[i++].ToUpper())
//                    {
//                        case "CHAR":
//                            newfld.DataType = FieldType.CHAR;
//                            IsEBCDIC = false;
//                            break;
//                        case "VARCHAR":
//                            newfld.DataType = FieldType.VARCHAR;
//                            IsEBCDIC = false;
//                            break;
//                        case "SMALLINT":
//                            newfld.DataType = FieldType.SMALLINT;
//                            IsEBCDIC = false;
//                            break;
//                        case "INTEGER":
//                            newfld.DataType = FieldType.INTEGER;
//                            IsEBCDIC = false;
//                            break;
//                        case "INT":
//                            newfld.DataType = FieldType.INTEGER;
//                            IsEBCDIC = false;
//                            break;
//                        case "DOUBLE":
//                            if ((ConvertDoubleSaksToLong) && newfld.Name.ToLower().Contains("sak"))
//                                newfld.DataType = FieldType.LONG;
//                            else
//                                newfld.DataType = FieldType.DOUBLE;
//                            IsEBCDIC = false;
//                            break;
//                        case "LONG":
//                            newfld.DataType = FieldType.LONG;
//                            IsEBCDIC = false;
//                            break;
//                        case "BIGINT":
//                            newfld.DataType = FieldType.LONG;
//                            IsEBCDIC = false;
//                            break;
//                        case "EBCDIC":
//                            if (IsEBCDIC == false)
//                            {
//                                Warnings.Add("\nEverything must be EBCDIC, or nothing\n");
//                                return null;
//                            }
//                            newfld.DataType = FieldType.EBCDIC;
//                            break;
//                        case "DATE":
//                            newfld.DataType = FieldType.DATE;
//                            IsEBCDIC = false;
//                            newfld.Format = tokens[i++].Replace('"', ' ').Trim(); //for the date format;
//                            break;
//                        default:
//                            Warnings.Add((@"Unknown data type " + tokens[i - 1] + "on line " +
//                                              (mask.FieldCount + 1).ToString() +
//                                              " field " +
//                                              (mask.FieldCount - ((i - 1) * numTokens)).ToString()) +
//                                              "\n");
//                            return null;
//                    }

//                    // Set the new field's ID
//                    newfld.Id = mask.FieldCount;

//                    // Set the padding
//                    newfld.Padding = newfld.Start - mask.RecordLength;

//                    // Update the record length with the padding
//                    mask.RecordLength += newfld.Padding;

//                    // Update the record with the stated field size
//                    mask.RecordLength += newfld.Size;

//                    // Update the max reclength if we beat it
//                    if (newfld.Size > mask.MaxRecLength)
//                        mask.MaxRecLength = newfld.Size;

//                    // Check for optional rownumber extra bit
//                    if (tokens.Length > i + 2)
//                    {
//                        if (tokens[i] == "Number" && tokens[i + 1] == "of" && tokens[i + 2] == "Rows:")
//                        {
//                            i += 3;
//                            // Try to parse the size
//                            if (int.TryParse(tokens[i], out ihold))
//                            {
//                                mask.oracount = ihold;
//                            }
//                        }

//                        // Optional Null paramter
//                        if (tokens[i] == "NULL" || tokens[i] == "NOTNULL")
//                        {
//                            newfld.CanNull = true;
//                            i++;
//                        }
//                    }
//                    // Add our field
//                    mask.fields.Add(newfld);
//                }
//                if (this.mask == null)
//                    this.mask = mask;


//                // We've made our mask
//                return mask;
//            }
//        }

//        public int find(string query, RecordField rf)
//        {
//            // Yes, I realized a little inheritance and interfacing could fix this mess, but comeon

//            // If it's the last record start at the beginning otherwise start at the next record
//            int start = (CurrentRecord == RecordCount) ? 0 : CurrentRecord;

//            switch (rf.DataType)
//            {
//                case FieldType.CHAR:
//                    string a = query;
//                    // For each record, starting at the current
//                    for (int i = 0, j = start; i < RecordCount; i++)
//                    {
//                        // See if the search term is equal to the data valuse
//                        if (SubStringSearch)
//                        {
//                            // Trim the strings
//                            if (((string)Data[j][(int)rf.Id]).Contains(a.Trim()))
//                            {
//                                return (j + 1);
////                                return GotoRecord(j + 1);
//                            }
//                        }
//                        else
//                            // Exact string match
//                            if (((string)Data[j][(int)rf.Id]).Trim() == a.Trim())
//                            {
//                                return (j + 1);
////                                return GotoRecord(j + 1);
//                            }
//                        // If we've reached the end of the file start back at the beginning
//                        // the outer loop will stop us when we finish
//                        if (++j == RecordCount) j = 0;
//                    }
//                    Warnings.Add("\n No records on field : \"" + rf.Name + "\" for value: " + a + "\n");
//                    return 0;
//                case FieldType.SMALLINT:
//                    short b;

//                    if (Int16.TryParse(query, out b))
//                    {
//                        for (int i = 0, j = start; i < RecordCount; i++)
//                        {
//                            if ((short)Data[j][(int)rf.Id] == b)
//                            {
//                                return (j + 1);
////                                return GotoRecord(j + 1);
//                            }
//                            if (++j == RecordCount) j = 0;
//                        }
//                    }
//                    else Warnings.Add("\nYour search term isn't a string...\n)");
//                    Warnings.Add("\n No records on field : \"" + rf.Name + "\" for value: " + query + "\n");
//                    return 0;
//                case FieldType.INTEGER:
//                    int c;
//                    if (Int32.TryParse(query, out c))
//                    {
//                        for (int i = 0, j = start; i < RecordCount; i++)
//                        {
//                            if ((int)Data[j][(int)rf.Id] == c)
//                            {
//                                return (j + 1);
////                                return GotoRecord(j + 1);
//                            }
//                            if (++j == RecordCount) j = 0;
//                        }
//                    }
//                    else Warnings.Add("\nYour search term isn't an integer\n");
//                    Warnings.Add("\n No records on field : \"" + rf.Name + "\" for value: " + query + "\n");
//                    return 0;
//                case FieldType.DOUBLE:
//                    double d;
//                    if (Double.TryParse(query, out d))
//                    {
//                        for (int i = 0, j = start; i < RecordCount; i++)
//                        {
//                            if ((double)Data[j][(int)rf.Id] == d)
//                            {
//                                return (j + 1);
////                                return GotoRecord(j + 1);
//                            }
//                            if (++j == RecordCount) j = 0;
//                        }
//                    }
//                    else Warnings.Add("\nYour search term isn't a double\n");
//                    Warnings.Add("\n No records on field : \"" + rf.Name + "\" for value: " + query + "\n");
//                    return 0;
//                case FieldType.LONG:
//                    long g;
//                    if (Int64.TryParse(query, out g))
//                    {
//                        for (int i = 0, j = start; i < RecordCount; i++)
//                        {
//                            if ((long)Data[j][(int)rf.Id] == g)
//                            {
//                                return (j + 1);

////                                return GotoRecord(j + 1);
//                            }
//                            if (++j == RecordCount) j = 0;
//                        }
//                    }
//                    else Warnings.Add("\nYour search term isn't a long\n");
//                    Warnings.Add("\n No records on field : \"" + rf.Name + "\" for value: " + query + "\n");
//                    return 0;
//                case FieldType.DATE:
//                    DateTime h;
//                    if ((h = DateTime.ParseExact(query, DateFormat, CultureInfo.CurrentCulture.DateTimeFormat)) != null)
//                    {
//                        for (int i = 0, j = start; i < RecordCount; i++)
//                        {
//                            if ((DateTime)Data[j][(int)rf.Id] == h)
//                            {
//                                return (j + 1);
////                                return GotoRecord(j + 1);
//                            }
//                            if (++j == RecordCount) j = 0;
//                        }
//                    }
//                    else Warnings.Add("\nAre you following the date format?\n");
//                    Warnings.Add("\n No records on field : \"" + rf.Name + "\" for value: " + query + "\n");
//                    return 0;
//                default:
//                    Warnings.Add("\nI'm confused by your input\n");
//                    return 0;
//            }

//        }

//        #endregion


//    }

//}
