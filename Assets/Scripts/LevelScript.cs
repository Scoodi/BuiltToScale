using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelScript : MonoBehaviour
{
    [SerializeField] private PlatformerCharacterScript player;
    private List<Rigidbody2D> debugRigidbodies = new List<Rigidbody2D>();
    [SerializeField] private float maxVelocity = 0.1f;
    //TODO add reference to inventory/placed rigidbodies
    private bool building = true;

    public void TryGoToPlatforming ()
    {
        if (building)
        {
            StartCoroutine(SettleCountdown());
        } else
        {
            Debug.Log("Restart level to rebuild");
        };
    }

    bool CheckIfSettled ()
    {
        bool settled = true;
        foreach (Rigidbody2D rb in debugRigidbodies)
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
                StopCoroutine(SettleCountdown());
            }
            Debug.Log("Waited " + i + " seconds");
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("Settled");
    }
}
