using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleporter : MonoBehaviour
{
    // Changes the scene to the next level if RunManager canAccessBossRoom is true
    public GameObject loadingCircle;
    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("collision");
        if (other.CompareTag("Player") && RunManager.Instance.CanAccessBossRoom)
        {

            Debug.Log("Teleporting to next level");
            loadingCircle.SetActive(true); 
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            loadingCircle.SetActive(false);
        }
    }
    public void LoadNextLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
