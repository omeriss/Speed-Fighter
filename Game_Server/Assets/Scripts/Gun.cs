using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Gun
{
    
    public int MaxAmmo;
    public float damage;
    public float range;
    public float timeBetweenShots;
    public float reloadTime;


    public int RemainBullets;
    public DateTime nextShot;

    public Gun(int MaxAmmo, float damage, float range, float timeBetweenShots, float reloadTime)
    {
        this.MaxAmmo = MaxAmmo;
        this.damage = damage;
        this.range = range;
        this.timeBetweenShots = timeBetweenShots;
        this.reloadTime = reloadTime;
        RemainBullets = MaxAmmo;
        nextShot = DateTime.Now;
    }

    public bool Shot()
    {
        if(RemainBullets > 0 && nextShot < DateTime.Now)
        {
            RemainBullets--;
            nextShot = DateTime.Now.AddSeconds(timeBetweenShots);
            return true;
        }
        else
        {
            return false;
        }
    }

    public void Reload()
    {
        nextShot = DateTime.Now.AddSeconds(reloadTime);
        RemainBullets = MaxAmmo;
    }


}


/*
class GunConstants
{
    public static int RifleMaxAmmo = 15;
    public static float RifleDamage = 40;
    public static float RifleRange = 1000;
    public static float RifleTimeBetweenShots = 0.5f;
    public static float RifleReloadTime = 1.5f;

}



public class Rifle: Gun
{
    public Rifle():base
    {

    }
}
*/