using UnityEngine;

public class HorseStaminaGearSystem : MonoBehaviour
{
    [System.Serializable]
    public class GearSetting
    {
        [Header("ギア設定")]
        public float forwardForce = 1000f;
        public float maxSpeed = 10f;

        [Header("スタミナ消費")]
        public float staminaDrainPerSecond = 5f;
    }

    [Header("ギア設定 1〜6")]
    [Tooltip("0番がギア1、5番がギア6です")]
    public GearSetting[] gearSettings = new GearSetting[6];

    [Header("現在のギア")]
    [Range(1, 6)]
    public int currentGear = 1;

    [Header("スタミナ")]
    public float maxStamina = 100f;
    public float currentStamina = 100f;

    [Header("スタミナ切れ時のギアダウン")]
    [Tooltip("スタミナ切れ中、何秒ごとにギアを下げるか")]
    public float exhaustedGearDownInterval = 1.5f;

    [Header("プレイヤー入力")]
    public bool usePlayerInput = true;
    public KeyCode gearUpKey = KeyCode.W;
    public KeyCode gearDownKey = KeyCode.S;

    public bool IsExhausted { get; private set; }

    private float exhaustedGearDownTimer;

    public float CurrentForwardForce
    {
        get
        {
            return gearSettings[currentGear - 1].forwardForce;
        }
    }

    public float CurrentMaxSpeed
    {
        get
        {
            return gearSettings[currentGear - 1].maxSpeed;
        }
    }

    void Reset()
    {
        SetupDefaultGears();
    }

    void OnValidate()
    {
        if (gearSettings == null || gearSettings.Length != 6)
        {
            GearSetting[] newSettings = new GearSetting[6];

            for (int i = 0; i < newSettings.Length; i++)
            {
                if (gearSettings != null && i < gearSettings.Length && gearSettings[i] != null)
                {
                    newSettings[i] = gearSettings[i];
                }
                else
                {
                    newSettings[i] = new GearSetting();
                }
            }

            gearSettings = newSettings;
        }

        currentGear = Mathf.Clamp(currentGear, 1, 6);
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    void Start()
    {
        if (gearSettings == null || gearSettings.Length != 6)
        {
            SetupDefaultGears();
        }

        for (int i = 0; i < gearSettings.Length; i++)
        {
            if (gearSettings[i] == null)
            {
                gearSettings[i] = new GearSetting();
            }
        }

        currentGear = Mathf.Clamp(currentGear, 1, 6);
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
    }

    void Update()
    {
        if (usePlayerInput)
        {
            HandleGearInput();
        }

        UpdateStamina();
        UpdateExhaustedGearDown();
    }

    void HandleGearInput()
    {
        if (Input.GetKeyDown(gearUpKey))
        {
            GearUp();
        }

        if (Input.GetKeyDown(gearDownKey))
        {
            GearDown();
        }
    }

    void UpdateStamina()
    {
        GearSetting gear = gearSettings[currentGear - 1];

        currentStamina -= gear.staminaDrainPerSecond * Time.deltaTime;
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);

        if (currentStamina <= 0f)
        {
            IsExhausted = true;
        }
    }

    void UpdateExhaustedGearDown()
    {
        if (!IsExhausted) return;

        exhaustedGearDownTimer += Time.deltaTime;

        if (exhaustedGearDownTimer >= exhaustedGearDownInterval)
        {
            exhaustedGearDownTimer = 0f;

            if (currentGear > 1)
            {
                GearDown();
            }
        }
    }

    public void GearUp()
    {
        if (IsExhausted)
        {
            return;
        }

        currentGear++;
        currentGear = Mathf.Clamp(currentGear, 1, 6);
    }

    public void GearDown()
    {
        currentGear--;
        currentGear = Mathf.Clamp(currentGear, 1, 6);
    }

    public void SetGear(int gear)
    {
        if (IsExhausted && gear > currentGear)
        {
            return;
        }

        currentGear = Mathf.Clamp(gear, 1, 6);
    }

    public float GetStaminaRate()
    {
        if (maxStamina <= 0f) return 0f;

        return currentStamina / maxStamina;
    }

    void SetupDefaultGears()
    {
        gearSettings = new GearSetting[6];

        for (int i = 0; i < gearSettings.Length; i++)
        {
            gearSettings[i] = new GearSetting();
        }

        gearSettings[0].forwardForce = 1200f;
        gearSettings[0].maxSpeed = 10f;
        gearSettings[0].staminaDrainPerSecond = 0.5f;

        gearSettings[1].forwardForce = 1500f;
        gearSettings[1].maxSpeed = 13f;
        gearSettings[1].staminaDrainPerSecond = 1.5f;

        gearSettings[2].forwardForce = 1800f;
        gearSettings[2].maxSpeed = 16f;
        gearSettings[2].staminaDrainPerSecond = 4f;

        gearSettings[3].forwardForce = 2200f;
        gearSettings[3].maxSpeed = 20f;
        gearSettings[3].staminaDrainPerSecond = 8f;

        gearSettings[4].forwardForce = 2600f;
        gearSettings[4].maxSpeed = 24f;
        gearSettings[4].staminaDrainPerSecond = 14f;

        gearSettings[5].forwardForce = 3200f;
        gearSettings[5].maxSpeed = 29f;
        gearSettings[5].staminaDrainPerSecond = 22f;
    }
}