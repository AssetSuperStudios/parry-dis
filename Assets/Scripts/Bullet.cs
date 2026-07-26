using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] public float movementSpeed = 24f;
    [SerializeField] private float lifeTime = 10.0f;
    
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();    
        
        // CRITICAL CRASH FIX: Move via velocity instead of transform.Translate
        if (rb != null)
        {
            rb.linearVelocity = transform.right * movementSpeed;
        }

        Destroy(gameObject, lifeTime);
    }
}
