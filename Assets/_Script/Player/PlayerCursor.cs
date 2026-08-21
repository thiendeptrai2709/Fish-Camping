using UnityEngine;

public class PlayerCursor : MonoBehaviour
{
    [Header("--- Cursor Lock Settings ---")]
    [SerializeField] private bool lockCursorAtStart = true;

    [Header("--- Custom Stylized Game Cursor ---")]
    [Tooltip("Texture con trỏ chuột tùy chỉnh (để trống sẽ tự động vẽ con trỏ game tuyệt đẹp)")]
    [SerializeField] private Texture2D defaultCursorTexture;
    [SerializeField] private Texture2D clickCursorTexture;
    [SerializeField] private Vector2 hotSpot = new Vector2(3, 3);
    [SerializeField] private CursorMode cursorMode = CursorMode.Auto;

    public static PlayerCursor Instance { get; private set; }

    private bool isCurrentLocked;
    private static Texture2D generatedDefaultCursor;
    private static Texture2D generatedClickCursor;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        InitCustomCursor();
    }

    private void Start()
    {
        ApplyCustomCursor(false);
        SetCursorState(lockCursorAtStart);
    }

    private void Update()
    {
        if (!isCurrentLocked)
        {
            if (Input.GetMouseButtonDown(0))
            {
                ApplyCustomCursor(true);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                ApplyCustomCursor(false);
            }
        }
    }

    private void InitCustomCursor()
    {
        if (defaultCursorTexture == null)
        {
            if (generatedDefaultCursor == null)
            {
                generatedDefaultCursor = CreateStylizedCursorTexture(false);
            }
            defaultCursorTexture = generatedDefaultCursor;
        }

        if (clickCursorTexture == null)
        {
            if (generatedClickCursor == null)
            {
                generatedClickCursor = CreateStylizedCursorTexture(true);
            }
            clickCursorTexture = generatedClickCursor;
        }
    }

    public void ApplyCustomCursor(bool isClicking = false)
    {
        Texture2D targetTex = isClicking ? clickCursorTexture : defaultCursorTexture;
        if (targetTex == null) targetTex = defaultCursorTexture;

        if (targetTex != null)
        {
            Cursor.SetCursor(targetTex, hotSpot, cursorMode);
        }
    }

    // Đặt trạng thái chuột
    public void SetCursorState(bool isLocked)
    {
        isCurrentLocked = isLocked;
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !isLocked;

        if (!isLocked)
        {
            ApplyCustomCursor(false);
        }
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            SetCursorState(isCurrentLocked);
            if (!isCurrentLocked)
            {
                ApplyCustomCursor(false);
            }
        }
    }

    // =========================================================================
    // THUẬT TOÁN VẼ CON TRỎ CHUỘT 3D LOW-POLY ĐA GIÁC ĐỈNH CAO (FACETED SHADING)
    // =========================================================================
    private static Texture2D CreateStylizedCursorTexture(bool isClick)
    {
        int size = 42;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        float scale = isClick ? 0.92f : 1.0f;
        float shift = isClick ? 2f : 0f;

        // Các đỉnh Đa Giác 3D Low-Poly (Tọa độ từ đỉnh góc trên trái)
        Vector2 vTip        = new Vector2(4 + shift, 4 + shift);
        Vector2 vLeft       = new Vector2(4 + shift, 31 * scale + shift);
        Vector2 vCenter     = new Vector2(14 * scale + shift, 22 * scale + shift);
        Vector2 vRight      = new Vector2(31 * scale + shift, 22 * scale + shift);
        Vector2 vTailLeft   = new Vector2(19 * scale + shift, 37 * scale + shift);
        Vector2 vTailBottom = new Vector2(25 * scale + shift, 34 * scale + shift);
        Vector2 vTailInner  = new Vector2(19 * scale + shift, 20 * scale + shift);

        // 1. Mặt Low-Poly Trái (Facet Left - Sáng nhất)
        Vector2[] facetLeft = new Vector2[] { vTip, vLeft, vCenter };

        // 2. Mặt Low-Poly Phải (Facet Right - Vùng chuyển tối)
        Vector2[] facetRight = new Vector2[] { vTip, vCenter, vRight };

        // 3. Mặt Đuôi Trái (Facet Tail Left - Vùng tối nhất)
        Vector2[] facetTailLeft = new Vector2[] { vCenter, vTailLeft, vTailBottom, vTailInner };

        // 4. Mặt Đuôi Phải (Facet Tail Right - Phản xạ môi trường)
        Vector2[] facetTailRight = new Vector2[] { vCenter, vTailInner, vRight };

        // Toàn bộ chu vi ngoài để làm viền đen và đổ bóng
        Vector2[] outerPoly = new Vector2[] { vTip, vLeft, vCenter, vTailLeft, vTailBottom, vTailInner, vRight };

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float visualY = (size - 1) - y;
                Vector2 centerP = new Vector2(x, visualY);

                float leftWeight = 0f;
                float rightWeight = 0f;
                float tailLeftWeight = 0f;
                float tailRightWeight = 0f;
                float shadowWeight = 0f;

                // Super-Sampling Anti-Aliasing 3x3
                for (int sy = -1; sy <= 1; sy++)
                {
                    for (int sx = -1; sx <= 1; sx++)
                    {
                        Vector2 sP = new Vector2(x + sx * 0.33f, visualY + sy * 0.33f);
                        if (IsPointInPolygon(sP, facetLeft)) leftWeight += 1f / 9f;
                        else if (IsPointInPolygon(sP, facetRight)) rightWeight += 1f / 9f;
                        else if (IsPointInPolygon(sP, facetTailLeft)) tailLeftWeight += 1f / 9f;
                        else if (IsPointInPolygon(sP, facetTailRight)) tailRightWeight += 1f / 9f;

                        // Đổ bóng góc (+2.5, +2.5)
                        Vector2 shadowP = new Vector2(sP.x - 2.5f, sP.y - 2.5f);
                        if (IsPointInPolygon(shadowP, outerPoly)) shadowWeight += 1f / 9f;
                    }
                }

                float totalInside = leftWeight + rightWeight + tailLeftWeight + tailRightWeight;

                // Khoảng cách tới đường viền ngoài
                float minOuterDist = 999f;
                for (int i = 0; i < outerPoly.Length; i++)
                {
                    Vector2 pA = outerPoly[i];
                    Vector2 pB = outerPoly[(i + 1) % outerPoly.Length];
                    float dist = DistanceToSegment(centerP, pA, pB);
                    if (dist < minOuterDist) minOuterDist = dist;
                }

                // Khoảng cách tới sống lưng giữa 3D (Spine: vTip -> vCenter)
                float spineDist = DistanceToSegment(centerP, vTip, vCenter);

                Color finalColor = Color.clear;

                // A. Vẽ bóng đổ Low-Poly
                if (shadowWeight > 0f && totalInside < 0.1f)
                {
                    finalColor = new Color(0f, 0f, 0f, 0.45f * shadowWeight);
                }

                // B. Vẽ thân Low-Poly
                if (totalInside > 0f)
                {
                    // 1. Viền ngoài đậm chất Low-Poly (Obsidian Charcoal Rim)
                    if (minOuterDist <= 1.5f)
                    {
                        Color rimColor = isClick ? new Color(0.05f, 0.25f, 0.15f, 1f) : new Color(0.1f, 0.08f, 0.06f, 1f);
                        finalColor = Color.Lerp(finalColor, rimColor, totalInside);
                    }
                    else
                    {
                        Color facetCol = Color.white;

                        // Tùy biến bảng màu Low-Poly (Vàng Kim / Ngọc Lục Bảo)
                        if (!isClick)
                        {
                            // 🌟 DEFAULT: Vàng Kim Loại Hoàng Kim 3D (Low Poly Gold)
                            if (leftWeight > 0.01f)
                                facetCol = Color.Lerp(new Color(1f, 1f, 0.96f, 1f), new Color(1f, 0.92f, 0.65f, 1f), (visualY / size));
                            else if (rightWeight > 0.01f)
                                facetCol = Color.Lerp(new Color(1f, 0.72f, 0.12f, 1f), new Color(0.95f, 0.48f, 0.05f, 1f), (visualY / size));
                            else if (tailLeftWeight > 0.01f)
                                facetCol = Color.Lerp(new Color(0.82f, 0.32f, 0.02f, 1f), new Color(0.65f, 0.2f, 0.02f, 1f), (visualY / size));
                            else if (tailRightWeight > 0.01f)
                                facetCol = Color.Lerp(new Color(0.95f, 0.55f, 0.08f, 1f), new Color(0.78f, 0.38f, 0.04f, 1f), (visualY / size));
                        }
                        else
                        {
                            // ⚡ CLICK: Ngọc Lục Bảo / Pha Lê Xanh (Low Poly Emerald)
                            if (leftWeight > 0.01f)
                                facetCol = Color.Lerp(new Color(0.9f, 1f, 0.95f, 1f), new Color(0.5f, 0.98f, 0.7f, 1f), (visualY / size));
                            else if (rightWeight > 0.01f)
                                facetCol = Color.Lerp(new Color(0.1f, 0.85f, 0.45f, 1f), new Color(0.05f, 0.65f, 0.35f, 1f), (visualY / size));
                            else if (tailLeftWeight > 0.01f)
                                facetCol = Color.Lerp(new Color(0.02f, 0.45f, 0.25f, 1f), new Color(0.01f, 0.3f, 0.15f, 1f), (visualY / size));
                            else if (tailRightWeight > 0.01f)
                                facetCol = Color.Lerp(new Color(0.08f, 0.7f, 0.38f, 1f), new Color(0.04f, 0.5f, 0.28f, 1f), (visualY / size));
                        }

                        // 2. Điểm nhấn sống lưng sắc cạnh Low-Poly 3D (Spine Edge Highlight)
                        if (spineDist <= 1.2f)
                        {
                            facetCol = Color.Lerp(facetCol, Color.white, 0.85f * (1f - spineDist / 1.2f));
                        }

                        finalColor = Color.Lerp(finalColor, facetCol, totalInside);
                    }
                }

                pixels[y * size + x] = finalColor;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    private static bool IsPointInPolygon(Vector2 p, Vector2[] poly)
    {
        bool inside = false;
        for (int i = 0, j = poly.Length - 1; i < poly.Length; j = i++)
        {
            if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
            {
                inside = !inside;
            }
        }
        return inside;
    }

    private static float DistanceToSegment(Vector2 p, Vector2 a, Vector2 b)
    {
        Vector2 ab = b - a;
        float l2 = ab.sqrMagnitude;
        if (l2 == 0f) return (p - a).magnitude;
        float t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / l2);
        Vector2 proj = a + t * ab;
        return (p - proj).magnitude;
    }
}