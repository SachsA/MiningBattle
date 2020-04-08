using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BuildQueue : MonoBehaviour
{
    public int queueIndex;
    [SerializeField]
    private GameObject cancelButton = null;
    [SerializeField]
    private GameObject deleteSpaceshipButton = null;

    private Text queueText;
    private Animator animator;
    private Image fillBuildTime;
    private Coroutine lerpLeftCoroutine;
    private Coroutine buildTimeCoroutine;

    // Awake is used to be sure fillBuildTime is set correctly before AnimateBuildTime can be called
    private void Awake()
    {
        queueText = GetComponent<Text>();
        animator = GetComponent<Animator>();
        fillBuildTime = transform.GetChild(1).GetComponent<Image>();
        fillBuildTime.fillAmount = 0;
        buildTimeCoroutine = null;
    }

    private void Update()
    {
        RectTransform rect = GetComponent<RectTransform>();
        Vector3 offset = rect.position;
        float scale = rect.localScale.x;
        Vector3 clickPosition = (Input.mousePosition - offset) / scale;

        if (rect.rect.Contains(clickPosition))
        {
            cancelButton.SetActive(true);
            deleteSpaceshipButton.SetActive(true);
        }
        else if (cancelButton.activeSelf)
        {
            cancelButton.SetActive(false);
            deleteSpaceshipButton.SetActive(false);
        }
    }

    // Is called at the end of "QueueDisappear" animation
    public void DestroyGameObject()
    {
        Destroy(gameObject);
    }

    public void SetDestroyed(string destroyedText)
    {
        queueText.text = destroyedText;
        fillBuildTime.fillAmount = 0;
        StopAllCoroutines();
        animator.SetTrigger("Disappear");
    }

    public void CancelQueue()
    {
        ConstructionQueue.Instance.RemoveQueueAtIndex(queueIndex);
    }

    public void DeleteSpaceship()
    {
        ConstructionQueue.Instance.DeleteSpaceshipFromQueueAtIndex(queueIndex);
    }

    #region Animations
    public void MoveLeft(float targetX)
    {
        // Stopping coroutine prevent multiple coroutines from running at the same time, and so, speed will be the same all the time and only 1 position will be targeted
        if (lerpLeftCoroutine != null)
            StopCoroutine(lerpLeftCoroutine);
        lerpLeftCoroutine = StartCoroutine(LerpPositionLeft(targetX));
    }

    // Move left with a slide effect for queues to be at the same spots all the time
    private IEnumerator LerpPositionLeft(float targetX)
    {
        float startTime = Time.time;
        float overTime = 0.3f;
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector2 endPos = startPos;

        endPos.x = targetX;
        while (Time.time < startTime + overTime)
        {
            rectTransform.anchoredPosition = Vector2.Lerp(startPos, endPos, (Time.time - startTime) / overTime);
            yield return null;
        }
        rectTransform.anchoredPosition = endPos;
    }

    public void AnimateBuildTime(float buildTime)
    {
        if (buildTimeCoroutine != null)
            StopCoroutine(buildTimeCoroutine);
        fillBuildTime.fillAmount = 1;
        buildTimeCoroutine = StartCoroutine(FillBuildAnimation(buildTime));
    }

    // Reduce fillAmount of building image (circle) over time and for buildTime
    private IEnumerator FillBuildAnimation(float buildTime)
    {
        float startTime = Time.time;
        float overTime = buildTime;

        while (Time.time < startTime + overTime)
        {
            fillBuildTime.fillAmount = Mathf.Lerp(1, 0, (Time.time - startTime) / overTime);
            yield return null;
        }
        fillBuildTime.fillAmount = 0;
    }
    #endregion Animations
}

