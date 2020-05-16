using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;


public enum CreatureType
{
    Herbivore,
    Carnivore
}
public abstract class CreatureAgent : Agent
{
    [Header("Creature Type")]
    public CreatureType CreatureType;
    [Header("Creature Points (100 Max)")]
    public float MaxEnergy;
    public float MatureSize;
    public float GrowthRate;
    public float EatingSpeed;
    public float MaxSpeed;
    public float AttackDamage;
    public float DefendDamage;
    //public float Eyesight;
    //public Area area;
    public float MaxLife;

    [Header("Monitoring")]
    public float Energy;
    public float Size;
    public float Age;
    public string currentAction;
    public float Life;
    public bool HeuristicActions;

    
    [Header("Child")]
    public GameObject ChildSpawn;
    public GameObject FoodPrefab;

    [Header("Species Parameters")]    
    public float AgeRate = .001f;

    protected Vector2 bounds;
    private GameObject Environment;
    private Rigidbody agentRB;
    protected float nextAction;
    private bool died;
    //private RayPerception rayPer;    
    //private TerrariumAcademy academy;
    private int count;
    
    private void Awake()
    {
        OnEpisodeBegin();
    }

    //public override void AgentReset()
    public override void OnEpisodeBegin()
    {
        //Called on every reset
        Size = 1;
        Energy = 1;
        Age = 0;
        Life = MaxLife;
        //bounds = Area.InstanceArea.GetBounds();
        bounds = GetEnvironmentBounds();
        var x = Random.Range(-bounds.x, bounds.x);
        var z = Random.Range(-bounds.y, bounds.y);
        transform.position = new Vector3(x, 1, z);
        //Area.Instance.AddGameObject(gameObject);
        TransformSize();
        Initialize();
    }
/* 
    public override void AgentOnDone()
    {
        
    } */

    //public override void InitializeAgent()
    public override void Initialize()
    {
        /// Initial setup, called when the agent is enabled
        base.Initialize();
        //rayPer = GetComponent<RayPerception>();
        agentRB = GetComponent<Rigidbody>();
        currentAction = "Idle";
        // add to the area
        //Area.Instance.AddGameObject(gameObject);
        //Debug.Log(Area.Instance.Herbivores.Count);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        /*
        float rayDistance = Eyesight;
        float[] rayAngles = { 20f, 90f, 160f, 45f, 135f, 70f, 110f };
        string[] detectableObjects = { "plant", "herbivore", "carnivore" };
        sensor.AddObservation(rayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
        */
        Vector3 localVelocity = transform.InverseTransformDirection(agentRB.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);
        sensor.AddObservation(Energy);
        sensor.AddObservation(Size);
        sensor.AddObservation(Age);
        sensor.AddObservation(Float(CanEat));
        sensor.AddObservation(Float(CanReproduce));
    }

    private float Float(bool val)
    {
        if (val) return 1.0f;
        else return 0.0f;
    }

    //public override void AgentAction(float[] vectorAction, string textAction)
    public override void OnActionReceived(float[] vectorAction)

    {
            
        //Action Space 7 float
        // 0 = Move
        // 1 = Eat
        // 2 = Reproduce
        // 3 = Attack
        // 4 = Defend
        // 5 = move orders
        // 6 = rotation

        if (vectorAction[0] > .5)
        {
            MoveAgent(vectorAction);
        }
        else if (vectorAction[1] > .5)
        {
            Eat();
        }
        else if (vectorAction[2] > .5)
        {
            Reproduce();
        }
        else if (vectorAction[3] > .5)
        {
            Attack();
        }
        else if (vectorAction[4] > .5)
        {
            Defend();
        }
    }

    protected virtual void Defend()
    {
        currentAction = "Defend";
        Energy -= .01f;
        nextAction = Time.timeSinceLevelLoad + (25 / MaxSpeed);
    }

    protected abstract void Attack();
        
    void Update()
    {
        if (OutOfBounds)
        {
            AddReward(-1f);
            EndEpisode();
            return;
        }
        if (Buried)
        {
            AddReward(-.5f);
            EndEpisode();
        }
        if (Dead) return;

        if (Life <= 0)
        {
            AddReward(-.5f);
            TransformToFood();
            EndEpisode();
            Debug.Log(Life);
        }
        if (CanGrow) Grow();        
        //if (CanReproduce) Reproduce();        
        Age += AgeRate; 
        MonitorLog();
    }

    public void FixedUpdate()
    {
        if (Time.timeSinceLevelLoad > nextAction)
        {
            currentAction = "Deciding";

            RequestDecision();
        }

    }

    public void MonitorLog()
    {
        MLAgents.Monitor.Log("Action", currentAction, transform);
        MLAgents.Monitor.Log("Size", Size / MatureSize, transform);
        MLAgents.Monitor.Log("Energy", Energy / MaxEnergy, transform);
        MLAgents.Monitor.Log("Age", Age / MatureSize, transform);
    }

    public bool OutOfBounds
    {
        get
        {
            // if (transform.position.y < 0) return true;
            // if (transform.position.x > bounds.x ||
            //     transform.position.x < -bounds.x ||
            //     transform.position.z > bounds.y ||
            //     transform.position.z < -bounds.y)
            //     {Debug.Log("Out");
            //     //Debug.Log("X: " + transform.position.x + "; Z: " + transform.position.z);
            //     return true;}
            //return false;
            return !(Area.Instance.InBounds(transform.position.x, transform.position.z));
        }
    }
    
    protected void TransformSize()
    {
        transform.localScale = Vector3.one * Mathf.Pow(Size,1/3);
    }

    bool CanGrow
    {
        get
        {
            return Energy > ((MaxEnergy / 2) + 1);
        }
    }

    protected abstract bool CanEat{ get; }

    protected GameObject FirstAdjacent(string tag)
    {
        var colliders = Physics.OverlapSphere(transform.position, 1.2f);
        foreach (var collider in colliders)
        {
            Debug.Log(collider.gameObject.tag);
            if (collider.gameObject.tag == tag && collider.transform != transform)
            {
                Debug.Log(collider.gameObject.tag);
                return collider.gameObject;
            }
        }
        return null;
    }

    protected GameObject FirstAdjacentDead(string tag)
    {
        var colliders = Physics.OverlapSphere(transform.position, 2f);
        foreach (var collider in colliders)
        {
            var obj = collider.gameObject.GetComponent<CreatureAgent>();
            if (collider.gameObject.tag == tag && obj != null && obj.died)
            {
                return collider.gameObject;
            }
        }
        return null;
    }

    bool CanReproduce
    {
        get
        {
            if (Size >= MatureSize && CanGrow) return true;
            else return false;
        }
    }

    bool Dead
    {
        get
        {
            if (died) return true;
            if (Age > MatureSize )
            {

                currentAction = "Dead";            
                died = true;
                Energy = Size;  //creature size is converted to energy
                TransformToFood();
                AddReward(.2f);
                EndEpisode();
                return true;
            }
            return false;
        }
    }

    bool Buried
    {
        get
        {
            Energy -= AgeRate;
            return Energy < 0;
        }
    }

    void Grow()
    {
        if (Size > MatureSize) return;
        Energy = Energy / 2;
        Size += GrowthRate * Random.value;
        nextAction = Time.timeSinceLevelLoad + (25 / MaxSpeed);
        currentAction ="Growing";
        TransformSize();
    }

    void Reproduce()
    {
        if (CanReproduce)
        {
            var vec = Random.insideUnitCircle * 5;
            var go = Instantiate(ChildSpawn, new Vector3(vec.x, 0, vec.y), Quaternion.identity, Environment.transform);
            go.name = go.name + (count++).ToString();
            var ca = go.GetComponent<CreatureAgent>();
            Debug.Log(Time.captureDeltaTime + " game object instantiated");
            ca.OnEpisodeBegin();
            Energy = Energy / 2;
            AddReward(.2f);            
            currentAction ="Reproducing";
            nextAction = Time.timeSinceLevelLoad + (25 / MaxSpeed);
        }
    }

    protected abstract void Eat();

    public void MoveAgent(float[] act)
    {
        Vector3 rotateDir = Vector3.zero;
        rotateDir = transform.up * Mathf.Clamp(act[6], -1f, 1f);
        if (act[5] > .5f)
        {
            transform.position = transform.position + transform.forward;
            Energy -= .01f;
        }
        // ROTATE less cost than moving forward
        else{
            Energy -= .005f;

        }
        //Energy += .1f;
        //transform.Rotate(rotateDir, Time.fixedDeltaTime * MaxSpeed);
        transform.Rotate(rotateDir * Time.fixedDeltaTime * 180f);
        currentAction = "Moving";
        // put this here because when testing manually this is annoying
        if(!HeuristicActions)
            nextAction = Time.timeSinceLevelLoad + (25 / MaxSpeed);
    }

    protected Vector2 GetEnvironmentBounds()
    {
        Environment = transform.parent.gameObject;
        var xs = Environment.transform.localScale.x;
        var zs = Environment.transform.localScale.z;
        return new Vector2(xs, zs) * 5;
    }

    public override void Heuristic(float[] heuristicRes)
    {
        // Put the actions into an array and return
        for (int i = 0; i < heuristicRes.Length; i++)
        {
            heuristicRes[i] = 0f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            // turn left
            heuristicRes[0] = 1f;
            heuristicRes[5] = 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            // turn left
            heuristicRes[0] = 1f;
            heuristicRes[6] = -1f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // turn right
            heuristicRes[0] = 1f;
            heuristicRes[6] = 1f;
        }
        // if (Input.GetKey(KeyCode.E))
        // {
        //     heuristicRes[1] = 1f;
        // }
        if (Input.GetKey(KeyCode.R))
        {
            heuristicRes[2] = 1f;
        }
    }

    public void TransformToFood()
    {
        var Food = Instantiate(FoodPrefab, transform.position, Quaternion.identity, transform.parent);
        //var ff= Food.GetComponent<Food>();
        //ff.Energy = Energy;
    }
}


