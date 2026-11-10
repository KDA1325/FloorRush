using UnityEngine;

public sealed class CabinPassengers : MonoBehaviour
{
    [SerializeField] private GameObject[] passengers;
    public int Slots => passengers.Length;

    public void Arrange(int count)
    {
        // Each passenger occupies one authored spot beside the boarding path.
        for (int i = 0; i < passengers.Length; i++)
        {
            if (i < count)
            {
                int pick = Random.Range(i, passengers.Length);
                (passengers[i], passengers[pick]) = (passengers[pick], passengers[i]);
            }
            passengers[i].SetActive(i < count);
        }
    }
}
