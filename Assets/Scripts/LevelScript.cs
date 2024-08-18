using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    [SerializeField] private float maxVelocity = 0.1f;
    //TODO add reference to inventory/placed rigidbodies

    private void Start()
    {
        PlatformerCharacterScript.Instance.swapModeAction.performed += _ => TryGoToPlatforming();
    }
    public void TryGoToPlatforming ()
    {
        if (PlatformerCharacterScript.Instance.building)
        {
            StartCoroutine("SettleCountdown");
        } else
        {
            Debug.Log("Restart level to rebuild");
        };
    }

    bool CheckIfSettled ()
    {
        bool settled = true;
        foreach (Rigidbody2D rb in InventoryUI.Instance.placedRBs)
        {
            if (rb.velocity.magnitude > maxVelocity)
            {
                Debug.Log("Pieces not settled!");
                settled = false; break;
            }
        }
        return settled;
    }

    IEnumerator SettleCountdown ()
    {
        for (int i = 0; i < 3; i++)
        {
            if (!CheckIfSettled())
            {
                StopCoroutine("SettleCountdown");
            }
            Debug.Log("Waited " + i + " seconds");
            yield return new WaitForSeconds(1f);
        }
        PlatformerCharacterScript.Instance.SwapMode();
        Debug.Log("Settled");
    }
}
