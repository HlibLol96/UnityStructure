using UnityEngine;
using MyAssets.Scripts.Core.SM;

public class FollowState : BaseState
{
    public FollowState(StateMachine stateMachine, EnemyMoving enemy) : base(stateMachine, enemy)
    {
        _speed = 10f;
    }
    public override void Enter()
    {
        _enemyMoving.Agent.speed = _speed;
    }
    public override void Update()
    {
        _enemyMoving.Agent.SetDestination(_enemyMoving.Player.position);
        if (Vector3.Distance(_enemyMoving.Agent.transform.position, _enemyMoving.Player.position) > 20)
        {
            _stateMachine.ChangeState(_enemyMoving.PatrolState);
        }
    }
}
