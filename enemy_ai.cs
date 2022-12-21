using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class Enemy : MonoBehaviour
{
    // The player GameObject
    public GameObject player;

    // The Seeker component that will be used to request paths from the pathfinding system
    public Seeker seeker;
    
    // The maximum distance the enemy will chase the player
  public float chaseDistance = 10.0f;

  // The speed at which the enemy will move
  public float speed = 5.0f;

  // The audio clip to play as a cue to lure the player
  public AudioClip cueClip;

  // The AudioSource component
  private AudioSource audioSource;

  // A flag to indicate whether the enemy is currently chasing the player
  private bool isChasing = false;

  // The path that the enemy is currently following
  private Path path;

  // The index of the current waypoint in the path
  private int currentWaypoint = 0;

  void Start()
  {
      // Get the AudioSource component
      audioSource = GetComponent<AudioSource>();
  }

  void Update()
  {
      // Check if the enemy has line of sight to the player
      RaycastHit2D hit = Physics2D.Linecast(transform.position, player.transform.position);
      if (hit.collider != null && hit.collider.gameObject == player)
      {
          // If the enemy has line of sight to the player, start chasing them
          isChasing = true;

          // Request a path to the player's position
          seeker.StartPath(transform.position, player.transform.position, OnPathComplete);
      }
      else
      {
          // If the enemy does not have line of sight to the player, stop chasing them
          isChasing = false;
      }

      if (isChasing)
      {
          // If the enemy is currently chasing the player and a path has been found, follow the path
          if (path != null)
          {
              // Check if the enemy has reached the end of the path
              if (currentWaypoint >= path.vectorPath.Count)
              {
                  // If the enemy has reached the end of the path, stop following the path
                  path = null;
                  return;
              }

              // Calculate the direction to the next waypoint
              Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
              dir *= speed * Time.deltaTime;
              
              // Move the enemy towards the next way point
              // Check if the enemy has reached the next waypoint
              if (Vector3.Distance(transform.position, path.vectorPath[currentWaypoint]) < 0.1f)
              {
                  // If the enemy has reached the next waypoint, move to the next waypoint
                  currentWaypoint++;
              }
          }
      }
      else
      {
          // If the enemy is not currently chasing the player, move randomly
          Vector3 randomDirection = Random.insideUnitCircle * chaseDistance;
          seeker.StartPath(transform.position, transform.position + randomDirection, OnPathComplete);

          // Play the audio cue occasionally
          if (Random.value < 0.1f)
          {
              audioSource.PlayOneShot(cueClip);
          }
      }
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
      // If the enemy collides with the player, end the game
      if (collision.gameObject.tag == "Player")
      {
          // End the game
          // (Replace this with your own game over logic)
          Debug.Log("Game Over!");
      }
  }

  // This function is called when a path has been calculated by the pathfinding system
  public void OnPathComplete(Path p)
  {
      // Check if the path is valid
      if (!p.error)
      {
          // If the path is valid, store a reference to the path and reset the waypoint index
          path = p;
          currentWaypoint = 0;
      }
    }
}
