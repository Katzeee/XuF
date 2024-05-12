namespace Xuf.Common
{
    public interface IState
    {
        public abstract void OnEnter();
        public abstract void OnUpdate();
        public abstract void OnExit();
    }


}
