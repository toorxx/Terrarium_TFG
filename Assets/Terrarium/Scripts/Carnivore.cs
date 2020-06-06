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
        Energy = MaxEnergy;
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
        var _vic = FirstAdjacent("herbivore");
        CreatureAgent vic = null;
        //only attack to hervibores
        if (_vic != null)
        {   
            vic = _vic.GetComponent<CreatureAgent>();
            if (vic.currentAction == "Defend")
            {
                //damage = ((AttackDamage * Size) - (vic.DefendDamage * vic.Size)) / (Size * vic.Size);
                //damage = vic.Life / 2;
                damage = AttackDamage - vic.DefendDamage;
            }
            else
            {
                //damage = ((AttackDamage * Size) - (1 * vic.Size)) / (Size * vic.Size);
                //damage = vic.Life;
                damage = AttackDamage;
            }
            Debug.Log(damage);
            Debug.Log(_vic);
        }
        if(damage > 0)
        {
            vic.Energy -= damage;
            if (vic.Energy <= 0)
            {
                AddReward(.25f);
                vic.killed = true;
            }
        }
        // else if(damage < 0){
        //     //TODO: Canviar aixo per life??
        //     Energy -= damage;
        // }
        Energy -= .1f;
    }

    override protected void Eat()
    {
        if (CanEat)
        {
            var adj = FirstAdjacent("food");
            if (adj != null)
            {
                var creature = adj.GetComponent<Food>();
                //var consume = Mathf.Min(creature.Energy, 5);
                // creature.Energy -= consume;
                // if (creature.Energy < .1)
                // {
                //     creature.Die();
                // }
                currentAction = "Eating";
                creature.Die();
                Energy += 5;
                AddReward(.25f);
            }
        }
    }

    protected override void Defend()
    {
        Attack();
    }
}