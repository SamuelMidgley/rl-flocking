using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shepherd : MonoBehaviour
{
    public float maxSpeed = 0.1f;


    // Update is called once per frame
    void Update()
    {
        float xdirection = Input.GetAxis("Horizontal");
        float ydirection = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(xdirection, ydirection, 0f);

        transform.position += moveDirection * maxSpeed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<Wall>(out Wall wall))
        {
            Debug.Log("Wall");
        }
        if (collision.TryGetComponent<Goal>(out Goal goal))
        {
            Debug.Log("Goal");
        }
    }
}
