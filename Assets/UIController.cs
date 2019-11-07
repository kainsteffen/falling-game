using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public Player player;
    public Text healthCounter;
    public Text ammoCounter;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        healthCounter.text = player.currentHealth.ToString() + " / " + player.maxHealth.ToString();
        ammoCounter.text = player.currentAmmo.ToString() + " / " + player.maxAmmo.ToString();
    }
}
