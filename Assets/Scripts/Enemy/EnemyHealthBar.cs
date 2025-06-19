using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Slider healthSlider; 

    public void SetHealth(float healthPercent)
    {
        if (healthSlider != null)
        {
            healthSlider.value = healthPercent;
        }
    }
}
