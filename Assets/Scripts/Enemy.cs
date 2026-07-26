using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Bullet Configurations")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float bulletInterval = 3.0f;
    [Tooltip("The speed assigned to bullets fired during normal phases")]
    [SerializeField] private float normalBulletSpeed = 24f;
    [Tooltip("The speed assigned to bullets fired during rage mode")]
    [SerializeField] private float rageBulletSpeed = 32f;
    [SerializeField] private Transform _offset;
    [SerializeField] private TMP_Text moveText;

    [Header("Scenes")]
    [SerializeField] private SceneSwap sceneSwapper;
    [Header("Rage Mode")]
    [SerializeField] private SpriteRenderer rageAnimation;

    private int moveCount;
    private int rageTime;
    private Animator enemyAnimator;
    private Transform enemyTransform;
    private Collider2D enemyCollider;
    private AudioSource[] audioSources;
    
    // TRACKER: Ensures bulletInterval division executes exactly once
    private bool hasTriggeredRage = false; 

    // Safety token architecture to clear threads on object destruction
    private CancellationTokenSource _loopCancellationTokenSource;

    void Start()
    {
        enemyAnimator = GetComponent<Animator>();
        enemyTransform = GetComponent<Transform>();
        enemyCollider = GetComponent<Collider2D>();
        audioSources = GetComponents<AudioSource>();

        moveCount = 15;
        rageTime = 5; // Fixed naming tracking bug
        MoveCounter(moveCount);

        // Start the loop using clean Async/Await pipeline architecture
        _loopCancellationTokenSource = new CancellationTokenSource();
        _ = RunAttackLoop(_loopCancellationTokenSource.Token);
    }

    private async Task RunAttackLoop(CancellationToken token)
    {
        try
        {
            // Initial safety setup delay
            await Awaitable.WaitForSecondsAsync(bulletInterval * 2, token);

            while (!token.IsCancellationRequested)
            {
                // CRITICAL TIMING GUARD: Step out if screen shifts or manager component drops
                if (sceneSwapper == null) break;

                if (moveCount <= 0)
                {
                    Debug.Log("YOU WIN");
                    sceneSwapper.SceneSwapper("Win Scene");
                    break;
                }

                // FIXED: Flags tracker so division calculations do not cycle infinitely
                if (moveCount <= rageTime && !hasTriggeredRage)
                {
                    hasTriggeredRage = true;
                    bulletInterval /= 3f;
                }

                if (enemyCollider != null) enemyCollider.enabled = true;

                int randomNumber = Random.Range(0, 3);
                moveCount--;
                if (moveCount <= rageTime && !hasTriggeredRage) {
                    rageAnimation.enabled = true;
                    rageAnimation.GetComponent<AudioSource>().Play();
                }
                MoveCounter(moveCount);

                if (token.IsCancellationRequested) break;

                if (randomNumber == 0) 
                {
                    audioSources[1].Play();
                    audioSources[2].PlayDelayed(0.3f);
                    await MeleeAttack(token);
                } 
                else 
                {
                    audioSources[0].Play();
                    await FireBullet(token);
                }
                
                await Awaitable.WaitForSecondsAsync(bulletInterval, token);
            }
        }
        catch (System.OperationCanceledException)
        {
            // Clean runtime thread extraction
        }
    }

    void MoveCounter(int moveNumber)
    {
        if (moveText != null)
        {
            moveText.text = $"MOVES: {moveNumber}";
        }
    }

    async Task FireBullet(CancellationToken token)
    {
        if (enemyAnimator != null) enemyAnimator.SetTrigger("isLRange");
        
        await Awaitable.WaitForSecondsAsync(0.2f, token);

        if (token.IsCancellationRequested || sceneSwapper == null) return;

        if (_bulletPrefab != null && _offset != null)
        {
            GameObject bulletObj = Instantiate(_bulletPrefab, _offset.position, transform.rotation);
            
            Bullet bulletComponent = bulletObj.GetComponent<Bullet>();
            if (bulletComponent != null)
            {
                // Dynamically fetch and pass the calculated structural tracking speeds
                float selectedSpeed = hasTriggeredRage ? rageBulletSpeed : normalBulletSpeed;
                bulletComponent.SetBulletSpeed(selectedSpeed);
            }
        }
    }

    async Task MeleeAttack(CancellationToken token)
    {
        if (enemyAnimator != null) enemyAnimator.SetTrigger("isTPMelee");
        
        await Awaitable.WaitForSecondsAsync(0.3f, token);
        if (token.IsCancellationRequested || sceneSwapper == null) return;

        if (enemyTransform != null) enemyTransform.localPosition = new Vector3(4.6f, 0f, 0f);
        
        await Awaitable.WaitForSecondsAsync(0.75f, token);
        if (token.IsCancellationRequested || sceneSwapper == null) return;
        
        if (enemyTransform != null) enemyTransform.localPosition = new Vector3(0f, 0f, 0f);
    }

    private void OnDestroy()
    {
        if (_loopCancellationTokenSource != null)
        {
            _loopCancellationTokenSource.Cancel();
            _loopCancellationTokenSource.Dispose();
        }
    }
}