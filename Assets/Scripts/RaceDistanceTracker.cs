using UnityEngine;

public class RaceDistanceTracker : MonoBehaviour
{
    [Header("ƒŒ[ƒXÝ’è")]
    public float goalDistance = 2000f;

    [Header("Œ»Ý‚Ì‘–s‹——£")]
    public float currentDistance = 0f;

    public bool IsFinished { get; private set; }

    private Vector3 lastPosition;
    private bool initialized;

    void Start()
    {
        lastPosition = transform.position;
        initialized = true;
    }

    void Update()
    {
        if (IsFinished) return;

        if (!initialized)
        {
            lastPosition = transform.position;
            initialized = true;
            return;
        }

        Vector3 currentPosition = transform.position;

        float movedDistance = Vector3.Distance(
            currentPosition,
            lastPosition
        );

        currentDistance += movedDistance;
        lastPosition = currentPosition;

        if (currentDistance >= goalDistance)
        {
            currentDistance = goalDistance;
            IsFinished = true;
        }
    }

    public float GetProgressRate()
    {
        if (goalDistance <= 0f) return 0f;

        return currentDistance / goalDistance;
    }
}