using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public LayerMask bulletLayer;
    public LayerMask meleeLayer;
    private Animator playerAnimator;
    private SpriteRenderer spriteRenderer;
    public Transform playerTransform;

    [Header("Parry Hierarchy Sizes")]
    [Tooltip("Smallest Circle (Inner Core)")]
    [SerializeField] private float safeParryRadius = 0.7f;
    [Tooltip("Middle Circle (Sweet Spot)")]
    [SerializeField] private float perfectParryRadius = 1.4f;
    [Tooltip("Largest Circle (Outer Edge)")]
    [SerializeField] private float parryRadius = 1.9f;
    
    [SerializeField] private float parryDelayMS = 90f;

    [Header("Hurt Blinking Settings")]
    [SerializeField] private int blinkCount = 4;          
    [SerializeField] private float blinkIntervalMs = 100f; 

    [Header("UI Feedback")]
    [SerializeField] private Text feedbackText; 
    [SerializeField] private float textDisplayDuration = 0.5f;
    [SerializeField] private Text hpText; 
    
    [Header("Scenes")]
    [SerializeField] private SceneSwap sceneSwapper;

    private bool isParrying = false;
    private bool isInvincible = false;
    private Vector3 offsetPosition;
    private int hpCounter = 3;

    [Header("Scoring")]
    [SerializeField] private Score score;
    private int perfectScore = 100;
    private int greatScore = 50;
    private int safeScore = 10;
    private int failScore = 0;
    private int missScore = -50; 

    void Start()
    {
        playerAnimator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (feedbackText != null) feedbackText.text = ""; 

        if (playerTransform == null) playerTransform = this.transform;

        offsetPosition = transform.position + new Vector3(0, 0.2f, 0);

        if (score != null) score.playerScore = 0;
    }

    void Update()
    {
        // Safety guard: Stop inputs if parrying or if the player is dead/handling a game over screen
        if (isParrying || (isInvincible && hpCounter <= 0)) return;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(ParryDelay());
        }
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartCoroutine(ParryDelay());
        }
    }

    IEnumerator ParryDelay()
    {
        isParrying = true;
        if (playerAnimator != null) playerAnimator.SetTrigger("isParry");

        float delayInSeconds = parryDelayMS / 1000f;
        int physicsFramesToWait = Mathf.RoundToInt(delayInSeconds / Time.fixedDeltaTime);

        if (physicsFramesToWait < 1) physicsFramesToWait = 1;

        for (int i = 0; i < physicsFramesToWait; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        TryParry();

        yield return new WaitForSeconds(0.1f);
        isParrying = false;
        
        Collider2D myCol = gameObject.GetComponent<Collider2D>();
        if (myCol != null) myCol.enabled = true;
    }

    void TryParry()
    {
        LayerMask combinedLayer = bulletLayer | meleeLayer;

        Collider2D[] safeHits = Physics2D.OverlapCircleAll(offsetPosition, safeParryRadius, combinedLayer);
        Collider2D[] perfectHits = Physics2D.OverlapCircleAll(offsetPosition, perfectParryRadius, combinedLayer);
        Collider2D[] greatHits = Physics2D.OverlapCircleAll(offsetPosition, parryRadius, combinedLayer);

        List<GameObject> processedBullets = new List<GameObject>();

        // 1. FIRST PRIORITY: SAFE PARRY
        int safeCount = 0;
        foreach (Collider2D hit in safeHits)
        {
            if (hit != null && hit.gameObject != null)
            {
                processedBullets.Add(hit.gameObject);
                ExecuteParryHit(hit);
                safeCount++;
            }
        }

        // 2. SECOND PRIORITY: PERFECT PARRY
        int perfectCount = 0;
        foreach (Collider2D hit in perfectHits)
        {
            if (hit != null && hit.gameObject != null && !processedBullets.Contains(hit.gameObject))
            {
                processedBullets.Add(hit.gameObject);
                ExecuteParryHit(hit);
                perfectCount++;
            }
        }

        // 3. THIRD PRIORITY: GOOD/NORMAL PARRY
        int greatCount = 0;
        foreach (Collider2D hit in greatHits)
        {
            if (hit != null && hit.gameObject != null && !processedBullets.Contains(hit.gameObject))
            {
                processedBullets.Add(hit.gameObject);
                ExecuteParryHit(hit);
                greatCount++;
            }
        }

        // --- DISPLAY UI FEEDBACK AND SAFE SCORE PROCESSING ---
        if (safeCount > 0)
        {
            ShowFeedbackText("SAFE PARRY!", Color.red);
            Debug.Log($"Safe Parry: {safeCount}");
            if (score != null)
            {
                score.playerScore += (safeScore * safeCount);
                score.safeCount += safeCount;
            }
        }
        else if (perfectCount > 0)
        {
            ShowFeedbackText("PERFECT PARRY!", Color.yellow);
            Debug.Log($"Perfect Parry: {perfectCount}");
            if (score != null)
            {
                score.playerScore += (perfectScore * perfectCount);
                score.perfectCount += perfectCount;
            }
        }
        else if (greatCount > 0)
        {
            ShowFeedbackText("GOOD PARRY!", Color.orange);
            Debug.Log($"Good Parry: {greatCount}");
            if (score != null)
            {
                score.playerScore += (greatScore * greatCount);
                score.greatCount += greatCount;
            }
        }

        if (safeCount == 0 && perfectCount == 0 && greatCount == 0)
        {
            ShowFeedbackText("Miss!", Color.black);
            Debug.Log("Miss");
            if (score != null)
            {
                score.playerScore += missScore;
                score.missCount += 1;
            }
        }
    }

    void ExecuteParryHit(Collider2D hit)
    {
        if ((bulletLayer.value & (1 << hit.gameObject.layer)) != 0) 
        {
            Destroy(hit.gameObject);
        }
        else if ((meleeLayer.value & (1 << hit.gameObject.layer)) != 0) 
        {
            hit.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isInvincible || isParrying) return;

        if ((bulletLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            ProcessFail(collision.gameObject, true);
        }
        else if ((meleeLayer.value & (1 << collision.gameObject.layer)) != 0)
        {
            ProcessFail(collision.gameObject, false);
        }
    }

    void ProcessFail(GameObject bullet, bool isBullet)
    {
        if (isBullet)
        {
            if (bullet != null) Destroy(bullet);
        } 
        else
        {
            if (bullet != null)
            {
                Collider2D col = bullet.GetComponent<Collider2D>();
                if (col != null) col.enabled = false;
            }
        }
        
        Debug.Log("Player hit.");
        hpCounter--;
        
        switch (hpCounter)
        {
            case 2:
                if (hpText != null) hpText.text = "❤️❤️";
                break;
            case 1:
                if (hpText != null) hpText.text = "❤️";
                break;
            case 0:
                if (hpText != null) hpText.text = "";
                
                // CRITICAL CRASH PROTECTION: Lock player states and clear collision bounds
                isInvincible = true; 
                isParrying = true; 
                Collider2D playerCol = GetComponent<Collider2D>();
                if (playerCol != null) playerCol.enabled = false;
                
                GameLose();
                return; // End execution early so the HurtBlinkRoutine doesn't fire on a dead object
        }

        if (score != null)
        {
            score.playerScore += failScore;
            score.failCount += 1;
        }

        if (hpCounter > 0)
        {
            StartCoroutine(HurtBlinkRoutine());
        }
    }

    void GameLose()
    {
        ShowFeedbackText("YOU LOST", Color.black);
        if (sceneSwapper != null)
        {
            sceneSwapper.SceneSwapper("Lose Scene");
        }
    }

    void ShowFeedbackText(string message, Color color)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
            feedbackText.color = color;
            StopCoroutine(ClearFeedbackText());
            StartCoroutine(ClearFeedbackText());
        }
    }

    IEnumerator ClearFeedbackText()
    {
        yield return new WaitForSeconds(textDisplayDuration);
        if (feedbackText != null) feedbackText.text = "";
    }

    // FIXED: Formatted the missing brackets and component protections
    IEnumerator HurtBlinkRoutine()
    {
        isInvincible = true;
        float delayInSeconds = blinkIntervalMs / 1000f;

        for (int i = 0; i < blinkCount; i++)
        {
            if (spriteRenderer != null) spriteRenderer.enabled = false;
            yield return new WaitForSeconds(delayInSeconds);

            if (spriteRenderer != null) spriteRenderer.enabled = true;
            yield return new WaitForSeconds(delayInSeconds);
        }

        if (spriteRenderer != null) spriteRenderer.enabled = true;
        isInvincible = false;
    }
}