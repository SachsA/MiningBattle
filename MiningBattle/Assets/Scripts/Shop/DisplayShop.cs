using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisplayShop : MonoBehaviour
{
    public bool isOpen;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        isOpen = false;
        animator = GetComponent<Animator>();
    }

    public void OpenShopMenu()
    {
        isOpen = true;
        animator.SetTrigger("OpenShop");
    }

    public void CloseShopMenu()
    {
        isOpen = false;
        animator.SetTrigger("CloseShop");
    }

    public void StartAnimation(string animationName)
    {
        animator.SetTrigger(animationName);
    }
}
