using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace FadePlus.Model
{
    public abstract class Field
    {
        public Type type { get; set; }
        public int Size { get { if (isNullable) return size + 1; else return size; }  }
//        public int Size { get{ return size; }  }
        public int size;
        public string Name { get; set; }
        public int startPosition { get; set; }
        public bool isNullable { get; set; }
        public bool SubTemplateIndicator { get; set; }

        public virtual void ReadValue(FileStream dataStream)
        {
            if(this.isNullable)
                dataStream.Seek(1, SeekOrigin.Current);
        }

        public abstract override string ToString();

        public abstract bool Equals(string query);

        public Field(Type typ, int length, string name, int startPosition, bool IsSubTemplateIndicator, bool isNullable)
        {
            this.type = typ;
            this.size = length;
            this.Name = name;
            this.startPosition = startPosition;
            this.SubTemplateIndicator = IsSubTemplateIndicator;
            this.isNullable = isNullable;
        }
    }
}
