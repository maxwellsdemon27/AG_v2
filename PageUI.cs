using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PageUI : MonoBehaviour
{
    public Animator animator;

    public bool pageOn = false;
    public int nowPage = 0;

    public List<Image> pageRects = new List<Image>();
    public List<GameObject> pageInfos = new List<GameObject>();

    public Image InfoRect;

    public void PageClick(int pageNo)
    {
        if(nowPage == 0)
        {
            nowPage = pageNo;
            pageOn = true;

            ViewPage(pageNo);
            InfoRect.enabled = true;
        }
        else if(nowPage == pageNo)
        {
            pageOn = false;
            nowPage = 0;

            ViewPage(0);
            InfoRect.enabled = false;
        }
        else
        {
            nowPage = pageNo;
            ViewPage(pageNo);
        }
         
        if (animator != null)
            animator.SetBool("On", pageOn);
    }

    private void ViewPage(int pageNo)
    {
        for(int i=0;i< pageRects.Count; i++)
        {
            if (i == pageNo - 1)
            {
                pageRects[i].enabled = true;
                pageInfos[i].SetActive(true);
            }
            else
            {
                pageRects[i].enabled = false;
                pageInfos[i].SetActive(false);
            }            
        }
    }
}
