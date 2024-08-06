using UnityEngine;

public class BonusScore : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.AddTeamScore();
            Destroy(gameObject);
        }
    }
}
