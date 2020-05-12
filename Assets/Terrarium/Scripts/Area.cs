using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Area : MonoBehaviour
{
    public List<GameObject> Plants;
    public List<GameObject> Herbivores;
    public List<GameObject> Carnivores;
    public Plant plantPrefab;

    private static Area InstanceArea;
    public GameObject controlledGO;

    private void Awake() 
    {
        InstanceArea = this;
        Plants = new List<GameObject>();
        Herbivores = new List<GameObject>();
        Carnivores = new List<GameObject>();
        controlledGO = new GameObject();
    }

    // Update is called once per frame
    void Update()
    {
        if(Plants.Count == 0)
        {
            var pos = Random.insideUnitCircle * 3 + new Vector2(transform.position.x, transform.position.z);
            Instantiate(plantPrefab, pos, Quaternion.identity, transform);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            int randomHerbIndex = Random.Range(0, Herbivores.Count);
            int randomCarnIndex = Random.Range(0, Carnivores.Count);
            if (controlledGO.tag == "herbivore")
                controlledGO = Carnivores.Count > 0 ? Carnivores[randomCarnIndex] : Herbivores[randomHerbIndex];
            else if (controlledGO.tag == "carnivore")
                controlledGO = Herbivores.Count > 0 ? Herbivores[randomCarnIndex] : Carnivores[randomHerbIndex];
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


}
