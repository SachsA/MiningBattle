using System.Collections.Generic;

namespace FlowPathfinding
{
    public class IntVectorComparer : EqualityComparer<IntVector2>
    {
        #region PublicMethods

        public override bool Equals(IntVector2 vector1, IntVector2 vector2)
        {
            return vector1.X == vector2.X && vector1.Y == vector2.Y;
        }

        public override int GetHashCode(IntVector2 obj)
        {
            return base.GetHashCode();
        }

        #endregion
    }
}
