using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHealth;
    private float currentHealth;

    public GameObject onDeathEffect;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(currentHealth < 0)
        {
            Die();
        }
    }

    public void Die()
    {
        Instantiate(onDeathEffect, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
    }
}
