using UnityEngine;
using UnityEngine.SceneManagement;
public class SlackResetScript : MonoBehaviour
{
    public void SetUpReset()
    {
        gameObject.SetActive(true);
    }
    public void RestartButton()
    {
        SceneManager.LoadScene("DcLineP2P");
    }
}
