namespace FlowPathfinding
{
    public struct IntVector2
    {
        #region PublicVariables

        public int X, Y;
        
        #endregion

        #region PublicMethods

        public IntVector2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static bool operator !=(IntVector2 vector1, IntVector2 vector2)
        {
            return vector1.X != vector2.X || vector1.Y != vector2.Y;
        }

        public static bool operator ==(IntVector2 vector1, IntVector2 vector2)
        {
            return vector1.X == vector2.X && vector1.Y == vector2.Y;
        }

        public override bool Equals(System.Object obj)
        {
            if (obj == null)
            {
                return false;
            }

            IntVector2 p = (IntVector2) obj;
            return (X == p.X) && (Y == p.Y);
        }
        
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion        
    }
}