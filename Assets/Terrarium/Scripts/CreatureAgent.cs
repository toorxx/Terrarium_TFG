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
public abstract class CreatureAgent : Agent
{
    public CreatureType CreatureType;
    [Header("Creature Points")]
    public float MaxEnergy;
    public float MatureSize;
    public float GrowthRate;
    public float MaxSpeed;
    public float AttackDamage;
    public float DefendDamage;
    //public float Eyesight;
    public Area area;
    public float MaxLife;

    [Header("Monitoring")]
    public float Energy;
    public float Size;
    public float Age;
    public string currentAction;
    public float Life;
    public bool HeuristicActions;
    public bool killed;

    public bool canDisappear; 
    EnvironmentParameters resetParams;

    
    [Header("Child")]
    public GameObject ChildSpawn;
    public GameObject FoodPrefab;

    [Header("Species Parameters")]    
    public float AgeRate = .001f;

    protected Vector2 bounds;
    private GameObject Environment;
    private Rigidbody agentRB;
    protected float nextAction;
    private int count;
    //public override void AgentReset()
    public override void OnEpisodeBegin()
    {
        //Called on every reset
        Size = 1;
        Energy = MaxEnergy;
        Age = 0;
        Life = MaxLife;
        killed = false;
        //bounds = Area.InstanceArea.GetBounds();
        bounds = GetEnvironmentBounds();
        var x = Random.Range(-bounds.x, bounds.x);
        var z = Random.Range(-bounds.y, bounds.y);
        transform.position = new Vector3(x, 1, z);
        TransformSize();
        if(this.tag.Equals("herbivore"))
            Area.Instance._min_plants = (int)resetParams.GetWithDefault("min_plants", Area.Instance._min_plants);
        if(this.tag.Equals("carnivore"))
            Area.Instance._min_herbivores = (int)resetParams.GetWithDefault("min_herbivores", Area.Instance._min_herbivores);
    }

    public override void Initialize()
    {
        /// Initial setup, called when the agent is enabled
        base.Initialize();
        //rayPer = GetComponent<RayPerception>();
        agentRB = GetComponent<Rigidbody>();
        currentAction = "Idle";
        resetParams = Academy.Instance.EnvironmentParameters;
        Area.Instance.AddGameObject(gameObject);
        //resetParams = Academy.Instance.EnvironmentParameters;
    }       

    void Update()
    {
        if (OutOfBounds)
        {
            AddReward(-1f);
            if(canDisappear)
                DestroyAgent();
            EndEpisode();
            return;
        }
        if (killed)
        {
            AddReward(-1f);
            if(canDisappear)
                DestroyAgent();
            EndEpisode();
        }
        if (Buried)
        {
            AddReward(-1f);
            if (canDisappear)
                DestroyAgent();
            EndEpisode();
        }
        if (Dead) {
            AddReward(1f);
            if(canDisappear)
                DestroyAgent();
            EndEpisode();
        }
        if (CanGrow) Grow();              
        Age += AgeRate;
        MonitorLog();
    }


    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 localVelocity = transform.InverseTransformDirection(agentRB.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);
        sensor.AddObservation(Energy);
        sensor.AddObservation(Size);
        sensor.AddObservation(Age);
        sensor.AddObservation(Float(CanEat));
        sensor.AddObservation(Float(CanReproduce));
    }

    public override void Heuristic(float[] act)
    {
        if(!HeuristicActions) return;
        // Put the actions into an array and return
        for (int i = 0; i < act.Length; i++)
        {
            act[i] = 0f;
        }
        if (Input.GetKey(KeyCode.W))
        {
            // turn left
            act[1] = 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            // turn left
            act[0] = 2f;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // turn right
            act[0] = 1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            //eat
            act[2] = 1f;
        }
        if (Input.GetKey(KeyCode.R))
        {
            //reproduce
            act[2] = 2f;
        }
        if (Input.GetKey(KeyCode.T))
        {
            // attack
            act[2] = 3f;
        }
        if (Input.GetKey(KeyCode.Y))
        {
            // defend
            act[2] = 4f;
        }

    }
    //public override void AgentAction(float[] vectorAction, string textAction)
    public override void OnActionReceived(float[] vectorAction)
    {
        // Action space is now DISCRETE
        // vectorAction[0] = Rotation
        // vectorAction[1] = Forward
        // vectorAction[2] = Action
        // Where Action can be {Eat, Reproduce, Attack, Deffend}
        MoveAgent(vectorAction);
        switch((int)vectorAction[2])
        {
            case 1:
                Eat();
                break;
            case 2:
                Reproduce();
                break;
            case 3:
                Attack();
                break;
            case 4:
                Defend();
                break;
        }
    }

    public void MoveAgent(float[] act)
    {
        Vector3 rotateDir = Vector3.zero;
        float rotationDir = 0f;
        // rotate
        if(act[0] == 1f) 
            rotationDir = 1f;
        else if (act[0] == 2f) 
            rotationDir = -1f;
        else rotationDir = 0f;
        rotateDir = transform.up * rotationDir;
        //transform.Rotate(rotateDir * Time.fixedDeltaTime * 90f);
        // move forward
        // if using physics perform different
        if(!agentRB.isKinematic)
        {
            if (act[1] == 0f)
                agentRB.velocity = new Vector3(0, 0, 0);
            else if (act[1] == 1f)
            //agentRB.AddForce(transform.forward * MaxSpeed, ForceMode.VelocityChange);
            agentRB.velocity = transform.TransformDirection(new Vector3(0, 0, MaxSpeed));
            //transform.Translate(0,0, MaxSpeed * Time.deltaTime, Space.Self);
            //transform.Translate(Vector3.forward * Time.deltaTime * MaxSpeed);
        } 
        else {
            transform.Translate(Vector3.forward * Time.deltaTime * MaxSpeed);
        }
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 90f);
        
        Energy -= .001f;    
            //transform.position = transform.position + transform.forward * MaxSpeed;

        currentAction = "Moving";
    }
       
    protected abstract void Eat();
    protected abstract void Attack();
    protected virtual void Defend()
    {
        currentAction = "Defend";
        Energy -= .01f;
        // nextaction = Time.timeSinceLevelLoad + (25 / MaxSpeed);
    }
    void Reproduce()
    {
        if (CanReproduce)
        {
            var vec = Random.insideUnitCircle * bounds.x;
            var go = Instantiate(ChildSpawn, new Vector3(vec.x, 0, vec.y), Quaternion.identity, Environment.transform);
            go.name = go.name + (count++).ToString();
            var ca = go.GetComponent<CreatureAgent>();
            ca.canDisappear = canDisappear;
            ca.OnEpisodeBegin();
            Energy = Energy / 2;
            AddReward(.25f);            
            currentAction ="Reproducing";
            // nextaction = Time.timeSinceLevelLoad + (25 / MaxSpeed);
        }
    } 

    public bool OutOfBounds
    {
        get
        {
            return !(Area.Instance.InBounds(transform.position.x, transform.position.z));
        }
    }
    
    protected void TransformSize()
    {
        var scale = new Vector3(1 / gameObject.transform.parent.localScale.x,
                                1 / gameObject.transform.parent.localScale.y, 
                                1 / gameObject.transform.parent.localScale.z);
        //Debug.Log(gameObject.transform.parent.localScale.x);
        gameObject.transform.localScale = scale;
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
            if (collider.gameObject.tag == tag && collider.transform != transform)
            {
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
            if (collider.gameObject.tag == tag && obj != null && obj.killed)
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
            if (Age > MatureSize )
            {
                currentAction = "Dead";            
                return true;
            }
            else
            {
                return false;
            }
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
        // nextaction = Time.timeSinceLevelLoad + (25 / MaxSpeed);
        currentAction ="Growing";
    }


    protected Vector2 GetEnvironmentBounds()
    {
        Environment = transform.parent.gameObject;
        var xs = Environment.transform.localScale.x;
        var zs = Environment.transform.localScale.z;
        return new Vector2(xs, zs) * 5;
    }
    public void TransformToFood()
    {
        Instantiate(FoodPrefab, transform.position, Quaternion.identity, transform.parent);
        //var ff= Food.GetComponent<Food>();
        //ff.Energy = Energy;
    }
    public void MonitorLog()
    {
        MLAgents.Monitor.Log("Action", currentAction, transform);
        MLAgents.Monitor.Log("Size", Size / MatureSize, transform);
        MLAgents.Monitor.Log("Energy", Energy / MaxEnergy, transform);
        MLAgents.Monitor.Log("Age", Age / MatureSize, transform);
    }

    private float Float(bool val)
    {
        if (val) return 1.0f;
        else return 0.0f;
    }

    protected abstract bool noAgents();
    protected abstract void DestroyAgent();
}





