using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    public Slider volumeSlider;
    public Slider brightnessSlider;

    void Start()
    {
        volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
        brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
    }

    void Update()
    {
        
    }

    void OnVolumeChanged(float value)
    {

    }

    void OnBrightnessChanged(float value)
    {

    }
}