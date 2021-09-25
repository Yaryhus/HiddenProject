using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [SerializeField] float value;
    [SerializeField] float maxAmmount;
    [SerializeField] float smooth;
    Vector3 initialPosition;

    // Start is called before the first frame update
    void Start()
    {
        initialPosition = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        float x = -Input.GetAxis("Mouse X") * value;
        float y = -Input.GetAxis("Mouse Y") * value;
        x = Mathf.Clamp(x, -maxAmmount, maxAmmount);
        y = Mathf.Clamp(y, -maxAmmount, maxAmmount);
        Vector3 lastPosition = new Vector3(x, y, 0);
        transform.localPosition = Vector3.Lerp(transform.localPosition, lastPosition + initialPosition, Time.deltaTime * smooth);
    }
}
