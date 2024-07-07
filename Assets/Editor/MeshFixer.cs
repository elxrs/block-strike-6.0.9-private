using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

public class MeshFixer : MonoBehaviour
{
    public class VertexExtension
    {
        public Vector3 position;

        public Color color;

        public Vector3 normal;

        public Vector4 tangent;

        public Vector2 uv0;

        public Vector2 uv2;

        public Vector4 uv3;

        public Vector4 uv4;

        public bool hasPosition;

        public bool hasColor;

        public bool hasNormal;

        public bool hasTangent;

        public bool hasUv0;

        public bool hasUv2;

        public bool hasUv3;

        public bool hasUv4;

        public VertexExtension(bool hasAllValues = false)
        {
            hasPosition = hasAllValues;
            hasColor = hasAllValues;
            hasNormal = hasAllValues;
            hasTangent = hasAllValues;
            hasUv0 = hasAllValues;
            hasUv2 = hasAllValues;
            hasUv3 = hasAllValues;
            hasUv4 = hasAllValues;
        }

        public static VertexExtension[] GetVertices(Mesh m)
        {
            if (m == null)
            {
                return null;
            }
            int vertexCount = m.vertexCount;
            VertexExtension[] array = new VertexExtension[vertexCount];
            Vector3[] vertices = m.vertices;
            Color[] colors = m.colors;
            Vector3[] normals = m.normals;
            Vector4[] tangents = m.tangents;
            Vector2[] uv = m.uv;
            Vector2[] array2 = m.uv2;
            List<Vector4> list = new List<Vector4>();
            List<Vector4> list2 = new List<Vector4>();
            m.GetUVs(2, list);
            m.GetUVs(3, list2);
            bool flag = vertices != null && vertices.Count() == vertexCount;
            bool flag2 = colors != null && colors.Count() == vertexCount;
            bool flag3 = normals != null && normals.Count() == vertexCount;
            bool flag4 = tangents != null && tangents.Count() == vertexCount;
            bool flag5 = uv != null && uv.Count() == vertexCount;
            bool flag6 = array2 != null && array2.Count() == vertexCount;
            bool flag7 = list != null && list.Count() == vertexCount;
            bool flag8 = list2 != null && list2.Count() == vertexCount;
            for (int i = 0; i < vertexCount; i++)
            {
                array[i] = new VertexExtension(false);
                if (flag)
                {
                    array[i].hasPosition = true;
                    array[i].position = vertices[i];
                }
                if (flag2)
                {
                    array[i].hasColor = true;
                    array[i].color = colors[i];
                }
                if (flag3)
                {
                    array[i].hasNormal = true;
                    array[i].normal = normals[i];
                }
                if (flag4)
                {
                    array[i].hasTangent = true;
                    array[i].tangent = tangents[i];
                }
                if (flag5)
                {
                    array[i].hasUv0 = true;
                    array[i].uv0 = uv[i];
                }
                if (flag6)
                {
                    array[i].hasUv2 = true;
                    array[i].uv2 = array2[i];
                }
                if (flag7)
                {
                    array[i].hasUv3 = true;
                    array[i].uv3 = list[i];
                }
                if (flag8)
                {
                    array[i].hasUv4 = true;
                    array[i].uv4 = list2[i];
                }
            }
            return array;
        }

        public static void GetArrays(IList<VertexExtension> vertices, out Vector3[] position, out Color[] color, out Vector2[] uv0, out Vector3[] normal, out Vector4[] tangent, out Vector2[] uv2, out List<Vector4> uv3, out List<Vector4> uv4)
        {
            int count = vertices.Count;
            position = new Vector3[count];
            color = new Color[count];
            uv0 = new Vector2[count];
            normal = new Vector3[count];
            tangent = new Vector4[count];
            uv2 = new Vector2[count];
            uv3 = new List<Vector4>(count);
            uv4 = new List<Vector4>(count);
            for (int i = 0; i < count; i++)
            {
                position[i] = vertices[i].position;
                color[i] = vertices[i].color;
                uv0[i] = vertices[i].uv0;
                normal[i] = vertices[i].normal;
                tangent[i] = vertices[i].tangent;
                uv2[i] = vertices[i].uv2;
                uv3.Add(vertices[i].uv3);
                uv4.Add(vertices[i].uv4);
            }
        }

        public static void SetMesh(Mesh m, IList<VertexExtension> vertices)
        {
            Vector3[] vertices2 = null;
            Color[] colors = null;
            Vector2[] uv = null;
            Vector3[] normals = null;
            Vector4[] tangents = null;
            Vector2[] array = null;
            List<Vector4> list = null;
            List<Vector4> list2 = null;
            VertexExtension VertexExtension = vertices[0];
            VertexExtension.GetArrays(vertices, out vertices2, out colors, out uv, out normals, out tangents, out array, out list, out list2);
            m.Clear();
            if (VertexExtension.hasPosition)
            {
                m.vertices = vertices2;
            }
            if (VertexExtension.hasColor)
            {
                m.colors = colors;
            }
            if (VertexExtension.hasUv0)
            {
                m.uv = uv;
            }
            if (VertexExtension.hasNormal)
            {
                m.normals = normals;
            }
            if (VertexExtension.hasTangent)
            {
                m.tangents = tangents;
            }
            if (VertexExtension.hasUv2)
            {
                m.uv2 = array;
            }
            if (VertexExtension.hasUv3 && list != null)
            {
                m.SetUVs(2, list);
            }
            if (VertexExtension.hasUv4 && list2 != null)
            {
                m.SetUVs(3, list2);
            }
        }
    }

    public static VertexExtension[] GeneratePerTriangleMesh(Mesh m)
    {
        VertexExtension[] vertices = VertexExtension.GetVertices(m);
        int subMeshCount = m.subMeshCount;
        VertexExtension[] array = new VertexExtension[m.triangles.Length];
        int[][] array2 = new int[subMeshCount][];
        int num = 0;
        for (int i = 0; i < subMeshCount; i++)
        {
            array2[i] = m.GetTriangles(i);
            int num2 = array2[i].Length;
            for (int j = 0; j < num2; j++)
            {
                array[num++] = vertices[array2[i][j]];
                array2[i][j] = num - 1;
            }
        }
        VertexExtension.SetMesh(m, array);
        m.subMeshCount = subMeshCount;
        for (int k = 0; k < subMeshCount; k++)
        {
            m.SetTriangles(array2[k], k);
        }
        return array;
    }

    public static void ApplyInverseTransform(Transform transform, Mesh mesh)
    {
        var verts = mesh.vertices;
        var norms = mesh.normals;
        var tans = mesh.tangents;
        var bounds = mesh.bounds;
        for (int i = 0; i < verts.Length; ++i)
        {
            var nvert = verts[i];

            nvert = transform.InverseTransformPoint(nvert);

            verts[i] = nvert;
        }
        for (int i = 0; i < norms.Length; ++i)
        {
            var nnorm = norms[i];

            nnorm = transform.InverseTransformDirection(nnorm);

            norms[i] = nnorm;
        }
        for (int i = 0; i < tans.Length; ++i)
        {
            var ntan = tans[i];

            var transformed = transform.InverseTransformDirection(ntan.x, ntan.y, ntan.z);

            ntan = new Vector4(transformed.x, transformed.y, transformed.z, ntan.w);

            tans[i] = ntan;
        }
        bounds.center = transform.InverseTransformPoint(bounds.center);
        bounds.extents = transform.InverseTransformPoint(bounds.extents);
        mesh.vertices = verts;
        mesh.normals = norms;
        mesh.tangents = tans;
        mesh.bounds = bounds;
    }

    public static void Copy(Mesh destMesh, Mesh src, int firstSubMesh, int subMeshCount, Vector3 origPosition)
    {
        destMesh.Clear();
        destMesh.name = src.name;
        destMesh.vertices = src.vertices;
        destMesh.uv = src.uv;
        destMesh.uv2 = src.uv2;
        destMesh.normals = src.normals;
        destMesh.tangents = src.tangents;
        destMesh.boneWeights = src.boneWeights;
        destMesh.colors = src.colors;
        destMesh.colors32 = src.colors32;
        destMesh.bindposes = src.bindposes;
        destMesh.subMeshCount = subMeshCount;
        for (int i = 0; i < subMeshCount; i++)
        {
            destMesh.SetIndices(src.GetIndices(firstSubMesh + i), src.GetTopology(firstSubMesh + i), i);
        }
    }

    [MenuItem("Tools/Mesh Fixer/Fix Combined Meshes")]
    static void FixMeshes(MenuCommand command)
    {
        Undo.RegisterCompleteObjectUndo(FindObjectsOfType<MeshRenderer>(), "FixMeshesDummyUndo");
        MeshFilter[] allMeshes = FindObjectsOfType<MeshFilter>();
        List<MeshFilter> combinedMeshes = new List<MeshFilter>();
        for (int i = 0; i < allMeshes.Length; i++)
        {
            if (allMeshes[i].sharedMesh != null && allMeshes[i].sharedMesh.name.Contains("Combined Mesh (root"))
            {
                combinedMeshes.Add(allMeshes[i]);
            }
        }
        for (int i = 0; i < combinedMeshes.Count; i++)
        {
            Mesh mesh = combinedMeshes[i].sharedMesh;
            SerializedObject serializedObject = new SerializedObject(combinedMeshes[i].gameObject.GetComponent<MeshRenderer>());
            int firstSubMesh = serializedObject.FindProperty("m_StaticBatchInfo").FindPropertyRelative("firstSubMesh").intValue;
            int subMeshCount = serializedObject.FindProperty("m_StaticBatchInfo").FindPropertyRelative("subMeshCount").intValue;
            Mesh meshNew = new Mesh();
            Copy(meshNew, mesh, firstSubMesh, subMeshCount, combinedMeshes[i].gameObject.transform.position);
            GeneratePerTriangleMesh(meshNew);
            ApplyInverseTransform(combinedMeshes[i].transform, meshNew);
            meshNew.RecalculateNormals();
            meshNew.RecalculateBounds();
            if (combinedMeshes[i].gameObject.GetComponent<MeshCollider>())
            {
                meshNew.name = combinedMeshes[i].gameObject.GetComponent<MeshCollider>().sharedMesh.name;
            }
            else
            {
                meshNew.name = combinedMeshes[i].gameObject.name + " Mesh " + Random.Range(1000, 9999);
            }
            combinedMeshes[i].gameObject.GetComponent<MeshFilter>().sharedMesh = meshNew;
        }
    }

    [MenuItem("Tools/Mesh Fixer/Fix Selected Combined Meshes")]
    static void FixSelectedMeshes(MenuCommand command)
    {
        Undo.RegisterCompleteObjectUndo(FindObjectsOfType<MeshRenderer>(), "FixMeshesSelectedDummyUndo");
        GameObject[] gos = Selection.gameObjects;
        List<MeshFilter> allMeshes = new List<MeshFilter>();
        for (int i = 0; i < gos.Length; i++)
        {
            if (gos[i].GetComponent<MeshFilter>())
            {
                allMeshes.Add(gos[i].GetComponent<MeshFilter>());
            }
        }
        List<MeshFilter> combinedMeshes = new List<MeshFilter>();
        for (int i = 0; i < allMeshes.Count; i++)
        {
            if (allMeshes[i].sharedMesh.name.Contains("Combined Mesh (root"))
            {
                combinedMeshes.Add(allMeshes[i]);
            }
        }
        for (int i = 0; i < combinedMeshes.Count; i++)
        {
            Mesh mesh = combinedMeshes[i].sharedMesh;
            SerializedObject serializedObject = new SerializedObject(combinedMeshes[i].gameObject.GetComponent<MeshRenderer>());
            int firstSubMesh = serializedObject.FindProperty("m_StaticBatchInfo").FindPropertyRelative("firstSubMesh").intValue;
            int subMeshCount = serializedObject.FindProperty("m_StaticBatchInfo").FindPropertyRelative("subMeshCount").intValue;
            Mesh meshNew = new Mesh();
            Copy(meshNew, mesh, firstSubMesh, subMeshCount, combinedMeshes[i].gameObject.transform.position);
            GeneratePerTriangleMesh(meshNew);
            ApplyInverseTransform(combinedMeshes[i].transform, meshNew);
            meshNew.RecalculateNormals();
            meshNew.RecalculateBounds();
            if (combinedMeshes[i].gameObject.GetComponent<MeshCollider>())
            {
                meshNew.name = combinedMeshes[i].gameObject.GetComponent<MeshCollider>().sharedMesh.name;
            }
            else
            {
                meshNew.name = combinedMeshes[i].gameObject.name + " Mesh " + Random.Range(1000, 9999);
            }
            combinedMeshes[i].gameObject.GetComponent<MeshFilter>().sharedMesh = meshNew;
        }
    }
}
