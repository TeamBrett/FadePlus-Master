using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FadePlus.Model
{
    class DataFile
    {
        public string filename;
        public long filesize;
        public int Id;
        public int RecordCount;
        public FileStream dataStream;
     
    }
}
