using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Worker : BTAgent
{
    public GameObject office;
    GameObject patron;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        Leaf patronStillWaiting = new Leaf(name + " Is Patron Waiting?", PatronWaiting);
        Leaf allocatePatron = new Leaf(name + " Allocate the Patron", AllocatePatron);
        Leaf goToPatron = new Leaf(name + " Go To Patron", GoToPatron);
        Leaf goToOffice = new Leaf(name + " Go To Office", GoToOffice);

        Sequence getPatron = new Sequence("Find a Patron");
        getPatron.AddChild(allocatePatron);

        BehaviourTree waiting = new BehaviourTree();
        waiting.AddChild(patronStillWaiting);

        DepSequence moveToPatron = new DepSequence("Moving To Patron", waiting, agent);
        moveToPatron.AddChild(goToPatron);

        getPatron.AddChild(moveToPatron);
        
        Selector beWorker = new Selector("Be a Worker");
        beWorker.AddChild(getPatron);
        beWorker.AddChild(goToOffice);

        tree.AddChild(beWorker);

    }
    
    public Node.Status PatronWaiting()
    {
        if (patron == null)
            return Node.Status.FAILURE;
        
        if(patron.GetComponent<PatronBehaviour>().isWaiting == true)
            return Node.Status.SUCCESS;
        else
            return Node.Status.FAILURE;

    }

    public Node.Status AllocatePatron()
    {
        if(Blackboard.Instance.patrons.Count == 0)
            return Node.Status.FAILURE;
        patron = Blackboard.Instance.patrons.Pop();
        if (patron == null)
            return Node.Status.FAILURE;
        return Node.Status.SUCCESS;
    }
    
    public Node.Status GoToPatron()
    {   
        if(patron == null) return Node.Status.FAILURE;
        Node.Status s = GoToLocation(patron.transform.position);
        if (s == Node.Status.SUCCESS)
        {
            patron.GetComponent<PatronBehaviour>().ticket = true;
            patron = null;
        }
        return s;
    }

    public Node.Status GoToOffice()
    {
        Node.Status s = GoToLocation(office.transform.position);
        patron = null;
        return s;
    }

}
