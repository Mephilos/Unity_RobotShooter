using UnityEngine;
using System.Collections;
public abstract class BaseEnemyState
{
    protected EnemyBrain brain;

    public BaseEnemyState(EnemyBrain brain)
    {
        this.brain = brain;
    }

    public abstract void Enter();
    public abstract void Execute();
    public abstract void Exit();
}

