using System;
using System.Collections.Generic;
using System.Text;

namespace SvgConverter
{
    public class FileItem
    {
        string _path;
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }

        public string Name
        {
            get { return System.IO.Path.GetFileName(_path); }
        }

        long _size;
        public long Size
        {
            get { return _size; }
            set { _size = value; }
        }
    }
}
