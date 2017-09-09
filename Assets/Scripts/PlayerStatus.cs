using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour {

    public int currentHP = 16;
    public int maxHP = 16;

    public Text hpText;

	// Use this for initialization
	void Start () {
        hpText.text = "HP: " + currentHP;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            TakeDamage(1);
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            currentHP = 0;
        }
        hpText.text = "HP: " + currentHP;
    }
}
