namespace FlowPathfinding
{
    public class MapAdjustedJob : ThreadedJob
    {
        #region PrivateVariables

        private readonly Pathfinder _pathfinder;

        #endregion

        #region ProtectedMethods

        protected override void ThreadFunction()
        {
            _pathfinder.worldData.WorldManager.InputChanges();
        }

        protected override void OnFinished()
        {}

        #endregion

        #region PublicMethods

        public MapAdjustedJob(Pathfinder pathfinder)
        {
            _pathfinder = pathfinder;
        }

        #endregion
    }
}
