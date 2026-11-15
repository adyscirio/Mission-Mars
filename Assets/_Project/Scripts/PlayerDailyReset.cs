using UnityEngine;

public class PlayerDailyReset : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform outsideSpawnPoint;

    private int lastSol = 1;

    private void Update()
    {
        if (ResourceManager.Instance == null || playerTransform == null || outsideSpawnPoint == null)
            return;

        if (ResourceManager.Instance.CurrentSol != lastSol)
        {
            lastSol = ResourceManager.Instance.CurrentSol;
            MovePlayerOutside();
        }
    }

    public void MovePlayerOutside()
    {
        CharacterController cc = playerTransform.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        playerTransform.position = outsideSpawnPoint.position;
        playerTransform.rotation = outsideSpawnPoint.rotation;

        if (cc != null) cc.enabled = true;
    }
}