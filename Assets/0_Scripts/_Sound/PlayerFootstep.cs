using UnityEngine;
using StarterAssets;

public class PlayerFootstep : MonoBehaviour
{
    [SerializeField] float walkInterval = .5f; // 재생주기
    [SerializeField] float runInterval = .3f;
    [SerializeField] float velocityThreshold = 2.0f; // 속도 감지

    [SerializeField] AudioClip[] stepClips;
    [SerializeField] AudioClip landClip; // 착지 소리;

    CharacterController controller;
    FirstPersonController firstPersonController;

    float nextStepTime;
    bool wasGround;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        firstPersonController = GetComponent<FirstPersonController>();
    }

    void Update()
    {
        CheckLand();
        CheckFootstep();
    }

    void CheckFootstep()
    {
        if (!controller.isGrounded) return; // 땅에서만

        Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        if (horizontalVelocity.magnitude < velocityThreshold) return;

        if (Time.time >= nextStepTime)
        {
            PlayStepSound();
            bool isSprinting = firstPersonController.GetCurrentSpeed() > firstPersonController.MoveSpeed;
            float interval = isSprinting ? runInterval : walkInterval;
            nextStepTime = Time.time + interval;
        }
    }

    void CheckLand()
    {
        // 전 프레임은 땅이 아니였는데 지금은 땅일때 착지 소리
        if (!wasGround && controller.isGrounded)
        {
            SoundManager.Instance.PlaySFX(landClip, transform.position, 1.0f);
        }

        wasGround = controller.isGrounded;
    }

    void PlayStepSound()
    {
        int index = Random.Range(0, stepClips.Length);
        AudioClip audioClip = stepClips[index];

        float randomPitch = Random.Range(.9f, 1.1f);
        SoundManager.Instance.PlaySFX(audioClip, transform.position, randomPitch);
    }
}
