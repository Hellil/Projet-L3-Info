using UnityEngine;

public class RespawnTrigger : MonoBehaviour
{
    public Transform respawnPoint;
    public GameObject player;

    public void RespawnPlayer()
    {
        // IMPORTANT : unpause avant de téléporter
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (player == null || respawnPoint == null)
            return;

        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        player.transform.position = respawnPoint.position;

        if (cc != null)
            cc.enabled = true;
    }
}