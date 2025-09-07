using UnityEngine;

[CreateAssetMenu(fileName = "NewRangeSensorProfile", menuName = "MultiAgent/Sensor Profile/Range")]
public class SensorProfile_Range : ScriptableObject
{
    [Header("Range Detection")]
    [Tooltip("Distancia máxima que el sensor puede medir en metros.")]
    [Range(0.1f, 200f)]
    public float maxRange = 100.0f;

    [Header("Noise Simulation")]
    [Tooltip("Magnitud del error o ruido aleatorio (gaussiano) añadido a la medición en metros.")]
    [Range(0f, 5f)]
    public float noiseMagnitude = 0.05f; // ej. +/- 5cm de error

    [Header("Performance")]
    [Tooltip("Frecuencia de actualización del sensor en Hz (veces por segundo).")]
    [Range(1f, 100f)]
    public float updateFrequency = 20.0f;

    [Header("Layer Filtering")]
    [Tooltip("Capa de objetos que el sensor puede detectar.")]
    public LayerMask obstacleLayerMask = -1; // Default to all layers

    [Header("Direction")]
    [Tooltip("Dirección del rayo del sensor relativa al transform (0,0,-1 = hacia abajo).")]
    public Vector3 rayDirection = Vector3.down;
}