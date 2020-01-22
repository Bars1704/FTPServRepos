using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_ServerTest01
{
    class File : Unit
    {
        public string Extension { get; set; }
        public bool InstantVuive { get; set; }

        public File (FileInfo finfo, string UserName)
        {
            Extension = finfo.Extension;
            InstantVuive = false;
            Size = finfo.Length;
            Name = finfo.Name;
            Path = finfo.FullName;
            Owner = UserName;
            CreateTime = finfo.CreationTime;
            ShareTime = DateTime.MinValue;
            Colour = Colours.Nocolour;
        }
    }
}
