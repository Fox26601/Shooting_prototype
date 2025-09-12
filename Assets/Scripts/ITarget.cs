namespace ShootingSystem
{
    public interface ITarget
    {
        void TakeDamage(int damage);
        bool IsAlive { get; }
        void OnHit();
        void OnDestroyed();
    }
}
