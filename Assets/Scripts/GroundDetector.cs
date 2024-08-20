using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GroundDetector : MonoBehaviour
{
    [SerializeField] private Collider2D footTrigger;
    [SerializeField] private PlatformerCharacterScript keith;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D (Collider2D col) {
        if (col.gameObject.CompareTag("Ground"))
        {
            keith.OnEnterGround();
        } else if (col.gameObject.CompareTag("Platform")) {
            keith.OnEnterGround(col.gameObject.transform);
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.CompareTag("Ground"))
        {
            List<Collider2D> overlapResults = new List<Collider2D>();
            Physics2D.OverlapCollider(footTrigger, new ContactFilter2D(), overlapResults);
            foreach (Collider2D col2 in overlapResults)
            {
                if (col2.gameObject.CompareTag("Ground"))
                {
                    return;
                }
            }
            keith.OnLeaveGround();
        } else if (col.gameObject.CompareTag("Platform")) {
            keith.OnLeaveGround(col.gameObject.transform);
        }
    }
}
