using UnityEngine;

public class SimpleVehicle : MonoBehaviour
{
    [SerializeField] private float mass = 1.0f;
    [SerializeField] private float maxForce = 1.0f;
    [SerializeField] private float maxSpeed = 2.0f;

    private Vector3 acceleration;

    public float Mass { get => Mathf.Max(0.001f, mass); }

    public float MaxForce { get => maxForce; }
    public float MaxSpeed { get => maxSpeed; }
    public Vector3 Position { get => transform.position; }

    public Vector3 CurrentVelocity { get; private set; }
    public Vector3 TurnVelocity { get; private set; } // orientation

    // Instantly teleport the vehicle to a different position
    public void Teleport(Vector3 worldPosition)
    {
        transform.position = worldPosition;
    }

    public void Steer(Vector3 desiredVelocity)
    {
        Vector3 force = CalculateSteeringForce(desiredVelocity);
        acceleration += force / Mass;
    }

    private Vector3 CalculateSteeringForce(Vector3 desiredVelocity)
    {
        var steerVector = desiredVelocity - CurrentVelocity;
        var clampedForce = Vector3.ClampMagnitude(steerVector, maxForce);
        return clampedForce;
    }

    private void ApplyMove()
    {
        CurrentVelocity = Vector3.ClampMagnitude(CurrentVelocity + acceleration, MaxSpeed);
        acceleration *= 0;

        transform.Translate(CurrentVelocity * Time.deltaTime, Space.World);
    }

    private void AdjustOrientation()
    {
        float speed = CurrentVelocity.magnitude;
        // Ternary operator
        // variable = condition ? <value when true> : <value when false>;
        TurnVelocity = Mathf.Approximately(speed, 0.0f) ? Vector3.zero : CurrentVelocity / speed;

        transform.up = Mathf.Approximately(TurnVelocity.magnitude, 0.0f) ?
            transform.up : TurnVelocity;
    }

    private void Update()
    {
        ApplyMove();
        //AdjustOrientation();
    }
}