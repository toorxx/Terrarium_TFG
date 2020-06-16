using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Plant : MonoBehaviour {
    [Header("Plant Points (30 Max)")]    
    public float MaxEnergy;
    public float MatureSize;
    public float GrowthRate;
    public float SeedSpreadRadius;
    public Area area;
    
    [Header("Monitoring")]
    public float Energy;
    public float Size;    
    public float Age;
    
    [Header("Seedling")]
    public GameObject SeedlingSpawn;
  
    [Header("Species Parameters")]
    public float EnergyGrowthRate = .01f;
    public float AgeRate = .001f;

    private Transform Environment;

    private void Start()
    {
        Size = 1;
        Energy = MaxEnergy;
        Age = 0;
        Environment = transform.parent;
        TransformSize();
        StartCoroutine(WaitToDie(120));
        Area.Instance.Plants.Add(gameObject);
    }
    
    void Update ()
    {        
        // if (CanGrow) Grow();
        // //Grow();
        // //if (Dead) Destroy(this);
        // if (CanReproduce) Reproduce();
        if(Dead) {
            Debug.Log("Plant Destroyed" + gameObject);
            Die();
        }
        // Age += AgeRate;
        // Energy += EnergyGrowthRate;               
    }

    void TransformSize()
    {
        var scale = new Vector3(1 / gameObject.transform.parent.localScale.x,
                                1 / gameObject.transform.parent.localScale.y, 
                                1 / gameObject.transform.parent.localScale.z);
        transform.localScale = scale * Size;
    }

    bool CanGrow
    {
        get
        {
            return Energy > ((MaxEnergy / 2) + 1);
        }
    }

    bool CanReproduce
    {
        get
        {
            if (Size >= MatureSize && CanGrow) return true;
            if (Size >= MatureSize) return true;
            else return false;
        }
    }    

    bool Dead
    {
        get
        {
            return Energy < 0 || Age > MatureSize;
        }
    }

    void Grow()
    {
        if (Size > MatureSize) return;
        Energy = Energy / 2;
        Size += GrowthRate * Random.value;
        TransformSize();
    }

    public void Reproduce()
    {
        var vec = Random.insideUnitCircle * SeedSpreadRadius 
            + new Vector2(transform.position.x, transform.position.z);
        if(Area.Instance.InBounds(vec.x, vec.y)){
            Instantiate(SeedlingSpawn, new Vector3(vec.x,0,vec.y), Quaternion.identity, Environment);
            Energy = Energy / 2;
        }
    }

    public void Die()
    {
        Area.Instance.Plants.Remove(gameObject);
        Destroy(gameObject);
    }

    IEnumerator WaitToDie(int time)
    {
        //yield on a new YieldInstruction that waits for 5 seconds.
        yield return new WaitForSecondsRealtime(time);
        Die();
    }


}

