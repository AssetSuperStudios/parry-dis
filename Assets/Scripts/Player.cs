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
    [Tooltip("How long the parry box stays active checking for bullets")]
    [SerializeField] private float parryActiveDuration = 0.1f;

    [Header("Hurt Blinking Settings")]
    [SerializeField] private int blinkCount = 4;          
    [SerializeField] private float blinkIntervalMs = 100f; 

    [Header("UI Feedback")]
    [SerializeField] private Text feedbackText; 
    [SerializeField] private float textDisplayDuration = 0.5f;
    [SerializeField] private Text hpText; 
    
    [Header("Scenes")]
    [SerializeField] private SceneSwap sceneSwapper;
    
    [Header("Juice & Responsiveness")]
    [Tooltip("How early a player can press parry before the action executes safely")]
    [SerializeField] private float inputBufferTime = 0.15f; 
    private float inputBufferCounter = 0f;

    private bool isWindUp = false; 
    private bool isParrying = false; 
    private bool isInvincible = false;
    private Vector3 offsetPosition;
    private int hpCounter = 3;

    private bool registeredParryHitInWindow = false;

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
        offsetPosition = transform.position + new Vector3(0, 0.2f, 0);

        if (inputBufferCounter > 0)
        {
            inputBufferCounter -= Time.deltaTime;
        }

        if ((Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) ||
            (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
        {
            inputBufferCounter = inputBufferTime;
        }

        if (isWindUp || isParrying || (isInvincible && hpCounter <= 0)) return;

        if (inputBufferCounter > 0)
        {
            inputBufferCounter = 0f;
            StartCoroutine(ParrySequenceRoutine());
        }
    }

    IEnumerator ParrySequenceRoutine()
    {
        isWindUp = true; 
        if (playerAnimator != null) playerAnimator.SetTrigger("isParry");

        float delayInSeconds = parryDelayMS / 1000f;
        int physicsFramesToWait = Mathf.RoundToInt(delayInSeconds / Time.fixedDeltaTime);
        if (physicsFramesToWait < 1) physicsFramesToWait = 1;

        for (int i = 0; i < physicsFramesToWait; i++)
        {
            yield return new WaitForFixedUpdate();
        }

        isWindUp = false;
        isParrying = true; 
        registeredParryHitInWindow = false;

        float elapsedTime = 0f;
        while (elapsedTime < parryActiveDuration)
        {
            TickParryDetection();
            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        if (!registeredParryHitInWindow)
        {
            ShowFeedbackText("Miss!", Color.black);
            if (score != null)
            {
                score.playerScore += missScore;
                score.missCount += 1;
            }
        }

        isParrying = false;
        
        Collider2D myCol = gameObject.GetComponent<Collider2D>();
        if (myCol != null) myCol.enabled = true;
    }

        void TickParryDetection()
    {
        LayerMask combinedLayer = bulletLayer | meleeLayer;

        Collider2D[] safeHits = Physics2D.OverlapCircleAll(offsetPosition, safeParryRadius, combinedLayer);
        Collider2D[] perfectHits = Physics2D.OverlapCircleAll(offsetPosition, perfectParryRadius, combinedLayer);
        Collider2D[] greatHits = Physics2D.OverlapCircleAll(offsetPosition, parryRadius, combinedLayer);

        List<GameObject> processedBullets = new List<GameObject>();

        int safeCount = 0;
        foreach (Collider2D hit in safeHits)
        {
            if (hit == null || !hit || hit.gameObject == null || !hit.gameObject || !hit.gameObject.activeSelf) continue;

            processedBullets.Add(hit.gameObject);
            ExecuteParryHit(hit);
            safeCount++;
        }

        int perfectCount = 0;
        foreach (Collider2D hit in perfectHits)
        {
            if (hit == null || !hit || hit.gameObject == null || !hit.gameObject || !hit.gameObject.activeSelf) continue;

            if (!processedBullets.Contains(hit.gameObject))
            {
                processedBullets.Add(hit.gameObject);
                ExecuteParryHit(hit);
                perfectCount++;
            }
        }

        int greatCount = 0;
        foreach (Collider2D hit in greatHits)
        {
            if (hit == null || !hit || hit.gameObject == null || !hit.gameObject || !hit.gameObject.activeSelf) continue;

            if (!processedBullets.Contains(hit.gameObject))
            {
                processedBullets.Add(hit.gameObject);
                ExecuteParryHit(hit);
                greatCount++;
            }
        }
        
        if (safeCount > 0)
        {
            registeredParryHitInWindow = true;
            ShowFeedbackText("SAFE PARRY!", Color.red);
            if (score != null)
            {
                score.playerScore += (safeScore * safeCount);
                score.safeCount += safeCount;
            }
        }
        else if (perfectCount > 0)
        {
            registeredParryHitInWindow = true;
            ShowFeedbackText("PERFECT PARRY!", Color.yellow);
            if (score != null)
            {
                score.playerScore += (perfectScore * perfectCount);
                score.perfectCount += perfectCount;
            }
        }
        else if (greatCount > 0)
        {
            registeredParryHitInWindow = true;
            ShowFeedbackText("GOOD PARRY!", Color.orange);
            if (score != null)
            {
                score.playerScore += (greatScore * greatCount);
                score.greatCount += greatCount;
            }
        }
    }

    void ExecuteParryHit(Collider2D hit)
    {
        if ((bulletLayer.value & (1 << hit.gameObject.layer)) != 0) 
        {
            hit.gameObject.SetActive(false); 
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
                
                // CRITICAL SHUTDOWN FOR GAME OVER: Terminate collision bounds entirely
                isInvincible = true; 
                isParrying = true; 
                Collider2D playerCol = GetComponent<Collider2D>();
                if (playerCol != null) playerCol.enabled = false;
                
                GameLose();
                return; // Early exit so the blinking routine does not trigger on a dead player
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

    private void OnDrawGizmos()
    {
        Vector3 currentOffset = transform.position + new Vector3(0, 0.2f, 0);

        // Gizmos turn BLUE during the 90ms windup, and their normal colors when ACTIVE
        if (isWindUp)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentOffset, safeParryRadius);
            Gizmos.DrawWireSphere(currentOffset, perfectParryRadius);
            Gizmos.DrawWireSphere(currentOffset, parryRadius);
        }
        else if (isParrying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(currentOffset, safeParryRadius);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(currentOffset, perfectParryRadius);
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(currentOffset, parryRadius);
        }
        else
        {
            // Default gray/faded look when resting
            Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            Gizmos.DrawWireSphere(currentOffset, parryRadius);
        }
    }
}