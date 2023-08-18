using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int id;
    public string username;

    public MeshRenderer playerModle;

    public float hpPercent;

    public Animator animator;


    //only for local player
    public int kills;
    public int deaths;
    public ParticleSystem shootParticls;


    public void SetHp(float hp)
    {
        if(hpPercent <= 0 && hp > 0)
        {
            playerModle.enabled = true;
            if (id == Client.instance.myId)
                UIManager.instance.deadPanle.SetActive(false);
        }

        hpPercent = hp;

        if(hpPercent <= 0)
        {
            playerModle.enabled = false;
            if(id == Client.instance.myId)
                UIManager.instance.deadPanle.SetActive(true);
        }
    }


    public enum Animations
    {
        Running = 0, Standing, Shot
    }

    public void SetAnimation(int animation)
    {
        if(animation == (int)Animations.Running)
        {
            animator.SetBool("Running", true);
        }
        if (animation == (int)Animations.Standing)
        {
            animator.SetBool("Running", false);
        }
        if (animation == (int)Animations.Shot)
        {
            animator.SetTrigger("shot");
        }
    }

    //only for local player

    public void PlayLocalShootParticles()
    {
        shootParticls.Play();
    }


}
