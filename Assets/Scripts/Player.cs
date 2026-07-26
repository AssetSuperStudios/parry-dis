using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic; // Added for tracking processed bullets
using UnityEngine.UI;
using System.Security.Cryptography;
using UnityEditor;

public class Player : MonoBehaviour
{
    public LayerMask bulletLayer;
    public LayerMask defaultLayer;
    private Animator playerAnimator;
    private SpriteRenderer spriteRenderer;
    public Transform playerTransform;

    [Header("Parry Hierarchy Sizes")]
    [Tooltip("Smallest Circle (Inner Core)")]
    [SerializeField] private float safeParryRadius = 1.0f;
    [Tooltip("Middle Circle (Sweet Spot)")]
    [SerializeField] private float perfectParryRadius = 1.4f;
    [Tooltip("Largest Circle (Outer Edge)")]
    [SerializeField] private float parryRadius = 2.0f;
    
    [SerializeField] private float parryDelayMS = 190f;

    [Header("Hurt Blinking Settings")]
    [SerializeField] private int blinkCount = 4;          
    [SerializeField] private float blinkIntervalMs = 100f; 

    [Header("UI Feedback")]
    [SerializeField] private Text feedbackText; 
    [SerializeField] private float textDisplayDuration = 0.5f;
    [SerializeField] private Text hpText; 
    [Header("Scenes")]
    [SerializeField]
    private SceneSwap sceneSwapper;

    private bool isParrying = false;
    private bool isInvincible = false;
    private Vector3 offsetPosition;
    private int hpCounter = 3;

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (feedbackText != null) feedbackText.text = ""; 

        if (playerTransform == null) playerTransform = this.transform;

        offsetPosition = transform.position + new Vector3(0, 0.2f, 0);
    }

    void Update()
    {
        if (isParrying) return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(ParryDelay());
        }
    }

    IEnumerator ParryDelay()
    {
        isParrying = true;
        playerAnimator.SetTrigger("isParry");

        float delayInSeconds = parryDelayMS / 1000f;
        int physicsFramesToWait = Mathf.RoundToInt(delayInSeconds / Time.fixedDeltaTime);

        for (int i = 0; i < physicsFramesToWait; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        TryParry();

        yield return new WaitForSeconds(0.1f);
        isParrying = false;
        gameObject.GetComponent<Collider2D>().enabled = true;
    }

    void TryParry()
    {
        LayerMask combinedLayer = bulletLayer | defaultLayer;

        // Gather bullets in all radiuses
        Collider2D[] safeHits = Physics2D.OverlapCircleAll(offsetPosition, safeParryRadius, combinedLayer);
        Collider2D[] perfectHits = Physics2D.OverlapCircleAll(offsetPosition, perfectParryRadius, combinedLayer);
        Collider2D[] normalHits = Physics2D.OverlapCircleAll(offsetPosition, parryRadius, combinedLayer);

        // This list tracks bullets we already destroyed so outer loops don't double-count them
        List<GameObject> processedBullets = new List<GameObject>();

        // 1. FIRST PRIORITY: SAFE PARRY (Smallest Circle)
        int safeCount = 0;
        foreach (Collider2D hit in safeHits)
        {
            if (hit != null && hit.gameObject != null)
            {
                processedBullets.Add(hit.gameObject);

                if ((bulletLayer.value & (1 << hit.gameObject.layer)) != 0) {Destroy(hit.gameObject);}
                else if ((defaultLayer.value & (1 << hit.gameObject.layer)) != 0) {hit.enabled = false;}

                safeCount++;
            }
        }

        // 2. SECOND PRIORITY: PERFECT PARRY (Middle Circle)
        int perfectCount = 0;
        foreach (Collider2D hit in perfectHits)
        {
            if (hit != null && hit.gameObject != null && !processedBullets.Contains(hit.gameObject))
            {
                processedBullets.Add(hit.gameObject);

                if ((bulletLayer.value & (1 << hit.gameObject.layer)) != 0) {Destroy(hit.gameObject);}
                else if ((defaultLayer.value & (1 << hit.gameObject.layer)) != 0) {hit.enabled = false;}

                perfectCount++;
            }
        }

        // 3. THIRD PRIORITY: GOOD/NORMAL PARRY (Largest Circle)
        int normalCount = 0;
        foreach (Collider2D hit in normalHits)
        {
            if (hit != null && hit.gameObject != null && !processedBullets.Contains(hit.gameObject))
            {
                processedBullets.Add(hit.gameObject);

                if ((bulletLayer.value & (1 << hit.gameObject.layer)) != 0) {Destroy(hit.gameObject);}
                else if ((defaultLayer.value & (1 << hit.gameObject.layer)) != 0) {hit.enabled = false;}

                normalCount++;
            }
        }

        // --- DISPLAY UI FEEDBACK BASED ON HIGHEST TIER TRIGGERED ---
        if (safeCount > 0)
        {
            ShowFeedbackText("SAFE PARRY!", Color.red);
            Debug.Log($"Safe Parry: {safeCount}");
        }
        else if (perfectCount > 0)
        {
            ShowFeedbackText("PERFECT PARRY!", Color.yellow);
            Debug.Log($"Perfect Parry: {perfectCount}");
        }
        else if (normalCount > 0)
        {
            ShowFeedbackText("GOOD PARRY!", Color.orange);
            Debug.Log($"Good Parry: {normalCount}");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isInvincible) return;

        if ((bulletLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            ProcessFail(collision.gameObject, true);
        }

        if ((defaultLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            ProcessFail(collision.gameObject, false);
        }
    }

    void ProcessFail(GameObject bullet, bool isBullet)
    {
        if (isBullet)
        {
            Destroy(bullet);
        } else
        {
            bullet.GetComponent<Collider2D>().enabled = false;
        }
        Debug.Log("Player hit.");
        switch (hpCounter)
        {
            case 3:
                hpCounter--;
                hpText.text = "❤️❤️";
                break;
            case 2:
                hpCounter--;
                hpText.text = "❤️";
                break;
            case 1:
                hpCounter--;
                hpText.text = "";
                GameLose();
                break;
        }
        StartCoroutine(HurtBlinkRoutine());
    }

    void GameLose()
    {
        ShowFeedbackText("YOU LOST", Color.black);
        sceneSwapper.SceneSwapper("Lose Scene");
    }

    IEnumerator HurtBlinkRoutine()
    {
        isInvincible = true;
        float delayInSeconds = blinkIntervalMs / 1000f;

        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.enabled = false;
            yield return new WaitForSeconds(delayInSeconds);

            spriteRenderer.enabled = true;
            yield return new WaitForSeconds(delayInSeconds);
        }

        spriteRenderer.enabled = true; 
        isInvincible = false;
    }

    void ShowFeedbackText(string message, Color textColor)
    {
        if (feedbackText == null) return;

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

    private void OnDrawGizmosSelected()
    {       
        // Red is inner core (Safe)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(offsetPosition, safeParryRadius);
        
        // Yellow is middle sweet spot (Perfect)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(offsetPosition, perfectParryRadius);
        
        // Orange is outer safety rim (Good)
        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange
        Gizmos.DrawWireSphere(offsetPosition, parryRadius);
    }
}