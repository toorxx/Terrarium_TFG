using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
public class Carnivore : CreatureAgent
{
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
    protected override bool CanEat
    {
        get
        {
            if (FirstAdjacent("food") != null) return true;
            else return false;
        }
    }
    protected override void Attack()
    {
        float damage = 0f;
        currentAction = "Attack";
        // put this here because when testing manually this is annoying
        if(!HeuristicActions)
            nextAction = Time.timeSinceLevelLoad + (25 / MaxSpeed);
        var _vic = FirstAdjacent("herbivore");
        CreatureAgent vic = null;
        //Debug.Log(_vic);
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
            vic.Life -= damage;
            if (vic.Life < 0)
            {
                AddReward(.25f);
            }
        }
        else if(damage < 0){
            //TODO: Canviar aixo per life??
            Energy -= damage;
        }
        Energy -= .1f;
    }

    override protected void Eat()
    {
        Debug.Log(CanEat);
        Debug.Log(FirstAdjacent("food"));
        if (CanEat)
        {
            var adj = FirstAdjacent("food");
            Debug.Log(adj);
            if (adj != null)
            {
                var creature = adj.GetComponent<Food>();
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

    public override void Heuristic(float[] heuristicRes)
    {
        // Put the actions into an array and return
        for (int i = 0; i < heuristicRes.Length; i++)
        {
            heuristicRes[i] = 0f;
        }
        if (Input.GetKey(KeyCode.I))
        {
            // turn left
            heuristicRes[0] = 1f;
            heuristicRes[5] = 1f;
        }
        if (Input.GetKey(KeyCode.J))
        {
            // turn left
            heuristicRes[0] = 1f;
            heuristicRes[6] = -1f;
        }
        else if (Input.GetKey(KeyCode.L))
        {
            // turn right
            heuristicRes[0] = 1f;
            heuristicRes[6] = 1f;
        }
        if (Input.GetKey(KeyCode.E))
        {
            heuristicRes[1] = 1f;
        }
        if (Input.GetKey(KeyCode.O))
        {
            heuristicRes[3] = 1f;
        }
    }
}