using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_ServerTest01
{
    class Folder: Unit
    {
        public int FilesInside { get; set; }
        public List<Unit> Files { get; set; }
        public void Serialization(string Path , Folder rootFile)
        {
            DirectoryInfo RootDir = new DirectoryInfo(Path);
            
        }
    }
}
