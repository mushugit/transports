using UnityEngine;

public class Cam : MonoBehaviour
{
    Vector3 defaultPosition;

    public float moveSpeed = 30f;
    public float minZoom = 2f;
    public float maxZoom = 64f;

    void Center(Vector2 center)
    {
        var backward = transform.forward / -transform.forward.magnitude;
        var maxIteration = (int)maxZoom;
        var iteration = 0;
        for (iteration = 0; iteration < maxIteration; iteration++)
        {
            if (Physics.Raycast(transform.position, Vector3.zero - transform.position))
                break;
            //transform.Translate(backward);
            //transform.SetPositionAndRotation(new Vector3(transform.position.x, transform.position.y, transform.position.x), transform.rotation);
        }
        defaultPosition = transform.position;
    }

    void Start()
    {
        defaultPosition = transform.position;
    }

    void Update()
    {
        /*if (World.gameLoading)
            return;*/

        var r = transform.rotation;
        transform.rotation = new Quaternion(0f, 0f, 0f, 0f);

        var hMove = Input.GetAxis("Horizontal") + Input.GetAxis("Vertical"); //TODO: Ajouter string dans une classe static des constantes
        var vMove = -Input.GetAxis("Horizontal") + Input.GetAxis("Vertical");

        transform.Translate(new Vector3(hMove * moveSpeed * Time.deltaTime, 0, vMove * moveSpeed * Time.deltaTime));

        if (Input.GetButton("ResetView"))
            transform.position = defaultPosition;

        transform.rotation = r;
        Vector3 positionBeforeZoom = transform.position;
        transform.Translate(new Vector3(0f, 0f, Input.GetAxis("Zoom") * Time.deltaTime));

        if (transform.position.y < minZoom || transform.position.y > maxZoom)
            transform.position = positionBeforeZoom;

    }
}
