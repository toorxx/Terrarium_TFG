using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    public int _initial_plants;
    public int _initial_herbivores;
    public int _initial_carnivores;
    public int _min_plants;
    public int _min_herbivores;
    public int _time_before_herb;
    public int _time_before_carn;
    public int _time_before_agents_deactivation;
    private bool agentsDeactivated;
    public List<GameObject> Plants;
    public List<GameObject> Herbivores;
    public List<GameObject> Carnivores;
    public Plant plantPrefab;
    public Herbivore herbivorePrefab;
    public Carnivore carnivorePrefab;

    private static Area InstanceArea;
    public GameObject controlledGO;

    private void Awake() 
    {
        InstanceArea = this;
        Plants = new List<GameObject>();
        Herbivores = new List<GameObject>();
        Carnivores = new List<GameObject>();
        controlledGO = new GameObject();
        agentsDeactivated = false;
        for(int i = 0; i < _initial_plants; i++)
        {
            Instantiate(plantPrefab, GetRandomPos(), Quaternion.identity, transform);
        }
        StartCoroutine(WaitToInstantiate(_time_before_herb, "herbivore"));
        StartCoroutine(WaitToInstantiate(_time_before_carn, "carnivore"));
        StartCoroutine(SetDeactivation());
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Time.time);
        // add plants randomly at x steps of time
        if (Time.frameCount % 1 == 0)
        {
            if(Plants.Count < _min_plants)
                Instantiate(plantPrefab, GetRandomPos(), Quaternion.identity, transform);
        }
        if(Herbivores.Count < _min_herbivores && _min_herbivores > 0)
            Instantiate(herbivorePrefab, GetRandomPos(), Quaternion.identity, transform);
        else if(Herbivores.Count > _min_herbivores && _min_herbivores > 0)
            Herbivores.RemoveRange(0, Herbivores.Count - _min_herbivores);
    }

    public static Area Instance { get { return InstanceArea; }}
    // bear in mind this is a workaround that works because its a rectangular plane
    public Vector2 GetBounds()
    {
        var x = transform.localScale.x * GetComponent<MeshFilter>().mesh.bounds.extents.x;
        var z = transform.localScale.x * GetComponent<MeshFilter>().mesh.bounds.extents.z;
        return(new Vector2(x, z));
    }

    public bool InBounds(float x, float z)
    {
        var localX = transform.localScale.x * GetComponent<MeshFilter>().mesh.bounds.extents.x;
        var localZ = transform.localScale.z * GetComponent<MeshFilter>().mesh.bounds.extents.x;
        return(x < localX && x > -localX && z < localZ && z > -localZ);
    }

    public void AddGameObject(GameObject go)
    {
        if (go.tag == "herbivore")
            Herbivores.Add(go);
        else if (go.tag == "carnivore")
            Carnivores.Add(go);
    }

    public Vector3 GetRandomPos()
    {
        var bounds = GetBounds();
        var x = Random.Range(-bounds.x, bounds.x);
        var z = Random.Range(-bounds.y, bounds.y);
        var rand2d = Random.insideUnitCircle * bounds.x;
        return( new Vector3(rand2d.x, 0, rand2d.y) );
    }

    public void MonitorLog()
    {
        MLAgents.Monitor.Log("Num Plants", Plants.Count, transform);
        MLAgents.Monitor.Log("Num herbivores", Herbivores.Count, transform);
        MLAgents.Monitor.Log("Num carnivores", Carnivores.Count, transform);
    }

    // set the agents to deactivate. We use this in order to not instantiate agents when episode is finished
    IEnumerator SetDeactivation()
    {
        yield return new WaitForSecondsRealtime(_time_before_agents_deactivation);
        foreach(var herbivore in Herbivores){
            herbivore.GetComponent<Herbivore>().canDisappear = true;
            Debug.Log(herbivore.GetComponent<Herbivore>().canDisappear);
        }
        Debug.Log(Herbivores.Count);

        foreach(var carnivore in Carnivores)
            carnivore.GetComponent<Carnivore>().canDisappear = true;
    }

    private void setAgentsList(string kind)
    {
        if(kind == "herbivore")
        {
            for (int i = 0; i < _initial_herbivores; i++)
                 Instantiate(herbivorePrefab, GetRandomPos(), Quaternion.identity, transform);
        }
        else if(kind == "carnivore")
        {
            for (int i = 0; i < _initial_carnivores; i++)
                Instantiate(carnivorePrefab, GetRandomPos(), Quaternion.identity, transform);
        }
    }

    IEnumerator WaitToInstantiate(int time, string kind)
    {
        // wait for the numbers of seconds specified by the user before instantiate agernts
        
        //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);

        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSecondsRealtime(time);
        setAgentsList(kind);
    }


}
