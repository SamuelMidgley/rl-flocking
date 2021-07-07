using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MLShepherd : Agent
{
    [SerializeField] private Transform targetTransform;
    public static List<string> resetNames = new List<string>();

    public override void OnEpisodeBegin()
    {
        resetNames.Add(transform.parent.name);
        transform.localPosition = new Vector3(-9f, 0f);
    }

    private void Update()
    {
        // Heading right direction
        // USE LOCAL POSITION

        // collect all agents headings and average
        //Vector3 avgHeading = Vector3.zero;

        //for (int i = 0; i < Flock.agents.Count; i++)
        //{
        //    avgHeading += Flock.agents[i].transform.up;
        //}

        //avgHeading /= Flock.agents.Count;

        // check if direction would land in the pen
        // use avg position and heading 
        // angle between avg pos at top and bottom of pen 
        // angle of heading, if angle of heading inbetween these give reward 
        // maybe give percentage of reward if kinda close (NOT SURE ABOUT THIS THOUGH)

        // SerializeField and add the size of the pen so it changes depending on the size of the gameobject


        // Circle thing
        // find centre point of flock
        // loop through distances from centre point to flock objects
        // function that rewards a smaller radius

        // Small reward for herdining one sheep but as percentage
        // End episode if no sheep around
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
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
        if (collision.TryGetComponent<Goal>(out Goal goal))
        {
            SetReward(+1f);
            EndEpisode();
        }

        if (collision.TryGetComponent<Wall>(out Wall wall))
        {
            SetReward(-1f);
            EndEpisode();
        }
    }
}
