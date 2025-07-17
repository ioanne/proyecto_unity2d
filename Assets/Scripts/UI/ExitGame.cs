using UnityEngine;
using UnityEngine.UI;

public class ExitGame : MonoBehaviour
{
    [SerializeField] private Button exitButton;

    private void Start()
    {
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(QuitGame);
        }
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}