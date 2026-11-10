using UnityEngine;

public sealed class CorridorPedestrians : MonoBehaviour
{
    [SerializeField, Min(.9f)] private float laneSpacing = 1.3f;

    public void Arrange(int count, SpawnStrip strip)
    {
        int rows = (count + 1) / 2;
        int openLane = Random.Range(0, 3);
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform person = transform.GetChild(i);
            person.gameObject.SetActive(i < count);
            if (i >= count) continue;
            int lane = i % 2;
            if (lane >= openLane) lane++;
            float x = (lane - 1) * laneSpacing + Random.Range(-.1f, .1f);
            person.localPosition = new Vector3(x, 0f, strip.Depth(i / 2, rows));
            if (i % 2 == 1) openLane = (openLane + Random.Range(1, 3)) % 3;
        }
    }
}
