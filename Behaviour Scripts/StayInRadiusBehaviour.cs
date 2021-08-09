using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Flock/Behaviour/Stay In Radius")]
public class StayInRadiusBehaviour : FlockBehaviour
{
    public Vector2 centre;
    public float radius = 15f;

    const int numViewDirections = 100;
    public LayerMask obstacleMask;

    public override Vector2 CalculateMove(FlockAgent agent, List<Transform> context, Flock flock)
    {
        //Vector2 centreOffset = centre - (Vector2)agent.transform.localPosition;
        //float t = centreOffset.magnitude / radius;
        //if (t < 0.9)
        //{
        //    return Vector2.zero;
        //}

        //return centreOffset * t * t;
        if (IsHeadingForCollision(agent.transform.position, agent.transform.up))
        {
            return ObstacleRays(agent.transform.position, agent.transform.up);
        }
        else
        {
            return agent.transform.up;
        }
    }

    bool IsHeadingForCollision(Vector3 pos, Vector3 dir)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(pos, 0.25f, dir, 1f, obstacleMask);
        bool collision = false;

        foreach (RaycastHit2D item in hits)
        {
            collision = true;
        }
        return collision;
    }

    Vector3 ObstacleRays(Vector3 pos, Vector3 dir)
    {
        // Get list of possible directions boid can travel
        Vector3[] rayDirections = new Vector3[numViewDirections];

        for (int i = 0; i < numViewDirections; i++)
        {
            float angle = (2 * Mathf.PI / numViewDirections) * i;
            float x = 1f * Mathf.Cos(angle);
            float y = 1f * Mathf.Sin(angle);

            rayDirections[i] = new Vector3(x, y, 0);
        }

        for (int i = 0; i < rayDirections.Length; i++)
        {
            if (!Physics2D.CircleCast(pos, 0.25f, rayDirections[i], 1f, obstacleMask))
            {
                return rayDirections[i];
            }
        }

        return dir;
    }
}