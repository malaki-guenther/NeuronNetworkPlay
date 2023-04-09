using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    AgentMovement movement;
    [SerializeField] float deathScore = -1;
    [SerializeField] float checkpointScore = 1;
    int lookingForCheckpointIndex = 0;
    public Collider[] checkPointTriggers;
    Vector3 startPot;
    Vector3 startRot;
    float score = 0;
    public bool playerDead = false;
    // Start is called before the first frame update
    void Start()
    {
        movement = GetComponent<AgentMovement>();
        startPot = transform.position;
        startRot = transform.rotation.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playerDead)
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            movement.input = input;
            score += Time.deltaTime;
        }
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        KillPlayer();
    }
    public void KillPlayer()
    {
        if (!playerDead)
        {
            playerDead = true;
            score += deathScore;
            Debug.Log($"Player score : {score}");
            lookingForCheckpointIndex = 0;
            score = 0;
            transform.position = startPot;
            transform.rotation = Quaternion.Euler(startRot);
            movement.StopAgent();
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Collider>() != null && other.GetComponent<Collider>() == checkPointTriggers[lookingForCheckpointIndex])
        {
            lookingForCheckpointIndex++;
            passedCheckPoint(checkpointScore);
            if (lookingForCheckpointIndex > checkPointTriggers.Length - 1)
            {
                lookingForCheckpointIndex = 0;
            }
        }
    }
    void passedCheckPoint(float scoreIncrease)
    {
        score += scoreIncrease;
    }
}
