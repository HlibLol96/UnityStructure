
using MyAssets.Scripts.Core.SM;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMoving : MonoBehaviour
{
    //Enemy移动组件 

    [Header("Enemy Moving")]
    [SerializeField] NavMeshAgent agent;// Enemy 
    [SerializeField]private List<Transform> waypoints = new List<Transform>();// Enemy waypoint 列表
    [SerializeField]private Transform player;// Player Transform

    private StateMachine stateMachine;// Enemy 状态机  
    public PatrolState  PatrolState { get; private set; }
    public FollowState  FollowState { get; private set; }
    public List<Transform> Waypoints => waypoints;
    public NavMeshAgent Agent => agent;
    public Transform Player => player;
    public int CurrentWaypointIndex  { get; set; } = 0;
   


    private void Awake()
    {
       
        stateMachine = new StateMachine();
        PatrolState = new PatrolState(stateMachine, this);
        FollowState = new FollowState(stateMachine, this);
            
        stateMachine.Initialize(PatrolState);
    }

    void Start()
    {
        CurrentWaypointIndex = Random.Range(0, waypoints.Count); 
    }


    private void Update()
    {
        stateMachine.CurrentState.Update();
    }
    private void FixedUpdate()
    {

        stateMachine.CurrentState.FixedUpdate();
    }
}
