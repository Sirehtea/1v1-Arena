using UnityEngine;
using UnityEngine.SceneManagement;  // Import the Scene Management namespace

public class Character : MonoBehaviour
{
    public float health = 100f;

    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        // Reload the current scene when the character dies
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}