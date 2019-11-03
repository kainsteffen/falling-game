using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject onDeathParticle;
    public float lifeTime;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        lifeTime -= Time.deltaTime;
        if(lifeTime < 0)
        {
            Destroy(gameObject);
        }

        if(collision.gameObject.tag != "Projectile")
        {
            Instantiate(onDeathParticle, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}
