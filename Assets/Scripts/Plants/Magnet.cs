using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Magnet : MonoBehaviour
{
    public Transform targetTransform;  // Where the equipment should fly to
    public float sightRange = 5f;      // How far the magnet shroom can see
    public float stealCooldown = 3f;

    public bool isStealing = false;

    void Update()
    {
        if (!isStealing)
        {
            TryFindMetalEquipment();
        }
    }

    void TryFindMetalEquipment()
    {
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("zombie");
        List<GameObject> possibleTargets = new List<GameObject>();
        List<Zombie> possibleZombies = new List<Zombie>();

        foreach (GameObject zombie in zombies)
        {
            Zombie z = zombie.GetComponent<Zombie>();
            if (z == null || !z.gameObject.activeInHierarchy) continue;

            float distance = Vector3.Distance(transform.position, zombie.transform.position);
            if (distance <= sightRange)
            {
                GameObject[] metalPieces = null;

                switch (z.zombieType)
                {
                    case Zombie.ZombieType.Buckethead:
                        metalPieces = z.bucketHeadPieces;
                        break;
                    case Zombie.ZombieType.Screen:
                        metalPieces = z.screenPieces;
                        break;
                    case Zombie.ZombieType.Football:
                        metalPieces = z.footballPieces;
                        break;
                }

                if (metalPieces != null && metalPieces.Length > 0 && metalPieces[0] != null)
                {
                    possibleTargets.Add(metalPieces[0]);
                    possibleZombies.Add(z);
                }
            }
        }

        if (possibleTargets.Count > 0)
        {
            int i = Random.Range(0, possibleTargets.Count);
            AudioManager.instance.Play("Magnet");
            StartCoroutine(StealEquipment(possibleTargets[i], possibleZombies[i]));
        }
    }

    IEnumerator StealEquipment(GameObject piece, Zombie z)
    {
        isStealing = true;

        Vector3 startPos = piece.transform.position;
        Vector3 startScale = piece.transform.localScale;

        // Detach and disable physics
        piece.transform.parent = null;
        if (piece.GetComponent<Rigidbody>() == null)
        {
            piece.AddComponent<Rigidbody>().isKinematic = true;
        }

        // Step 1: Slerp to target position over 0.8 seconds
        float moveDuration = 0.8f;
        float moveTime = 0f;
        z.RemoveEquipment();

        while (moveTime < moveDuration)
        {
            float t = moveTime / moveDuration;
            piece.transform.position = Vector3.Slerp(startPos, targetTransform.position, t);
            moveTime += Time.deltaTime;
            yield return null;
        }

        piece.transform.position = targetTransform.position;

        // Step 2: Slerp scale to 0 over 10 seconds
        float scaleDuration = 10f;
        float scaleTime = 0f;

        while (scaleTime < scaleDuration)
        {
            float t = scaleTime / scaleDuration;
            piece.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
            scaleTime += Time.deltaTime;
            yield return null;
        }

        Destroy(piece);
        

        Debug.Log("Piece stolen and destroyed. Ready for next steal in " + stealCooldown + "s");
        yield return new WaitForSeconds(stealCooldown);
        isStealing = false;
    }
}
