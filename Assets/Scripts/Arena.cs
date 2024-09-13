using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arena : MonoBehaviour
{
    // Assign these from the Unity Inspector
    public GameObject object1;
    public GameObject object2;
    public GameObject object3;

    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        // Trigger the same animation for all three objects at the start
        StartCoroutine(PlayShoot());
    }

    IEnumerator PlayShoot()
    {

        animator.SetBool("Move", true);

        yield return new WaitForSeconds(5f);

        animator.SetBool("Move", false);
    }
}
