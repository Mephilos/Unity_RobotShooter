using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public abstract class EnemyBrain : Enemy
{
    // public enum AIState { Patrol, Combat, Search }
    // [SerializeField] protected AIState currentState;
    // [SerializeField] protected float combatTime = 5f;
    // protected float combatDuration;
    // protected bool isActing = false;

    public NavMeshAgent Agent => agent;
    public EnemySight Sight => sight;
    public EnemyTactic Tactic => tactic;
    public Animator Animator => animator;

    public Vector3 LastPlayerPosition { get; set; }
    public BaseEnemyState PatrolState { get; private set; }
    public BaseEnemyState CombatState { get; private set; }
    public BaseEnemyState SearchState { get; private set; }

    protected BaseEnemyState currentState;
    protected NavMeshAgent agent;
    protected EnemySight sight;
    protected EnemyTactic tactic;


    protected override void Awake()
    {
        base.Awake();
        agent = GetComponent<NavMeshAgent>();
        sight = GetComponent<EnemySight>();
        tactic = GetComponent<EnemyTactic>();
    }

    protected virtual void InitailizeState()
    {
        PatrolState = new EnemyPatrolState(this);
        SearchState = new EnemySearchState(this);
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        ChangeState(PatrolState); // 초기 상태 패트롤
    }

    protected override void Update()
    {
        if (playerTransform == null) return;

        if (sight.CanSeePlayer(playerTransform))
        {
            LastPlayerPosition = playerTransform.position;

            if (currentState != CombatState)
            {
                OnFound();
                ChangeState(CombatState);
            }
        }
        currentState?.Execute();
    }

    public void ChangeState(BaseEnemyState newEnemyState)
    {
        if (currentState == newEnemyState) return;
        currentState?.Exit();
        currentState?.Enter();
        currentState = newEnemyState;
    }
    protected override void OnDamage()
    {
        if (currentState == CombatState) return;

        StopAllCoroutines();

        base.OnDamage();

        Vector3 lookDir = (playerTransform.position - transform.position).normalized;
        lookDir.y = 0;

        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        if (currentState != CombatState)
        {
            Debug.Log("기습당함");

            LastPlayerPosition = playerTransform.position;
            ChangeState(CombatState);
        }
    }

    protected abstract IEnumerator CombatRoutine();

    public void StartStateCorutine(IEnumerator routine)
    {
        StopAllCoroutines();
        StartCoroutine(routine);
    }

    protected virtual void OnFound() { }
}
