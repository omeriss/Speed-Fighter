using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Player : MonoBehaviour
{
    public int id;
    public string userName;
    private bool[] inputs;



    public CharacterController controller;
    public Transform PlayerGround;
    public Transform PlayerSide;
    public Transform ShootPosition;
    public float groundDis = 0.1f;
    public float SideDis = 0.70f;
    public LayerMask GroundLayer;


    private Vector3 velocity;
    private float gravity = -10;
    private bool OnGround;
    private bool wallTouching;
    private float jumpVelocity = 5;
    private float MoveSpeed = 5;

    public float hpPercent = 100;
    public int kills;
    public int deaths;


    public Gun gun;

    private int animation;

    public enum Animations
    {
        Running = 0, Standing, Shot
    }

    public void Intitialize(int id, string userName)
    {
        this.id = id;
        this.userName = userName;
        this.inputs = new bool[5];
        velocity = Vector3.zero;
        hpPercent = 100;
        kills = 0;
        animation = (int)Animations.Standing;
        deaths = 0;
        transform.position = new Vector3(0, -24, -30);

        gun = new Gun(15, 40, 1000, 0.5f, 1.5f);
        ServerSend.SendBullets(id, gun.RemainBullets);


        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        System.Random r = new System.Random();
        transform.position = spawnPoints[r.Next(0, spawnPoints.Length)].transform.position;
        ServerSend.SendChatMassage(userName + " connected", id);
    }

    private void FixedUpdate()
    {
        Vector2 dir = Vector2.zero;

        dir.x += (inputs[3] ? 1 : 0) - (inputs[2] ? 1 : 0);
        dir.y += (inputs[0] ? 1 : 0) - (inputs[1] ? 1 : 0);

        if (controller.enabled)
        {
            if(animation == (int)Animations.Standing && (dir.y != 0 || dir.x != 0))
            {
                animation = (int)Animations.Running;
                ServerSend.ChangeAnimation(id, animation);
            }
            else if(animation == (int)Animations.Running && (dir.y == 0 || dir.x == 0))
            {
                animation = (int)Animations.Standing;
                ServerSend.ChangeAnimation(id, animation);
            }
            Move(dir, inputs[4]);
        }
    }

    private void Move(Vector2 dir, bool jumping)
    {
        OnGround = Physics.CheckSphere(PlayerGround.position, groundDis, GroundLayer);
        wallTouching = Physics.CheckSphere(PlayerSide.position, SideDis, GroundLayer);


        if (OnGround && velocity.y < 0)
        {
            velocity.y = 0;
        }
        else
        {
            velocity.y += gravity * Time.fixedDeltaTime;
        }

        if (jumping && OnGround)
        {
            velocity.y = jumpVelocity;
        }



        controller.Move(velocity * Time.fixedDeltaTime);

        if (!(wallTouching && (jumping || !OnGround) && velocity.y > 0))
            controller.Move((transform.right * dir.x + transform.forward * dir.y) * MoveSpeed * Time.fixedDeltaTime);

        ServerSend.PlayerPosition(this);
        ServerSend.PlayerRotation(this);
    }

    public void Shoot(Vector3 ShootDir)
    {
        if (gun == null)
            Debug.Log("gun is null");
        if (!gun.Shot())
            return;
        ServerSend.ChangeAnimation(id, (int)Animations.Shot);
        ServerSend.SendNextShot(id, (float)(gun.nextShot - DateTime.Now).TotalMilliseconds / 1000f);
        ServerSend.SendBullets(id, gun.RemainBullets);
        RaycastHit bulletStop;
        bool bulletHit =  Physics.Raycast(ShootPosition.position, ShootDir, out bulletStop, gun.range);
        if (bulletHit && bulletStop.collider.CompareTag("Player"))
        {
            bulletStop.collider.GetComponent<Player>().Hit(gun.damage, id);
        }
        else
        {
            ServerSend.SendHit(id, false, false);
        }
    }

    public void Hit(float gunDamage, int shooterId)
    {
        hpPercent -= gunDamage;
        if(hpPercent <= 0)
        {
            hpPercent = 0;
            deaths++;
            if(Server.clients[shooterId]!=null && Server.clients[shooterId].player != null)
            {
                Server.clients[shooterId].player.kills++;
            }
            controller.enabled = false;
            transform.position = new Vector3(-1000, -1000, -1000);
            ServerSend.PlayerPosition(this);
            ServerSend.SendHit(shooterId, true, true);
            ServerSend.SendState(id, hpPercent);
            ServerSend.SendChatMassage($"{Server.clients[shooterId].player.userName} killed {userName}");
            StartCoroutine(Respawn());
        }
        else
        {
            ServerSend.SendState(id, hpPercent);
            ServerSend.SendHit(shooterId, true, false);
        }

    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(7);

        gun.RemainBullets = gun.MaxAmmo;
        ServerSend.SendBullets(id, gun.RemainBullets);

        GameObject[] spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        System.Random r = new System.Random();
        transform.position = spawnPoints[r.Next(0, spawnPoints.Length)].transform.position;
        ServerSend.PlayerPosition(this);
        controller.enabled = true;
        hpPercent = 100;
        ServerSend.SendState(id, hpPercent);
        animation = (int)Animations.Standing;
        ServerSend.ChangeAnimation(id, animation);

    }


    
    public void SetInputs(bool[] inputs, Quaternion rotation)
    {
        this.inputs = inputs;
        transform.rotation = rotation;
    }


}
