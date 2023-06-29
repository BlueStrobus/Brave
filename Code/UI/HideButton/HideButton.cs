using UnityEngine;
using UnityEngine.UI;

public class MenuButtonController : MonoBehaviour
{
    public Button menuButton;
    public GameObject[] buttons;

    private bool isMenuOpen = false;

    private void Start()
    {
        menuButton.onClick.AddListener(ToggleMenu);
        HideButtons();
    }

    private void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;

        if (isMenuOpen)
        {
            ShowButtons();
        }
        else
        {
            HideButtons();
        }
    }

    private void ShowButtons()
    {
        foreach (GameObject button in buttons)
        {
            button.SetActive(true);
        }
    }

    private void HideButtons()
    {
        foreach (GameObject button in buttons)
        {
            button.SetActive(false);
        }
    }
}