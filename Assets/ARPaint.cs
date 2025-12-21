using UnityEngine;
using UnityEngine.UI;

public class ARPaint : MonoBehaviour
{
    public Renderer drawingBoardRenderer;
    public FlexibleColorPicker colorPicker;

    [Header("브러시 설정")]
    public Texture2D brushTipTexture;
    [Range(0.01f, 1f)] public float brushOpacity = 1.0f;
    [Range(0.1f, 2f)] public float brushScale = 0.5f;

    [Header("도안 이미지")]
    public Texture2D[] animalPatterns;

    private Texture2D drawingTexture;
    private Texture2D currentOriginalTexture;
    private Color currentBrushColor = Color.black;
    private bool isEraser = false;
    private Vector2? lastUVPosition = null;

    void Start()
    {
        if (animalPatterns.Length > 0)
        {
            ChangePattern(0);
        }
    }

    void Update()
    {
        if (!isEraser && colorPicker != null)
        {
            currentBrushColor = colorPicker.color;
        }

        if (Input.GetMouseButton(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current != null &&
                (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() ||
                 (Input.touchCount > 0 && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))))
            {
                lastUVPosition = null;
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == drawingBoardRenderer.transform)
                {
                    Vector2 currentUV = hit.textureCoord;

                    if (lastUVPosition.HasValue)
                    {
                        DrawLine(lastUVPosition.Value, currentUV);
                    }
                    else
                    {
                        DrawBrush(currentUV);
                    }

                    lastUVPosition = currentUV;
                }
            }
        }
        else
        {
            lastUVPosition = null;
        }
    }

    void DrawLine(Vector2 startUV, Vector2 endUV)
    {
        float distance = Vector2.Distance(startUV, endUV);
        float step = 1.0f / (drawingTexture.width * brushScale * 0.5f);

        for (float t = 0; t <= 1.0f; t += step / distance)
        {
            Vector2 lerpUV = Vector2.Lerp(startUV, endUV, t);
            DrawBrush(lerpUV);
        }
    }

    void DrawBrush(Vector2 uv)
    {
        int textureSize = drawingTexture.width;
        int brushWidth = (int)(brushTipTexture.width * brushScale);
        int brushHeight = (int)(brushTipTexture.height * brushScale);

        int centerX = (int)(uv.x * textureSize);
        int centerY = (int)(uv.y * textureSize);
        int startX = centerX - brushWidth / 2;
        int startY = centerY - brushHeight / 2;

        for (int x = 0; x < brushWidth; x++)
        {
            for (int y = 0; y < brushHeight; y++)
            {
                Color brushPixel = brushTipTexture.GetPixelBilinear((float)x / brushWidth, (float)y / brushHeight);

                if (brushPixel.a <= 0.01f) continue;

                int targetX = startX + x;
                int targetY = startY + y;

                if (targetX >= 0 && targetX < textureSize && targetY >= 0 && targetY < textureSize)
                {
                    Color existingColor = drawingTexture.GetPixel(targetX, targetY);
                    Color targetColor;

                    if (isEraser)
                    {
                        targetColor = currentOriginalTexture.GetPixel(targetX, targetY);
                    }
                    else
                    {
                        targetColor = currentBrushColor;
                    }

                    Color blendedColor = Color.Lerp(existingColor, targetColor, brushPixel.a * brushOpacity);
                    blendedColor.a = 1.0f;

                    drawingTexture.SetPixel(targetX, targetY, blendedColor);
                }
            }
        }
        drawingTexture.Apply();
    }

    public void ChangePattern(int index)
    {
        if (index < 0 || index >= animalPatterns.Length) return;

        currentOriginalTexture = animalPatterns[index];

        drawingTexture = new Texture2D(currentOriginalTexture.width, currentOriginalTexture.height, TextureFormat.RGBA32, false);
        drawingTexture.SetPixels(currentOriginalTexture.GetPixels());
        drawingTexture.Apply();

        drawingBoardRenderer.material.mainTexture = drawingTexture;

        SetDrawMode();
    }

    public void ClearAll()
    {
        drawingTexture.SetPixels(currentOriginalTexture.GetPixels());
        drawingTexture.Apply();
    }

    public void SetEraserMode()
    {
        isEraser = true;
    }
    public void SetDrawMode()
    {
        isEraser = false;
    }
}