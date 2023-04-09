using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.UIElements;
using System.Runtime.InteropServices.WindowsRuntime;
using OpenCover.Framework.Model;

public class AgentManager : MonoBehaviour
{

    [SerializeField] int generationNumber = 1;
    [SerializeField] float highestScoreOfLastGeneration = 0;
    [SerializeField] float timePassedSinceStartOftest = 0;
    [SerializeField] int NumAgent = 0;
    [SerializeField] float timeForEachTest = 160;
    [SerializeField] GameObject NeuralNetworkAgentPrefab;
    GameObject[] agents;
    [SerializeField] Vector3 spawnLocation = Vector3.zero;
    [SerializeField] Vector3 spawnRotation = Vector3.zero;
    [SerializeField] Collider[] checkPointTriggers;
    [SerializeField] string NeuralNetworkFilePath = @"D:\UnityProjects\SelfDrivingCar\Assets\NetworkFiles\Network.txt";
    [SerializeField] bool saveNeuralNetwork = true;
    [SerializeField] PlayerController player;
    // Start is called before the first frame update
    void Start()
    {
        bool NetworkExisits = false;
        MutatingNeuralNetwork existingNueralNetwork = new MutatingNeuralNetwork(new int[0]);
        if (System.IO.File.Exists(NeuralNetworkFilePath))
        {
            existingNueralNetwork = new MutatingNeuralNetwork(NeuralNetworkFilePath);
            NetworkExisits = true;
        }
        agents = new GameObject[NumAgent];
        player.checkPointTriggers = checkPointTriggers;
        for (int i = 0; i < NumAgent; i++)
        {
            agents[i] = Instantiate(NeuralNetworkAgentPrefab, spawnLocation, Quaternion.Euler(spawnRotation));
            agents[i].GetComponent<Collider>().enabled = false;
            agents[i].GetComponent<AgentController>().checkPointTriggers = checkPointTriggers;
            if (NetworkExisits)
            {
                if(i == 0)
                {
                    agents[i].GetComponent<AgentController>().network = new MutatingNeuralNetwork(existingNueralNetwork);
                }
                else
                {
                    agents[i].GetComponent<AgentController>().UpdateNetwork(existingNueralNetwork);
                }
                
            }
        }
        for (int i = 0; i < agents.Length; i++)
        {
            for (int x = 0; x < agents.Length; x++)
            {
                if (x != i)
                {
                    Physics.IgnoreCollision(agents[x].GetComponent<Collider>(), agents[i].GetComponent<Collider>(), true);

                }
            }
            agents[i].GetComponent<Collider>().enabled = true;
        }
    }

    bool livingPeeps = true;
    // Update is called once per frame
    void Update()
    {
        if (timePassedSinceStartOftest < timeForEachTest && livingPeeps)
        {
            timePassedSinceStartOftest += Time.deltaTime;
            bool allDead = true;
            if (player.playerDead)
            {
                for (int i = 0; i < agents.Length; i++)
                {
                    if (!agents[i].GetComponent<AgentController>().dead)
                    {
                        allDead = false;
                        i = agents.Length;
                    }
                }
            }
            else
            {
                allDead = false;
            }
            
            livingPeeps = !allDead;
        }
        else
        {
            livingPeeps = false;
            player.KillPlayer();
            agents = agents.OrderByDescending(x => x.gameObject.GetComponent<AgentController>().network.Fitness).ToArray();
            MutatingNeuralNetwork bestNetwork = new MutatingNeuralNetwork(agents[0].GetComponent<AgentController>().network);
            float bestFitness = agents[0].GetComponent<AgentController>().network.Fitness;

            highestScoreOfLastGeneration = bestFitness;
            if (NumAgent % 2 == 0 && NumAgent == agents.Length)
            {
                GameObject[] newAgents = new GameObject[agents.Length];
                for (int i = agents.Length - 1; i >= 0; i--)
                {
                    newAgents[i] = Instantiate(NeuralNetworkAgentPrefab, spawnLocation, Quaternion.Euler(spawnRotation));
                    if (i >= NumAgent / 2)
                    {
                        newAgents[i].GetComponent<AgentController>().UpdateNetwork(agents[i - (NumAgent / 2)].GetComponent<AgentController>().network);
                    }
                    else
                    {
                        newAgents[i].GetComponent<AgentController>().network = new MutatingNeuralNetwork(agents[i].GetComponent<AgentController>().network);
                    }
                    Destroy(agents[i]);
                    newAgents[i].GetComponent<AgentController>().checkPointTriggers = checkPointTriggers;
                }

                agents = newAgents;

                for (int i = 0; i < agents.Length; i++)
                {
                    for (int x = 0; x < agents.Length; x++)
                    {
                        if (x != i)
                        {
                            Physics.IgnoreCollision(agents[x].GetComponent<Collider>(), agents[i].GetComponent<Collider>(), true);
                        }
                    }

                }
            }
            else if (NumAgent == agents.Length)
            {
                for (int i = 0; i < NumAgent; i++)
                {
                    Destroy(agents[i]);
                    agents[i] = Instantiate(NeuralNetworkAgentPrefab, spawnLocation, Quaternion.Euler(spawnRotation));
                    if (i != NumAgent - 1)
                    {
                        agents[i].GetComponent<AgentController>().UpdateNetwork(bestNetwork);
                    }
                    else
                    {
                        agents[i].GetComponent<AgentController>().network = new MutatingNeuralNetwork(bestNetwork);
                    }
                    agents[i].GetComponent<AgentController>().checkPointTriggers = checkPointTriggers;
                }
                for (int i = 0; i < agents.Length; i++)
                {
                    for (int x = 0; x < agents.Length; x++)
                    {
                        if (x != i)
                        {
                            Physics.IgnoreCollision(agents[x].GetComponent<Collider>(), agents[i].GetComponent<Collider>(), true);
                        }
                    }

                }
            }
            else
            {
                for (int i = 0; i < agents.Length; i++)
                {
                    Destroy(agents[i]);
                }
                agents = new GameObject[NumAgent];
                for (int i = 0; i < NumAgent; i++)
                {
                    Destroy(agents[i]);
                    agents[i] = Instantiate(NeuralNetworkAgentPrefab, spawnLocation, Quaternion.Euler(spawnRotation));
                    if (i != NumAgent - 1)
                    {
                        agents[i].GetComponent<AgentController>().UpdateNetwork(bestNetwork);
                    }
                    else
                    {
                        agents[i].GetComponent<AgentController>().network = bestNetwork;
                    }
                    agents[i].GetComponent<AgentController>().checkPointTriggers = checkPointTriggers;
                }
                for (int i = 0; i < agents.Length; i++)
                {
                    for (int x = 0; x < agents.Length; x++)
                    {
                        if (x != i)
                        {
                            Physics.IgnoreCollision(agents[x].GetComponent<Collider>(), agents[i].GetComponent<Collider>(), true);
                        }
                    }

                }
            }

            generationNumber++;
            timePassedSinceStartOftest = 0;
            livingPeeps = true;
            player.playerDead = false;
        }
    }
    private void OnApplicationQuit()
    {
        if (saveNeuralNetwork)
        {
            agents = agents.OrderByDescending(x => x.gameObject.GetComponent<AgentController>().network.Fitness).ToArray();
            agents[0].GetComponent<AgentController>().network.SaveNueralNetwork(NeuralNetworkFilePath);
        }
    }

}
