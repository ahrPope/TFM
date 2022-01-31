using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    public float inputDelay = 0.1f;
    public float forwardVel = 12;
    public float rotateVel = 100;

    Quaternion targetRotation;
    Vector3 position;
    Rigidbody rBody;
    float forwardInput, turnInput;
    
    public Quaternion TargetRotation
    {
        get {return targetRotation;}
    }
    // Start is called before the first frame update
    void Start()
    {
        targetRotation = transform.rotation;
        position = transform.position;
        rBody = GetComponent<Rigidbody>();
        forwardInput = turnInput = 0;
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
        Turn();
    }

    void FixedUpdate()
    {
        Run();
    }

    public void Run()
    {
        if (Mathf.Abs(forwardInput) > inputDelay )
        {
            Vector3 v = transform.forward * forwardInput * forwardVel;
            v[1] = -1f;
            rBody.velocity = v;
        }
        else{
            rBody.velocity = new Vector3(0, -1f, 0) ;
        }

    }

    void Turn()
    {
        targetRotation *= Quaternion.AngleAxis(rotateVel * turnInput * Time.deltaTime, Vector3.up);
        transform.rotation = targetRotation;
    }

    void GetInput()
    {
        forwardInput = Input.GetAxis("Vertical");
        turnInput = Input.GetAxis("Horizontal");
    }


}
