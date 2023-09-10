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
    public bool SeePlayer()
    {
        Vector3 dir = player.position-npc.transform.position;
        float angle = Vector3.Angle(dir,npc.transform.forward);
        if(dir.magnitude<visionDist&&angle<visionAngle)
        {
            return true;
        }
        return false;
    }
    public bool AttackPlayer()
    {
        Vector3 dir = player.position - npc.transform.position;
        if(dir.magnitude<shootDist)
        return true;
        return false;
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
        anim.SetTrigger("isIdle");
        base.Enter();
    }
    public override void Update()
    {
        if(SeePlayer())
        {
            next = new Pursue(npc, agent, anim, player);
            stage = EVENT.Exit;
        }
        else if(Random.Range(0,100)<10)
        {
            next = new Patrol(npc,agent,anim,player);
            stage = EVENT.Exit;
        }
    }
    public override void Exit()
    {
        anim.ResetTrigger("isIdle");
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
        float lastDist = Mathf.Infinity;
        for (int i = 0; i < Environment.Singleton.Wp.Count; i++)
        {
            GameObject wp = Environment.Singleton.Wp[i];
            float dist = Vector3.Distance(npc.transform.position,wp.transform.position);
            if(dist<lastDist)
            {
                currentIndex = i-1;
                lastDist = dist;
            }
        }
        anim.SetTrigger("isWalking");
        base.Enter();
    }
    public override void Update()
    {
        if(agent.remainingDistance<1f)
        {
            currentIndex=currentIndex>= Environment.Singleton.Wp.Count-1 ? 0 : currentIndex + 1;
            agent.SetDestination(Environment.Singleton.Wp[currentIndex].transform.position); 
        }
        if (SeePlayer())
        {
            next = new Pursue(npc, agent, anim, player);
            stage = EVENT.Exit;
        }
    }
    public override void Exit()
    {
        anim.ResetTrigger("isWalking");
        base.Exit();
    }
}
public class Pursue : State
{
    public Pursue(GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
    :base(_npc, _agent, _anim, _player)
    {
        stateName = STATE.Pursue;
        agent.speed = 5f;
        agent.isStopped = false;
    }
    public override void Enter()
    {
        anim.SetTrigger("isRunning");
        base.Enter();
    }
    public override void Update()
    {
        agent.SetDestination(player.position);
        if(agent.hasPath)
        {
            if(AttackPlayer())
            {
                next = new Attack(npc,agent,anim,player);
                stage = EVENT.Exit;
            }
            else if(!SeePlayer())
            {
                next = new Patrol(npc, agent, anim, player);
                stage = EVENT.Exit;
            }
        }
    }
    public override void Exit()
    {
        anim.ResetTrigger("isRunning");
        base.Exit();
    }
}
public class Attack : State 
{
    float rotationSpeed = 2f;
    AudioSource shoot;

    public Attack (GameObject _npc, NavMeshAgent _agent, Animator _anim, Transform _player)
    : base(_npc, _agent, _anim, _player)
    {
        stateName = STATE.Attack;
        shoot = npc.GetComponent<AudioSource>();
    }
    public override void Enter()
    {
        anim.SetTrigger("isShooting");
        shoot.Play();
        agent.isStopped = true;
        base.Enter();
    }
    public override void Update()
    {
        Vector3 dir = player.position - npc.transform.position;
        float angle = Vector3.Angle(dir, npc.transform.forward);
        dir.y = 0;

        npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation,Quaternion.LookRotation(dir),Time.deltaTime*rotationSpeed);
        if(!AttackPlayer())
        {
            next = new Idle(npc,agent,anim,player);
            stage=EVENT.Exit;
        }
    }
    public override void Exit()
    {
        anim.ResetTrigger("isShooting");
        shoot.Stop();
        base.Exit();
    }

}