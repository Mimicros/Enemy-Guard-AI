using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class State
{
    public enum STATE
    {
        Idle, Patrol, Pursue, Attack
    };
    public enum EVENT
    {
        Enter, Update, Exit
    }
    public STATE stateName;
    protected EVENT stage;
    protected GameObject npc;
    protected Animator anim;
    protected NavMeshAgent agent;
    protected Transform player;
    protected State next;

    float visionDist = 10;
    float visionAngle = 30;
    float shootDist = 7;

    public State(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
    {
        npc = _npc;
        agent = _agent;
        anim = _anim;
        player = _player;
        stage = EVENT.Enter;
    }
    public virtual void Enter() { stage = EVENT.Update; }
    public virtual void Update() { stage = EVENT.Update; }
    public virtual void Exit() { stage = EVENT.Exit; }

    public State Process()
    {
        if(stage==EVENT.Enter)Enter();
        if(stage==EVENT.Update)Update();
        if(stage==EVENT.Exit)
        {
            Exit();
            return next;
        }
        return this;
    }
}
public class Idle : State
{
    public Idle(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
    :base(_npc,_agent,_anim,_player)
    {
        stateName=STATE.Idle;
    }
    public override void Enter()
    {
        anim.SetTrigger("IsIdle");
        base.Enter();
    }
    public override void Update()
    {
        if(Random.Range(0,100)<10)
        {
            next = new Patrol(npc,agent,anim,player);
            stage = EVENT.Exit;
        }
        base.Update();
    }
    public override void Exit()
    {
        anim.ResetTrigger("IsIdle");
        base.Exit();
    }
}
public class Patrol : State
{
    int currentIndex = -1;

    public Patrol(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
    : base(_npc, _agent, _anim, _player)
    {
        agent.speed = 2f;
        agent.isStopped=false;
        stateName = STATE.Patrol;
    }
    public override void Enter()
    {
        currentIndex = 0;
        anim.SetTrigger("IsWalking");
        base.Enter();
    }
    public override void Update()
    {
        if(agent.remainingDistance<1f)
        {
            currentIndex=currentIndex>= Environment.Singleton.Wp.Count-1 ? 0 : currentIndex + 1;
            agent.SetDestination(Environment.Singleton.Wp[currentIndex].transform.position); 
        }
        base.Update();
    }
    public override void Exit()
    {
        anim.ResetTrigger("IsWalking");
        base.Exit();
    }
}