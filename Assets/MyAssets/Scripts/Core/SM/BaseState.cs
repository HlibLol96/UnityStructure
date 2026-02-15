
namespace MyAssets.Scripts.Core.SM
{
    public abstract class BaseState
    {
        protected readonly EnemyMoving _enemyMoving;
        protected readonly StateMachine _stateMachine;
        protected float _speed = 1f;
        protected BaseState(StateMachine stateMachine,  EnemyMoving enemy)
        {
            _stateMachine = stateMachine;
            _enemyMoving = enemy;
          
        }
        public virtual void Update() { }
        public virtual void Enter() 
        {
            _enemyMoving.Agent.speed = _speed;
        }
        public virtual void Exit() { }
        public virtual void FixedUpdate() { }

    }
}
