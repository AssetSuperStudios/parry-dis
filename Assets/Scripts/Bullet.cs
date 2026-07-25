using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] 
    public float movementSpeed = 24f;
    private Rigidbody2D rb;
    [SerializeField] 
    float lifeTime = 10.0f;


    void Start()
    {
        rb = GetComponent<Rigidbody2D>();    
        Destroy(gameObject, lifeTime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.right * movementSpeed * Time.deltaTime);
    }
}
