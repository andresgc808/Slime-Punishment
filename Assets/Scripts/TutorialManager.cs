using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour {
    public GameObject[] popUps;
    private int popupIndex = 0;
    public float waitTimeActivate = 3f;
    public float waitTimeDuration = 5f;

    private void Start() {
        StartCoroutine(ShowPopupsSequence());
    }

    private IEnumerator ShowPopupsSequence() {
        while (popupIndex < popUps.Length) {
            // Deactivate all pop-ups
            for (int i = 0; i < popUps.Length; i++) {
                popUps[i].SetActive(false);
            }

            // Activate the current pop-up
            popUps[popupIndex].SetActive(true);

            if (popupIndex == 0) {
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D));
            } else if (popupIndex == 1) {
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            } else if (popupIndex == 2) {
                yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Mouse0));
            } else {
                yield return new WaitForSeconds(waitTimeDuration);
            }

            // Deactivate the current pop-up
            popUps[popupIndex].SetActive(false);
            popupIndex++;
            yield return new WaitForSeconds(waitTimeActivate);
        }
    }
}
