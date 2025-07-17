namespace Xuf.Core
{
    public interface IGameSystem
    {
        public int Priority { get; }
        public void Update(float deltaTime, float unscaledDeltaTime) { }
    }
}
