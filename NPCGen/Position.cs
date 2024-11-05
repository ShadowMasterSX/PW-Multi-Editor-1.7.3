using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NpcGen_Editor.Classes
{
    public class Position
    {
        public float PosX;

        public float PosY;

        public float PosZ;

        public float DirX;

        public float DirY;

        public float DirZ;

        public Position(float X, float Y, float Z, float DX, float DY, float DZ)
        {
            PosX = X;
            PosY = Y;
            PosZ = Z;
            DirX = DX;
            DirY = DY;
            DirZ = DZ;
        }
    }
}
