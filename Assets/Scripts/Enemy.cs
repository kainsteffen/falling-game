using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float maxHealth;
    private float currentHealth;

    public Transform target;
    public float shootingCooldown;
    public float shootingTimer;
    public float shootForce;

    public GameObject projectile;
    public GameObject onDeathEffect;
    // Start is called before the first frame update
    void Start()
    {
        shootingTimer = shootingCooldown;
    }

    // Update is called once per frame
    void Update()
    {
        if(target && shootingTimer < 0)
        {
            Vector3 direction = target.position - transform.position;
            Shoot(direction);
        } else
        {
            shootingTimer -= Time.deltaTime;
        }

        if(currentHealth < 0)
        {
            Die();
        }
    }

    public void Shoot(Vector3 direction)
    {
        GameObject newProjectile = Instantiate(projectile, transform.position, transform.rotation);
        newProjectile.GetComponent<Rigidbody2D>().AddForce(direction * shootForce);
        shootingTimer = shootingCooldown;
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
