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
        }
        if(damage > 0)
        {
            vic.Energy -= damage;
            if (vic.Energy <= 0)
            {
                AddReward(.25f);
                // Can change this deppending on other things
                Energy = Mathf.Min(MaxEnergy, Energy + Mathf.Min(damage, 5));
                vic.killed = true;
            }
        }
        Energy -= .1f;
    }

    override protected void Eat()
    {
        // currentAction = "Eating";
        // Energy += 5;
    }

    protected override void Defend()
    {
        Attack();
    }
}