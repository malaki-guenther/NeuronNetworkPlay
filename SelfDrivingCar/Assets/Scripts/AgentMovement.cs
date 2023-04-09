using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector2 input = Vector2.zero;
    [SerializeField] Rigidbody rb;
    public float turnSpeed = 20;
    public float accellerationSpeed = 10;
    [SerializeField] float speedLimit = 60;
    public float currentSpeed = 0;
    void Start()
    {
       
        
        rb.useGravity = false;
        rb.velocity = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        //turning
        if (input.x > 0)
        {
            transform.Rotate(new Vector3(0, turnSpeed * Time.deltaTime, 0));
        }
        else if (input.x < 0)
        {
            transform.Rotate(new Vector3(0, -turnSpeed * Time.deltaTime, 0));
        }
        //speeding up and slowing down
        Vector3 vel = rb.velocity;
        
        if (input.y > 0 && currentSpeed+(accellerationSpeed * Time.deltaTime) < speedLimit)
        {
            currentSpeed += accellerationSpeed*Time.deltaTime;
            
        }
        else if (input.y < 0 && currentSpeed-(accellerationSpeed * Time.deltaTime) > -speedLimit)
        {
            currentSpeed -= accellerationSpeed * Time.deltaTime;
            
        }
        vel = transform.forward * currentSpeed;
        rb.velocity = vel;
    }
    public void StopAgent()
    {
        rb.velocity = Vector3.zero;
        currentSpeed = 0;
        input = Vector2.right;
    }
}
