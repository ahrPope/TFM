using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;
using Random = UnityEngine.Random;

public class RoombaAgent : Agent
{

    public float inputDelay = 0.1f;
    public float forwardVel = 10;
    public float rotateVel = 300;

    Quaternion targetRotation;

    public GameObject waste;

    private List<GameObject> actors = new List<GameObject>();

    //Métodos para otorgar recompensas al agente
    //GiveReward() se llama cuando el agente recoge una basura, mientras que GivePunish() se llama cuando el agente está colisionando con una pared
    internal void GiveReward()
    {
        AddReward(+1);
    }

    internal void GivePunish()
    {
        AddReward(-0.1f);
    }

    Rigidbody rBody;
    float forwardInput, turnInput;

    public Quaternion TargetRotation
    {
        get { return targetRotation; }
    }

    public override void Initialize()
    {
        targetRotation = transform.rotation;
        rBody = GetComponent<Rigidbody>();
        forwardInput = turnInput = 0;
    }

    //Al inicio de cada episodio se eliminan todas las basuras que no se han recogido y se vuelven a crear
    public override void OnEpisodeBegin()
    {
        //Al empezar un episodio buscamos todas las basuras y las eliminamos
        DestroyWastes("Waste");
        createWaste(10);

        transform.position = new Vector3(transform.parent.parent.position.x - 8f + Random.Range(3, 15), 0.3f, transform.parent.parent.position.z - 13.5f + Random.Range(3, 30));
    }

    
   //DestroyWastes se encarga de obtener el transform padre y de llamar a DestroyChildObject para destruir todas las basuras de un mismo parque
    public void DestroyWastes(string tag)
    {
        actors.Clear();
        Transform parent = transform.parent.parent;
        DestroyChildObject(parent, tag);
    }

    // DestroyChildObject() se llama de forma recursiva para obtener y finalmente destruir todas las basuras de un mismo parque
    public void DestroyChildObject(Transform parent, string tag)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child.tag == tag)
            {
                actors.Add(child.gameObject);
            }
            if (child.childCount > 0)
            {
                DestroyChildObject(child, tag);
            }
        }

        foreach(GameObject actor in actors)
        {
            Destroy(actor);
        }
    }

    //Metodos para crear las basuras
    public void createWaste(int nWaste)
    {
        //if nWaste 
        for (int i = nWaste; i > 0; i--)
        {
            var iWaste = instantiateWaste();
            makeChild(iWaste, transform.parent.parent.gameObject);
            
        }
    }

    public GameObject instantiateWaste()
    {
        //if 
        return Instantiate(waste, new Vector3(transform.parent.parent.position.x - 8f + Random.Range(3, 15), 0.3f, transform.parent.parent.position.z - 13.5f + Random.Range(3, 30)), Quaternion.identity);

    }

    public void makeChild(GameObject son, GameObject parent)
    {
        son.transform.parent = parent.transform;
    }

    //No hace falta añadir observaciones, ya que el componente de RayPerceptionSensor ya se los añade de forma automatica
    public override void CollectObservations(VectorSensor sensor)
    {

        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.W))
        {
            discreteActionsOut[0] = 0;
        }
        if (Input.GetKey(KeyCode.A))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            discreteActionsOut[0] = 2;
        }



    }

    //Para cada accion que se recibe, se comprueba si la roomba se ha spawneado por debajo del parque y si es así se reinicia el episodio
    //Despues de eso, se mueve al agente y finalmente se añade la recompensa negativa por tiempo.
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        if (transform.position.y < 0)
        {
            EndEpisode();
        }

        MoveAgent(actionBuffers.DiscreteActions);

        

        AddReward(-1f / MaxStep);
    }

    //A cada frame desde los metodos de update se mueve al agente
    private void Update()
    {
        Turn();
    }

    private void FixedUpdate()
    {
        Run();
    }

    //MoveAgent() lo que hace es cambiar los las variables que usan los metodos Run() y Turn()
    public void MoveAgent(ActionSegment<int> act)
    {

        int action = act[0];
        forwardInput = 0;
        turnInput = 0;

        switch (action)
        {
            case 0:
                forwardInput = 1;
                break;
            case 1:
                turnInput = 1;
                break;
            case 2:
                turnInput = -1;
                break;
        }

    }

    //Estos metodos se encargan de mover al agente según los valores de forwardInput y turnInput
    void Run()
    {
        //rBody.velocity = transform.forward * forwardInput * forwardVel;
        if (Mathf.Abs(forwardInput) > inputDelay)
        {
            Vector3 v = transform.forward * forwardInput * forwardVel;
            v[1] = -1f;
            rBody.velocity = v;
        }
        else
        {
            rBody.velocity = new Vector3(0, -1f, 0);
        }
    }

    void Turn()
    {
        targetRotation *= Quaternion.AngleAxis(rotateVel * turnInput * Time.deltaTime, Vector3.up);
        transform.rotation = targetRotation;
    }


}
