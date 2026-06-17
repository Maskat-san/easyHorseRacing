using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

[RequireComponent(typeof(Rigidbody))]
public class HorseController : MonoBehaviour
{
    [Header("レール設定")]
    [Tooltip("内側から外側の順にSplineContainerを登録してください")]
    public SplineContainer[] laneRails;

    [Tooltip("現在走っているレール番号")]
    public int currentRailIndex = 0;

    [Header("ウマの物理設定")]
    public float forwardForce = 2000f;
    public float railSteeringForce = 1200f;
    public float maxSpeed = 20f;

    [Header("レーン変更")]
    public float laneChangeCooldown = 0.3f;

    [Header("Spline追従")]
    public float lookAhead = 0.02f;
    public float rotationSpeed = 8f;

    private Rigidbody rb;
    private float laneChangeTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        if (laneRails == null || laneRails.Length == 0)
        {
            Debug.LogError("laneRailsにSplineContainerを登録してください。");
        }

        currentRailIndex = Mathf.Clamp(
            currentRailIndex,
            0,
            laneRails.Length - 1
        );
    }

    void Update()
    {
        HandleRailInput();
    }

    void FixedUpdate()
    {
        if (laneRails == null || laneRails.Length == 0) return;

        MoveAlongCurrentRail();
    }

    void HandleRailInput()
    {
        laneChangeTimer -= Time.deltaTime;

        if (laneChangeTimer > 0f) return;

        float h = Input.GetAxisRaw("Horizontal");

        if (h > 0.5f)
        {
            currentRailIndex++;
            currentRailIndex = Mathf.Clamp(
                currentRailIndex,
                0,
                laneRails.Length - 1
            );

            laneChangeTimer = laneChangeCooldown;
        }
        else if (h < -0.5f)
        {
            currentRailIndex--;
            currentRailIndex = Mathf.Clamp(
                currentRailIndex,
                0,
                laneRails.Length - 1
            );

            laneChangeTimer = laneChangeCooldown;
        }
    }

    void MoveAlongCurrentRail()
    {
        SplineContainer currentRail = laneRails[currentRailIndex];

        float3 horsePos = transform.position;

        float3 nearestPoint;
        float t;

        SplineUtility.GetNearestPoint(
            currentRail.Spline,
            currentRail.transform.InverseTransformPoint(horsePos),
            out nearestPoint,
            out t
        );

        Vector3 railPosWorld =
            currentRail.transform.TransformPoint((Vector3)nearestPoint);

        float targetT = Mathf.Repeat(t + lookAhead, 1f);

        Vector3 railTargetPos =
            (Vector3)currentRail.EvaluatePosition(targetT);

        Vector3 forwardVec =
            (railTargetPos - railPosWorld).normalized;

        if (forwardVec.sqrMagnitude < 0.001f)
            return;

        // 前進力
        rb.AddForce(forwardVec * forwardForce, ForceMode.Force);

        // 現在のレールへ戻る力
        Vector3 toRail = railTargetPos - transform.position;

        Vector3 lateralCorrection =
            Vector3.ProjectOnPlane(toRail, forwardVec);

        if (lateralCorrection.sqrMagnitude > 0.001f)
        {
            rb.AddForce(
                lateralCorrection.normalized * railSteeringForce,
                ForceMode.Force
            );
        }

        // 向き補正
        Vector3 lookDir = rb.linearVelocity;
        lookDir.y = 0f;

        if (lookDir.magnitude > 1f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(lookDir.normalized, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.fixedDeltaTime * rotationSpeed
            );
        }

        // 速度制限
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized * maxSpeed;
        }

        Debug.DrawLine(transform.position, railTargetPos, Color.green);
        Debug.DrawRay(railPosWorld, forwardVec * 5f, Color.red);
    }
}