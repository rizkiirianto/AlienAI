using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject playerPrefab;
    GameObject player;
    public GameObject[] abilityTriggers;
    public Image[] abilityIcons;
    public GameObject gameOverPanel;
    Pathfinding pathfinding;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        pathfinding = GameObject.Find("Hunter").GetComponent<Pathfinding>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!player)
        {
            GameObject newPlayer = Instantiate(playerPrefab);
            newPlayer.transform.position = new Vector3(-17.56f, 2.51f, -4.09f);
            newPlayer.name = "Player";
            player = newPlayer;
        }
    }

    public void Abilities(int index)
    {
        pathfinding.moreLikely(index);
        //abilityIcons[index].color = Color.green;
    }

    public void ShowGameOver()
    {
        if(gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
            Time.timeScale = 0f;
            Debug.Log("Game Over!");
        }
    }

}
