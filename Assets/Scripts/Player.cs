using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public LayerMask bulletLayer;
    public KeyCode parryKey = KeyCode.Space;

    [Header("Parry Radius")]
    [SerializeField]
    private float perfectParryRadius = 1.5f;
    [SerializeField]
    private float parryRadius = 2.1f;
    private Animator playerAnimator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Fetch the Player's Animator component
        playerAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryParry();
        }
    }

    void TryParry()
    {
        // Start the parry animation
        playerAnimator.SetTrigger("isParry");

        // 1. Check the smaller circle first (Perfect Parry)
        Collider2D[] perfectHits = Physics2D.OverlapCircleAll(transform.position, perfectParryRadius, bulletLayer);
        
        if (perfectHits.Length > 0)
        {
            int perfectParryCount = 0;
            foreach (Collider2D hit in perfectHits)
            {
                Destroy(hit.gameObject);
                perfectParryCount++;
            }
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
            Debug.Log($"Parry: {parryCount}");
        }
    }
    
}
