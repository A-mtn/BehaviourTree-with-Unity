using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopMe : BTAgent
{
    public GameObject[] point;
    public GameObject robber;

    public override void Start()
    {
        base.Start();
        Sequence selectPatrolPoint = new Sequence ("Select Object to Patrol");
        for (int i = 0; i < point.Length; i++)
        {
            Leaf gtp = new Leaf("Go To" + point[i].name, i, GoToCheckpoint);
            selectPatrolPoint.AddChild(gtp);
        }

        Sequence chaseRobber = new Sequence("Chase");
        Leaf canSee = new Leaf("Can See Robber?", CanSeeRobeer);
        Leaf chase = new Leaf("Chase Robber", ChaseRobber);

        chaseRobber.AddChild(canSee);
        chaseRobber.AddChild(chase);
        
        Inverter cantSeeRobber = new Inverter("Cant See Robber");
        cantSeeRobber.AddChild(canSee);

        BehaviourTree patrolConditions = new BehaviourTree();
        Sequence condition = new Sequence("Patrol Conditions");
        condition.AddChild(cantSeeRobber);
        patrolConditions.AddChild(condition);
        DepSequence patrol = new DepSequence("Patrol Until", patrolConditions, agent);
        patrol.AddChild(selectPatrolPoint);

        Selector beCop = new Selector("be a Cop");
        beCop.AddChild(patrol);
        beCop.AddChild(chaseRobber);

        tree.AddChild(beCop);
    }


    public Node.Status GoToCheckpoint(int i)
    {
        Node.Status s = GoToLocation(point[i].transform.position);
        return s;
    }

    public Node.Status CanSeeRobeer()
    {
        return CanSee(robber.transform.position, "Robber", 15.0f, 120);
    }

    Vector3 rl;
    public Node.Status ChaseRobber()
    {
        float chaseDistance = 20.0f;
        if(state == ActionState.IDLE)
        {
            rl = this.transform.position - (transform.position - robber.transform.position).normalized * chaseDistance;
        }
        return GoToLocation(rl);
    }
}
