using System;
using System.Collections.Generic;
using UnityEngine;

public class RenderPlane : MonoBehaviour
{
    [SerializeField]
    private Camera camera_;

    [SerializeField]
    private MeshFilter mesh;

    private List<int> triangles = new List<int>();
    private List<Vector3> vertices = new List<Vector3>();

    void Start()
    {

        for(int i = 0; i < triangles.Count; ++i)
        {
            if (i % 3 == 0)
                Debug.LogFormat("Triangle {0}: \n", i / 3);
            
            Debug.LogFormat("Vertex {0}: {1}\n", i, vertices[triangles[i]]);
        }
    }

    void Update()
    {
    }

    private void OnDrawGizmos()
    {
        var halfHeight = camera_.orthographicSize;
        var halfWidth = halfHeight * camera_.aspect;

        var angle = transform.eulerAngles.y * Mathf.Deg2Rad;
        var xCoord = Mathf.Cos(angle) * halfWidth;
        var zCoord = Mathf.Sin(angle) * halfWidth;

        Gizmos.color = Color.red;

        Gizmos.DrawLine(new Vector3(-xCoord, -halfHeight, zCoord), 
                        new Vector3(xCoord, -halfHeight, -zCoord));

        Gizmos.DrawLine(new Vector3(xCoord, -halfHeight, -zCoord), 
                        new Vector3(xCoord, halfHeight, -zCoord));

        Gizmos.DrawLine(new Vector3(xCoord, halfHeight, -zCoord), 
                        new Vector3(-xCoord, halfHeight, zCoord));

        Gizmos.DrawLine(new Vector3(-xCoord, halfHeight, zCoord),
                        new Vector3(-xCoord, -halfHeight, zCoord));

        mesh.sharedMesh.GetTriangles(triangles, 0);
        mesh.sharedMesh.GetVertices(vertices);

        List<Vector3> newVertices = new List<Vector3>();

        // n = (a, 0, c)
        var normal = new Vector3(Mathf.Sin(angle), 0, Mathf.Cos(angle));

        // for each triangle
        for (int i = 0; i < triangles.Count; i += 3)
        {
            Tuple<Vector3, Vector3>[] lines =
            {
                new Tuple<Vector3, Vector3>(vertices[triangles[i]], vertices[triangles[i + 1]]),
                new Tuple<Vector3, Vector3>(vertices[triangles[i]], vertices[triangles[i + 2]]),
                new Tuple<Vector3, Vector3>(vertices[triangles[i + 2]], vertices[triangles[i + 1]])
            };

            // for each line pairing
            foreach (var line in lines)
            {
                // l = A - B
                var lineVec = line.Item1 - line.Item2;

                // l0 = A
                var vertexA = line.Item1;

                // n * l
                var normalLineDotProd = Vector3.Dot(normal, lineVec);

                // -l0 * n
                var vertexNormalDotProd = Vector3.Dot(-vertexA, normal);

                // if n * l = 0
                if (normalLineDotProd == 0)
                {
                    // if -l0 * n != 0
                    if (vertexNormalDotProd != 0)
                        continue;
                    else
                    // else add both A and B to dots
                    {
                        newVertices.Add(line.Item1);
                        newVertices.Add(line.Item2);
                    }
                }
                else
                {
                    // q = - (a * Ax + c * Az) / (a * (Ax - Bx) + c * (Az - Bz))
                    var q = (normal.x * line.Item1.x + normal.z * line.Item1.z) / (normal.x * (line.Item1.x - line.Item2.x) + normal.z * (line.Item1.z - line.Item2.z));

                    if (q > 1 || q < 0)
                        continue;

                    // dot = A + q * (A - B)
                    var vertex = line.Item1 + q * (line.Item2 - line.Item1);

                    newVertices.Add(vertex);
                }
            }
        }

        Gizmos.color = Color.red;

        foreach (var vertex in newVertices)
        {

            Gizmos.DrawWireSphere(vertex, 0.03f);
        }
    }
}
