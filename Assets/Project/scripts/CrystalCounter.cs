using UnityEngine;
using TMPro;
using ColliderEventSystem;

public class CrystalCounter : MonoBehaviour
{
    public TMP_Text counterText;     // drag your text object here
    public IntVariable crystals;     // drag the crystals asset here
    public int needed = 15;

    void Update()
    {
        counterText.text = crystals.RuntimeValue + "/" + needed;
    }
}