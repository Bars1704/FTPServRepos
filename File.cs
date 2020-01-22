using System;
using System.Collections.Generic;

namespace Client_ServerTest01
{
    class File
    {
        public string Name { get; set; }
        public string Extension { get; set; }
        public string Path { get; set; }
        public float Size { get; set; }
        public string Owner { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ShareTime { get; set; }
        public bool IsFolder { get; set; }
        public List<File> Files { get; set; }
        public enum Colours 
        {
            Nocolour,Red,Orange,Yellow,Green,Cyan,Blue,Violet,Black,Gray,Brown
        }
        //TODO: Написать конструкторы
    }
}
