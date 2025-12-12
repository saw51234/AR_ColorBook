using UnityEngine;

public class ARPaint : MonoBehaviour
{
    public Renderer drawingBoardRenderer;
    public int textureSize = 1024;
    public int brushSize = 10;

    public FlexibleColorPicker colorPicker;

    private Texture2D drawingTexture;
    private Color currentBrushColor;

    void Start()
    {
        drawingTexture = new Texture2D(textureSize, textureSize);
        var pixels = drawingTexture.GetPixels();
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        drawingTexture.SetPixels(pixels);
        drawingTexture.Apply();

        drawingBoardRenderer.material.mainTexture = drawingTexture;

        if (colorPicker != null)
        {
            colorPicker.color = Color.black;
        }
    }

    void Update()
    {
        if (colorPicker != null)
        {
            currentBrushColor = colorPicker.color;
        }

        if (Input.GetMouseButton(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
            {
                return;
            }
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform == drawingBoardRenderer.transform)
                {
                    DrawOnTexture(hit.textureCoord);
                }
            }
        }
    }

    void DrawOnTexture(Vector2 uv)
    {
        int x = (int)(uv.x * textureSize);
        int y = (int)(uv.y * textureSize);

        for (int i = -brushSize; i < brushSize; i++)
        {
            for (int j = -brushSize; j < brushSize; j++)
            {
                int targetX = x + i;
                int targetY = y + j;

                if (targetX >= 0 && targetX < textureSize && targetY >= 0 && targetY < textureSize)
                {
                    drawingTexture.SetPixel(targetX, targetY, currentBrushColor);
                }
            }
        }
        drawingTexture.Apply();
    }

    public void SetEraser()
    {
        if (colorPicker != null)
        {
            colorPicker.color = Color.white;
        }
    }
}