using System;
using System.Collections.Generic;

namespace Client_ServerTest01
{
    abstract class Unit
    {
        public long Size { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Owner { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ShareTime { get; set; }  
        public Colours Colour { get; set; }
        public enum Colours 
        {
            Nocolour,Red,Orange,Yellow,Green,Cyan,Blue,Violet,Black,Gray,Brown
        }
    }
}
