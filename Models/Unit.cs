using System;
using System.IO;

namespace Client_ServerTest01
{
    internal abstract class Unit
    { 
        public Colours Colour { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime ShareTime { get; set; }
        public long Size { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public string Path { get; set; }
        public Unit()
        {

        }
        public Unit(string UserName) {
            Owner = UserName;
            ShareTime = DateTime.MinValue;
            Colour = Colours.Nocolour;
        }
        public enum Colours
        {
            Nocolour, Red, Orange, Yellow, Green, Cyan, Blue, Violet, Black, Gray, Brown
        }
    }
}