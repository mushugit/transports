using Newtonsoft.Json;

public class RoadVehiculeCharacteristics
{
    [JsonProperty]
    public int Capacity { get; private set; }
    [JsonProperty]
    public float Speed { get; private set; }

    [JsonConstructor]
    public RoadVehiculeCharacteristics(int capacity, float speed)
    {
        Capacity = capacity;
        Speed = speed;
    }

}
