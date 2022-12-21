using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.ML-Agents;
using Unity.ML-Agents.Sensors;

public class VoiceImitationAgent : Agent
{
    // The AudioSource component that will play the audio
    public AudioSource audioSource;

    // The neural network model for voice imitation
    public NeuralNetwork model;

    // The audio data recorded from the microphone
    public byte[] microphoneData;

    // The current frame of the audio data being processed
    private int currentFrame;

    // The input and output buffers for the neural network
    private float[] inputs;
    private float[] outputs;

    // The rate at which the audio is played, in Hz
    public int sampleRate = 44100;

    // The interval at which the agent takes an action, in seconds
    public float actionInterval = 0.1f;

    void Start()
    {
        // Initialize the input and output buffers for the neural network
        inputs = new float[model.GetInputCount()];
        outputs = new float[model.GetOutputCount()];
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Extract features from the current frame of the audio data and add them to the sensor
        ExtractFeatures(microphoneData, currentFrame, inputs);
        sensor.AddObservation(inputs);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        // Use the neural network model to predict the output audio sample based on the input features
        model.Forward(inputs, outputs);

        // Add the predicted audio sample to the microphone data
        AddSample(outputs, microphoneData);

        // Train the neural network model on the input features and the predicted output
        model.Backward(inputs, outputs);

        // Increment the current frame
        currentFrame++;

        // Wait for the specified interval before taking the next action
        yield return new WaitForSeconds(actionInterval);
    }

    void ExtractFeatures(byte[] audioData, int frame, float[] features)
{
    // Calculate the number of samples in the audio data
    int numSamples = audioData.Length / sizeof(float);

    // Calculate the number of samples per frame
    int samplesPerFrame = sampleRate * actionInterval;

    // Calculate the starting sample index for the current frame
    int startSample = frame * samplesPerFrame;

    // Calculate the ending sample index for the current frame
    int endSample = startSample + samplesPerFrame;

    // Check if the ending sample index is within the bounds of the audio data
    if (endSample > numSamples)
    {
        // If the ending sample index is out of bounds, set it to the last sample in the audio data
        endSample = numSamples;
    }

    // Initialize the feature index
    int featureIndex = 0;

    // Extract the pitch and spectral features from the audio data
    for (int i = startSample; i < endSample; i++)
    {
        // Convert the audio sample from a byte to a float
        float sample = BitConverter.ToSingle(audioData, i * sizeof(float));

        // Extract the pitch feature
        features[featureIndex] = ExtractPitchFeature(sample);
        featureIndex++;

        // Extract the spectral features
        ExtractSpectralFeatures(sample, features, featureIndex);
        featureIndex += numSpectralFeatures;
    }

    // Extract the prosodic features from the audio data
    ExtractProsodicFeatures(audioData, startSample, endSample, features, featureIndex);
}

float ExtractPitchFeature(float sample)
{
    // Calculate the number of samples in the audio data
    int numSamples = microphoneData.Length / sizeof(float);

    // Calculate the autocorrelation of the audio data
    float[] autocorrelation = CalculateAutocorrelation(microphoneData, numSamples);

    // Find the maximum value in the autocorrelation array
    float maxValue = autocorrelation[0];
    int maxIndex = 0;
    for (int i = 1; i < numSamples; i++)
    {
        if (autocorrelation[i] > maxValue)
        {
            maxValue = autocorrelation[i];
            maxIndex = i;
        }
    }

    // Calculate the pitch frequency based on the maximum value in the autocorrelation array
    float pitchFrequency = (float)sampleRate / maxIndex;

    // Return the pitch frequency as a feature
    return pitchFrequency;
}

float[] CalculateAutocorrelation(byte[] audioData, int numSamples)
{
    // Initialize an array to store the autocorrelation values
    float[] autocorrelation = new float[numSamples];

    // Iterate over the samples in the audio data
    for (int i = 0; i < numSamples; i++)
    {
        // Initialize the sum to 0
        float sum = 0;

        // Iterate over the samples in the audio data, starting at the current sample
        for (int j = 0; j < numSamples - i; j++)
        {
            // Convert the audio sample from a byte to a float
            float sample = BitConverter.ToSingle(audioData, j * sizeof(float));

            // Add the product of the current sample and the sample at the specified offset to the sum
            sum += sample * BitConverter.ToSingle(audioData, (j + i) * sizeof(float));
        }

        // Store the sum as the autocorrelation value at the current index
        autocorrelation[i] = sum;
    }

    // Return the autocorrelation array
    return autocorrelation;
}
