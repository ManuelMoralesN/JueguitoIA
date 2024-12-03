using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject gameOverUI;  // Canvas de Game Over
    public GameObject victoryUI;   // Canvas de Victoria
    public float delayBeforeGameOver = 2f;  // Tiempo en segundos antes de mostrar el Game Over
    public float delayBeforeVictory = 2f;   // Tiempo en segundos antes de mostrar Victoria

    void Start()
    {
        gameOverUI.SetActive(false);  // Asegurarse de que la UI de Game Over esté desactivada al iniciar
        victoryUI.SetActive(false);   // Asegurarse de que la UI de Victoria esté desactivada al iniciar
    }

    // Método para mostrar la pantalla de Game Over
    public void ShowGameOver()
    {
        StartCoroutine(ShowGameOverUICoroutine());
    }

    // Corutina para esperar antes de mostrar la pantalla de Game Over
    IEnumerator ShowGameOverUICoroutine()
    {
        yield return new WaitForSeconds(delayBeforeGameOver);
        Time.timeScale = 0f;  // Congelar el tiempo

        gameOverUI.SetActive(true);
    }

    // Método para mostrar la pantalla de Victoria
    public void ShowVictory()
    {
        StartCoroutine(ShowVictoryUICoroutine());
    }

    // Corutina para esperar antes de mostrar la pantalla de Victoria
    IEnumerator ShowVictoryUICoroutine()
    {
        yield return new WaitForSeconds(delayBeforeVictory);
        victoryUI.SetActive(true);  // Activar la UI de victoria antes de detener el tiempo
        yield return new WaitForSeconds(0.5f); // Dar tiempo para que la pantalla de victoria se vea correctamente
        Time.timeScale = 0f;  // Congelar el tiempo
    }

    // Método para reiniciar el nivel cuando se presiona el botón de reinicio
    public void RestartLevel()
    {
        Time.timeScale = 1f;  // Restablecer el tiempo
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);  // Reiniciar la escena actual
    }
}