using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class RLSingle : Agent
{
    public static bool resetFlock;

    float totalGeneral;
    float totalDirectional;

    float numFlock;

    Vector3 avgHeading;
    Vector3 avgPosition;

    public override void OnEpisodeBegin()
    {
        totalGeneral = 0;
        totalDirectional = 0;

        resetFlock = true;

        // This determines what happens when the episode resets or initially starts
        transform.position = new Vector3(-13f, Random.Range(-10f, 10f)); // Randomly spawns the MLShepherd along the -10x point
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Determines what observations the MLShepherd has 
        sensor.AddObservation(transform.position); // Position of MLShepherd
        sensor.AddObservation(Flock.entryPenPos); // Middle of Pen
        sensor.AddObservation(avgPosition); // Position of Flock Agent
        //sensor.AddObservation(avgHeading); // Direction of Flock Agent
    }

    private void Update()
    {
        // All reward variables
        float generalPoints = 0f;
        float headingPoints = 0f;

        numFlock = Flock.agents.Count;

        avgHeading = Vector3.zero;
        avgPosition = Vector3.zero;

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
            //Debug.Log("shep: " + transform.position);
            //Debug.Log("sheep: " + avgPosition);
            if (3f < toFlockDistance && toFlockDistance < 10f)
            {
                if (totalGeneral < 2f)
                {
                    generalPoints += 0.05f;

                }
            }

            // DIRECTIONAL REWARD
            // The following code works out if the flock is heading towards the pen
            // Calculates the angles from the average position of the flock to the top and bottom of pen
            //float topAngle = Vector3.Angle(avgPosition, Flock.topPenPos);
            //Debug.Log("top:" + topAngle);
            //float bottomAngle = Vector3.Angle(avgPosition, Flock.bottomPenPos);
            //Debug.Log("bottom:" + bottomAngle);

            float topAngle = Mathf.Atan2((Flock.topPenPos.y - avgPosition.y), (Flock.topPenPos.x - avgPosition.x)) * 180 / Mathf.PI;
            float bottomAngle = Mathf.Atan2((Flock.bottomPenPos.y - avgPosition.y), (Flock.bottomPenPos.x - avgPosition.x)) * 180 / Mathf.PI;


            // Calculates the angle of the average heading of the flock
            float flockAngle = Mathf.Atan2(avgHeading.y, avgHeading.x) * 180 / Mathf.PI;

            // If the angle of the average heading of the flock is between the top and bottom of the pen then a reward is added
            if (bottomAngle < flockAngle && flockAngle < topAngle)
            {
                if (totalDirectional < 5f)
                {
                    headingPoints = 0.015f;
                }
            }


            // TOTAL REWARD FOR UPDATE
            // Collecting all rewards and adding them to the total reward
            totalGeneral += generalPoints;
            totalDirectional += headingPoints;
            AddReward(generalPoints + headingPoints);
        }

        if (numFlock == 0) // All flock agents have been herded
        {
            Debug.Log("Total Rewards");
            Debug.Log("General: " + totalGeneral);
            Debug.Log("Directional: " + totalDirectional);

            //float herdingTime = Time.time - startTime;
            float herdingTime = 10; // 10 * (1 - step / 6500);
            if (totalGeneral+totalDirectional > 2f)
            {
                AddReward(herdingTime);
            }
            else
            {
                AddReward(5f);
            }

            Debug.Log("Final Reward: " + herdingTime);

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
        transform.position += new Vector3(moveX, moveY) * Time.deltaTime * moveSpeed;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Allows manual control of the agent
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }
}

