using UnityEngine;
using System.Collections.Generic;

public class ScrollingPlatform : MonoBehaviour
{
    [Header("연결할 두 플랫폼")]
    [SerializeField] private Transform platformA;
    [SerializeField] private Transform platformB;
    [SerializeField] private float scrollSpeed = 3f;

    [Header("곡선 지형 (월드 단위)")]
    [Min(0f)] [SerializeField] private float waveHeight = 0.45f;
    [Tooltip("한 번 올라갔다 내려오는 언덕의 폭")]
    [Min(1f)] [SerializeField] private float hillWidth = 8f;
    [Tooltip("언덕 사이의 완전히 평평한 구간 길이")]
    [Min(0f)] [SerializeField] private float flatLength = 24f;
    [Tooltip("높은 평지와 낮은 평지에 머무르는 구간 길이")]
    [Min(0f)] [SerializeField] private float plateauLength = 8f;
    [Range(16, 128)] [SerializeField] private int segments = 64;

    [Header("바닥 아래 채움")]
    [SerializeField] private Color groundFillColor = new Color(0.28f, 0.28f, 0.28f, 1f);
    [Min(1f)] [SerializeField] private float groundFillDepth = 12f;
    [Tooltip("플랫폼 안쪽으로 회색 채움을 겹쳐 렌더링 틈을 가립니다.")]
    [Min(0f)] [SerializeField] private float groundFillOverlap = 0.08f;

    private sealed class Surface
    {
        public Transform transform;
        public SpriteRenderer sprite;
        public PolygonCollider2D collider;
        public GameObject visual;
        public Mesh mesh;
        public GameObject fillVisual;
        public Mesh fillMesh;
        public Bounds bounds;
        public Vector3[] vertices;
        public Vector3[] fillVertices;
        public Vector2[] outline;
        public bool generated;
    }

    private Surface a;
    private Surface b;
    private readonly List<Surface> surfaces = new List<Surface>();
    private float baseTop;
    private float scrollDistance;
    private Camera viewCamera;
    private Material fillMaterial;

    private void Start()
    {
        if (platformA == null || platformB == null || platformA == platformB ||
            !platformA.TryGetComponent(out SpriteRenderer first) || first.sprite == null ||
            !platformB.TryGetComponent(out SpriteRenderer second) || second.sprite == null)
        {
            Debug.LogError("서로 다른 두 바닥과 SpriteRenderer를 연결해주세요.", this);
            enabled = false;
            return;
        }

        segments = Mathf.Clamp(segments, 16, 128);
        viewCamera = Camera.main;

        Shader fillShader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
        if (fillShader == null)
            fillShader = Shader.Find("Sprites/Default");

        if (fillShader == null)
        {
            Debug.LogError("바닥 아래를 채울 Sprite 셰이더를 찾지 못했습니다.", this);
            enabled = false;
            return;
        }

        fillMaterial = new Material(fillShader)
        {
            name = "Runtime Ground Fill",
            color = groundFillColor,
            mainTexture = Texture2D.whiteTexture
        };

        baseTop = first.bounds.max.y;
        a = CreateSurface(platformA, first);
        b = CreateSurface(platformB, second);
        surfaces.Add(a);
        surfaces.Add(b);
        PlaceAfter(b, a);
        EnsureCoverage();
        foreach (Surface surface in surfaces)
            UpdateSurface(surface);
    }

    private Surface CreateSurface(Transform target, SpriteRenderer sprite)
    {
        Surface surface = new Surface
        {
            transform = target,
            sprite = sprite,
            bounds = sprite.sprite.bounds,
            vertices = new Vector3[(segments + 1) * 2],
            fillVertices = new Vector3[(segments + 1) * 2],
            outline = new Vector2[(segments + 1) * 2]
        };
        surface.collider = target.GetComponent<PolygonCollider2D>();
        if (surface.collider == null)
            surface.collider = target.gameObject.AddComponent<PolygonCollider2D>();
        surface.collider.pathCount = 1;
        surface.collider.offset = Vector2.zero;
        surface.collider.isTrigger = false;

        surface.visual = new GameObject("Wave Surface");
        surface.visual.layer = target.gameObject.layer;
        surface.visual.transform.SetParent(target, false);
        surface.mesh = new Mesh { name = "Curved Ground" };
        surface.mesh.MarkDynamic();
        surface.visual.AddComponent<MeshFilter>().sharedMesh = surface.mesh;
        MeshRenderer renderer = surface.visual.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = sprite.sharedMaterial;
        renderer.sortingLayerID = sprite.sortingLayerID;
        renderer.sortingOrder = sprite.sortingOrder;
        MaterialPropertyBlock properties = new MaterialPropertyBlock();
        properties.SetTexture("_MainTex", sprite.sprite.texture);
        renderer.SetPropertyBlock(properties);

        surface.fillVisual = new GameObject("Ground Fill");
        surface.fillVisual.layer = target.gameObject.layer;
        surface.fillVisual.transform.SetParent(target, false);
        surface.fillMesh = new Mesh { name = "Ground Fill" };
        surface.fillMesh.MarkDynamic();
        surface.fillVisual.AddComponent<MeshFilter>().sharedMesh = surface.fillMesh;
        MeshRenderer fillRenderer = surface.fillVisual.AddComponent<MeshRenderer>();
        fillRenderer.sharedMaterial = fillMaterial;
        fillRenderer.sortingLayerID = sprite.sortingLayerID;
        fillRenderer.sortingOrder = sprite.sortingOrder - 1;

        Vector2[] uv = new Vector2[surface.vertices.Length];
        Color[] colors = new Color[uv.Length];
        Color[] fillColors = new Color[uv.Length];
        int[] triangles = new int[segments * 6];
        Vector2[] spriteVertices = sprite.sprite.vertices;
        Vector2[] spriteUv = sprite.sprite.uv;
        ushort[] spriteTriangles = sprite.sprite.triangles;
        int p = spriteTriangles[0], q = spriteTriangles[1], r = spriteTriangles[2];
        Vector2 edge1 = spriteVertices[q] - spriteVertices[p];
        Vector2 edge2 = spriteVertices[r] - spriteVertices[p];
        float determinant = edge1.x * edge2.y - edge1.y * edge2.x;

        // Affine UV mapping also preserves an atlas sprite's packed coordinates.
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Lerp(surface.bounds.min.x, surface.bounds.max.x, (float)i / segments);
            for (int row = 0; row < 2; row++)
            {
                int index = i * 2 + row;
                Vector2 point = new Vector2(x, row == 0 ? surface.bounds.min.y : surface.bounds.max.y);
                Vector2 delta = point - spriteVertices[p];
                float u = (delta.x * edge2.y - delta.y * edge2.x) / determinant;
                float v = (edge1.x * delta.y - edge1.y * delta.x) / determinant;
                uv[index] = spriteUv[p] + u * (spriteUv[q] - spriteUv[p]) + v * (spriteUv[r] - spriteUv[p]);
                colors[index] = sprite.color;
                fillColors[index] = Color.white;
            }
            if (i == segments) continue;
            int t = i * 6, n = i * 2;
            triangles[t] = n; triangles[t + 1] = n + 1; triangles[t + 2] = n + 2;
            triangles[t + 3] = n + 2; triangles[t + 4] = n + 1; triangles[t + 5] = n + 3;
        }
        surface.mesh.vertices = surface.vertices;
        surface.mesh.uv = uv;
        surface.mesh.colors = colors;
        surface.mesh.triangles = triangles;
        surface.fillMesh.vertices = surface.fillVertices;
        surface.fillMesh.colors = fillColors;
        surface.fillMesh.triangles = triangles;
        sprite.enabled = false;
        return surface;
    }

    private void FixedUpdate()
    {
        if (a == null) return;
        scrollDistance += scrollSpeed * Time.fixedDeltaTime;
        Vector3 movement = Vector3.left * scrollSpeed * Time.fixedDeltaTime;
        foreach (Surface surface in surfaces)
            surface.transform.position += movement;

        GetViewEdges(out float left, out float right);
        // Recycle a whole piece only beyond the visible area and its safety margin.
        while (RightEdge(surfaces[0]) < left)
        {
            Surface first = surfaces[0];
            PlaceAfter(first, surfaces[surfaces.Count - 1]);
            surfaces.RemoveAt(0);
            surfaces.Add(first);
        }
        while (LeftEdge(surfaces[surfaces.Count - 1]) > right)
        {
            Surface last = surfaces[surfaces.Count - 1];
            PlaceBefore(last, surfaces[0]);
            surfaces.RemoveAt(surfaces.Count - 1);
            surfaces.Insert(0, last);
        }
        EnsureCoverage();
        foreach (Surface surface in surfaces)
            UpdateSurface(surface);
    }

    private void GetViewEdges(out float left, out float right)
    {
        float center = viewCamera != null ? viewCamera.transform.position.x : 0f;
        float halfWidth = viewCamera != null && viewCamera.orthographic
            ? viewCamera.orthographicSize * viewCamera.aspect : 20f;
        left = center - halfWidth - 2f;
        right = center + halfWidth + 2f;
    }

    private float LeftEdge(Surface surface)
    {
        return surface.transform.TransformPoint(new Vector3(surface.bounds.min.x, 0f, 0f)).x;
    }

    private void PlaceBefore(Surface surface, Surface other)
    {
        surface.transform.position += Vector3.right * (LeftEdge(other) - RightEdge(surface));
    }

    private void EnsureCoverage()
    {
        GetViewEdges(out float left, out float right);
        while (LeftEdge(surfaces[0]) > left)
        {
            Surface extra = CreateExtraSurface();
            PlaceBefore(extra, surfaces[0]);
            surfaces.Insert(0, extra);
        }
        while (RightEdge(surfaces[surfaces.Count - 1]) < right)
        {
            Surface extra = CreateExtraSurface();
            PlaceAfter(extra, surfaces[surfaces.Count - 1]);
            surfaces.Add(extra);
        }
    }

    private Surface CreateExtraSurface()
    {
        GameObject piece = new GameObject("Platform Extension");
        piece.layer = platformA.gameObject.layer;
        piece.transform.SetParent(platformA.parent, false);
        piece.transform.localPosition = platformA.localPosition;
        piece.transform.localRotation = platformA.localRotation;
        piece.transform.localScale = platformA.localScale;
        SpriteRenderer sprite = piece.AddComponent<SpriteRenderer>();
        sprite.sprite = a.sprite.sprite;
        sprite.sharedMaterial = a.sprite.sharedMaterial;
        sprite.color = a.sprite.color;
        sprite.sortingLayerID = a.sprite.sortingLayerID;
        sprite.sortingOrder = a.sprite.sortingOrder;
        Surface result = CreateSurface(piece.transform, sprite);
        result.generated = true;
        return result;
    }

    private float RightEdge(Surface surface)
    {
        return surface.transform.TransformPoint(new Vector3(surface.bounds.max.x, 0f, 0f)).x;
    }

    private void PlaceAfter(Surface surface, Surface other)
    {
        float left = surface.transform.TransformPoint(new Vector3(surface.bounds.min.x, 0f, 0f)).x;
        surface.transform.position += Vector3.right * (RightEdge(other) - left);
    }

    private void UpdateSurface(Surface surface)
    {
        float thickness = surface.bounds.size.y * surface.transform.lossyScale.y;
        float fillBottomY = viewCamera != null && viewCamera.orthographic
            ? viewCamera.transform.position.y - viewCamera.orthographicSize - groundFillDepth
            : baseTop - groundFillDepth;

        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Lerp(surface.bounds.min.x, surface.bounds.max.x, (float)i / segments);
            Vector3 world = surface.transform.TransformPoint(new Vector3(x, 0f, 0f));
            float height = baseTop + GetHillHeight(world.x);
            world.y = height;
            Vector3 top = surface.transform.InverseTransformPoint(world);
            world.y -= thickness;
            Vector3 bottom = surface.transform.InverseTransformPoint(world);
            world.y += Mathf.Min(groundFillOverlap, thickness * 0.75f);
            Vector3 fillTop = surface.transform.InverseTransformPoint(world);
            world.y = fillBottomY;
            Vector3 fillBottom = surface.transform.InverseTransformPoint(world);
            surface.vertices[i * 2] = bottom;
            surface.vertices[i * 2 + 1] = top;
            surface.fillVertices[i * 2] = fillBottom;
            surface.fillVertices[i * 2 + 1] = fillTop;
            surface.outline[i] = top;
            surface.outline[surface.outline.Length - 1 - i] = bottom;
        }
        surface.mesh.vertices = surface.vertices;
        surface.mesh.RecalculateBounds();
        surface.mesh.RecalculateNormals();
        surface.fillMesh.vertices = surface.fillVertices;
        surface.fillMesh.RecalculateBounds();
        surface.collider.SetPath(0, surface.outline);
    }

    private float GetHillHeight(float worldX)
    {
        float width = Mathf.Max(1f, hillWidth);
        float gap = Mathf.Max(0f, flatLength);
        float hold = Mathf.Max(0f, plateauLength);
        float longWidth = width + hold;
        float position = Mathf.Repeat(worldX + scrollDistance, width + 2f * longWidth + 3f * gap);
        float height = Mathf.Max(0f, waveHeight);

        // Short hill -> flat -> raised plateau -> flat -> lowered plateau -> flat.
        if (position < width)
            return (1f - Mathf.Cos(position / width * Mathf.PI * 2f)) * 0.5f * height;
        position -= width + gap;
        if (position < 0f) return 0f;
        if (position < longWidth) return PlateauShape(position, width * 0.5f, hold) * height;
        position -= longWidth + gap;
        if (position < 0f) return 0f;
        if (position < longWidth) return -PlateauShape(position, width * 0.5f, hold) * height;
        return 0f;
    }

    private static float PlateauShape(float position, float rampWidth, float hold)
    {
        if (position < rampWidth)
            return Mathf.SmoothStep(0f, 1f, position / rampWidth);
        if (position < rampWidth + hold)
            return 1f;
        return Mathf.SmoothStep(1f, 0f, (position - rampWidth - hold) / rampWidth);
    }

    private void OnDestroy()
    {
        foreach (Surface surface in surfaces)
            Release(surface);

        if (fillMaterial != null)
            Destroy(fillMaterial);
    }

    private void Release(Surface surface)
    {
        if (surface == null) return;
        if (surface.sprite != null) surface.sprite.enabled = true;
        if (surface.mesh != null) Destroy(surface.mesh);
        if (surface.visual != null) Destroy(surface.visual);
        if (surface.fillMesh != null) Destroy(surface.fillMesh);
        if (surface.fillVisual != null) Destroy(surface.fillVisual);
        if (surface.generated && surface.transform != null) Destroy(surface.transform.gameObject);
    }
}
