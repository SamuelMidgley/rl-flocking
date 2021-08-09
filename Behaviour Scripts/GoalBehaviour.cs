using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behaviour/Goal")]

public class GoalBehaviour : FlockBehaviour
{
    public override Vector2 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock)
    {
        // Weak steering force towards the the pen
        float x = Flock.entryPenPos.x;
        float y = Random.Range(Flock.bottomPenPos.y, Flock.topPenPos.y);

        Vector3 GoalMove = new Vector3(x, y, 0);

        float goalDist = Vector3.Distance(Flock.entryPenPos, agent.transform.position);

        GoalMove *= goalDist;

        return GoalMove;
    }
}
