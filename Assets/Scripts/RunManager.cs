using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
Singleton class that manages the game state.

It is responsible for: keeping track of the remaining enemies and whether or not they can access the boss room.
It also keep track of the time since the game started to track scores

*/
public class RunManager : MonoBehaviour
{
    public static RunManager Instance { get; private set; }

    private int _remainingEnemies;
    private bool _canAccessBossRoom;
    private float _timeSinceStart;

    public int RemainingEnemies => _remainingEnemies;
    public bool CanAccessBossRoom => _canAccessBossRoom;
    public bool timerOngoing;
    public float TimeSinceStart => _timeSinceStart;

    // reference to TMP timer text
    public TMPro.TextMeshProUGUI timerText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        _canAccessBossRoom = false;
        _timeSinceStart = 0f;
        timerOngoing = true;
    }

    private void Update()
    {
        UpdateTimer();
        _timeSinceStart += Time.deltaTime;
    }

    private void UpdateTimer()
    {
        int minutes = (int)(_timeSinceStart / 60);
        int seconds = (int)(_timeSinceStart % 60);

        timerText.text = $"{minutes:00}:{seconds:00}";
    }

    public void DecreaseRemainingEnemies()
    {
        _remainingEnemies--;

        if (_remainingEnemies <= 0)
        {
            _canAccessBossRoom = true;
            timerOngoing = false;
        }
    }
}
