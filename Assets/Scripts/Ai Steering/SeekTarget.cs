using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SeekTarget : MonoBehaviour
{
    private SimpleVehicle vehicle;

    private Vector3 desiredVelocity;
    private float clippedSpeed;
    private float distanceToUse;
    [SerializeField] private PathRenderer pathRenderer;

    [SerializeField] private float slowingRadius = 1.0f;
    [SerializeField] private float stoppingDistance = 0.05f;

    public Queue<Vector3> pathCoord = new();
    public LineRenderer lr;
    public int counter
    {
        get;
        set;
    }
    private Vector3 target;
    // Start is called before the first frame update

    private void Awake()
    {
        vehicle = GetComponent<SimpleVehicle>();
    }

    private void Update()
    {
        if (GameManagerStates.playState && !GameManagerStates.editState)
        {
            if (lr.positionCount <= 0)
            {
                vehicle.Steer(Vector3.zero);
                return;
            }

            float minDist = Mathf.Clamp( (vehicle.MaxSpeed / 10.0f) - 0.2f, 0.01f, 0.1f );
            if ((target - vehicle.Position).magnitude < minDist && counter < lr.positionCount - 1)
            {
                counter++;
                pathCoord.Enqueue(lr.GetPosition(counter));
            }

            if ((lr.GetPosition(lr.positionCount -1) - vehicle.Position).magnitude < minDist) 
            {
                lr.positionCount = 0;
            }
                vehicle.Steer(Calculate());
        }
        else
        {
            vehicle.Steer(Vector3.zero);
        }
    }

    private Vector3 Calculate()
    {
        if (lr.positionCount > 1)
        {
            if (pathCoord.Count <= 0)
            {
                pathCoord.Enqueue(lr.GetPosition(counter));
            }

            target = pathCoord.Dequeue();
            Vector3 displacement = target - vehicle.Position;
            Vector3 targetDirection = (displacement).normalized;

            if (target == lr.GetPosition(lr.positionCount - 1))
            {
                Vector3 distance = vehicle.Position - target;

                if (distance.magnitude <= stoppingDistance)
                {
                    desiredVelocity = Vector3.zero;
                }
                else
                {
                    if (distance.magnitude < stoppingDistance)
                    {
                        distanceToUse = stoppingDistance;
                    }
                    else
                    {
                        distanceToUse = distance.magnitude;
                    }

                    float rampedSpeed = vehicle.MaxSpeed * (distanceToUse / slowingRadius);

                    if (rampedSpeed < vehicle.MaxSpeed)
                    {
                        clippedSpeed = rampedSpeed;
                    }
                    else
                    {
                        clippedSpeed = vehicle.MaxSpeed;
                    }
                }
                desiredVelocity = targetDirection.normalized * clippedSpeed;
            }
            else
            {
                desiredVelocity = targetDirection.normalized * vehicle.MaxSpeed;
            }
            return desiredVelocity;

        }
        return Vector3.zero;
    }

}

