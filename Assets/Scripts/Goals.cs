using UnityEngine;

public class Goal : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        GameManager.Instance.AnnounceWinner(other.tag);
    }
}