using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControls : MonoBehaviour
{
    public Joystick joystick;
    public ParticleSystem dashParticle;
    // Start is called before the first frame update
    void Awake()
    {
        joystick = GetComponent<Joystick>();
    }

    private void OnEnable()
    {
        joystick.QuickSwipe += Dash;
    }

    // Update is called once per frameWhy 
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, joystick.inputAngle, 0);
        if(joystick.inputDistance > 0)
        {
            transform.Translate(Vector3.forward * (joystick.inputDistance / 50) *  Time.deltaTime);
        }
    }

    void Dash(Vector3 direction)
    {
        transform.rotation = Quaternion.LookRotation(direction);
        GetComponent<Rigidbody>().AddForce(transform.forward * 2000);
        dashParticle.Play();
    }
}
