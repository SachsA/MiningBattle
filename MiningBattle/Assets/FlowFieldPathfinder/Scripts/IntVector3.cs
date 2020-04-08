using System;

namespace FlowPathfinding
{
    public struct IntVector3
    {
        #region PublicVariables

        public int X, Y, Z;

        #endregion

        #region PublicMethods

        public IntVector3(int x, int y, int z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static bool operator !=(IntVector3 vector1, IntVector3 vector2)
        {
            return vector1.X != vector2.X || vector1.Y != vector2.Y || vector1.Z != vector2.Z;
        }

        public static bool operator ==(IntVector3 vector1, IntVector3 vector2)
        {
            return vector1.X == vector2.X && vector1.Y == vector2.Y && vector1.Z == vector2.Z;
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            IntVector3 p = (IntVector3) obj;
            return (X == p.X) && (Y == p.Y) && (Z == p.Z);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        #endregion
    }
}