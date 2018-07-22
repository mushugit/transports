using UnityEngine;

public class VehiculeRender : MonoBehaviour
{

    public GameObject Truck;
    public GameObject Cargo;
    public GameObject Cargo2;

    public float Speed { get; private set; }

    private Vector3 _finish;

    private const float factor = 30;

    public RoadVehicule vehicule;
    public bool CanMove { get; private set; } = false;

    public static Component Build(Component vehiculePrefab, Vector3 startingPosition, RoadVehicule vehicule)
    {
        var r = Instantiate(vehiculePrefab, startingPosition, Quaternion.identity);
        var rv = r.GetComponent<VehiculeRender>();
        rv.vehicule = vehicule;
        return r;
    }

    public void Init(Cell start, Cell finish, RoadVehiculeCharacteristics type, float position = 0)
    {
        //Debug.Log($"Truck init at {start}, target at {finish} (p={position} s={speed})");
        Speed = type.Speed;
        Cargo2.SetActive(type.Capacity > 1);

        if (finish != null)
        {
            _finish = new Vector3(finish.X, 0, finish.Y);
            if (position != 0)
            {
                transform.position = Vector3.MoveTowards(new Vector3(start.X, 0, start.Y), this._finish, position);
            }
        }
    }

    public void InitColor(Color truckColor, Color cargoColor)
    {
        var tr = Truck.GetComponent<Renderer>();
        tr.material.color = truckColor;
        var cr = Cargo.GetComponent<Renderer>();
        var cr2 = Cargo2.GetComponent<Renderer>();
        cr.material.color = cargoColor;
        cr2.material.color = cargoColor;
    }

    public void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, _finish, factor * Speed * Time.fixedDeltaTime);
    }
}

