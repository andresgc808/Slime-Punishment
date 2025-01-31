using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] popUps;
    private int popupIndex = 0;
    public float waitTimeActivate = 3f;
    public float waitTimeDuration = 5f;
    private bool isWaiting = false;

    private void Update()
    {
        if (!isWaiting && popupIndex < popUps.Length)
        {
            for (int i = 0; i < popUps.Length; i++)
            {
                if (i == popupIndex)
                {
                    popUps[i].SetActive(true);
                }
                else
                {
                    popUps[i].SetActive(false);
                }
            }

            if (popupIndex == 0)
            {
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                {
                    StartCoroutine(WaitAndActivateNextPopup());
                }
            }
            else if (popupIndex == 1)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    StartCoroutine(WaitAndActivateNextPopup());
                }
            }
            else if (popupIndex == 2)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    StartCoroutine(WaitAndActivateNextPopup());
                }
            }
            else if (popupIndex == 3)
            {
                StartCoroutine(ShowPopupForDuration());
            }
            else if (popupIndex == 4)
            {
                StartCoroutine(ShowPopupForDuration());
            }
            else if (popupIndex == 5)
            {
                StartCoroutine(ShowPopupForDuration());
            }
            else if (popupIndex == 6)
            {
                StartCoroutine(ShowPopupForDuration());
            }
        }
    }

    private IEnumerator WaitAndActivateNextPopup()
    {
        isWaiting = true;
        popUps[popupIndex].SetActive(false);
        yield return new WaitForSeconds(waitTimeActivate);
        popupIndex++;
        isWaiting = false;
    }

    private IEnumerator ShowPopupForDuration()
    {
        isWaiting = true;
        popUps[popupIndex].SetActive(true);
        yield return new WaitForSeconds(waitTimeDuration);
        popUps[popupIndex].SetActive(false); // Deactivate the popup after the wait time
        popupIndex++;
        isWaiting = false;
    }
}
