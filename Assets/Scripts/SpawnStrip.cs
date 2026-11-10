using UnityEngine;

[System.Serializable]
public struct SpawnStrip
{
    [InspectorName("시작 Z")] public float start;
    [InspectorName("끝 Z")] public float end;
    [InspectorName("최소 줄 간격"), Min(.55f)] public float spacing;

    public SpawnStrip(float start, float end, float spacing)
    {
        this.start = start;
        this.end = end;
        this.spacing = spacing;
    }

    public int Rows => 1 + Mathf.FloorToInt(Mathf.Abs(end - start) / Mathf.Max(.55f, spacing));

    public float Depth(int row, int rows)
    {
        return Mathf.Lerp(start, end, rows == 1 ? .5f : (float)row / (rows - 1));
    }
}
