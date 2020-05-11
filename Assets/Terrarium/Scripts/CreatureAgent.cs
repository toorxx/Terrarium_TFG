﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;


public enum CreatureType
{
    Herbivore,
    Carnivore
}
public class CreatureAgent : Agent
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
    public Area area;

    [Header("Monitoring")]
    public float Energy;
    public float Size;
    public float Age;
    public string currentAction;
    
    [Header("Child")]
    public GameObject ChildSpawn;

    [Header("Species Parameters")]    
    public float AgeRate = .001f;

    private Vector2 bounds;
    private GameObject Environment;
    private Rigidbody agentRB;
    private float nextAction;
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
        bounds = Area.InstanceArea.GetBounds();
        var x = Random.Range(-bounds.x, bounds.x);
        var z = Random.Range(-bounds.y, bounds.y);
        transform.position = new Vector3(x, 1, z);
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
        //area.AddObjectToList(gameObject);
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

    void Defend()
    {
        currentAction = "Defend";
        nextAction = Time.timeSinceLevelLoad + (25 / MaxSpeed);
    }

    void Attack()
    {
        float damage = 0f;
        currentAction = "Attack";
        nextAction = Time.timeSinceLevelLoad + (25 / MaxSpeed);
        var _vic = FirstAdjacent("herbivore");
        CreatureAgent vic = null;
        if (_vic != null)
        {   
            vic = _vic.GetComponent<CreatureAgent>();
            if (vic.currentAction == "Defend")
            {
                damage = ((AttackDamage * Size) - (vic.DefendDamage * vic.Size)) / (Size * vic.Size);
            }
            else
            {
                damage = ((AttackDamage * Size) - (1 * vic.Size)) / (Size * vic.Size);
            }
        }
        else
        {
            _vic = FirstAdjacent("carnivore");
            if (_vic != null)
            {
                vic = _vic.GetComponent<CreatureAgent>();
                if (vic.currentAction == "Attack")
                {
                    damage = ((AttackDamage * Size) - (vic.AttackDamage * vic.Size)) / (Size * vic.Size);
                }
                else
                {
                    damage = ((AttackDamage * Size) - (vic.DefendDamage * vic.Size)) / (Size * vic.Size);
                }
            }
        }

        if(damage > 0)
        {
            vic.Energy -= damage;
            if (vic.Energy < 0)
            {
                AddReward(.25f);
            }
        }
        else if(damage < 0){
            Energy -= damage;
        }
        Energy -= .1f;
    }
        
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
            return !(Area.InstanceArea.InBounds(transform.position.x, transform.position.z));
        }
    }
    
    void TransformSize()
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

    bool CanEat
    {
        get
        {
            if(CreatureType == CreatureType.Herbivore)
            {
                if (FirstAdjacent("plant") != null) return true;
            }
            if(CreatureType == CreatureType.Carnivore)
            {
                if (FirstAdjacentDead("herbivore") != null) return true;
            }
            return false;
        }
    }

    private GameObject FirstAdjacent(string tag)
    {
        var colliders = Physics.OverlapSphere(transform.position, 1.2f);
        foreach (var collider in colliders)
        {
            if (collider.gameObject.tag == tag && collider.transform != transform)
            {
                //Debug.Log(collider.gameObject.tag);
                return collider.gameObject;
            }
        }
        return null;
    }

    private GameObject FirstAdjacentDead(string tag)
    {
        var colliders = Physics.OverlapSphere(transform.position, 1.2f);
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

    public void Eat()
    {
        
        if (CanEat)
        {
            if (CreatureType == CreatureType.Herbivore)
            {
                var adj = FirstAdjacent("plant");
                Debug.Log(adj);
                if (adj != null)
                {
                    var creature = adj.GetComponent<Plant>();
                    var consume = Mathf.Min(creature.Energy, 5);
                    creature.Energy -= consume;
                    if (creature.Energy < .1)
                    {
                        creature.Die();
                    }
                    Energy += consume;
                    AddReward(.1f);
                    nextAction = Time.timeSinceLevelLoad + (25 / EatingSpeed);
                    currentAction = "Eating";
                    Debug.Log(currentAction);

                }
            }
        }
    }

    public void MoveAgent(float[] act)
    {
        Vector3 rotateDir = Vector3.zero;
        rotateDir = transform.up * Mathf.Clamp(act[6], -1f, 1f);
        if (act[5] > .5f)
        {
            transform.position = transform.position + transform.forward;
        }
        Energy -= .01f;
        //Energy += .1f;
        //transform.Rotate(rotateDir, Time.fixedDeltaTime * MaxSpeed);
        transform.Rotate(rotateDir * Time.fixedDeltaTime * 180f);
        currentAction = "Moving";
        //nextAction = Time.timeSinceLevelLoad + (25 / MaxSpeed);
    }

    private Vector2 GetEnvironmentBounds()
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
        if (Input.GetKey(KeyCode.E))
        {
            heuristicRes[1] = 1f;
        }
        if (Input.GetKey(KeyCode.R))
        {
            heuristicRes[2] = 1f;
        }
    }

    
}
