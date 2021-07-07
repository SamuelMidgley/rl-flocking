using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behaviour/Separation")]
public class SeparationBehaviour : FlockBehaviour
{
    public override Vector2 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock)
    {
        // if no neighbours, return no adjustment
        if (context.Count == 0)
            return Vector2.zero;

        // add all points together and average
        Vector2 separationMove = Vector2.zero;
        int nAvoid = 0;
        foreach (Transform item in context)
        {
            if (Vector2.SqrMagnitude(item.localPosition - agent.transform.localPosition) < flock.SquareAvoidanceRadius)
            {
                nAvoid++;
                separationMove += (Vector2)(agent.transform.localPosition - item.localPosition);
            } 
        }
        if (nAvoid > 0)
            separationMove /= nAvoid;

        return separationMove;
    }
}
