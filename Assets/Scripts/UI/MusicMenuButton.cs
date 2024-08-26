using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicMenuButton : MonoBehaviour
{

    [SerializeField] MusicMenuButtonController musicMenuButtonController;
    [SerializeField] Animator animator;
    [SerializeField] MusicAnimatorFunctions musicAnimatorFunctions;
    [SerializeField] int thisIndex;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(musicMenuButtonController.index == thisIndex)
        {
            animator.SetBool ("selected", true);
            if(Input.GetAxis ("Submit") == 1){
                animator.SetBool ("pressed", true);
            }else if (animator.GetBool ("pressed")){
                animator.SetBool ("pressed", false);
                musicAnimatorFunctions.disableOnce = true;
            }
        }else{
            animator.SetBool ("selected", false);
        }
    }
}
