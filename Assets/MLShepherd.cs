using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MLShepherd : Agent
{
    public static List<string> resetNames = new List<string>();

    float headingPoints = 0f;
    float innerCirclePoints = 0f;
    float numFlock;
    float prevnumFlock = 8;

    public override void OnEpisodeBegin()
    {
        resetNames.Add(transform.parent.name);
        transform.localPosition = new Vector3(-17f, Random.Range(-5f,5f));
    }

    private void Update()
    {
        // Heading right direction (local position)
        // maybe give percentage of reward if kinda close (NOT SURE ABOUT THIS THOUGH)
        Vector3 avgHeading = Vector3.zero;
        Vector3 avgPosition = Vector3.zero;

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


            // Stay close
            float rangeDistance = 5f;
            float toFlockDistance = Vector3.Distance(transform.position, avgPosition);
            if (toFlockDistance > rangeDistance)
            {
                AddReward(-0.01f * Mathf.Pow(rangeDistance - toFlockDistance, 2));
            }

            // Heading right direction
            float topAngle = Vector3.Angle(avgPosition, Flock.topPenPos);
            float bottomAngle = Vector3.Angle(avgPosition, Flock.bottomPenPos);

            float flockAngle = Mathf.Atan2(avgHeading.y, avgHeading.x) * 180 / Mathf.PI;

            if (bottomAngle < flockAngle && flockAngle < topAngle)
            {
                headingPoints += 0.00001f * numFlock;
            }

            // Circle thing
            List<float> flockDistances = new List<float>();

            for (int i = 0; i < numFlock; i++)
            {
                float flockDistance = Vector3.Distance(avgPosition, Flock.agents[i].transform.position);
                flockDistances.Add(flockDistance);
            }

            float maxDistance = flockDistances.Max();
            float maxFlockDistance = 5f;

            innerCirclePoints = 0.00001f * (maxFlockDistance - maxDistance);
            AddReward(headingPoints + innerCirclePoints);

            if (prevnumFlock < numFlock)
            {
                AddReward(0.01f * (prevnumFlock - numFlock));
            }

            prevnumFlock = numFlock;
        }

        if (numFlock == 0)
        {
            Debug.Log("Yay");
            AddReward(10f);
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(Flock.entryPenPos);

        for (int i = 0; i < Flock.agents.Count; i++)
        {
            sensor.AddObservation(Flock.agents[i].transform.position);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveY = actions.ContinuousActions[1];

        float moveSpeed = 8f;
        transform.localPosition += new Vector3(moveX, moveY) * Time.deltaTime * moveSpeed;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Wall>(out Wall wall))
        {
            AddReward(-5f);
            EndEpisode();
        }
    }
}
