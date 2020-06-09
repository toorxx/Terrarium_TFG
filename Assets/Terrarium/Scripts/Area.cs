using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    public int _initial_plants;
    public int _initial_herbivores;
    public int _initial_carnivores;
    public int _min_plants;
    public List<GameObject> Plants;
    public List<GameObject> Herbivores;
    public List<GameObject> Carnivores;
    public Plant plantPrefab;
    public Herbivore herbivorePrefab;
    public Carnivore carnivorePrefab;
    public Food FoodPrefab;

    private static Area InstanceArea;
    public GameObject controlledGO;

    private void Awake() 
    {
        InstanceArea = this;
        Plants = new List<GameObject>();
        Herbivores = new List<GameObject>();
        Carnivores = new List<GameObject>();
        controlledGO = new GameObject();
        for(int i = 0; i < _initial_plants; i++)
        {
            Instantiate(plantPrefab, GetRandomPos(), Quaternion.identity, transform);
        }
        for(int i = 0; i < _initial_herbivores; i++)
        {
            Instantiate(herbivorePrefab, GetRandomPos(), Quaternion.identity, transform);
        }
        for(int i = 0; i < _initial_carnivores; i++)
        {
            Instantiate(carnivorePrefab, GetRandomPos(), Quaternion.identity, transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        // add plants randomly at x steps of time
        if (Time.frameCount % 1 == 0){
            if(Plants.Count < _min_plants)
            {
                Instantiate(plantPrefab, GetRandomPos(), Quaternion.identity, transform);
            }
        }
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

    public void InstantiateFood(Vector3 pos)
    {
        Instantiate(FoodPrefab, pos, Quaternion.identity, transform);
    }

    public Vector2 GetRandomPos()
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


}
