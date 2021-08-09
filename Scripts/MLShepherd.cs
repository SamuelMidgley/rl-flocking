using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MLShepherd : Agent
{
    float totalGeneral;
    float totalDirectional;
    float totalCollectional;

    float numFlock;
    float step = 0;

    Vector3 avgHeading;
    Vector3 avgPosition;

    public override void OnEpisodeBegin()
    {
        // This determines what happens when the episode resets or initially starts
        transform.localPosition = new Vector3(-17f, Random.Range(-5f,5f)); // Randomly spawns the MLShepherd along the -17x point
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Determines what observations the MLShepherd has 
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(Flock.topPenPos);
        sensor.AddObservation(Flock.bottomPenPos);

        sensor.AddObservation(avgHeading);
        sensor.AddObservation(avgPosition);

        for (int i = 0; i < Flock.agents.Count; i++)
        {
            // Adds each flock agents position and velocity to the observations
            sensor.AddObservation(Flock.agents[i].transform.position);
            //sensor.AddObservation(Flock.agents[i].transform.up);
        }
    }

    //void OnDrawGizmosSelected()
    //{
    //    // Draw a yellow sphere at the transform's position
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(transform.position, 3f);
    //}

    private void Update()
    {
        // All reward variables
        float generalPoints = 0f;
        float headingPoints = 0f;
        float innerCirclePoints = 0f;

        step++; // Step variable used to determine how long the simulation is running for, this is then used for speed related rewards

        // Find the average Position and Heading of the flock
        avgHeading = Vector3.zero;
        avgPosition = Vector3.zero;

        numFlock = Flock.agents.Count;

        if (numFlock > 0)
        {
            for (int i = 0; i < numFlock; i++)
            {
                avgHeading += Flock.agents[i].transform.up;
                avgPosition += Flock.agents[i].transform.position;
            }

            avgHeading /= numFlock;
            avgPosition /= numFlock;


            // GENERAL HERDING REWARD

            // The following block of code ensures that the shepherd stays the correct distance from the flock of sheep
            // Does not go further than 10 units
            // Does not go closer than 2 units
            float toFlockDistance = Vector3.Distance(transform.position, avgPosition);
            if (3f < toFlockDistance && toFlockDistance < 10f)
            {
                generalPoints += 0.0005f;

                // DIRECTIONAL REWARD

                // The following code works out if the flock is heading towards the pen
                // Calculates the angles from the average position of the flock to the top and bottom of pen
                float topAngle = Vector3.Angle(avgPosition, Flock.topPenPos);
                float bottomAngle = Vector3.Angle(avgPosition, Flock.bottomPenPos);

                // Calculates the angle of the average heading of the flock
                float flockAngle = Mathf.Atan2(avgHeading.y, avgHeading.x) * 180 / Mathf.PI;

                // If the angle of the average heading of the flock is between the top and bottom of the pen then a reward is added
                if (bottomAngle < flockAngle && flockAngle < topAngle)
                {
                    headingPoints = 0.001f * numFlock;
                }


                // COLLECTION REWARD

                // This determines how well the shepherd is keeping the sheep together
                // Creates a list of the distances from all the flock agents to the average position of the flock
                List<float> flockDistances = new List<float>();

                for (int i = 0; i < numFlock; i++)
                {
                    float flockDistance = Vector3.Distance(avgPosition, Flock.agents[i].transform.position);
                    flockDistances.Add(flockDistance);
                }

                // Finds the flock agent that is furthest from the average position of the flock
                float maxDistance = flockDistances.Max();
                float maxFlockDistance = 10f;

                if (maxDistance < maxFlockDistance)
                {
                    innerCirclePoints = 0.003f;
                }

                // TOTAL REWARD FOR UPDATE
                // Collecting all rewards and adding them to the total reward
                totalGeneral += generalPoints;
                totalDirectional += headingPoints;
                totalCollectional += innerCirclePoints;
                AddReward(generalPoints + headingPoints + innerCirclePoints);
            }
            
            // Points for herding individual flock agents, reward higher if more than one flock agent herded in an update
            if (Flock.numHerded > 0)
            {
                //generalPoints += 1;
                Flock.numHerded--;
            }
        }

        if (numFlock == 0) // All flock agents have been herded
        {
            Debug.Log("Total Rewards");
            Debug.Log("General: " + totalGeneral);
            Debug.Log("Directional: " + totalDirectional);
            Debug.Log("Collection: " + totalCollectional);
            AddReward(10f);


            if (step < 10000) // Have been herded within the time limit
            {
                Debug.Log("In time");
                if (step < 5000)
                {
                    // If herded very quickly given large reward
                    AddReward(20f);
                }
                else
                {
                    // If herded moderately quickly reward given dependent on speed
                    AddReward(1 - (step - 5000) / 10000);
                }
            }

            EndEpisode();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Determines whether the MLShepherd collides with the walls
        if (collision.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-5f); // Large negative reward
            EndEpisode();
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        float moveSpeed = 12f;
        transform.localPosition += new Vector3(moveX, moveY) * Time.deltaTime * moveSpeed;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Allows manual control of the agent
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }
}
