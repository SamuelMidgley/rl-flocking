using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behaviour/Avoidance")]
public class ShepherdAvoidanceBehaviour : FlockBehaviour
{
    public override Vector2 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock)
    {
        //Vector3 shepherdPos = GameObject.Find("Shepherd").transform.position;
        Vector2 avoidanceMove = Vector2.zero;

        if (Vector3.SqrMagnitude(flock.shepherdPos - agent.transform.localPosition) < flock.SquareShepherdAvoidanceRadius)
        {
            avoidanceMove += (Vector2)(agent.transform.localPosition - flock.shepherdPos);
            return avoidanceMove;
        }
        else return Vector2.zero;
    }
}
