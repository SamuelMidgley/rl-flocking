using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 [CreateAssetMenu(menuName = "Flock/Behaviour/Cohesion")]
public class CohesionBehaviour : FlockBehaviour
{
    public override Vector2 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock)
    {
        // if no neighbours, return no adjustment
        if (context.Count == 0)
            return Vector2.zero;

        // add all points together and average
        Vector2 cohesionMove = Vector2.zero;
        foreach (Transform item in context)
        {
            cohesionMove += (Vector2)item.localPosition;
        }
        cohesionMove /= context.Count;

        // create offset from agent position
        cohesionMove -= (Vector2)agent.transform.localPosition;
        return cohesionMove;
    }




}
