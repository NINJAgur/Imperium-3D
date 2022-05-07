using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    static int numEmpires = 6;

    public Sprite[] empireLogos = new Sprite[numEmpires];
    public GameObject CurrentLogoImage;
    public Slider slider;
    public Dropdown resolutionDropdown;
    Resolution[] resolutions;

    public AudioClip menuMusic;

    string[] empireNames = { "Rome", "Carthage", "Egypt", "Ghaul", "Macedon", "Arevaci" };

    private AudioSource[] allAudioSources;

    void Start()
    {
        allAudioSources = FindObjectsOfType(typeof(AudioSource)) as AudioSource[];
        foreach (AudioSource audioS in allAudioSources)
        {
            audioS.Stop();
        }

        transform.gameObject.GetComponent<AudioSource>().clip = menuMusic;
        transform.gameObject.GetComponent<AudioSource>().Play();

        int CurrentResolutionIndex = 0;
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();

        for (int i = 0; i < resolutions.Length; i++)
        {
            string Option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(Option);

            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                CurrentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = CurrentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int ResolutionIndex)
    {
        Resolution resolution = resolutions[ResolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }


    public void PlayGame()
    {
        PlayerEmpireName.playerEmpireName = empireNames[(int)slider.value];
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

    }

    public void SetEditor()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 2);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void Update()
    {
        slider.maxValue = numEmpires - 1;
        CurrentLogoImage.GetComponent<Image>().sprite = empireLogos[(int)slider.value];
    }

    public void SetVolume(float Volume)
    {
        // 
    }

    public void SetQuality(int qualtiyIndex)
    {
        QualitySettings.SetQualityLevel(qualtiyIndex);
    }

    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
    }

}
