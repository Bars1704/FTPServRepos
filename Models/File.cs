using System;
using System.IO;

namespace Client_ServerTest01
{
    internal class File : Unit
    {
        public string Extension { get; set; }
        public bool InstantVuive { get; set; }

        public File(FileInfo finfo, string UserName)
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

        public File(FileInfo finfo, File OldFile)
        {
            Extension = finfo.Extension;
            InstantVuive = false;
            Size = finfo.Length;
            Name = finfo.Name;
            Path = finfo.FullName;
            Owner = OldFile.Owner;
            CreateTime = finfo.CreationTime;
            ShareTime = OldFile.ShareTime;
            Colour = OldFile.Colour;
        }
    }
}