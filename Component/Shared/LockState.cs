using Frontend.Common;

namespace Frontend.Component.Shared
{
    public class LockState
    {
        public bool IsActive { get; private set; } = true;
        public event Action? OnLockLost;

        public void Invalidate(Result _)
        {
            IsActive = false;
            OnLockLost?.Invoke();
        }
    }
}
