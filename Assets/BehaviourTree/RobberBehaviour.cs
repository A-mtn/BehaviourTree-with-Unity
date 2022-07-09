using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobberBehaviour : BTAgent
{
    public GameObject diamond;
    public GameObject van;
    public GameObject backdoor;
    public GameObject frontdoor;
    public GameObject painting;
    public GameObject cop;

    private GameObject pickup;

    public GameObject[] art;

    Leaf goToBackDoor;
    Leaf goToFrontDoor;

    [Range(0,1000)]
    public int money = 0;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        
        Leaf goToDiamond = new Leaf("Go To Diamond", GoToDiamond, 1);
        Leaf  goToPainting = new Leaf("Go To Painting", GoToPainting, 2);
        Leaf hasGotMoney = new Leaf("Has Got Money", HasMoney);
        Leaf goToVan = new Leaf("Go To Van", GoToVan);

        goToBackDoor = new Leaf("Go To Backdoor", GoToBackDoor, 2);
        goToFrontDoor = new Leaf("Go To Frontdoor", GoToFrontDoor, 1);
        PSelector opendoor = new PSelector("Open Door");
        RSelector selectObject = new RSelector ("Select Object to Steal");
        for (int i = 0; i < art.Length; i++)
        {
            Leaf gta = new Leaf("Go To " + art[i].name, i, GoToArt);
            selectObject.AddChild(gta);
        }

        Sequence runAway = new Sequence("Run Away");
        Leaf canSee = new Leaf("Can See Cop", CanSeeCop);
        Leaf flee = new Leaf("Flee From Cop", FleeFromCop);

        Inverter invertMoney  = new Inverter("Invert Money");
        invertMoney.AddChild(hasGotMoney);
 
        opendoor.AddChild(goToFrontDoor);
        opendoor.AddChild(goToBackDoor);

        Inverter cantSeeCop = new Inverter("Cant See Cop");
        cantSeeCop.AddChild(canSee);

        Leaf isOpen = new Leaf("Is Closed", IsOpen);
        Inverter isClosed = new Inverter("Is Closed");
        isClosed.AddChild(isOpen);

        BehaviourTree stealConditions = new BehaviourTree();
        Sequence conditions = new Sequence("Stealing Conditions");
        conditions.AddChild(isClosed);
        conditions.AddChild(cantSeeCop);
        conditions.AddChild(invertMoney);
        stealConditions.AddChild(conditions);

        DepSequence steal = new DepSequence("Steal Something", stealConditions, agent);
        //steal.AddChild(isClosed);
       // steal.AddChild(invertMoney);
        steal.AddChild(opendoor);
        steal.AddChild(selectObject);
        steal.AddChild(goToVan);

        Selector stealWithFallBack = new Selector("Steal With Fall Back");
        stealWithFallBack.AddChild(steal);
        stealWithFallBack.AddChild(goToVan);

        runAway.AddChild(canSee);
        runAway.AddChild(flee);

        Selector beThief = new Selector("Be a thief");
        beThief.AddChild(stealWithFallBack);
        beThief.AddChild(runAway);

        tree.AddChild(beThief);

        tree.PrintTree();

        StartCoroutine("DecreaseMoney");
    }

    IEnumerator DecreaseMoney()
    {
        while (true)
        {
            money = Mathf.Clamp(money - 50, 0, 1000);
            yield return new WaitForSeconds(Random.Range(1, 5));
        }
    }

    public Node.Status CanSeeCop()
    {
        return CanSee(cop.transform.position, "Cop", 10, 90);
    }

    public Node.Status FleeFromCop()
    {
        return Flee(cop.transform.position, 10);
    }

    public Node.Status HasMoney()
    {
        if(money < 900)
            return Node.Status.FAILURE;
        return Node.Status.SUCCESS;
    }

    public Node.Status GoToDiamond()
    {
        if (!diamond.activeSelf)
        {
            return Node.Status.FAILURE;
        }
        Node.Status s = GoToLocation(diamond.transform.position);
        if(s == Node.Status.SUCCESS)
        {
            diamond.transform.parent = this.gameObject.transform;
            pickup = diamond;
        }
        return s;
    }

    public Node.Status GoToPainting()
    {
        if (!painting.activeSelf)
        {
            return Node.Status.FAILURE;
        }
        Node.Status s = GoToLocation(painting.transform.position);
        if(s == Node.Status.SUCCESS)
        {
            painting.transform.parent = this.gameObject.transform;
            pickup = painting;
        }
        return s;
    }

    public Node.Status GoToArt(int i)
    {
        if (!art[i].activeSelf) return Node.Status.FAILURE;

        Node.Status s = GoToLocation(art[i].transform.position);
        if(s == Node.Status.SUCCESS)
        {
            art[i].transform.parent = this.gameObject.transform;
            pickup = art[i];
        }
        return s;
    }

    public Node.Status GoToVan()
    {
        Node.Status s = GoToLocation(van.transform.position);
        if(s == Node.Status.SUCCESS)
        {
            if (pickup != null)
            {
                pickup.SetActive(false);
                money += 100;
                pickup = null;
            }
        }
        return s;
    }

    public Node.Status GoToBackDoor()
    {
        Node.Status s = GoToDoor(backdoor);
        if (s == Node.Status.FAILURE)
            goToBackDoor.sortOrder = 10;
        else
            goToBackDoor.sortOrder = 1;
        return s;
    }

    public Node.Status GoToFrontDoor()
    {
        Node.Status s = GoToDoor(frontdoor);
        if (s == Node.Status.FAILURE)
            goToFrontDoor.sortOrder = 10;
        else
            goToFrontDoor.sortOrder = 1;
        return s;
    }
}
