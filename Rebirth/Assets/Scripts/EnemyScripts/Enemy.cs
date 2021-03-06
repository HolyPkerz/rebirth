﻿using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public float AtackFreq;
    public float Damage; // Damage Enemy does to playe
    public bool InPursuit;
    public float PursuitDistance;
    public float Speed;
    public bool TouchingPlayer;
    private CharacterController _cont;
    private float _nextAttack;
    private GameObject _playerTarget;
    public bool isAlive;
    public AudioSource zombieIsHit;
    public int Health;

    public bool IsBoss;

    public float TakeDamageFreq;
    private float _nextTakeDamage;

    // Use this for initialization
    private void Start()
    {
        isAlive = true;
        _playerTarget = GameObject.FindGameObjectWithTag("Player");

        zombieIsHit = GetComponent<AudioSource>();
        _cont = GetComponent<CharacterController>();
        Speed = 2;
        Damage = 10f;
        //Can we set up separate puruit variables for x and y axis?
        PursuitDistance = 20; //Enemies that exist just outside of the width of the viewport will pursue.
        InPursuit = false;

        //Freqeuency in which enemy does damage to the player
        AtackFreq = .5f; //Eveery 2s
        _nextAttack = 0f;

        //Used so enemy can have health
        TakeDamageFreq = .4f;
        _nextTakeDamage = 0f;

        TouchingPlayer = false;
    }

    private void Update()
    {
	   //I'm not sure we need this script. We are addressing Enemy through bullet collision
       //and health checks on line 150
       //if (!isAlive)
       // {
       //     Destroy(gameObject);
       // }

        if (!InPursuit)
        {
            InPursuit = ShouldPursuit();
        }
        
		else
        {
            if (!TouchingPlayer)
            {
                PursuitPlayer();
            }
        }
    }

    private bool ShouldPursuit()
    {
        float distanceToPlayer = Vector3.Distance(_playerTarget.transform.position, transform.position);
        return distanceToPlayer <= PursuitDistance;
    }

    // For AI Referenced 
    //http://answers.unity3d.com/questions/603634/having-issues-rotating-2d-sprites-to-face-another.html
    private void PursuitPlayer()
    {
        Vector3 newRotation = Quaternion.LookRotation(_playerTarget.transform.position - transform.position).eulerAngles;
        //for some reason this stil needs to be locked on the z
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(newRotation), 1);

        //Updating with .Move so that it obeys CharacterController Physics e.g dont go through walls
        Vector3 newPosition = transform.forward * Speed;
        _cont.Move(newPosition*Time.deltaTime);

		if(gameObject.name == "Demon" && !TouchingPlayer) 
		{
			gameObject.GetComponent<Animation>().Play ("Walk");
		}
    }



    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log(other.gameObject.name);

        var playerState = GameObject.Find("PlayerSprite").GetComponent<PlayerState>();

        if (other.tag == "Player" || other.gameObject.tag == "Player" || other.name == "PlayerSprite")
        {

            Debug.Log(this.name + "Touching Player");

            //GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>().DealDamage(Damage);

            TouchingPlayer = true;

			if (Time.time > _nextAttack)
			{
				if(gameObject.name == "Demon") 
				{
					gameObject.GetComponent<Animation>().Play ("Punch");
				}

				_nextAttack = Time.time + AtackFreq;
				playerState.DealDamage(Damage);
			}
		}
		else
		{
			TouchingPlayer = false;
		}
		
		//Dont try this at home. For some reason it doesnt like the tag...
        if (other.gameObject.tag == "Bullet" || other.tag == "Bullet" || other.name == "Bullet(Clone)")
        {
            //Gives Player Treasure for killing enemy with axe
            //GetComponent<AudioSource> ().Play ();
            //Triggers zombie groan sound when taking damage if this enemy is in fact a zombie
            if (gameObject.name == "ZombieWalker" || gameObject.name == "ZombieCrawler" ||
			    gameObject.name == "ZombieWalker(Clone)" || gameObject.name == "ZombieCrawler(Clone)")
            {
                AudioSource.PlayClipAtPoint(zombieIsHit.clip, Camera.main.transform.position);
            }
			if (gameObject.name == "Demon")
			{
				//Make demon sounds
			}

            //Lose one health
            
            //Prevents enemy from being hurt twice in one trigger by forcing it to wait
            if (Time.time > _nextTakeDamage)
            {
                _nextTakeDamage = Time.time + TakeDamageFreq;
                Health--;
            }


            if (Health <= 0)
            {
                //changed spelling to match with the lowercase d
				if (gameObject.name == "Demon")
				{
					Debug.Log("Should Die.");
					isAlive = false;
					// Next two variables needed to allow the demon to die.
					InPursuit = false;
					PursuitDistance = 0;
					gameObject.GetComponent<Animation>().Play("Death");

				    GameObject.FindGameObjectWithTag("Player").GetComponent<GamePlayUI>().WonGame = true;

				}
				else
				{
					//Destroy the Enemy
                	Destroy(gameObject);
                	PlayerState.KilledEnemyTreasure(5f);
				}
            }
           

            //Destroy the Bullet this solves an issue with collisions
            Destroy(other.gameObject);
        }
    }


    //Changed to continuously deal damage and not kill the enemy
    private void OnTriggerStay(Collider other)
    {
        // Debug.Log(other.gameObject.name);

        var playerState = GameObject.Find("PlayerSprite").GetComponent<PlayerState>();

        if (other.tag == "Player" || other.gameObject.tag == "Player" || other.name == "PlayerSprite")
        {
            //GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerState>().DealDamage(Damage);

            TouchingPlayer = true;
			if (Time.time > _nextAttack)
			{
				if(gameObject.name == "Demon") 
				{
					gameObject.GetComponent<Animation>().Play ("Punch");
				}
				_nextAttack = Time.time + AtackFreq;
				playerState.DealDamage(Damage);
			}
		}
		else
		{
			TouchingPlayer = false;
		}
    }
}