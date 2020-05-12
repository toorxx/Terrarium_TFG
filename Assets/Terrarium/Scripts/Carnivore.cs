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
            if (FirstAdjacentDead("herbivore") != null) return true;
            else return false;
        }
    }
    protected override void Attack()
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

    override protected void Eat()
    {
        // decide what to do here

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
        if (Input.GetKey(KeyCode.R))
        {
            heuristicRes[2] = 1f;
        }
    }
}