using Unity.Burst.Intrinsics;
using UnityEngine;

public class DrawArc : MonoBehaviour
{
    public Transform card1;  // Ссылка на объект Card1
    public Transform card2;  // Ссылка на объект Card2
    public LineRenderer lineRenderer;
    public int pointCount = 50;  // Количество точек для отрисовки дуги
    public float arcHeight = 0.005f; // Высота дуги
    public Vector3 offset1;
    public Vector3 offset2;

    void Start()
    {
        lineRenderer = this.GetComponent<LineRenderer>();
        offset1 = new(0, -0.33f, -0.33f);
        offset2 = new(0, -0.33f, -0.33f);
    }

    private void Update()
    {
        DrawCurve(card1, card2);    
    }

    public void DrawCurve(Transform card1, Transform card2)
    {
        this.card1 = card1;
        this.card2 = card2;
        // Преобразуем позиции объектов Card1 и Card2 в локальные координаты
        Vector3 startPoint = card1.parent.localPosition + offset1 + new Vector3(0, card1.localPosition.y,0);
        Vector3 endPoint = card2.parent.localPosition + offset2 + new Vector3(0, card2.localPosition.y, 0);
        arcHeight = -(endPoint - startPoint).magnitude * 0.03f;

        // Массив для хранения точек дуги
        Vector3[] positions = new Vector3[pointCount];

        // Вычисляем точки для отрисовки дуги
        for (int i = 0; i < pointCount; i++)
        {
            float t = i / (float)(pointCount - 1); // Нормализованное значение от 0 до 1
            Vector3 point = Vector3.Lerp(startPoint, endPoint, t); // Линейная интерполяция между startPoint и endPoint

            // Добавляем высоту дуги
            point.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            positions[i] = point;
        }

        // Устанавливаем точки в LineRenderer
        lineRenderer.positionCount = pointCount;
        lineRenderer.SetPositions(positions);
    }
}
