using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float smoothSpeed = 10f;
    public Vector3 offset;

    public bool FixedY;
    public bool DEBUG_LookAt;

    void FixedUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        if (FixedY)
            desiredPosition = new Vector3(desiredPosition.x, transform.position.y, desiredPosition.z);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        if (DEBUG_LookAt)
            transform.LookAt(target);
    }
    public void targetFly(bool b)
    {
        FixedY = !b;
    }
}
