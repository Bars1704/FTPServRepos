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
        public int FilesInsideCount { get; set; }
        public List<Unit> Files { get; set; }
        public Folder(DirectoryInfo dinfo,string UserName,int FileCount)
        {
            Name = dinfo.Name;
            Path = dinfo.FullName;
            Owner = UserName;
            CreateTime = dinfo.CreationTime;
            ShareTime = DateTime.MinValue;
            Colour = Colours.Nocolour;
            Size = GetSize(dinfo);
            Files = GetFilesInside(dinfo, UserName, ref FileCount);
            FilesInsideCount = FileCount;
        }
        public long GetSize(DirectoryInfo dinfo)
        {
            long Size = 0;
            var Files = dinfo.GetFiles();
            foreach (var CurFile in Files)
            {
                Size += CurFile.Length;
            }
            var Dirs = dinfo.GetDirectories();
            foreach (var CurDir in Dirs)
            {
                Size += GetSize(CurDir);
            }
            return Size;
        }
        public List<Unit> GetFilesInside(DirectoryInfo dinfo,string UserName ,ref int FilesCount )
        {
            var Files = dinfo.GetFiles();
            var Dirs = dinfo.GetDirectories();
            List<Unit> AllFiles = new List<Unit>();
            foreach (var CurFile in Files)
            {
                AllFiles.Add(new File(CurFile, UserName));
                FilesCount++;
            }
            foreach (var CurDir in Dirs)
            {
                AllFiles.Add(new Folder(CurDir, UserName, FilesCount));
            }
            return AllFiles;
        }
        public void Serialization(string Path , Folder rootFile)
        {
            DirectoryInfo RootDir = new DirectoryInfo(Path);
            var Files = RootDir.GetFiles();
        }
    }
}
