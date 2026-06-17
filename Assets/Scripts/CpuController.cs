using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class CpuController : MonoBehaviour
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

    [Header("CPUレーン変更")]
    [Tooltip("レーン変更を考える最短時間")]
    public float minLaneDecisionTime = 1.0f;

    [Tooltip("レーン変更を考える最長時間")]
    public float maxLaneDecisionTime = 3.0f;

    [Tooltip("実際にレーン変更する確率")]
    [Range(0f, 1f)]
    public float laneChangeChance = 0.45f;

    [Tooltip("レーン変更後のクールダウン")]
    public float laneChangeCooldown = 0.5f;

    [Header("Spline追従")]
    public float lookAhead = 0.02f;
    public float rotationSpeed = 8f;

    [Header("スタミナギア")]
    private HorseStaminaGearSystem staminaGear;

    private Rigidbody rb;

    private float laneChangeTimer;
    private float laneDecisionTimer;

    private float baseForwardForce;
    private float baseMaxSpeed;

    void Start()
    {
        staminaGear = GetComponent<HorseStaminaGearSystem>();
        rb = GetComponent<Rigidbody>();

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        if (laneRails == null || laneRails.Length == 0)
        {
            Debug.LogError("laneRailsにSplineContainerを登録してください。");
            return;
        }

        currentRailIndex = Mathf.Clamp(
            currentRailIndex,
            0,
            laneRails.Length - 1
        );

        ResetLaneDecisionTimer();
    }

    void Update()
    {
        HandleCpuLaneDecision();
    }

    void FixedUpdate()
    {
        if (laneRails == null || laneRails.Length == 0) return;

        MoveAlongCurrentRail();
    }

    void HandleCpuLaneDecision()
    {
        laneChangeTimer -= Time.deltaTime;
        laneDecisionTimer -= Time.deltaTime;

        if (laneDecisionTimer > 0f) return;

        ResetLaneDecisionTimer();

        if (laneChangeTimer > 0f) return;
        if (laneRails == null || laneRails.Length <= 1) return;

        // 確率でレーン変更する
        if (Random.value > laneChangeChance)
        {
            return;
        }

        int direction = ChooseLaneChangeDirection();

        if (direction == 0)
        {
            return;
        }

        currentRailIndex += direction;

        currentRailIndex = Mathf.Clamp(
            currentRailIndex,
            0,
            laneRails.Length - 1
        );

        laneChangeTimer = laneChangeCooldown;
    }

    int ChooseLaneChangeDirection()
    {
        // 一番内側なら外側にしか行けない
        if (currentRailIndex <= 0)
        {
            return 1;
        }

        // 一番外側なら内側にしか行けない
        if (currentRailIndex >= laneRails.Length - 1)
        {
            return -1;
        }

        // 中間レーンならランダムで内側 or 外側
        return Random.value < 0.5f ? -1 : 1;
    }

    void ResetLaneDecisionTimer()
    {
        laneDecisionTimer = Random.Range(minLaneDecisionTime, maxLaneDecisionTime);
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

        // Splineと逆方向に進む
        float targetT = Mathf.Repeat(t - lookAhead, 1f);

        Vector3 railTargetPos =
            currentRail.transform.TransformPoint(
                (Vector3)currentRail.Spline.EvaluatePosition(targetT)
            );

        Vector3 forwardVec =
            (railTargetPos - railPosWorld).normalized;

        if (forwardVec.sqrMagnitude < 0.001f)
            return;

        // 前進力
        float useForwardForce = forwardForce;

        if (staminaGear != null)
        {
            useForwardForce = staminaGear.CurrentForwardForce;
        }

        rb.AddForce(forwardVec * useForwardForce, ForceMode.Force);

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
        float useMaxSpeed = maxSpeed;

        if (staminaGear != null)
        {
            useMaxSpeed = staminaGear.CurrentMaxSpeed;
        }

        if (rb.linearVelocity.magnitude > useMaxSpeed)
        {
            rb.linearVelocity =
                rb.linearVelocity.normalized * useMaxSpeed;
        }

        Debug.DrawLine(transform.position, railTargetPos, Color.green);
        Debug.DrawRay(railPosWorld, forwardVec * 5f, Color.red);
    }
}