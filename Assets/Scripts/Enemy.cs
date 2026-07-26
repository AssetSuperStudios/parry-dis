using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    private Coroutine _bulletFiring;
    [Header("Bullet")]
    [SerializeField]
    private GameObject _bulletPrefab;
    [SerializeField] 
    float bulletInterval = 3.0f;
    [SerializeField] 
    private Transform _offset;
    [SerializeField] 
    private Text moveText;

    private int moveCount;
    private Animator enemyAnimator;
    private Transform enemyTransform;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Fetch the Enemy's Animator component
        enemyAnimator = GetComponent<Animator>();
        // Initialize move counter
        moveCount = 10;
        MoveCounter(moveCount);

        // Fetch the Enemy's Transform component
        enemyTransform = GetComponent<Transform>();

        _bulletFiring = StartCoroutine(BulletFiring());
    }

    private IEnumerator BulletFiring()
    {
        WaitForSeconds delay = new WaitForSeconds(bulletInterval);

        while (true)
        {
            if (moveCount == 0)
            {
                Debug.Log("YOU WIN");
                break;
            }

            gameObject.GetComponent<Collider2D>().enabled = true;

            // Randomize
            var randomNumber = Random.Range(0, 3);
            MoveCounter(--moveCount);
            if (randomNumber == 0) {MeleeAttack();} else {FireBullet();}
            
            yield return delay;
        }
    }

    void MoveCounter(int moveNumber)
    {
        moveText.text = $"MOVES: {moveNumber}";
    }

    async Task FireBullet()
    {
        // Start the long range animation
        enemyAnimator.SetTrigger("isLRange");
        await Awaitable.WaitForSecondsAsync(0.2f);

        Instantiate(_bulletPrefab, _offset.position, transform.rotation);
    }

    async Task MeleeAttack()
    {
        // Start the tp melee animation
        enemyAnimator.SetTrigger("isTPMelee");
        await Awaitable.WaitForSecondsAsync(0.3f);

        enemyTransform.localPosition = new Vector3(4.6f, 0f, 0f);
        
        await Awaitable.WaitForSecondsAsync(0.75f);
        enemyTransform.localPosition = new Vector3(0f, 0f, 0f);
    }

    private void DisableBulletFire()
    {
        if (_bulletFiring != null) StopCoroutine(_bulletFiring);
    }
}
