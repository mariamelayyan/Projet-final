using UnityEngine;
using UnityEngine.UI;
using ColliderEventSystem;

public class HealthBar : MonoBehaviour
{
    public IntVariable health;
    public Image fillImage;
    public int maxHealth = 50;
    public float smoothSpeed = 5f;
    public Gradient colorByHealth;   // set in the Inspector, or leave default

    float shown = 1f;

    void Update()
    {
        float target = Mathf.Clamp01((float)health.RuntimeValue / maxHealth);
        shown = Mathf.Lerp(shown, target, Time.deltaTime * smoothSpeed);
        fillImage.fillAmount = shown;
        fillImage.color = colorByHealth.Evaluate(shown);
    }
}
