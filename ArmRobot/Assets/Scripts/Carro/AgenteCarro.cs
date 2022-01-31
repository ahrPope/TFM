using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System;

public class AgenteCarro : Agent
{

    public GameObject carroRobot;
    public GameObject robot;
    public GameObject guia;
    public GameObject cube;
    public GameObject goalBox;

    ArticulationBody articulationBody;
    MeshCollider robotMeshCollider;
    Collider carroCollider;
    Rigidbody carroRobotRigibody;
    Vector3 m_EulerAngleVelocity;

    bool cubeIsGripped;

    public bool llegadoACubo = false;

    private float maxCubeX = 2.05f;
    private float minCubeX = 1.55f;
    private float maxCubeZ = 2f;
    private float minCubeZ = -2f;
    private float maxCubeY = 1.5f;
    private float minCubeY = 0.77f;

    private float maxGuiaX = 1.56f;
    private float minGuiaX = -1.77f;
    private float maxGuiaZ = 2f;
    private float minGuiaZ = -2f;
    private float maxGuiaY = 0.71f;
    private float minGuiaY = 0.72f;

    private float maxCarroX = 1.27f;
    private float minCarroX = -1.425f;
    private float maxCarroZ = 1.5f;
    private float minCarroZ = -2f;
    private float maxCarroY = 0.87f;
    private float minCarroY = 0.87f;

    public override void Initialize()
    {
        carroRobotRigibody = carroRobot.GetComponent<Rigidbody>();
        articulationBody = robot.GetComponent<ArticulationBody>();
        robotMeshCollider = robot.GetComponent<MeshCollider>();
        carroCollider = carroRobot.GetComponent<Collider>();
        
    }

    public override void OnEpisodeBegin()
    {


        llegadoACubo = false;
        //carroRobot.transform.localPosition = new Vector3(3.65300012f,2.70749998f, -1.391f);
        carroRobot.transform.localPosition = new Vector3(2.28900003f, 2.70749998f, -1.391f);
        carroRobot.transform.rotation = Quaternion.Euler(0, 90, 0);
        //Vector3 posicionIncialCubo = new Vector3(4.15f, 3.104379f, -1.391f);
        //Vector3 posicionIncialCubo = new Vector3(4.15f,  3.104379f, UnityEngine.Random.Range(0.3f, -3f));

        //cube.transform.localPosition = posicionIncialCubo;

        carroRobotRigibody.velocity = new Vector3(0, 0, 0);
        carroRobotRigibody.angularVelocity = new Vector3(0, 0, 0);

    }


    public override void CollectObservations(VectorSensor sensor)
    {
        //Durante el entrenamiento del carro está puesto el booleano del propio agente llamado "llegadoACubo", pero en el entorno final debería ser sustituido por "cubeIsGripped" que está definido y es accesible desde el brazo robot.

        //sensor.AddObservation(cubeIsGripped);
        sensor.AddObservation(llegadoACubo);
        sensor.AddObservation(normalizarRotacion(carroRobot.transform.rotation.eulerAngles.y));
        //if (!cubeIsGripped)
        if (!llegadoACubo)
        {
            //Orientacion del carro
            

            //Posicion de la pieza
            //Posicion de la guia
            Vector3 direction = cube.transform.position - guia.transform.position;

            //Debug.Log(direction);

            Vector3 directionNormalized;
            directionNormalized.x = normalizarValor(direction.x, 3.7f, 0f);
            directionNormalized.y = 0f;
            directionNormalized.z = normalizarValor(direction.z, 3.9f, -3.8f);

            
            sensor.AddObservation(direction);


        }
        else{

            Vector3 direction = guia.transform.position - goalBox.transform.position;

            

            Vector3 directionNormalized;
            directionNormalized.x = normalizarValor(direction.x, 3.5f, 0.4f);
            directionNormalized.y = 0f;
            directionNormalized.z = normalizarValor(direction.z, 3.6f, -3.8f);

            //Debug.Log(directionNormalized);

            sensor.AddObservation(direction);
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            Debug.Log("b");
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            Debug.Log("b");
            discreteActionsOut[0] = 2;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[0] = 3;
        }

    }

    public void MoveAgent(ActionSegment<int> act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var action = act[0];

        switch (action)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
            case 3:
                rotateDir = transform.up * -1f;
                break;
            case 4:
                break;
        }

        carroRobot.transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f);
        carroRobotRigibody.AddForce(dirToGo * 0.8f, ForceMode.Force);
    }

    


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //Esto es para evitar que el carro se gire
        carroRobotRigibody.angularVelocity = new Vector3(0, 0, 0);
        carroRobot.transform.rotation = Quaternion.Euler(new Vector3(0, carroRobot.transform.rotation.eulerAngles.y, 0));
        MoveAgent(actionBuffers.DiscreteActions);

        articulationBody.TeleportRoot(new Vector3(carroRobot.transform.position.x, 0.76f, carroRobot.transform.position.z), carroRobot.transform.rotation);


        if (!llegadoACubo)
        {
            //AddReward(Math.Abs(1 - normalizarValor(Vector3.Distance(guia.transform.position, cube.transform.position), 5.15f, 0.3f)) / 500);
            if (normalizarValor(Vector3.Distance(cube.transform.position, guia.transform.position), 5, 0.25f) < 0.015f)
            {
                //Debug.Log("ESTA CERCA");

                //AddReward(+0.0001f);
                //Cambiar por una variable que se modifica en el yaml durante el entrenamiento
                //Esto sería parte del curriculum, de momento se modifica de forma manual
                if (false)
                //if (UnityEngine.Random.Range(1000, 0) < 5)
                {
                    //Debug.Log("CAMBIAMOS AL GOAL");
                    SetReward(+1);
                    llegadoACubo = true;
                }
            }

         
        }
        else
        {
            //AddReward(Math.Abs(1 - normalizarValor(Vector3.Distance(guia.transform.position, goalBox.transform.position), 5.15f, 0.2f)) / 500);
            
            if (normalizarValor(Vector3.Distance(goalBox.transform.position, guia.transform.position), 5, 0.25f) < 0.015f)
            {
                //Debug.Log("LLEGADO A CUBO");
                SetReward(+4);
                //EndEpisode();

            }
            
        }
        


        AddReward(-1f / MaxStep);

    }

    public void Finish()
    {
        EndEpisode();
    }

    //HELPER
    public float normalizarValor(float valorRecibido, float valorMaximo, float valorMinimo)
    {
        float valorNormalizado;

        valorNormalizado = (valorRecibido - valorMinimo) / (valorMaximo - valorMinimo);

        if (valorNormalizado <= 0)
        {
            valorNormalizado = 0;

        }
        else if (valorNormalizado >= 1)
        {
            valorNormalizado = 1;
        }

        return valorNormalizado;
    }

    public float normalizarRotacion(float valorRecibido)
    {
        float eliminarVueltas = valorRecibido % 360;


        if (eliminarVueltas < 0)
        {
            eliminarVueltas = Math.Abs(eliminarVueltas);
            eliminarVueltas = 360 - eliminarVueltas;
        }

        float valorNormalizado = (eliminarVueltas - 0) / (360 - 0);

        return valorNormalizado;
    }

}
