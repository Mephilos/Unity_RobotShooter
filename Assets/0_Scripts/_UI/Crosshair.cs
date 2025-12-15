using UnityEngine;

public class Crosshair : MonoBehaviour
{
    [SerializeField] RectTransform top;
    [SerializeField] RectTransform bottom;
    [SerializeField] RectTransform left;
    [SerializeField] RectTransform right;

    [SerializeField] float spreadMultiplier = 500f;
    [SerializeField] float spreadSmooth = 15f;
    [SerializeField] float aimNarrow = 5f;
    ActiveWeapon activeWeapon;

    float spreadMove = 0;

    public void Initialize(ActiveWeapon activeWeapon)
    {
        this.activeWeapon = activeWeapon;
    }
    void Update()
    {
        if (activeWeapon == null) return;

        float currentSpread = activeWeapon.GetCurrentSpread() * spreadMultiplier;
        spreadMove = Mathf.Lerp(currentSpread, currentSpread, Time.deltaTime * spreadSmooth);

        top.anchoredPosition = new Vector2(0, spreadMove + aimNarrow);
        bottom.anchoredPosition = new Vector2(0, -spreadMove - aimNarrow);
        left.anchoredPosition = new Vector2(-spreadMove - aimNarrow, 0);
        right.anchoredPosition = new Vector2(spreadMove + aimNarrow, 0);
    }

}
