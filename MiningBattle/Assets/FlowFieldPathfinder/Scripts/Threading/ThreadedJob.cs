using System.Collections;
using System.Threading;

namespace FlowPathfinding
{
    public class ThreadedJob
    {
        #region PrivateVariables

        private bool _mIsDone;
        
        private readonly object _mHandle = new object();
        
        private Thread _mThread;

        #endregion

        #region PrivateMethods

        private bool IsDone
        {
            get
            {
                bool tmp;
                lock (_mHandle)
                {
                    tmp = _mIsDone;
                }
                return tmp;
            }
            set
            {
                lock (_mHandle)
                {
                    _mIsDone = value;
                }
            }
        }
        
        private void Run()
        {
            ThreadFunction();
            IsDone = true;
        }
        
        private IEnumerator WaitFor()
        {
            while (!Update())
            {
                yield return null;
            }
        }

        #endregion

        #region ProtectedMethods

        protected virtual void ThreadFunction() {}

        protected virtual void OnFinished() {}

        #endregion

        #region PublicMethods

        public void Start()
        {
            _mThread = new Thread(Run);
            _mThread.Start();
        }
        
        public void Abort()
        {
            _mThread.Abort();
        }
        
        public bool Update()
        {
            if (IsDone)
            {
                OnFinished();
                return true;
            }
            return false;
        }

        #endregion
    }
}