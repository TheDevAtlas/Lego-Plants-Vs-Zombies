using UnityEngine;

public class MoveCam : MonoBehaviour
{
    // Define the horizontal bounds for the camera movement.
    // bounds.x is the left limit, and bounds.y is the right limit.
    public Vector2 bounds = new Vector2(-10f, 10f);
    
    // Maximum speed the camera will reach.
    public float speed = 5f;
    
    // The time (in seconds) it takes to accelerate from 0 to max speed.
    public float accelerationTime = 0.5f;
    
    // Private variable to keep track of the current horizontal speed.
    private float currentSpeed = 0f;
    
    // The target position toward which the camera will interpolate.
    private Vector3 targetPosition;

    void Start()
    {
        // Initialize the target position to the camera's starting position.
        targetPosition = transform.position;
    }

    void Update()
    {
        float moveInput = 0f;

        // Check for left movement using A or Left Arrow key.
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            moveInput = -1f;
        }
        // Check for right movement using D or Right Arrow key.
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            moveInput = 1f;
        }
        
        if (moveInput != 0f)
        {
            // Calculate the acceleration rate.
            // This determines how much the speed increases per second.
            float accelerationRate = speed / accelerationTime;
            // Gradually adjust currentSpeed toward (moveInput * max speed)
            currentSpeed = Mathf.MoveTowards(currentSpeed, moveInput * speed, accelerationRate * Time.deltaTime);
        }
        else
        {
            // No input: instantly reset currentSpeed.
            // This keeps the smooth stopping behavior already present via Slerp.
            currentSpeed = 0f;
        }

        // Adjust the target position along the x-axis based on the current speed.
        targetPosition.x += currentSpeed * Time.deltaTime;

        // Clamp the target position x value within the specified bounds.
        targetPosition.x = Mathf.Clamp(targetPosition.x, bounds.x, bounds.y);

        // Smoothly interpolate the current position toward the target position using Slerp.
        transform.position = Vector3.Slerp(transform.position, targetPosition, speed * Time.deltaTime);
    }
}
