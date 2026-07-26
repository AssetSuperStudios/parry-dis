using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Bullet")]
    [SerializeField] private GameObject _bulletPrefab;
    [SerializeField] private float bulletInterval = 3.0f;
    [SerializeField] private Transform _offset;
    [SerializeField] private TMP_Text moveText;

    [Header("Scenes")]
    [SerializeField] private SceneSwap sceneSwapper;

    private int moveCount;
    private Animator enemyAnimator;
    private Transform enemyTransform;
    private Collider2D enemyCollider;
    
    // Cancellation token to safely stop the loop when the GameObject is destroyed
    private CancellationTokenSource _loopCancellationTokenSource;

    void Start()
    {
        enemyAnimator = GetComponent<Animator>();
        enemyTransform = GetComponent<Transform>();
        enemyCollider = GetComponent<Collider2D>();

        moveCount = 10;
        MoveCounter(moveCount);

        // Start the loop using Async/Await
        _loopCancellationTokenSource = new CancellationTokenSource();
        _ = RunAttackLoop(_loopCancellationTokenSource.Token);
    }

    private async Task RunAttackLoop(CancellationToken token)
    {
        try
        {
            await Awaitable.WaitForSecondsAsync(bulletInterval * 2, token);

            while (!token.IsCancellationRequested)
            {
                if (moveCount <= 0)
                {
                    Debug.Log("YOU WIN");
                    if (sceneSwapper != null) sceneSwapper.SceneSwapper("Win Scene");
                    break;
                }

                if (enemyCollider != null) enemyCollider.enabled = true;

                int randomNumber = Random.Range(0, 3);
                moveCount--;
                MoveCounter(moveCount);

                if (randomNumber == 0) 
                {
                    await MeleeAttack(token);
                } 
                else 
                {
                    await FireBullet(token);
                }
                
                await Awaitable.WaitForSecondsAsync(bulletInterval, token);
            }
        }
        catch (System.OperationCanceledException)
        {
            // Clean exit when task is cancelled or object is destroyed
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

        if (_bulletPrefab != null && _offset != null)
        {
            Instantiate(_bulletPrefab, _offset.position, transform.rotation);
        }
    }

    async Task MeleeAttack(CancellationToken token)
    {
        if (enemyAnimator != null) enemyAnimator.SetTrigger("isTPMelee");
        
        await Awaitable.WaitForSecondsAsync(0.3f, token);

        if (enemyTransform != null) enemyTransform.localPosition = new Vector3(4.6f, 0f, 0f);
        
        await Awaitable.WaitForSecondsAsync(0.75f, token);
        
        if (enemyTransform != null) enemyTransform.localPosition = new Vector3(0f, 0f, 0f);
    }

    private void OnDestroy()
    {
        // Safety feature: stops the async loop instantly if the Enemy dies/is destroyed
        if (_loopCancellationTokenSource != null)
        {
            _loopCancellationTokenSource.Cancel();
            _loopCancellationTokenSource.Dispose();
        }
    }
}