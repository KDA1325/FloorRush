using UnityEngine;

public sealed class CabinPassengers : MonoBehaviour
{
    [SerializeField, Min(.6f)] private float sideOffset = .74f;

    public void Arrange(int count, SpawnStrip strip)
    {
        int slots = strip.Rows * 2;
        int placed = 0;
        for (int slot = 0; slot < slots && placed < count; slot++)
        {
            if (Random.Range(0, slots - slot) >= count - placed) continue;
            Transform person = transform.GetChild(placed++);
            float side = slot % 2 == 0 ? -1f : 1f;
            person.localPosition = new Vector3(side * sideOffset, 0f, strip.Depth(slot / 2, strip.Rows));
            person.localRotation = Quaternion.Euler(0f, 180f + side * 23f, 0f);
        }
        for (int i = 0; i < transform.childCount; i++)
            transform.GetChild(i).gameObject.SetActive(i < placed);
    }
}
