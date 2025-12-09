using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Feedbacks;

public class HitIndicator : MonoBehaviour
{
    public static HitIndicator Instance;

    // [SerializeField] Image normalHit;
    // [SerializeField] Image criticalHit;
    [SerializeField] MMF_Player normalFeedback;
    [SerializeField] MMF_Player criticalFeedback;

    // [SerializeField] float disPlayTime = .2f;
    // [SerializeField] float fadeSpeed = 5f;

    Coroutine fadeRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        // else
        // {
        //     Destroy(gameObject);
        // }
    }

    public void ShowMaker(bool isWeakHit)
    {
        // Image makerImage = isWeakHit ? criticalHit : normalHit;

        // if (fadeRoutine != null)
        // {
        //     StopCoroutine(fadeRoutine);
        // }
        // fadeRoutine = StartCoroutine(HitRoutine(makerImage));

        if (isWeakHit)
        {
            criticalFeedback?.PlayFeedbacks();
        }
        else
        {
            normalFeedback?.PlayFeedbacks();
        }
    }

    // IEnumerator HitRoutine(Image maker)
    // {
    //     Color c = maker.color;
    //     c.a = 1f;
    //     maker.color = c;

    //     yield return new WaitForSeconds(disPlayTime);

    //     while (maker.color.a > 0)
    //     {
    //         c.a -= Time.deltaTime * fadeSpeed;
    //         maker.color = c;
    //         maker.transform.localScale = Vector3.Lerp(maker.transform.localScale, Vector3.one, Time.deltaTime * 10f);
    //         yield return null;
    //     }
    // }
}
