using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuRenderer : MonoBehaviour
{
    public Button startButton;
    public Button continueButton;
    public Button exitButton;

    void Start()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(() =>
            {
                Application.Quit();
            });
        }

        bool hasSave = SaveSystem.HasSave();
        if (!hasSave)
        {
            if (continueButton != null)
            {
                Destroy(continueButton.gameObject);
            }
        }
        else
        {
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(() =>
                {
                    SaveSystem.ClearNewGameRequest();
                    SceneManager.LoadScene(1);
                });
            }
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(() =>
            {
                SaveSystem.RequestNewGame();
                SceneManager.LoadScene(1);
            });
        }
    }
}
