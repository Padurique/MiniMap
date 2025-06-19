using UnityEngine;
using UnityEngine.UI;

public class StaminaBar : MonoBehaviour
{
    public Slider slider; 

    public void SetMaxStamina(int maxStamina)
    {
        slider.maxValue = maxStamina; 
        slider.value = maxStamina; 
    }

    public void SetStamina(int stamina)
    {
        slider.value = stamina; 
    }
}
