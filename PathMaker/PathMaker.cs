//Made by Alex Meesters, www.alexmeesters.nl / www.low-scope.com
//Licence: CC0 - https://creativecommons.org/share-your-work/public-domain/cc0/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Generates a path based on an array of transforms and a starting location
/// </summary>
public class PathMaker : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform startPoint;

    [Header("Configuration"), Range(0,180)]
    [Tooltip("It will not concider a path when the target has a angle lower then this. " +
        "0 means the next point has to be parrallel. 90 means it is perpendicular.")]
    [SerializeField] private float maxAllowedAngleOffset = 120;

    [Tooltip("How much height is taken into concideration in comparison of other axis. (2 = Two times as much)")]
    [SerializeField] private float heightWeight = 0.25f;

    private float maxAllowedDot;

    private void Awake()
    {
        maxAllowedDot = Mathf.Lerp(1, -1, (maxAllowedAngleOffset) / 180);
    }

    private void Start()
    {
        // Example of how to apply the script, you can delete the Start function if you
        // are only going to call GetPath() outside of this script.
        if (startPoint != null)
        {
            // Call GetPath, with all BoxColliders in the scene
            var points = GetPath(startPoint, FindObjectsOfType<BoxCollider>()
                .Select(a => a.transform).ToArray());

            // Draw result
            for (int i = points.Count - 1; i >= 1; i--)
            {
                Debug.DrawLine(points[i], points[i - 1], Color.blue, 100);
            }

            // Connect line with initial point
            Debug.DrawLine(points[0], points[points.Count - 1], Color.green, 100);
        }
    }

    /// <summary>
    /// Returns an array of points, from start to finish.
    /// </summary>
    /// <param name="start">The starting position, it will use the forward vector to determine the initial direction </param>
    /// <param name="input">Array of points from start to finish.</param>
    /// <param name="closeLoop">Include the first point at the end of the list. </param>
    /// <param name="direction">Direction to start looking in. If unused it will use the startTransform forward vector. </param>
    /// <returns></returns>
    public List<Vector3> GetPath(Transform start, Transform[] input, Vector3? searchDirection = null , bool closeLoop = false)
    {
        if (start == null)
        {
            Debug.LogWarning("No startpoint set");
            return null;
        }

        if (input == null && input.Length == 0)
        {
            Debug.LogWarning("TrackMaker: Invalid points added.");
            return null;
        }

        if (heightWeight == 0)
        {
            Debug.LogError("Heightweight should not be 0.");
            heightWeight = 0.01f;
        }

        var points = input.ToList();

        Transform activePoint = start;

        var activeDirection = (searchDirection == null)? 
            start.forward : (Vector3)searchDirection;

        List<Vector3> verifiedPoints = new List<Vector3>
        {
            start.position
        };

        // Keep iterating untill all checkpoints are gone.
        while (points.Count > 0)
        {
            int closestIndex = -1;

            float lowestDistance = Mathf.Infinity;
            Vector3 bestDirection = Vector3.zero;

            for (int i = 0; i < points.Count; i++)
            {
                var direction = (points[i].position - activePoint.position);

                if (heightWeight != 1)
                {
                    direction.y /= heightWeight;
                }

                var dirNormalized = direction.normalized;
                var pointDistance = direction.sqrMagnitude;

                // Store the checkpoint index if it is the lowest one
                if (pointDistance < lowestDistance)
                {
                    var dot = Vector3.Dot(activeDirection, dirNormalized);

                    // Only select the checkpoint if it is in front of the last direction
                    if (dot > maxAllowedDot)
                    {
                        closestIndex = i;
                        lowestDistance = pointDistance;
                        bestDirection = dirNormalized;
                    }
                }
            }

            // In case we cannot find any candidate for a new point, we brake the loop
            if (closestIndex == -1)
            {
                break;
            }
            else
            {
                // We have a new active checkpoint
                activePoint = points[closestIndex];
                activeDirection = bestDirection;

                verifiedPoints.Add(activePoint.position);
                points.Remove(activePoint);
            }
        }

        if (closeLoop)
        {
            verifiedPoints.Add(startPoint.position);
        }

        return verifiedPoints;
    }
}
