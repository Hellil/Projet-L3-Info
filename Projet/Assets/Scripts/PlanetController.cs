using UnityEngine;

public class PlanetController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerController playerController = other.GetComponent<PlayerController>();
        if (playerController != null)
        {
            if (playerController.currentPlanet == transform) return;
            playerController.currentPlanet = transform;
            playerController.EnterNewGravityField();
        }
    }
}
