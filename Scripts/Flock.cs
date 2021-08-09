using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour
{
    public FlockAgent agentPreFab;
    public static List<FlockAgent> agents = new List<FlockAgent>();
    public FlockBehaviour behaviour;

    public static Vector3 entryPenPos;
    public static Vector3 topPenPos;
    public static Vector3 bottomPenPos;

    public static int startingCount = 1;
    const float AgentDensity = 0.2f;

    [Range(1f, 100f)]
    public float driveFactor = 5f;
    [Range(1f, 100f)]
    public float maxSpeed = 2f;
    [Range(1f, 10f)]
    public float neighborRadius = 1.5f;
    [Range(0f, 1f)]
    public float avoidanceRadiusMultiplier = 0.5f;
    [Range(0f, 10f)]
    public float shepherdAvoidanceRadius;
    [Range(10f, 40f)]
    public float stationaryRadius = 25f;

    public Vector3 shepherdPos;
    public static float numHerded = 0;

    float squareMaxSpeed;
    float squareNeighborRadius;
    float squareAvoidanceRadius;
    float squareShepherdAvoidanceRadius;
    float squareStationaryRadius;

    public LayerMask obstacleMask;
    public LayerMask flockMask;

    const int numViewDirections = 8;


    public float SquareAvoidanceRadius { get { return squareAvoidanceRadius; } }
    public float SquareShepherdAvoidanceRadius { get { return squareShepherdAvoidanceRadius; } }


    // Start is called before the first frame update
    void Start()
    {
        squareMaxSpeed = maxSpeed * maxSpeed;
        squareNeighborRadius = neighborRadius * neighborRadius;
        squareAvoidanceRadius = squareNeighborRadius * avoidanceRadiusMultiplier * avoidanceRadiusMultiplier;
        squareShepherdAvoidanceRadius = shepherdAvoidanceRadius * shepherdAvoidanceRadius;
        squareStationaryRadius = stationaryRadius * stationaryRadius;

        // Uses function below to instatiate the flock
        agents = spawnFlock();

        // Pen constraints
        SpriteRenderer spriteRenderer = GameObject.Find("Pen").GetComponent<SpriteRenderer>();
        Vector3 penPos = spriteRenderer.transform.position;
        Vector3 penSize = spriteRenderer.bounds.size;

        entryPenPos = new Vector3(penPos.x - penSize.x / 2, 0, 0);
        topPenPos = new Vector3(penPos.x - penSize.x/2, penSize.y/2, 0);
        bottomPenPos = new Vector3(penPos.x - penSize.x / 2, - penSize.y / 2, 0);
    }

    // Update is called once per frame
    void Update()
    {
        // Reset Flock when ML episode ends
        if (RLSingle.resetFlock)
        {
            for (int i = 0; i < agents.Count; i++)
            {
                Destroy(agents[i].gameObject);
            }

            agents.Clear();
            agents = spawnFlock();

            RLSingle.resetFlock = false;
        }

        // Position of shepherd
        shepherdPos = GameObject.Find("Shepherd").transform.position;

        for (int i = 0; i < agents.Count; i++)
        {
            // Removes any flock agents that enter the pen
            Vector3 agentpos = agents[i].transform.position;
            Vector3 agentdir = agents[i].transform.up;
            if (agentpos.x > entryPenPos.x && agentpos.y < topPenPos.y && agentpos.y > bottomPenPos.y)
            {
                Destroy(agents[i].gameObject);
                numHerded++;
                agents.Remove(agents[i]);
            }

            // If any flock agent not in pen then standard flock behaviour occurs
            else
            {
                if (Vector3.SqrMagnitude(shepherdPos - agents[i].transform.position) < squareStationaryRadius)
                {
                    List<Transform> context = GetNearbyObjects(agents[i]);
                    Vector2 move = behaviour.CalculateMove(agents[i], context, this);

                    if (IsHeadingForCollision(agentpos, agentdir))
                    {
                        Vector2 collisionAvoidDir = ObstacleRays(agentpos, agentdir);
                        move += 4 * collisionAvoidDir;
                    }

                    move *= driveFactor;
                    if (move.sqrMagnitude > squareMaxSpeed)
                    {
                        move = move.normalized * maxSpeed;
                    }
                    agents[i].Move(move);
                }
            }
        }
    }

    public List<FlockAgent> spawnFlock()
    {
        // Function to instaniate flock, used everytime a training episode ends
        List<FlockAgent> agentsSp = new List<FlockAgent>();
        for (int i = 0; i < startingCount; i++)
        {
            FlockAgent newAgent = Instantiate(
                agentPreFab,
                transform.position + AgentDensity * startingCount * (Vector3)Random.insideUnitCircle,
                Quaternion.Euler(Vector3.forward * Random.Range(0f, 360f)),
                transform
                );
            newAgent.name = "Agent" + i;
            agentsSp.Add(newAgent);
        }
        return agentsSp;
    }

    List<Transform> GetNearbyObjects(FlockAgent agent)
    {
        List<Transform> context = new List<Transform>();
        Collider2D[] contextColliders = Physics2D.OverlapCircleAll(agent.transform.position, neighborRadius, flockMask);
        foreach (Collider2D c in contextColliders)
        {
            if (c != agent.AgentCollider)
            {
                context.Add(c.transform);
            }
        }
        return context;
    }

    bool IsHeadingForCollision(Vector3 pos, Vector3 dir)
    {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(pos, 0.25f, dir, 3f, obstacleMask);
        bool collision = false;

        foreach (RaycastHit2D item in hits)
        {
            collision = true;
        }
        return collision;
    }

    Vector3 ObstacleRays(Vector3 pos, Vector3 dir)
    {
        float angle;
        float angleIncrement = (2 * Mathf.PI / numViewDirections);
        for (int i = 0; i < numViewDirections / 2; i++)
        {
            if (dir.x > 0)
            {
                angle = Mathf.PI / 2 - Mathf.Atan2(dir.y, dir.x);
            }
            else
            {
                angle = 3 * Mathf.PI / 2 - Mathf.Atan2(dir.y, dir.x);
            }

            float angleRight = angle + angleIncrement * i;

            Vector3 dirRight = AngleToUnitCircle(angleRight);

            if (!Physics2D.CircleCast(pos, 0.25f, dirRight, 3f, obstacleMask))
            {
                //return dirRight;
                return AngleToUnitCircle(angleRight);
            }

            float angleLeft = angle - 2 * Mathf.PI - angleIncrement * i;
            Vector3 dirLeft = AngleToUnitCircle(angleLeft);

            if (!Physics2D.CircleCast(pos, 0.25f, dirLeft, 3f, obstacleMask))
            {
                //return dirLeft;
                return AngleToUnitCircle(angleRight);
            }
        }
        return dir;
    }

    Vector3 AngleToUnitCircle(float angle)
    {
        float x = 1f * Mathf.Cos(angle);
        float y = 1f * Mathf.Sin(angle);
        return new Vector3(x, y, 0); ;
    }
}