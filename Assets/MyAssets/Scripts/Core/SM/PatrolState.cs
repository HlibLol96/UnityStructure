using MyAssets.Scripts.Core.SM;
using UnityEngine;



public class PatrolState : BaseState
{
    public PatrolState(StateMachine stateMachine, EnemyMoving enemy) : base(stateMachine, enemy)
    {
        _speed = 1f;
    }
    public override void Enter()
    {    
        base.Enter();
    }

    public override void Update()
    {
        _enemyMoving.Agent.SetDestination(_enemyMoving.Waypoints[_enemyMoving.CurrentWaypointIndex].position); // 设置 Enemy 目标位置为当前 waypoint 的位置
                                                                       // 如果 Enemy 已经接近当前 waypoint，则切换到下一个 waypoint 

        if (Vector3.Distance(_enemyMoving.transform.position, _enemyMoving.Waypoints[_enemyMoving.CurrentWaypointIndex].transform.position) < 2)
        {
            _enemyMoving.CurrentWaypointIndex = (_enemyMoving.CurrentWaypointIndex + 1) % _enemyMoving.Waypoints.Count;

        }
       
        if (Vector3.Distance(_enemyMoving.Agent.transform.position, _enemyMoving.Player.position) < 20)
        {
            _stateMachine.ChangeState(_enemyMoving.FollowState);
        }
    }
}
