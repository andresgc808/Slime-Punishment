using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircularLoading : MonoBehaviour {
    public Image loadingImage;
    public Text loadingText;

    [Range(0,1)]
    public float loadingProgress = 0;

    public Teleporter teleporter;

    public void Awake() {
        teleporter = GetComponent<Teleporter>();
    }

    private void Start() {
        if (loadingImage != null) {
            loadingText.text = "Loading...";
        }   
    }

    private void Update() {
        loadingImage.fillAmount = loadingProgress;

        if (loadingProgress >= 1) {
            teleporter.LoadNextLevel();
        } 
    }
}

