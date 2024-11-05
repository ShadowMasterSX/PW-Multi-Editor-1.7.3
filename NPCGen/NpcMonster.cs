using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpcGen_Editor.Classes
{
    public class NpcMonster
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public NpcMonster(int iD, string name)
        {
            Id = iD;
            Name = name;
        }
        public NpcMonster() { }
    }

    public class NpcMonster_String
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public NpcMonster_String(string iD, string name)
        {
            Id = iD;
            Name = name;
        }
        public NpcMonster_String() { }
    }

}
