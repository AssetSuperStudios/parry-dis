using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public LayerMask bulletLayer;
    private Animator playerAnimator;

    [Header("Parry")]
    [SerializeField]
    public KeyCode parryKey = KeyCode.Space;
    [SerializeField]
    private float safeParryRadius = 1f;
    [SerializeField]
    private float perfectParryRadius = 1.3f;
    [SerializeField]
    private float parryRadius = 2f;
    [SerializeField]
    private float parryDelayMS = 210f;

    [Header("Hurt Blinking Settings")]
    [SerializeField] 
    private int blinkCount = 4;          // How many times the sprite flashes
    [SerializeField] 
    private float blinkIntervalMs = 100f; // Speed of each flash (e.g., 100ms on, 100ms off)

    [Header("UI Feedback")]
    [SerializeField] 
    private Text feedbackText; // Drag your Legacy Text GameObject here
    [SerializeField] 
    private float textDisplayDuration = 0.5f;

    private SpriteRenderer spriteRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Fetch the Player's Animator component
        playerAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Clear the legacy text on game startup
        if (feedbackText != null) feedbackText.text = ""; 
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(ParryDelay());
        }
    }

    IEnumerator ParryDelay()
    {
        // Start the parry animation
        playerAnimator.SetTrigger("isParry");

        yield return new WaitForSeconds(parryDelayMS/1000f); 

        TryParry();
    }

    void TryParry()
    {
        // 1. Check the smaller circle first (Perfect Parry)
        Collider2D[] safeHits = Physics2D.OverlapCircleAll(transform.position, safeParryRadius, bulletLayer);
        
        if (safeHits.Length > 0)
        {
            int safeParryCount = 0;
            foreach (Collider2D hit in safeHits)
            {
                Destroy(hit.gameObject);
                safeParryCount++;
            }
            ShowFeedbackText("SAFE PARRY!", Color.red);
            Debug.Log($"Safe Parry: {safeParryCount}");
        }

        // 1. Check the smaller circle first (Perfect Parry)
        Collider2D[] perfectHits = Physics2D.OverlapCircleAll(transform.position, perfectParryRadius, bulletLayer);
        
        if (perfectHits.Length > 0)
        {
            int perfectParryCount = 0;
            foreach (Collider2D hit in perfectHits)
            {
                if (hit != null && hit.gameObject != null)
                {
                    Destroy(hit.gameObject);
                    perfectParryCount++;
                }
            }
            ShowFeedbackText("PERFECT PARRY!", Color.yellow);
            Debug.Log($"Perfect Parry: {perfectParryCount}");
        }

        Collider2D[] normalHits = Physics2D.OverlapCircleAll(transform.position, parryRadius, bulletLayer);
        
        if (normalHits.Length > 0)
        {
            int parryCount = 0;
            foreach (Collider2D hit in normalHits)
            {
                if (hit != null && hit.gameObject != null)
                {
                    Destroy(hit.gameObject);
                    parryCount++;
                }
            }
            ShowFeedbackText("PARRY!", Color.orange);
            Debug.Log($"Parry: {parryCount}");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if ((bulletLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            ProcessFail(collision.gameObject);
        }
    }

    void ProcessFail(GameObject bullet)
    {
        Destroy(bullet);
        Debug.Log("Player hit.");

        // Start the visual blinking effect
        StartCoroutine(HurtBlinkRoutine());
    }

    IEnumerator HurtBlinkRoutine()
    {
        float delayInSeconds = blinkIntervalMs / 1000f;

        // Loop for the designated number of blinks
        for (int i = 0; i < blinkCount; i++)
        {
            // Turn the sprite off (invisible)
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(delayInSeconds);

            // Turn the sprite back on (visible)
            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(delayInSeconds);
        }

        // Safety check to ensure the sprite isn't left invisible when finished
        spriteRenderer.enabled = true; 
    }

    void ShowFeedbackText(string message, Color textColor)
    {
        if (feedbackText == null) return;

        // Interrupt any lingering text clear timers so text updates instantly
        StopCoroutine("ClearTextRoutine");
        
        feedbackText.text = message;
        feedbackText.color = textColor;

        StartCoroutine(ClearTextRoutine());
    }

    IEnumerator ClearTextRoutine()
    {
        yield return new WaitForSeconds(textDisplayDuration);
        feedbackText.text = ""; 
    }
}
