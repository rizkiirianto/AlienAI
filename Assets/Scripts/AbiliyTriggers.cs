using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbiliyTriggers : MonoBehaviour
{
    GameManager manager;
    public int index;
    public int counter = 0;
    public float timer = 1.0f;
    public bool count = false;
    // Start is called before the first frame update
    void Start()
    {
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        AssignIndexFromManager();
        Debug.Log("Trigger " + gameObject.name + " pakai index " + index);
    }

    private void AssignIndexFromManager()
    {
        if (manager == null || manager.abilityTriggers == null)
            return;

        for (int i = 0; i < manager.abilityTriggers.Length; i++)
        {
            if (manager.abilityTriggers[i] == gameObject)
            {
                index = i;
                return;
            }
        }

        Debug.LogWarning("Trigger " + gameObject.name + " tidak ditemukan di array abilityTriggers GameManager. Index tetap " + index);
    }

    // Update is called once per frame
    void Update()
    {
        if (count)
        {
            timer -= Time.deltaTime;
            if(timer <= 0f)
            {
                counter++;
                count = false;
                timer = 1f;
                if (counter >= 2)
                {
                    Debug.Log("Memicu ability dari trigger " + gameObject.name + " dengan index " + index);
                    manager.Abilities(index);
                    counter = 0;
                }
            }
        }
        else
            timer = 1.0f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            count = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.tag == "Player") //reset timer when they leave the area
        {
            count = false;
            timer = 1.0f;
        }
    }
}
