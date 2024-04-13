using UnityEngine;

public class Movement : MonoBehaviour
{
    public float forwardSpeed = 5f;
    public float lateralSpeed = 5f;
    public float rotationSpeed = 10f;
    public Transform targetLocation;
    private bool obstacleDetected = false;



    // Update is called once per frame
    void Update()
    {
        //Check for obstacle detection and adjust movement behavior accordingly
        if (!obstacleDetected)
        {
            MoveForward();
        }

        AvoidObstacles();

    }

    private void AvoidObstacles()
{
    // Define raycast parameters
    float rayLength = 10f;
    float halfAngle = 90f;
    int numRays = 5;
    float forwardThreshold = 3f; // Threshold distance to continue moving forward
    obstacleDetected = false; // Reset obstacle detection flag

    // Calculate the angle between each raycast
    float angleIncrement = 2f * halfAngle / (numRays - 1);

    // Initialize variables to store left and right avoidance directions
    Vector3 avoidLeft = Vector3.zero;
    Vector3 avoidRight = Vector3.zero;

    // Perform raycast in a triangular pattern
    for (int i = 0; i < numRays; i++)
    {
        float angle = -halfAngle + i * angleIncrement;

        Vector3 direction = Quaternion.AngleAxis(angle, transform.up) * transform.forward;

        // Perform raycast in the current direction
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, rayLength))
        {
            // Check if the obstacle is a wall or tagged appropriately
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Obstacle"))
            {
                // Calculate avoidance direction perpendicular to the obstacle normal
                Vector3 avoidanceDir = Vector3.Cross(Vector3.up, hit.normal);
                avoidanceDir.y = 0.0f;

                // Visualize rays
                Debug.DrawLine(transform.position, hit.point, Color.red);
                Debug.DrawLine(hit.point, hit.point + hit.normal, Color.green);

                // Update avoidance directions based on obstacle position
                if (Vector3.Dot(Vector3.Cross(Vector3.up, transform.forward), avoidanceDir) < 0)
                {
                    // Obstacle is to the left
                    avoidLeft += avoidanceDir.normalized;
                }
                else
                {
                    // Obstacle is to the right
                    avoidRight += avoidanceDir.normalized;
                }

                obstacleDetected = true;

                // Check if the distance to the hit point is greater than the forward threshold
                if (Vector3.Distance(transform.position, hit.point) > forwardThreshold)
                {
                    // Move forward if the obstacle hit point is far enough
                    MoveForward();
                    return; 
                }
            }
        }
    }

    // Choose the best avoidance direction based on obstacle positions
    Vector3 combinedAvoidDir = (avoidLeft.normalized + avoidRight.normalized).normalized;

    // Adjust movement direction away from the obstacles
    if (obstacleDetected)
    {
        MoveSideways(combinedAvoidDir);
    }
    else
    {
        // If no obstacle detected, move forward again
        MoveForward();
    }
}







    private void MoveSideways(Vector3 direction)
    {
        transform.Translate(direction * lateralSpeed * Time.deltaTime, Space.World);
    }

    private void MoveForward()
    {
        Vector3 targetDirection = (targetLocation.position - transform.position).normalized;
        targetDirection.y = 0;

        transform.Translate(targetDirection * forwardSpeed * Time.deltaTime, Space.World);

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

    }
}