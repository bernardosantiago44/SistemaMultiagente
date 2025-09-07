using UnityEngine;

[CreateAssetMenu(fileName = "NewCameraSensorProfile", menuName = "MultiAgent/Sensor Profile/Camera")]
public class SensorProfile_Camera : ScriptableObject
{
    [Header("Field of View")]
    [Tooltip("Campo de visión angular de la cámara en grados.")]
    [Range(1f, 180f)]
    public float fieldOfView = 90.0f;

    [Header("Detection Range")]
    [Tooltip("Distancia máxima a la que la cámara puede detectar objetos.")]
    [Range(1f, 100f)]
    public float viewRange = 50.0f;

    [Header("Target Detection")]
    [Tooltip("Etiqueta de los objetos que este sensor debe detectar (ej. 'Player', 'Target', 'Person').")]
    public string targetTag = "Player";

    [Header("Performance")]
    [Tooltip("Frecuencia de actualización del sensor en Hz (veces por segundo).")]
    [Range(1f, 60f)]
    public float updateFrequency = 10.0f;

    [Header("Noise Simulation")]
    [Tooltip("Probabilidad de detectar un objeto cuando está en rango (0.0 = nunca, 1.0 = siempre).")]
    [Range(0f, 1f)]
    public float detectionAccuracy = 0.95f;

    [Tooltip("Probabilidad de falso positivo por frame (0.0 = nunca, 1.0 = siempre).")]
    [Range(0f, 0.1f)]
    public float falsePositiveRate = 0.01f;

    [Header("Layer Filtering")]
    [Tooltip("Capas que bloquean la línea de visión (obstacles, walls, etc).")]
    public LayerMask obstacleLayerMask = -1;
}