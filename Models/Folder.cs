using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace Client_ServerTest01
{
    [Serializable]
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
        public Folder(DirectoryInfo dinfo, int FileCount,Folder OldFolder)
        {
            Name = dinfo.Name;
            Path = dinfo.FullName;
            Owner = OldFolder.Owner;
            CreateTime = dinfo.CreationTime;
            ShareTime = OldFolder.ShareTime;
            Colour = OldFolder.Colour;
            Size = GetSize(dinfo);
            Files = GetFilesInside(dinfo, OldFolder.Owner, ref FileCount,OldFolder);
            
            FilesInsideCount = FileCount;
        }
        private long GetSize(DirectoryInfo dinfo)
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
        private List<Unit> GetFilesInside(DirectoryInfo dinfo,string UserName ,ref int FilesCount )
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
        private List<Unit> GetFilesInside(DirectoryInfo dinfo, string UserName, ref int FilesCount, Folder OldFolder)
        {
            var Files = dinfo.GetFiles();
            var Dirs = dinfo.GetDirectories();
            List<Unit> AllFiles = new List<Unit>();
            foreach (var CurFile in Files)
            {
                File OldFile = (File)OldFolder.Files.Find((x) => x.Name == CurFile.Name && x is File);
                AllFiles.Add(new File(CurFile,OldFile));
                FilesCount++;
            }
            foreach (var CurDir in Dirs)
            {
                OldFolder = (Folder)OldFolder.Files.Find((x) => x.Name == CurDir.Name&&x is Folder);
                AllFiles.Add(new Folder(CurDir, FilesCount, OldFolder));
            }
            return AllFiles;
        }
        public void Serialization(string Path , Folder rootFile,string UserName)
        {
            BinaryFormatter Serializer = new BinaryFormatter();
            FileInfo Check = new FileInfo(Path + "\\Cache.dat");
            FileStream fs;
            DirectoryInfo RootDir = new DirectoryInfo(Path);
            Folder RootFile;
            if (Check.Exists)
            {
                fs = new FileStream(Path + "\\Cache.dat", FileMode.Open);
                Folder OldFolder = (Folder)Serializer.Deserialize(fs);
                RootFile = new Folder(RootDir, 0, OldFolder);
                
            }
            else
            { 
                RootFile = new Folder(RootDir, UserName, 0);
            }
            RootFile.Name = "Root";
            fs = new FileStream(Path + "\\Cache.dat", FileMode.Create);
            Serializer.Serialize(fs, rootFile);
        }
    }
}
