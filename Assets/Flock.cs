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

    [Range(1, 50)]
    public int startingCount = 20;
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
    [Range(0f, 20f)]
    public float xPen = 10f;

    public Vector3 shepherdPos;

    float squareMaxSpeed;
    float squareNeighborRadius;
    float squareAvoidanceRadius;
    float squareShepherdAvoidanceRadius;
    float squareStationaryRadius;
    float numHerded = 0;


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

        agents = spawnFlock();

        // Pen constraints
        SpriteRenderer spriteRenderer = GameObject.Find("Pen").GetComponent<SpriteRenderer>();
        Vector3 penPos = spriteRenderer.transform.localPosition;
        Vector3 penSize = spriteRenderer.bounds.size;

        entryPenPos = new Vector3(penPos.x - penSize.x / 2, 0, 0);
        topPenPos = new Vector3(penPos.x - penSize.x/2, penSize.y/2, 0);
        bottomPenPos = new Vector3(penPos.x - penSize.x / 2, - penSize.y / 2, 0);
    }

        // Update is called once per frame
        void Update()
    {
        if (MLShepherd.resetNames.Contains(transform.parent.name))
        {
            for (int i = 0; i < agents.Count; i++)
            {
                Destroy(agents[i].gameObject);
            }

            agents.Clear();
            agents = spawnFlock();

            MLShepherd.resetNames.Remove(transform.parent.name);
        }

        // position of shepherd, would be ideal to include this here but need to change input to calculate move
        shepherdPos = GameObject.Find("Shepherd").transform.localPosition;

        for (int i = 0; i < agents.Count; i++)
        {
            if (agents[i].transform.localPosition.x > xPen)
            {
                Destroy(agents[i].gameObject);
                numHerded++;
                agents.Remove(agents[i]);
                //Debug.Log(numHerded);
            }
            else
            {
                if (Vector3.SqrMagnitude(shepherdPos - agents[i].transform.localPosition) < squareStationaryRadius)
                {
                    List<Transform> context = GetNearbyObjects(agents[i]);
                    Vector2 move = behaviour.CalculateMove(agents[i], context, this);
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
        Collider2D[] contextColliders = Physics2D.OverlapCircleAll(agent.transform.localPosition, neighborRadius);
        foreach (Collider2D c in contextColliders)
        {
            if (c != agent.AgentCollider)
            {
                context.Add(c.transform);
            }
        }
        return context;
    }
}
