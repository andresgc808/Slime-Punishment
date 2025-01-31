using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleporter : MonoBehaviour
{
    // Changes the scene to the next level if RunManager canAccessBossRoom is true
    public GameObject loadingCircle;
    public bool inPortal = false;
    [SerializeField] public GameObject intructions;
    // Update is called once per frame
    void Update()
    {
        if (inPortal && Input.GetKeyDown(KeyCode.E)) {
            LoadNextLevel();
        }

        
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (RunManager.Instance != null) {
            Debug.Log("collision" + RunManager.Instance.CanAccessBossRoom);
            if (other.CompareTag("Player") && RunManager.Instance.CanAccessBossRoom) {
                inPortal = true;
                Debug.Log("Teleporting to next level");
                loadingCircle.SetActive(true);
            } else if (other.CompareTag("Player") && !RunManager.Instance.CanAccessBossRoom) {
                intructions.SetActive(true);
            }
            } else {
            if (other.CompareTag("Player") && SceneManager.GetActiveScene().buildIndex == 1) {
                inPortal = true;
                Debug.Log("Teleporting to next level");
                loadingCircle.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            inPortal = false;
            loadingCircle.SetActive(false);
        }
        if (intructions != null) {
            intructions.SetActive(false);
        }
    }
    public void LoadNextLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
