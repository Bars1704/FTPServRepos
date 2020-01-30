namespace Client_ServerTest01
{
    public class Permissions
    {
        public Permissions()
        {
            Dcreate = false;
            DDelete = false;
            DList = true;
            FAppend = true;
            FDelete = false;
            FRead = true;
            FWrite = true;
        }

        public bool Dcreate { get; set; }
        public bool DDelete { get; set; }
        public bool DList { get; set; }
        public bool FAppend { get; set; }
        public bool FDelete { get; set; }
        public bool FRead { get; set; }
        public bool FWrite { get; set; }
    }
}