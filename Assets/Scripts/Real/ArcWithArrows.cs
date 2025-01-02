using System;
using UnityEngine;

public class ArcWithArrows : MonoBehaviour
{
    public Transform startTransform;  // Объект, откуда начинается дуга
    public Transform endTransform;    // Объект, куда заканчивается дуга
    public Vector3 ArcOffset;
    public Vector3 arcMidPointOffset;   // Высота дуги
    public float arrowSize = 0.5f; // Размер стрелок

    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private LineRenderer startArrowRenderer;
    [SerializeField] private LineRenderer endArrowRenderer;

    private void Start()
    {
        lineRenderer.positionCount = 50; // Количество точек на дуге
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        SetupArrow();
        // Установка дуги и стрелок
        CreateArc();
        PlaceArrows();
    }

    private void Update()
    {
        CreateArc();
        PlaceArrows();
    }

    public void SetEventHolder(AnimalArea area)
    {
        area.OnCardPosUpdateChanged += CreateArc;
        area.OnCardPosUpdateChanged += PlaceArrows;

    }

    public void UpdateArcWithArrows(Transform startTransform, Transform endTransform, Vector3 arcOffset, Vector3 midPointOffset, float arrowSize = 0.5f)
    {
        this.startTransform = startTransform;
        this.endTransform = endTransform;
        this.arcMidPointOffset = midPointOffset;
        this.arrowSize = arrowSize;
        this.ArcOffset = arcOffset;
        SetupArrow();
        CreateArc();
        PlaceArrows();
    }

    private void SetupArrow()
    {
        startArrowRenderer.positionCount = 3;  // Стрелка состоит из 3 точек
        startArrowRenderer.startWidth = 0.05f;
        startArrowRenderer.endWidth = 0.05f;

        endArrowRenderer.positionCount = 3;  // Стрелка состоит из 3 точек
        endArrowRenderer.startWidth = 0.05f;
        endArrowRenderer.endWidth = 0.05f;
    }

    public Vector3 GetPositionRelativeFromArea(Transform transform) {
        return this.transform.parent.position - transform.parent.position + ArcOffset;
    }

    private void CreateArc()
    {
        
        Vector3 start = GetPositionRelativeFromArea(startTransform);
        Vector3 end = GetPositionRelativeFromArea(endTransform);

        Vector3 midPoint = (start + end) / 2 + arcMidPointOffset;
        midPoint.z -= (end - start).magnitude*0.1f;
        for (int i = 0; i < lineRenderer.positionCount; i++)
        {

            float t = (float)i / (lineRenderer.positionCount - 1);
            Vector3 pointOnCurve = Vector3.Lerp(Vector3.Lerp(start, midPoint, t), Vector3.Lerp(midPoint, end, t), t);
            lineRenderer.SetPosition(i, pointOnCurve);
        }
    }

    private void PlaceArrows()
    {
        Vector3 start = GetPositionRelativeFromArea(startTransform);
        Vector3 end = GetPositionRelativeFromArea(endTransform);
        // Стрелка на начале дуги
        Vector3 startDirection = (lineRenderer.GetPosition(1) - start).normalized;
        DrawArrow(startArrowRenderer, start, startDirection);

        // Стрелка на конце дуги
        Vector3 endDirection = (end - lineRenderer.GetPosition(lineRenderer.positionCount - 2)).normalized;
        DrawArrow(endArrowRenderer, end, endDirection);
    }

    private void DrawArrow(LineRenderer arrowRenderer, Vector3 position, Vector3 direction)
    {
        // Базовая точка стрелки
        arrowRenderer.SetPosition(0, position);

        // Конечные точки, формирующие "углы" стрелки
        Vector3 arrowHeadLeft = position - (direction * arrowSize) + (Quaternion.Euler(0, 45, 0) * direction * arrowSize);
        Vector3 arrowHeadRight = position - (direction * arrowSize) + (Quaternion.Euler(0, -45, 0) * direction * arrowSize);

        arrowRenderer.SetPosition(1, arrowHeadLeft);
        arrowRenderer.SetPosition(2, arrowHeadRight);
    }
}
