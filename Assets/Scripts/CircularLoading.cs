using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircularLoading : MonoBehaviour {
    public Image loadingImage;

    [Range(0,1)]
    public float loadingProgress = 0;

    public Teleporter teleporter;

    public void Awake() {
        teleporter = GetComponent<Teleporter>();
    }

    private void Start() {
        if (loadingImage != null) {
        }   
    }

    private void Update() {
        // 3 second loading time
        loadingProgress += Time.deltaTime / 5;

        loadingImage.fillAmount = loadingProgress;

        if (loadingProgress >= 1) {
            teleporter.LoadNextLevel();
        } 
    }
}

