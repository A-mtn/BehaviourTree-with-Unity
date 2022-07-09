using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PatronBehaviour : BTAgent
{
    public GameObject[] art;
    public GameObject home;
    public GameObject frontdoor;

    [Range(0, 1000)]
    public int boredom = 0;

    public bool ticket = false;
    public bool isWaiting = false;

    public override void Start()
    {
        base.Start();

        Leaf goToHome = new Leaf(name + " Go To Home", GoToHome);
        Leaf goToFrontDoor = new Leaf(name + " Go To Frontdoor", GoToFrontDoor);
        Leaf isBored = new Leaf(name + " Is Bored", IsBored);
        Leaf isOpen = new Leaf(name + " Is Open", IsOpen);
        
        RSelector selectObject = new RSelector (name + " Select Art to See");
        for (int i = 0; i < art.Length; i++)
        {
            Leaf gta = new Leaf(name + " Go To " + art[i].name, i, GoToArt);
            selectObject.AddChild(gta);
        }

        Sequence viewArt = new Sequence(name + " View Art");
        viewArt.AddChild(isOpen);
        viewArt.AddChild(isBored);
        viewArt.AddChild(goToFrontDoor);

        Leaf noTicket = new Leaf(name + " Wait For Ticket", NoTicket);
        Leaf isWaiting = new Leaf(name +  " Waiting for Worker", IsWaiting);

        BehaviourTree waitForTicket = new BehaviourTree();
        waitForTicket.AddChild(noTicket);

        Loop getTicket = new Loop(name + " Ticket", waitForTicket);
        getTicket.AddChild(isWaiting);

        viewArt.AddChild(getTicket);

        BehaviourTree whileBored = new BehaviourTree();
        whileBored.AddChild(isBored);
        Loop lookAtPaintings = new Loop(name + " See Multiple Arts", whileBored);
        lookAtPaintings.AddChild(selectObject);

        viewArt.AddChild(lookAtPaintings);
        viewArt.AddChild(goToHome);

        BehaviourTree galleryOpenCondition = new BehaviourTree();
        galleryOpenCondition.AddChild(isOpen);
        DepSequence bePatron = new DepSequence(name + " Be an Art Patron", galleryOpenCondition, agent);
        bePatron.AddChild(viewArt);

        Selector viewArtWithFallBack = new Selector(name + " View Art With Fallback");
        viewArtWithFallBack.AddChild(bePatron);
        viewArtWithFallBack.AddChild(goToHome);

        tree.AddChild(viewArtWithFallBack);

        StartCoroutine("IncreaseBoredom");
    }

    IEnumerator IncreaseBoredom()
    {
        while (true)
        {
            boredom = Mathf.Clamp(boredom + 20, 0, 1000);
            yield return new WaitForSeconds(Random.Range(1, 5));
        }
    }

    public Node.Status GoToArt(int i)
    {
        if (!art[i].activeSelf) return Node.Status.FAILURE;

        Node.Status s = GoToLocation(art[i].transform.position);
        if (s == Node.Status.SUCCESS)
        {
            boredom = Mathf.Clamp(boredom - 150, 0, 1000);
        }
        return s;
    }

    public Node.Status GoToHome()
    {
        Node.Status s = GoToLocation(home.transform.position);
        isWaiting = false;
        return s;
    }

    public Node.Status GoToFrontDoor()
    {
        Node.Status s = GoToDoor(frontdoor);
        return s;
    }

    public Node.Status IsBored()
    {
        if (boredom < 100)
            return Node.Status.FAILURE;
        else
            return Node.Status.SUCCESS;
    }

    public Node.Status NoTicket()
    {
        if(ticket || IsOpen() == Node.Status.FAILURE)
        {
            return Node.Status.FAILURE;
        }
        else
            return Node.Status.SUCCESS;
    }

    public Node.Status IsWaiting()
    {
        if(Blackboard.Instance.RegisterPatron(this.gameObject))
        {
            isWaiting = true;
            return Node.Status.SUCCESS;
        }
        else
        {
            return Node.Status.FAILURE;
        }
    }

}
