using UnityEngine;

public class ShakyCamSmooth : MonoBehaviour
{
    [Tooltip("Maximum angular offset from the original rotation (in degrees)")]
    public float angleRange = 5f;

    [Tooltip("Speed factor for how quickly the Perlin noise changes over time")]
    public float noiseSpeed = 1f;

    [Tooltip("Smoothing time for dampening the rotation changes (in seconds)")]
    public float smoothTime = 0.5f;

    private Vector3 initialRotationEuler;
    private Vector2 noiseOffset = new Vector2(0f, 100f);
    private Vector2 currentOffset = Vector2.zero;
    private Vector2 velocity = Vector2.zero;

    private void Start()
    {
        initialRotationEuler = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        float deltaNoise = deltaTime * noiseSpeed;

        // Advance noise offset
        noiseOffset.x += deltaNoise;
        noiseOffset.y += deltaNoise;

        // Calculate Perlin noise based offset
        float targetOffsetX = (Mathf.PerlinNoise(noiseOffset.x, 0f) - 0.5f) * 2f * angleRange;
        float targetOffsetY = (Mathf.PerlinNoise(noiseOffset.y, 0f) - 0.5f) * 2f * angleRange;

        // Smooth damping
        currentOffset.x = Mathf.SmoothDampAngle(currentOffset.x, targetOffsetX, ref velocity.x, smoothTime, Mathf.Infinity, deltaTime);
        currentOffset.y = Mathf.SmoothDampAngle(currentOffset.y, targetOffsetY, ref velocity.y, smoothTime, Mathf.Infinity, deltaTime);

        // Apply new rotation
        transform.rotation = Quaternion.Euler(
            initialRotationEuler.x + currentOffset.x,
            initialRotationEuler.y + currentOffset.y,
            initialRotationEuler.z
        );
    }
}
