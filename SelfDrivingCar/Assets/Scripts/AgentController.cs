using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentController : MonoBehaviour
{
    public MutatingNeuralNetwork network;
    [SerializeField] int[] layerSetup = { 5, 4, 4, 2 };
    [SerializeField] float raysDistance = 5f;
    [SerializeField] LayerMask obstacleLayers;
    [SerializeField] AgentMovement movement;
    float rayDegreeSplit;
    [SerializeField] float deathScore = -1;
    [SerializeField] float checkpointScore = 1;
    public Collider[] checkPointTriggers;
    int lookingForCheckpointIndex = 0;
    public bool dead;
    public float timeOfLife = 0;
    // Start is called before the first frame update
    void Start()
    {
        network = new MutatingNeuralNetwork(layerSetup);

        if (layerSetup[0] != 1)
        {
            rayDegreeSplit = 180 / (layerSetup[0] - 2);
        }
        else
        {
            rayDegreeSplit = 90;
        }
        movement = GetComponent<AgentMovement>();
    }
    // Update is called once per frame
    void Update()
    {
        if (!dead)
        {
            timeOfLife += Time.deltaTime;
            int numOfrays = layerSetup[0]-1;
            float[] distances = new float[layerSetup[0]];
            if (numOfrays != 1)
            {


                for (int i = 0; i < numOfrays; i++)
                {
                    float degreeForIteration = (rayDegreeSplit * i) - transform.rotation.eulerAngles.y;
                    RaycastHit ray;
                    if (Physics.Raycast(transform.position,
                        (
                            new Vector3(Mathf.Cos(degreeForIteration * Mathf.Deg2Rad), 0,Mathf.Sin(degreeForIteration * Mathf.Deg2Rad)
                        ) ),
                        out ray, raysDistance, obstacleLayers))
                    {
                        distances[i] = ray.distance;
                        Debug.DrawLine(
                            transform.position,

                            transform.position + (

                                    new Vector3(
                                        Mathf.Cos(
                                            degreeForIteration * Mathf.Deg2Rad
                                            )
                                        , 0,
                                        Mathf.Sin(
                                            degreeForIteration * Mathf.Deg2Rad
                                            )
                                    )
                                * ray.distance
                            )

                            , Color.red
                            );
                    }
                    else
                    {
                        distances[i] = raysDistance;
                        Debug.DrawLine(
                             transform.position,

                             transform.position + (
                                     new Vector3(
                                         Mathf.Cos(
                                             degreeForIteration * Mathf.Deg2Rad
                                             )
                                         , 0,
                                         Mathf.Sin(
                                             degreeForIteration * Mathf.Deg2Rad
                                             )
                                     )

                                 * raysDistance
                             )

                             , Color.blue
                             );
                    }

                }
            }
            else
            {
                RaycastHit ray;
                if (Physics.Raycast(transform.position, transform.forward, out ray, raysDistance, obstacleLayers))
                {
                    distances[0] = ray.distance;
                    Debug.DrawLine(transform.position, transform.position + (transform.forward * ray.distance), Color.red);
                }
                else
                {
                    distances[0] = raysDistance;
                    Debug.DrawLine(transform.position, transform.position + (transform.forward * raysDistance), Color.blue);
                }
            }
            distances[distances.Length - 1] = movement.currentSpeed;
            float[] networkOutput = network.FeedForward(distances);
            Vector2 inputs = new Vector2(networkOutput[0], networkOutput[1]);
            movement.input = inputs;
        }
    }
    void passedCheckPoint(float scoreIncrease)
    {
        network.AddFitness(scoreIncrease);
    }
    public void UpdateNetwork(MutatingNeuralNetwork networkToUpdateTo)
    {
        network = new MutatingNeuralNetwork(networkToUpdateTo);
        network.Mutate();
    }
    public void UpdateNetwork(MutatingNeuralNetwork networkToUpdateTo, float chancesOfMutation)
    {
        network = new MutatingNeuralNetwork(networkToUpdateTo);
        network.Mutate(chancesOfMutation / 4);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!dead)
        {
            Debug.Log("Died");
            gameObject.GetComponent<MeshRenderer>().enabled = false;
        }
        dead = true;
        network.AddFitness(timeOfLife + deathScore);

    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Collider>() != null && other.GetComponent<Collider>() == checkPointTriggers[lookingForCheckpointIndex] && !dead)
        {
            lookingForCheckpointIndex++;
            Debug.Log("Passed checkpoint " + (lookingForCheckpointIndex - 1));
            passedCheckPoint(checkpointScore);
            if (lookingForCheckpointIndex > checkPointTriggers.Length - 1)
            {
                lookingForCheckpointIndex = 0;
            }
        }
    }
}
