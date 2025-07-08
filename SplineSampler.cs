using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Utility for sampling points inside a closed spline in XZ using Poisson Disk Sampling.
/// </summary>
public static class SplineSampler
   {
   /// <summary>
   /// Generates Poisson Disk sampled points inside a closed spline (XZ projection).
   /// </summary>
   /// <param name="spline">Closed spline points (must have at least 3 points, projected in XZ)</param>
   /// <param name="minDistance">Minimum distance between sampled points</param>
   /// <param name="maxAttempts">Number of attempts to find a valid point around an active sample</param>
   /// <param name="heightProvider">Optional: function to provide Y value based on XZ (e.g., terrain height)</param>
   /// <param name="targetCount">Optional: maximum number of points to generate</param>
   /// <returns>List of sampled Vector3 points</returns>
   public static List<Vector3> SamplePoints(List<Vector3> spline, float minDistance = 1f, int maxAttempts = 30, System.Func<float, float, float> heightProvider = null, int? targetCount = null)
      {
      List<Vector3> result = new List<Vector3>();

      if (spline == null || spline.Count < 3)
         return result;

      // Bounding box
      float minX = float.MaxValue, maxX = float.MinValue;
      float minZ = float.MaxValue, maxZ = float.MinValue;

      foreach (var pt in spline)
         {
         minX = Mathf.Min(minX, pt.x);
         maxX = Mathf.Max(maxX, pt.x);
         minZ = Mathf.Min(minZ, pt.z);
         maxZ = Mathf.Max(maxZ, pt.z);
         }

      float cellSize = minDistance / Mathf.Sqrt(2);
      int gridWidth = Mathf.CeilToInt((maxX - minX) / cellSize);
      int gridHeight = Mathf.CeilToInt((maxZ - minZ) / cellSize);

      Vector2[,] grid = new Vector2[gridWidth, gridHeight];
      bool[,] gridOccupied = new bool[gridWidth, gridHeight];

      List<Vector2> activeList = new List<Vector2>();

      // Seed first point
      Vector2 firstPoint;
      int seedTries = 0;
      do
         {
         firstPoint = new Vector2(Random.Range(minX, maxX), Random.Range(minZ, maxZ));
         seedTries++;
         if (seedTries > 1000) return result;
         } while (!IsPointInPolygonXZ(spline, firstPoint));

      activeList.Add(firstPoint);
      AddPoint(result, firstPoint, heightProvider);
      grid[(int)((firstPoint.x - minX) / cellSize), (int)((firstPoint.y - minZ) / cellSize)] = firstPoint;
      gridOccupied[(int)((firstPoint.x - minX) / cellSize), (int)((firstPoint.y - minZ) / cellSize)] = true;

      // Early return if target is 1 or less
      if (targetCount.HasValue && result.Count >= targetCount.Value)
         return result;

      while (activeList.Count > 0)
         {
         int idx = Random.Range(0, activeList.Count);
         Vector2 center = activeList[idx];
         bool found = false;

         for (int k = 0; k < maxAttempts; k++)
            {
            float angle = Random.Range(0, Mathf.PI * 2);
            float radius = Random.Range(minDistance, 2 * minDistance);
            Vector2 candidate = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            if (candidate.x < minX || candidate.x > maxX || candidate.y < minZ || candidate.y > maxZ)
               continue;

            if (!IsPointInPolygonXZ(spline, candidate))
               continue;

            int cellX = (int)((candidate.x - minX) / cellSize);
            int cellY = (int)((candidate.y - minZ) / cellSize);

            bool tooClose = false;

            for (int i = Mathf.Max(0, cellX - 2); i <= Mathf.Min(gridWidth - 1, cellX + 2); i++)
               {
               for (int j = Mathf.Max(0, cellY - 2); j <= Mathf.Min(gridHeight - 1, cellY + 2); j++)
                  {
                  if (gridOccupied[i, j])
                     {
                     if (Vector2.Distance(candidate, grid[i, j]) < minDistance)
                        {
                        tooClose = true;
                        break;
                        }
                     }
                  }
               if (tooClose) break;
               }

            if (!tooClose)
               {
               activeList.Add(candidate);
               AddPoint(result, candidate, heightProvider);
               grid[cellX, cellY] = candidate;
               gridOccupied[cellX, cellY] = true;
               found = true;

               // Stop early if we've hit the requested count
               if (targetCount.HasValue && result.Count >= targetCount.Value)
                  return result;

               break;
               }
            }

         if (!found)
            activeList.RemoveAt(idx);
         }

      return result;
      }

   private static void AddPoint(List<Vector3> list, Vector2 pt, System.Func<float, float, float> heightProvider)
      {
      float y = heightProvider != null ? heightProvider(pt.x, pt.y) : 0f;
      list.Add(new Vector3(pt.x, y, pt.y));
      }

   public static bool IsPointInPolygonXZ(List<Vector3> polygon, Vector2 point)
      {
      int crossings = 0;
      int count = polygon.Count;

      for (int i = 0; i < count; i++)
         {
         Vector3 a = polygon[i];
         Vector3 b = polygon[(i + 1) % count];

         if (((a.z > point.y) != (b.z > point.y)) &&
             (point.x < (b.x - a.x) * (point.y - a.z) / (b.z - a.z + Mathf.Epsilon) + a.x))
            {
            crossings++;
            }
         }

      return (crossings % 2) == 1;
      }
   }
