using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Magnet : MonoBehaviour
{
    public Transform targetTransform;  // Where the equipment should fly to
    public float sightRange = 5f;      // How far the magnet shroom can see
    public float zTolerance = 0.3f;    // For lane matching
    public float stealCooldown = 3f;

    private bool isStealing = false;

    void Update()
    {
        if (!isStealing)
        {
            GameObject equipment = TryFindMetalEquipment();
            if (equipment != null)
            {
                StartCoroutine(StealEquipment(equipment));
            }
        }
    }

    GameObject TryFindMetalEquipment()
    {
        GameObject[] zombies = GameObject.FindGameObjectsWithTag("zombie");
        List<GameObject> possibleTargets = new List<GameObject>();

        foreach (GameObject zombie in zombies)
        {
            Zombie z = zombie.GetComponent<Zombie>();
            if (z == null || !z.gameObject.activeInHierarchy) continue;

            float distance = Vector3.Distance(transform.position, zombie.transform.position);
            bool inSameLane = Mathf.Abs(zombie.transform.position.z - transform.position.z) <= zTolerance;

            if (distance <= sightRange && inSameLane)
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

                if (metalPieces != null)
                {
                    foreach (var piece in metalPieces)
                    {
                        if (piece.activeInHierarchy)
                        {
                            possibleTargets.Add(piece);
                        }
                    }
                }
            }
        }

        if (possibleTargets.Count > 0)
        {
            return possibleTargets[Random.Range(0, possibleTargets.Count)];
        }

        return null;
    }

    IEnumerator StealEquipment(GameObject piece)
    {
        isStealing = true;

        Vector3 startPos = piece.transform.position;
        Vector3 startScale = piece.transform.localScale;

        // Detach the piece
        piece.transform.parent = null;
        if (piece.GetComponent<Rigidbody>() == null)
        {
            piece.AddComponent<Rigidbody>().isKinematic = true; // Disable physics
        }

        float duration = 10f;
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;

            // Slerp position
            piece.transform.position = Vector3.Slerp(startPos, targetTransform.position, t);

            // Slerp scale to zero
            piece.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            time += Time.deltaTime;
            yield return null;
        }

        Destroy(piece);
        yield return new WaitForSeconds(stealCooldown);
        isStealing = false;
    }
}
