using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class LevelManager : MonoBehaviour
{
    [SerializeField] TMP_Text enemiesLeftText;
    [SerializeField] GameObject winText;

    int enemiesLeft = 0;

    public void AdjustEnemiesLeft(int amount)
    {
        enemiesLeft += amount;
        enemiesLeftText.text = Constants.ENEMIES_LEFT_STRING + enemiesLeft.ToString("D2");

        if (enemiesLeft <= 0)
        {
            winText.SetActive(true);
        }
    }
}
