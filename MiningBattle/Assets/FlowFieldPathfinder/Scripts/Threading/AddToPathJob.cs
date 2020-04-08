namespace FlowPathfinding
{
    public class AddToPathJob : ThreadedJob
    {
        #region PrivateVariables

        private readonly Pathfinder _pathfinder;
        private readonly FlowFieldPath _path;
        private readonly WorldArea _area;
        private readonly Tile _tile;

        #endregion

        #region ProtectedMethods

        protected override void ThreadFunction()
        {
            _pathfinder.worldData.IntegrationFieldManager.CreateExtraField(_area, _tile, _path);
        }

        protected override void OnFinished()
        {
            _pathfinder.PathAdjusted(_path, _area, _tile);
        }

        #endregion

        #region PublicMethods

        public AddToPathJob(WorldArea area, Tile tile, FlowFieldPath path, Pathfinder pathfinder)
        {
            _path = path;
            _area = area;
            _tile = tile;
            _pathfinder = pathfinder;
        }

        #endregion
    }
}