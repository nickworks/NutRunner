using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerStatus : MonoBehaviour {

    public int currentHP = 16;
    public int maxHP = 16;

    public int score = 0;

    public Text hpText;
    public Text scoreText;

    public AudioSource audioSource;
    public AudioClip eatAcornSound;
    public AudioClip[] squirrelYells;
    public AudioClip explosionSound;

    public GameObject explosion;

    public Renderer squirrelMesh;
    float blinkCounter = 0;
    float blinkRate = .2f;

    public bool deathTime = false;
    float deathCounter = 0;
    float deathRate = 1f;

	// Use this for initialization
	void Start () {
        UpdateHPDisplay();
    }
	
	// Update is called once per frame
	void Update () {
        if (!squirrelMesh.enabled && !deathTime)
        {
            blinkCounter += Time.deltaTime;
            if (blinkCounter >= blinkRate)
            {
                squirrelMesh.enabled = true;
            }
        }

        if (deathTime)
        {
            deathCounter += Time.deltaTime;
            if (deathCounter >= deathRate)
            {
                SceneManager.LoadScene("GameOver");
            }
        }
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            TakeDamage(4);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Acorn"))
        {
            Destroy(other.gameObject);
            ReplenishHealth(1);
            score += 100;
            GetComponent<PlayerController>().speedForwards *= 1.02f;
            UpdateScoreDisplay();
            PlaySound(eatAcornSound);
        }
    }

    public void TakeDamage(int amount)
    {
        int index = Random.Range(0, 3);
        PlaySound(squirrelYells[index]);
        squirrelMesh.enabled = false;
        blinkCounter = 0;
        currentHP -= amount;
        if (currentHP <= 0)
        {
            currentHP = 0;
            Death();
        }
        UpdateHPDisplay();
    }

    public void ReplenishHealth(int amount)
    {
        currentHP += amount;
        if (currentHP >= maxHP)
        {
            currentHP = maxHP;
        }
        UpdateHPDisplay();
    }

    public void UpdateHPDisplay()
    {
        hpText.text = "HP: " + currentHP;
    }

    public void UpdateScoreDisplay()
    {
        scoreText.text = "Score: " + score;
    }

    public void Death()
    {
        deathTime = true;
        PlaySound(explosionSound);
        GetComponent<PlayerController>().speedForwards = 0;
        squirrelMesh.enabled = false;
        Instantiate(explosion, transform.position, Quaternion.identity);
        //SceneManager.LoadScene("GameOver");
    }

    public void PlaySound(AudioClip clip)
    {
        audioSource.PlayOneShot(clip);
    }
}
