using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HuntingBehavior : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(GetComponent<Rigidbody>().linearVelocity == Vector3.zero)
        {
            GetComponent<Rigidbody>().linearVelocity = new Vector3(5.0f, 0f, 0f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            Debug.Log("Pemain Tertangkap!");
            
            
            
            other.enabled = false; // Disable collider
            
            // Disable visual player
            Renderer renderer = other.GetComponent<Renderer>();
            if(renderer != null) renderer.enabled = false;
            
            // Disable script kontrol player jika ada
            MonoBehaviour[] scripts = other.GetComponents<MonoBehaviour>();
            foreach(MonoBehaviour script in scripts)
            {
                if(script.GetType().Name != "HuntingBehavior")
                    script.enabled = false;
            }
            
            // Call GameManager
            GameManager gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
            if(gameManager != null)
            {
                gameManager.ShowGameOver();
            }
        }
    }

}
