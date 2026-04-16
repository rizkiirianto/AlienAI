using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pathfinding : MonoBehaviour
{
    public Transform[] searchLocations; //get basics established, always have them
    public List<Transform> advancedLocations; //for when probability stars changing
    private NavMeshAgent nav;
    private int nextPoint;
    public int seenCount = 0;
    private bool isSeen = false, notSeen = false;
    public bool seePlayer = false, fleeUsed = false;
    public float followTimer, followTimerStandard = 1.5f;
    GameObject player;
    GameManager manager;
    HunterFOV fov;
    DirectorAI director;

    // Start is called before the first frame update
    void Start()
    {
        nav = GetComponent<NavMeshAgent>();
        player = GameObject.Find("Player");
        manager = GameObject.Find("GameManager").GetComponent<GameManager>();
        fov = GetComponent<HunterFOV>();
        director = FindObjectOfType<DirectorAI>();

        if (advancedLocations == null)
            advancedLocations = new List<Transform>();

        followTimer = followTimerStandard;
        for(int i = 0; i < searchLocations.Length; i++)
        {
            advancedLocations.Add(searchLocations[i]);
        }
    }


    private void FixedUpdate()
    {
        seePlayer = HunterFOV.inFOV(transform, player.transform, 45f, 20f, 12f, 5f);
        if (director != null)
            director.SetDirectSight(seePlayer);

        if (seePlayer)
        {
            isSeen = true;
            Pursue();
        }

        else if (!nav.pathPending && nav.remainingDistance < 1f)
        {
            notSeen = true;
            GoToNextLocation();
        }

        if (isSeen && notSeen) //has been in and out of FOV, add to counter
        {
            seenCount++;
            isSeen = false;
            notSeen = false;
            if (seenCount >= 5)
                FleeAbility();
        }
    }

    public void GoToNextLocation()
    {
        if (advancedLocations.Count == 0)
            return;
        nav.destination = advancedLocations[nextPoint].position;
        nextPoint = Random.Range(0, advancedLocations.Count);
        if (advancedLocations[nextPoint].position == nav.destination)
            nextPoint = (nextPoint + 1) % advancedLocations.Count;

    }

    public void Pursue()
    {
        if (!seePlayer)
        {
            GoToNextLocation();
            return;
        }
        nav.destination = player.transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if(!player)
            player = GameObject.Find("Player");

        if (seePlayer)
        {
            followTimer -= Time.deltaTime;
            if (followTimer <= 0f)
            {
                FollowingAbility();
                followTimerStandard++;
                followTimer = followTimerStandard; //make it take longer to advance next time
            }
        }
        else
            followTimer = followTimerStandard;
    }

    public void moreLikely(int index) //alien "learns" from player actions- searches in their common hiding spots
    {
        if (searchLocations == null || searchLocations.Length == 0)
        {
            Debug.LogWarning("searchLocations belum diisi di Inspector.");
            return;
        }

        if (index < 0 || index >= searchLocations.Length)
        {
            Debug.LogWarning("Index spot tidak valid: " + index + ". Jumlah spot: " + searchLocations.Length);
            return;
        }

        if (searchLocations[index] == null)
        {
            Debug.LogWarning("searchLocations pada index " + index + " masih kosong (null).");
            return;
        }

        advancedLocations.Add(searchLocations[index]);
        if (director != null)
            director.ReportHidingSpotUsed(index);
        Debug.Log("AI Belajar! Lokasi sembunyi index ke-" + index + " ditambahkan ke rute patroli.");
    }

    public void FleeAbility() //makes hunter faster if player keeps running away
    {
        if (!fleeUsed)
        {
            nav.speed += 5;
            fleeUsed = true;
        }
    }

    public void FollowingAbility()
    {
        fov.FollowingAbility();
    }
}
