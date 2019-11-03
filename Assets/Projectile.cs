using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float damageAmount;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject collider = collision.collider.gameObject;
        if (collider.tag == "Enemy")
        {
            collider.GetComponent<Enemy>().TakeDamage(damageAmount);
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
