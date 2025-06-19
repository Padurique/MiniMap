using UnityEngine;
using UnityEngine.UI;

public class ManaBar : MonoBehaviour
{
    public Slider slider;

    public void SetMaxMana(int maxMana)
    {
        slider.maxValue = maxMana;
        slider.value = maxMana;
    }

    public void SetMana(int mana)
    {
        slider.value = mana;
    }
}
