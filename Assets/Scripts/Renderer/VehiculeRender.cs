using UnityEngine;

public class VehiculeRender : MonoBehaviour
{

    public GameObject Truck;
    public GameObject Cargo;

    private Vector3 finish;
    public float Speed;
    private float factor = 30;

    public RoadVehicule vehicule;
    public bool CanMove { get; private set; } = false;

    public static Component Build(Component vehiculePrefab, Vector3 startingPosition, RoadVehicule vehicule)
    {
        var r = Instantiate(vehiculePrefab, startingPosition, Quaternion.identity);
        var rv = r.GetComponent<VehiculeRender>();
        rv.vehicule = vehicule;
        return r;
    }

    public void Init(Cell start, Cell finish, float speed, float position = 0)
    {
        //Debug.Log($"Truck init at {start}, target at {finish} (p={position} s={speed})");
        this.Speed = speed;
        if (finish != null)
        {
            this.finish = new Vector3(finish.X, 0, finish.Y);
            if (position != 0)
                transform.position = Vector3.MoveTowards(new Vector3(start.X, 0, start.Y), this.finish, position);
        }
        CheckCanMove();
    }

    public void InitColor(Color truckColor, Color cargoColor)
    {
        var tr = Truck.GetComponent<Renderer>();
        tr.material.color = truckColor;
        var cr = Cargo.GetComponent<Renderer>();
        cr.material.color = cargoColor;
    }

    public void FixedUpdate()
    {
        //Debug.Log($"S={Speed} T={Time.fixedDeltaTime} M={Factor * Speed * Time.fixedDeltaTime}");
        if (CanMove)
            transform.position = Vector3.MoveTowards(transform.position, finish, factor * Speed * Time.fixedDeltaTime);
        else
            CheckCanMove();
    }


    private void CheckCanMove()
    {
        var parent = vehicule.ParentVehicule;
        if (parent == null)
        {
            CanMove = true;
        }
        else
        {
            if (parent.NextVehiculeSteps > RoadVehicule.frameskipToNextVehicule && parent.VehiculeObjetRenderer.CanMove)
            {
                parent.NextLeaving();
                CanMove = true;
            }
        }
    }

    private void OnValidate()
    {
        if (vehicule != null)
            vehicule.Speed = Speed;
    }
}

