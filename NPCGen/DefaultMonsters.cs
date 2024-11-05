using System;
using System.Collections.Generic;
using System.Linq;

namespace NpcGen_Editor.Classes
{
    public class DefaultMonsters
    {
        public List<ExtraMonsters> MobDops;

        public int Amount_in_group { get; set; }

        public byte bAutoRevive { get; set; }

        public byte BInitGen { get; set; }

        public byte BValicOnce { get; set; }

        public int dwGenId { get; set; }

        public int iGroupType { get; set; }

        public int Life_time { get; set; }

        public int Location { get; set; }

        public int MaxRespawnTime { get; set; }

        public int Trigger_id { get; set; }

        public int Type { get; set; }

        public float X_direction { get; set; }

        public float X_position { get; set; }

        public float X_random { get; set; }

        public float Y_direction { get; set; }

        public float Y_position { get; set; }

        public float Y_random { get; set; }

        public float Z_direction { get; set; }

        public float Z_position { get; set; }

        public float Z_random { get; set; }
    }
}
