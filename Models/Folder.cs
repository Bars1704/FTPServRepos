using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client_ServerTest01
{
    class Folder:PseudoFile
    {
        public List<PseudoFile> Files { get; set; }
    }
}
