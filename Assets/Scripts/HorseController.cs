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

    [Header("ウマの移動設定")]
    public float targetSpeed = 16f;
    public float acceleration = 8f;
    public float railFollowStrength = 12f;
    public float maxSpeed = 20f;

    [Header("レーン変更")]
    public float laneChangeCooldown = 0.3f;

    [Header("Spline追従")]
    public float lookAhead = 0.015f;
    public float rotationSpeed = 10f;

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
            enabled = false;
            return;
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

        if (currentRail == null) return;

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

        float targetT;

        if (currentRail.Spline.Closed)
        {
            targetT = Mathf.Repeat(t + lookAhead, 1f);
        }
        else
        {
            targetT = Mathf.Clamp01(t + lookAhead);
        }

        Vector3 railTargetPos =
            currentRail.transform.TransformPoint(
                (Vector3)currentRail.EvaluatePosition(targetT)
            );

        // レールの進行方向
        Vector3 forwardVec = railTargetPos - railPosWorld;
        forwardVec.y = 0f;

        if (forwardVec.sqrMagnitude < 0.001f)
            return;

        forwardVec.Normalize();

        // レール上の目標位置へ戻る方向
        Vector3 toRail = railPosWorld - transform.position;
        toRail.y = 0f;

        // 前後方向を除去して、横ズレだけを補正
        Vector3 lateralCorrection =
            Vector3.ProjectOnPlane(toRail, forwardVec);

        // 現在速度
        Vector3 currentVelocity = rb.linearVelocity;
        Vector3 horizontalVelocity =
            new Vector3(currentVelocity.x, 0f, currentVelocity.z);

        // レール方向への理想速度
        Vector3 desiredVelocity = forwardVec * targetSpeed;

        // レールからズレていたら、横方向にも補正を加える
        desiredVelocity += lateralCorrection * railFollowStrength;

        // 最大速度制限
        if (desiredVelocity.magnitude > maxSpeed)
        {
            desiredVelocity = desiredVelocity.normalized * maxSpeed;
        }

        // 現在速度を理想速度へ滑らかに近づける
        Vector3 newHorizontalVelocity = Vector3.Lerp(
            horizontalVelocity,
            desiredVelocity,
            Time.fixedDeltaTime * acceleration
        );

        rb.linearVelocity = new Vector3(
            newHorizontalVelocity.x,
            rb.linearVelocity.y,
            newHorizontalVelocity.z
        );

        // 向き補正
        if (newHorizontalVelocity.magnitude > 0.5f)
        {
            Quaternion targetRotation =
                Quaternion.LookRotation(newHorizontalVelocity.normalized, Vector3.up);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                Time.fixedDeltaTime * rotationSpeed
            );
        }

        Debug.DrawLine(transform.position, railPosWorld, Color.green);
        Debug.DrawRay(railPosWorld, forwardVec * 5f, Color.red);
    }
}