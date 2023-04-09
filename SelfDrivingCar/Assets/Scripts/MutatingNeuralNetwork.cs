using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;

public class MutatingNeuralNetwork : IComparable<MutatingNeuralNetwork>
{
    private int[] layers; // num of neurons in each layer
    private float[][] neurons; // neurons outputs
    private float[][][] weights; // weights for each neuron
    public string NeuralNetworkFilePath;
    public float Fitness { get; set; } = 0; // the score for the neuralNetwork
    public MutatingNeuralNetwork(int[] layers)
    {
        this.layers = new int[layers.Length];
        //iterating over each array point to avoid creating a referance error where changing the value here changes the value in the input
        for (int i = 0; i < layers.Length; i++)
        {
            this.layers[i] = layers[i];
        }

        //creating the neurons and weights
        InitNeurons();
        InitWeights();
    }
    public MutatingNeuralNetwork(MutatingNeuralNetwork copyNetwork)
    {
        //initializing all arrays so we arent 
        layers = new int[copyNetwork.layers.Length];
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = copyNetwork.layers[i];
        }
        InitNeurons();
        InitWeights();
        CopyWeights(copyNetwork.weights);


    }
    public MutatingNeuralNetwork(string FilePathForNetwork)
    {
        NeuralNetworkFilePath = FilePathForNetwork;
        if (File.Exists(NeuralNetworkFilePath))
        {
            //the file will be set up like this
            //first line will be layers
            //second line will be weights info
            //Ex:
            //5,4,4,3,
            //6,7,8,9,3,5,4,3,2,5,3,2,6,1,3,1,
            using (StreamReader stream = new StreamReader(NeuralNetworkFilePath))
            {
                string line1 = stream.ReadLine();
                string line2 = stream.ReadLine();

                //this is a check to make sure this is a nueral network file
                string checkLine3 = stream.ReadLine();
                if (checkLine3 != null)
                {
                    throw new FileNotForNeuralNetworkException("File is not a neural network file. Too many lines.", NeuralNetworkFilePath);
                }
                //population layers for initlization of weigths
                List<int> layersList = new List<int>();
                try
                {
                    while (line1.Contains(','))
                    {

                        layersList.Add(int.Parse(line1.Substring(0, line1.IndexOf(','))));
                        line1 = line1.Substring(line1.IndexOf(',')+1);
                    }
                }
                catch (Exception e)
                {
                    throw new FileNotForNeuralNetworkException($"File is not a neural network file. Layer line contained invalid characters.\n Exeption Message : {e.Message}", NeuralNetworkFilePath);
                }


                layers = layersList.ToArray();
                //initizialiting the neurons and weights
                InitNeurons();
                InitWeights();
                //copying weights base on weights line/line 2
                CopyWeights(line2);
                //closing stream
                stream.Close();

            }
        }
        else
        {
            throw new NueralNetworkFileNotFoundException("File at the end of the file path for the neural network does not exist. Cannot create neural network.", NeuralNetworkFilePath);
        }

    }
    public void AddFitness(float fit)
    {
        this.Fitness += fit;
    }
    private void CopyWeights(float[][][] weights)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int k = 0; k < weights[i].Length; k++)
            {
                for (int j = 0; j < weights[i][k].Length; j++)
                {

                    this.weights[i][k][j] = weights[i][k][j];

                }
            }
        }
    }
    private void CopyWeights(string weightsString)
    {
        //copying weights over
        try
        {
            for (int i = 0; i < weights.Length; i++)
            {
                for (int k = 0; k < weights[i].Length; k++)
                {
                    for (int j = 0; j < weights[i][k].Length; j++)
                    {
                        if (weightsString.Contains(','))
                        {
                            weights[i][k][j] = float.Parse(weightsString.Substring(0, weightsString.IndexOf(',')));
                            weightsString = weightsString.Substring(weightsString.IndexOf(',') + 1);
                        }
                        else
                        {
                            throw new FileNotForNeuralNetworkException($"File is not a neural network file. Weight line did not include all weights.", NeuralNetworkFilePath);
                        }
                    }
                }
            }
        }
        catch (Exception e)
        {
            throw new FileNotForNeuralNetworkException($"File is not a neural network file. Weight line contained invalid characters.\n Exeption Message : {e.Message}", NeuralNetworkFilePath);
        }
    }
    private void InitNeurons()
    {
        List<float[]> neuronsList = new List<float[]>();
        for (int i = 0; i < layers.Length; i++)
        {
            neuronsList.Add(new float[layers[i]]);
        }
        neurons = neuronsList.ToArray();
    }
    private void InitWeights()
    {

        List<float[][]> weightList = new List<float[][]>();

        for (int i = 1; i < layers.Length; i++)
        {
            List<float[]> layerWeightList = new List<float[]>();
            int neuronsInPreviousLayer = layers[i - 1];

            for (int j = 0; j < neurons[i].Length; j++)
            {

                float[] neuronWeights = new float[neuronsInPreviousLayer];

                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    neuronWeights[k] = UnityEngine.Random.Range(-1f, 1f);
                }
                layerWeightList.Add(neuronWeights);
            }

            weightList.Add(layerWeightList.ToArray());
        }

        weights = weightList.ToArray();
    }
    public float[] FeedForward(float[] inputs)
    {
        //iterating over each array point to avoid creating a referance error where changing the value here changes the value in the input
        //making first layer of neurons equal our inputs
        for (int i = 0; i < inputs.Length; i++)
        {
            neurons[0][i] = inputs[i];
        }
        //iterating through the layers to find the values of each layer
        for (int i = 1; i < layers.Length; i++)
        {
            for (int j = 0; j < neurons[i].Length; j++)
            {
                float value = 0.25f;
                for (int k = 0; k < neurons[i - 1].Length; k++)
                {
                    value += weights[i - 1][j][k] * neurons[i - 1][k];
                }
                neurons[i][j] = (float)Math.Tanh(value);
            }
        }


        return neurons[neurons.Length - 1];//returning the last layer only

    }
    /// <summary>
    /// has a 0.8% chance of mutating for each weight in the network weights
    /// </summary>
    public void Mutate()
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    float weight = weights[i][j][k];

                    float randomNumber = UnityEngine.Random.Range(0f, 1f) * 100;
                    if (randomNumber <= .2f)
                    {//mutation1
                     //flip sign of weight
                    }
                    else if (randomNumber <= .4f)
                    {//mutation2
                     //pick random weight between -1 and 1
                        weight = UnityEngine.Random.Range(-1f, 1f);
                    }
                    else if (randomNumber <= .6f)
                    {//mutation3
                     //randomly increase by 0 to 100%
                        float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
                        weight *= factor;
                    }
                    else if (randomNumber <= .8f)
                    {//mutation4
                     //randomly decrease by 0 to 100%
                        float factor = UnityEngine.Random.Range(0f, 1f);
                        weight *= factor;
                    }


                    weights[i][j][k] = weight;
                }
            }
        }
    }
    /// <summary>
    /// has a chanceForEachMutation * 4 chance of mutating for each weight in the network weights
    /// </summary>
    /// <param name="chanceForEachMutation">the percent for each mutation to possibly activate</param>
    public void Mutate(float chanceForEachMutation)
    {
        for (int i = 0; i < weights.Length; i++)
        {
            for (int j = 0; j < weights[i].Length; j++)
            {
                for (int k = 0; k < weights[i][j].Length; k++)
                {
                    float weight = weights[i][j][k];

                    float randomNumber = UnityEngine.Random.Range(0f, 1f) * 100;
                    if (randomNumber <= chanceForEachMutation * 1)
                    {//mutation1
                     //flip sign of weight
                    }
                    else if (randomNumber <= chanceForEachMutation * 2)
                    {//mutation2
                     //pick random weight between -1 and 1
                        weight = UnityEngine.Random.Range(-1f, 1f);
                    }
                    else if (randomNumber <= chanceForEachMutation * 3)
                    {//mutation3
                     //randomly increase by 0 to 100%
                        float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
                        weight *= factor;
                    }
                    else if (randomNumber <= chanceForEachMutation * 4)
                    {//mutation4
                     //randomly decrease by 0 to 100%
                        float factor = UnityEngine.Random.Range(0f, 1f);
                        weight *= factor;
                    }


                    weights[i][j][k] = weight;
                }
            }
        }
    }
    /// <summary>
    /// used for sorting in assending order of the best neural network
    /// </summary>
    /// <param name="other">the neural network this object is being compared to</param>
    /// <returns>the sorting int</returns>
    public int CompareTo(MutatingNeuralNetwork other)
    {
        if (other == null || Fitness > other.Fitness)
            return 1;
        else if (Fitness < other.Fitness)
            return -1;
        else
            return 0;
    }
    public void SaveNueralNetwork()
    {
        if (NeuralNetworkFilePath != "")
        {
            if (File.Exists(NeuralNetworkFilePath))
            {
                File.Delete(NeuralNetworkFilePath);
            }
            using (StreamWriter stream = new StreamWriter(File.Create(NeuralNetworkFilePath)))
            {
                //creating the first line of the file
                string line1 = "";
                for (int i = 0; i < layers.Length; i++)
                {
                    line1 += $"{layers[i]},";
                }
                //creating the second line of the file
                string line2 = "";
                for (int i = 0; i < weights.Length; i++)
                {
                    for (int k = 0; k < weights[i].Length; k++)
                    {
                        for (int j = 0; j < weights[i][k].Length; j++)
                        {
                            line2 += $"{weights[i][k][j]},";
                        }
                    }
                }
                //putting info into file
                stream.WriteLine(line1);
                stream.WriteLine(line2);
                //closing stream
                stream.Close();
            }
        }
    }
    public void SaveNueralNetwork(string FilePath)
    {
        NeuralNetworkFilePath = FilePath;
        //deleting the previous file
        if (File.Exists(NeuralNetworkFilePath))
        {
            File.Delete(NeuralNetworkFilePath);
        }
        //creating a new file
        using (StreamWriter stream = new StreamWriter(File.Create(NeuralNetworkFilePath)))
        {
            //creating the first line of the file
            string line1 = "";
            for (int i = 0; i < layers.Length; i++)
            {
                line1 += $"{layers[i]},";
            }
            //creating the second line of the file
            string line2 = "";
            for (int i = 0; i < weights.Length; i++)
            {
                for (int k = 0; k < weights[i].Length; k++)
                {
                    for (int j = 0; j < weights[i][k].Length; j++)
                    {
                        line2 += $"{weights[i][k][j]},";
                    }
                }
            }
            //putting info into file
            stream.WriteLine(line1);
            stream.WriteLine(line2);
            //closing stream
            stream.Close();
        }
    }
}

public class NueralNetworkFileNotFoundException : Exception
{
    public string FilePath { get; }
    public NueralNetworkFileNotFoundException() { }
    public NueralNetworkFileNotFoundException(string message) : base(message) { }
    public NueralNetworkFileNotFoundException(string message, Exception inner) : base(message, inner) { }
    public NueralNetworkFileNotFoundException(string message, string FilePath) : this(message)
    {
        this.FilePath = FilePath;
    }
}
public class FileNotForNeuralNetworkException : Exception
{
    public string FilePath { get; }
    public FileNotForNeuralNetworkException() { }
    public FileNotForNeuralNetworkException(string message) : base(message) { }
    public FileNotForNeuralNetworkException(string message, Exception inner) : base(message, inner) { }
    public FileNotForNeuralNetworkException(string message, string FilePath) : this(message)
    {
        this.FilePath = FilePath;
    }
}