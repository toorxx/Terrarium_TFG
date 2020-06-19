using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
public class Carnivore : CreatureAgent
{
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
            if(damage > 0)
            {
                vic.Energy -= damage;
                if (vic.Energy <= 0)
                {
                    AddReward(.25f);
                    Energy = Mathf.Min(MaxEnergy, Energy + Mathf.Min(damage, 5));
                    // Can change this deppending on other things
                    vic.killed = true;
                }
            } 
            else 
                Energy += damage;
        } 
        // if attack with no reason subtract energy
        else Energy -= 0.01f;
    }

    override protected void Eat()
    {
        Attack();
    }

    protected override void Defend()
    {
        Attack();
    }

    protected override bool noAgents()
    {
        if(Area.Instance.Carnivores.Count > 0)
            return false;
        else
            return true;
    }

    protected override void DestroyAgent()
    {
        Area.Instance.Carnivores.Remove(gameObject);
        Destroy(gameObject);
    }
}