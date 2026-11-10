using UnityEngine;

public sealed class CorridorPedestrians : MonoBehaviour
{
    [System.Serializable]
    private struct Pair
    {
        public Transform first;
        public Transform second;
    }

    [SerializeField] private Pair[] rows;
    [SerializeField, Min(.1f)] private float laneSpacing = 1.3f;

    public int Slots => rows.Length * 2;

    public void Arrange(int count)
    {
        int openLane = Random.Range(0, 3);
        int remainingSlots = Slots;
        foreach (var row in rows)
        {
            Place(row.first, openLane == 0 ? 1 : 0, ref count, ref remainingSlots);
            Place(row.second, openLane == 2 ? 1 : 2, ref count, ref remainingSlots);
            // Leave a different passage open at the next pair.
            openLane = (openLane + Random.Range(1, 3)) % 3;
        }
    }

    private void Place(Transform person, int lane, ref int remainingPeople, ref int remainingSlots)
    {
        bool occupied = Random.Range(0, remainingSlots) < remainingPeople;
        remainingSlots--;
        person.gameObject.SetActive(occupied);
        if (!occupied) return;
        remainingPeople--;
        Vector3 position = person.localPosition;
        position.x = (lane - 1) * laneSpacing + Random.Range(-.1f, .1f);
        person.localPosition = position;
    }
}
