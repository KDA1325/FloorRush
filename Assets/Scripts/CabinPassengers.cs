using UnityEngine;

public sealed class CabinPassengers : MonoBehaviour
{
    [SerializeField] private GameObject[] passengers;
    [SerializeField, Min(1)] private int maximumPassengers = 3;

    public void Arrange()
    {
        int count = Random.Range(1, Mathf.Min(maximumPassengers, passengers.Length) + 1);
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
