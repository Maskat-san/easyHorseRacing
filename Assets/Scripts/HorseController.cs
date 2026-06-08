using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class HorseController : MonoBehaviour
{
    [Header("ペース設定（5段階）")]
    [Tooltip("各ペースの速度を設定します。例: 10, 12, 15, 18, 20")]
    public float[] paceSpeeds = new float[5];
    [Tooltip("各ペースで1秒間に消費するスタミナ量")]
    public float[] paceStaminaConsumption = new float[5];

    [Header("スタミナ＆ペナルティ")]
    public float maxStamina = 2000f;
    public float slowSpeed = 5f;
    [Header("鞭（ブースト）設定")]
    public float boostSpeedMultiplier = 1.5f;
    public float boostDuration = 1.0f; 
    public float whipStaminaCost = 50f;
    private Rigidbody rb;
    private bool isBoosting = false;
    private int currentPace = 0;

    [HideInInspector]
    public float currentStamina;
    
    public int GetCurrentPace() => currentPace;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        currentStamina = maxStamina;
    }

    public void OnPaceUp()
    {
        currentPace = Mathf.Min(currentPace + 1, 5);
    }

    public void OnPaceDown()
    {
        currentPace = Mathf.Max(currentPace - 1, 0);
    }
    

    public void OnWhip(InputValue value)
    {
        if (value.isPressed && !isBoosting && currentStamina >= whipStaminaCost)
        {
            StartCoroutine(BoostCoroutine());
        }
    }

    private IEnumerator BoostCoroutine()
    {
        currentStamina -= whipStaminaCost;
        isBoosting = true;
        yield return new WaitForSeconds(boostDuration);
        isBoosting = false;
    }

    void Update()
    {
        if (currentPace > 0 && !isBoosting)
        {
            currentStamina -= paceStaminaConsumption[currentPace - 1] * Time.deltaTime;
        }
        currentStamina = Mathf.Max(0, currentStamina);
    }

void FixedUpdate()
{
    float currentTargetSpeed;

    if (currentPace == 0)
        {
            currentTargetSpeed = 0;
        }

    else if (currentStamina <= 0)
        {
            currentTargetSpeed = slowSpeed;
        }

    else
        {
            currentTargetSpeed = paceSpeeds[currentPace - 1];
        }

    if (isBoosting)
        {
            currentTargetSpeed *= boostSpeedMultiplier;
        }

    Vector3 forwardVelocity = transform.forward * currentTargetSpeed;
    rb.linearVelocity = new Vector3(forwardVelocity.x, rb.linearVelocity.y, forwardVelocity.z);
    }
}