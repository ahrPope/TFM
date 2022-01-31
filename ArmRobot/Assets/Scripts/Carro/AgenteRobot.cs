using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

using Random = UnityEngine.Random;
using System.IO;

public class AgenteRobot : Agent
{

    //Elementos del entorno
    public GameObject endEffector;
    public GameObject cube;
    public GameObject robot;
    public GameObject table1;
    public GameObject table2;
    public GameObject goal;
    public GameObject goalBox;
    public GameObject carro;

    RobotController robotController;
    ArticulationBody articulationBody;
    TargetTouchDetector touchDetector;
    TableTouchDetector tableTouchDetector1;
    TableTouchDetector tableTouchDetector2;
    PincherController pincherController;

    MeshCollider robotMeshCollider;
    Collider carroCollider;

    //Variables del estado de la pieza
    [SerializeField]
    bool cubeIsGripped = false;
    [SerializeField]
    bool cubeIsReleased = false;
    [SerializeField]
    bool midCheckpoint = false;
    //Variables sobre el entrenamiento del brazo robot, todas estas se configuran desde el 
    //La aparicion del cubo es aleatoria
    [SerializeField]
    bool cubeIsRandom = false;

    //Si estamos entrenando al brazo unicamente a agarrar el cubo
    [SerializeField]
    bool grabBoxCurriculum = true;

    //Cual es la distancia maxima de aparicion del cubo
    [SerializeField]
    float distanciaMaximaActual;

    //Al inicio de cada entorno se obtienen las recompensas del archivo yaml y se guardan en el objeto SaveRewards que est� definido m�s abajo
    private SaveRewards rewards = new SaveRewards();

    //Posicion inicial del cubo
    private Vector3 cubeStartingpos;

    //Variables maximas y minimas para la normalizacion del cubo y el robot.
    private float maxHandX = 4.3f;
    private float minHandX = 0.2f;
    private float maxHandZ = 0.55f;
    private float minHandZ = -3.3f;
    private float maxHandY = 3f;
    private float minHandY = 4f;

    private float maxCubeX = 4.3f;
    private float minCubeX = 0.2f;
    private float maxCubeZ = 0.55f;
    private float minCubeZ = -3.3f;
    private float maxCubeY = 0.5f;
    private float minCubeY = 1.5f;

    private float maxRobotX = 3.66f;
    private float minRobotX = 0.86f;
    private float maxRobotZ = 0.35f;
    private float minRobotZ = -3.165f;



    //Se llama la primera vez que se inicializa el entorno, se usa para cargar los valores de entorno del yaml
    public void Awake()
    {


        if (Academy.Instance.EnvironmentParameters.GetWithDefault("randomBox", 0) == 0)
        {
            rewards.randomBox = false;
        }
        else
        {
            rewards.randomBox = true;
        }

        if (Academy.Instance.EnvironmentParameters.GetWithDefault("randomCube", 0) == 0)
        {
            cubeIsRandom = false;
        }
        else
        {
            cubeIsRandom = true;
        }

        if (Academy.Instance.EnvironmentParameters.GetWithDefault("grabBoxCurriculum", 0) == 0)
        {
            grabBoxCurriculum = false;
        }
        else
        {
            grabBoxCurriculum = true;
        }




        rewards.rewardGrip = Academy.Instance.EnvironmentParameters.GetWithDefault("rewardGrip", 0);
        rewards.rewardReleaseInArea = Academy.Instance.EnvironmentParameters.GetWithDefault("rewardReleaseInArea", 0);
        rewards.rewardFinish = Academy.Instance.EnvironmentParameters.GetWithDefault("rewardFinish", 0);
        rewards.rewardCubeKnockedOff = Academy.Instance.EnvironmentParameters.GetWithDefault("rewardCubeKnockedOff", 0);
        rewards.rewardCubeReleased = Academy.Instance.EnvironmentParameters.GetWithDefault("rewardCubeReleased", 0);
        rewards.rewardTime = Academy.Instance.EnvironmentParameters.GetWithDefault("rewardTime", 0);


        robotController = robot.GetComponent<RobotController>();
        articulationBody = robot.GetComponent<ArticulationBody>();
        touchDetector = cube.GetComponent<TargetTouchDetector>();
        tableTouchDetector1 = table1.GetComponent<TableTouchDetector>();
        tableTouchDetector2 = table2.GetComponent<TableTouchDetector>();
        pincherController = endEffector.GetComponent<PincherController>();

        robotMeshCollider = robot.GetComponent<MeshCollider>();
        carroCollider = carro.GetComponent<Collider>();

        Physics.IgnoreCollision(robotMeshCollider, carroCollider, true);

    }


    // AGENT

    //Se ejecuta al inicio de cada episodio
    public override void OnEpisodeBegin()
    {



        //Si queremos la caja de destino es aleatoria se determina su posicion
        if (rewards.randomBox)
        {
            goalBox.transform.localPosition = new Vector3(Random.Range(-0.70f, -0.33f), goalBox.transform.localPosition.y, Random.Range(-0.25f, 0.26f));

        }

        //Se resetean los parametros del robot
        //IMPORTANTE: La rotacion 0.0f es la ultima rotacion que ha sido guardada en el editor de unity
        //TODO: Buscar los valores adecuados para el entenamiento por curriculum y que se modifiquen en ejecucion
        float[] defaultRotations = { 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f };
        robotController.StopAllJointRotations();
        robotController.ForceJointsToRotations(defaultRotations);
        pincherController.ResetGripToOpen();

        cubeIsGripped = false;
        cubeIsReleased = false;
        midCheckpoint = false;

        touchDetector.hasTouchedTarget = false;
        tableTouchDetector1.hasTouchedTable = false;
        tableTouchDetector2.hasTouchedTable = false;



        //Vector3 posicionIncialCubo = new Vector3(4.13f, 3.104379f, -1.391f);
        Vector3 posicionIncialCubo = new Vector3(4.15f, 3.104379f, UnityEngine.Random.Range(-0.8f, -2.3f));

        //distanciaMaximaActual = Academy.Instance.EnvironmentParameters.GetWithDefault("curriculumGrip", 0.11f);


        //Aleatorizacion de la pieza
        if (cubeIsRandom)
        {
            //Cambiar la generacion de la pieza 
            if (true)
            {
                //Si la distancia en X es mas que 0.18 el brazo no llega
                if (distanciaMaximaActual > 0.18)
                {
                    cubeStartingpos = new Vector3(Random.Range(posicionIncialCubo.x - distanciaMaximaActual, posicionIncialCubo.x + 0.18f), posicionIncialCubo.y, Random.Range(posicionIncialCubo.z - distanciaMaximaActual, posicionIncialCubo.z + distanciaMaximaActual));
                }
                else
                {
                    cubeStartingpos = new Vector3(Random.Range((posicionIncialCubo.x - distanciaMaximaActual), posicionIncialCubo.x + distanciaMaximaActual), posicionIncialCubo.y, Random.Range(posicionIncialCubo.z - distanciaMaximaActual, posicionIncialCubo.z + distanciaMaximaActual));
                }
            }
        }
        else
        {
            cubeStartingpos = posicionIncialCubo;
        }

        cube.transform.localPosition = cubeStartingpos;

        //Rotacion de la pieza
        //float rotacionActual = Academy.Instance.EnvironmentParameters.GetWithDefault("rotation", 90f);
        //cube.transform.localRotation = Quaternion.Euler(0, Random.Range(-rotacionActual, rotacionActual), 0);
        cube.transform.localRotation = Quaternion.Euler(0, 0, 0);

        //Eliminamos la velocidad de la pieza
        cube.GetComponent<Rigidbody>().velocity = Vector3.zero;

        
       
        

    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (robotController.joints[0].robotPart == null)
        {
            // No robot is present, no observation should be added
            return;
        }

        //Debug.Log(robot.transform.localPosition);

        //Observacion si la pieza est� agarrada
        sensor.AddObservation(cubeIsGripped);

        //Observacion del agarre de la pinza
        sensor.AddObservation(pincherController.grip);

        if (!cubeIsGripped)
        {
            //sensor.AddObservation(cube.transform.rotation);

            //Observaciones de la rotacion de la pieza y la pinza
            sensor.AddObservation(normalizarValor(cube.transform.rotation.eulerAngles.y, 360, 0));
            sensor.AddObservation(normalizarValor(endEffector.transform.localRotation.eulerAngles.x, 360, 0));

            Vector3 cubePosition = cube.transform.position - robot.transform.position;
            Vector3 cubePositionNormalized;
            cubePositionNormalized.x = normalizarValor(cubePosition.x, 3.6f, 0f);
            cubePositionNormalized.y = normalizarValor(cubePosition.y, 0.8f, -0.2f);
            cubePositionNormalized.z = normalizarValor(cubePosition.z, 3.7f, -3.7f);

            sensor.AddObservation(cubePositionNormalized);

            // Posicion relativa de la pinza
            Vector3 handpos = endEffector.transform.position - robot.transform.position;
            Vector3 handNormalized;

            handNormalized.x = normalizarValor(handpos.x, 0.6f, -0.6f);
            handNormalized.y = normalizarValor(handpos.y, 0.8f, 0f);
            handNormalized.z = normalizarValor(handpos.z, 0.6f, -0.6f);

            sensor.AddObservation(handNormalized);
            //Vector3 endPosition = handNormalized - robotNormalized;
            

            Vector3 handCubePos = cubePosition - handpos;
            Vector3 handCubePosNormalized;
            handCubePosNormalized.x = normalizarValor(handCubePos.x, 0.5f, -0.4f);
            handCubePosNormalized.y = normalizarValor(handCubePos.y, 0f, -0.8f);
            handCubePosNormalized.z = normalizarValor(handCubePos.z, 4f, -3.8f);

            sensor.AddObservation(handCubePosNormalized);
            //Debug.Log(cube.transform.localPosition);
            
            //Posiciones de la caja
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);


        }
        else
        {



            sensor.AddObservation(cube.transform.rotation.eulerAngles.y);
            sensor.AddObservation(endEffector.transform.localRotation.eulerAngles.x);

            //Posiciones del cubo
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(Vector3.zero);

            //Posicion relativa de la caja
            Vector3 robotNormalized;
            robotNormalized.x = normalizarValor(robot.transform.localPosition.x, maxRobotX, minRobotX);
            robotNormalized.y = normalizarValor(robot.transform.localPosition.y, 4f, 3f);
            robotNormalized.z = normalizarValor(robot.transform.localPosition.z, maxRobotZ, minRobotZ);

            Vector3 goalNormalized;
            goalNormalized.x = normalizarValor(goal.transform.localPosition.x, maxCubeX, minCubeX);
            goalNormalized.y = normalizarValor(goal.transform.localPosition.y, maxCubeY, minCubeY);
            goalNormalized.z = normalizarValor(goal.transform.localPosition.z, maxCubeZ, minCubeZ);

            Vector3 goalPosition = goalNormalized - robotNormalized;
            sensor.AddObservation(goalPosition);


            Vector3 handNormalized;

            Vector3 handpos = endEffector.transform.position;

            handNormalized.x = normalizarValor(handpos.x, maxHandX, minHandX);
            handNormalized.y = normalizarValor(handpos.y, maxHandY, minHandY);
            handNormalized.z = normalizarValor(handpos.z, maxHandZ, minHandZ);

            // Posicion relativa de la pinza con la caja
            Vector3 endPosition = handNormalized - robotNormalized;
            sensor.AddObservation(endPosition);
            sensor.AddObservation(goalPosition - endPosition);

        }


        //Observaciones del robot
        sensor.AddObservation(normalizarRotacion(robotController.GetCurrentJointRotations()[0]));
        sensor.AddObservation(normalizarRotacion(robotController.GetCurrentJointRotations()[1]));
        sensor.AddObservation(normalizarRotacion(robotController.GetCurrentJointRotations()[2]));
        sensor.AddObservation(normalizarRotacion(robotController.GetCurrentJointRotations()[3]));
        sensor.AddObservation(normalizarRotacion(robotController.GetCurrentJointRotations()[4]));
        sensor.AddObservation(normalizarRotacion(robotController.GetCurrentJointRotations()[5]));
        sensor.AddObservation(normalizarRotacion(robotController.GetCurrentJointRotations()[6]));




    }

    public override void OnActionReceived(ActionBuffers actions)
    {

        ActionSegment<int> act = actions.DiscreteActions;

        // Movimiento del robot RESTAMOS 1 POR QUE NO SON JOINTS
        for (int jointIndex = 0; jointIndex < act.Length - 1; jointIndex++)
        {
            RotationDirection rotationDirection = ActionIndexToRotationDirection(act[jointIndex]);
            robotController.RotateJoint(jointIndex, rotationDirection, false);

            //Comprobamos que la altura de los joints no sea menor que la altura a la que est� el robot
            if (robotController.joints[jointIndex].robotPart.transform.position.y < 0.8f)
            {
                SetReward(-2f);
                Finish();
            }
        }


        //Movimiento de la pinza
        int pincherInput = act[7];
        pincherController.gripState = GripStateForInput(pincherInput - 1);



        //Si detectamos el agarre se da la recompensa y se activa el flag cubeIsGripped para que solo se de la recompensa una vez
        if (estaAgarrado())
        {
            cubeIsGripped = true;
            carro.GetComponent<AgenteCarro>().llegadoACubo = true;
            

            Debug.Log("Cube Gripped");

            SetReward(+2f);

            //Una peque�a comprobacion para ver los agarres en zonas nuevas
            if (cube.transform.localPosition.x > (cubeStartingpos.x + distanciaMaximaActual - 0.01) || cube.transform.localPosition.x < (cubeStartingpos.x - distanciaMaximaActual + 0.01) && cube.transform.localPosition.z > (cubeStartingpos.z + distanciaMaximaActual - 0.01) || cube.transform.localPosition.z < (cubeStartingpos.z - distanciaMaximaActual + 0.01))
            {
                Debug.Log("A");
            }

            //Si tenemos el curriculum activado esto va a hacer que al principio solo aprenda a agarrar el cubo, una vez pase de un cierto punto avanzar� en el entrenamiento.
            if (grabBoxCurriculum)
            {
                Debug.Log("Estamos en curriculum, asi que reseteamos!");
                carro.GetComponent<AgenteCarro>().Finish();
                Finish();

            }
        }


        if (estaAgarradoConstante())
        {
            cube.transform.position = endEffector.GetComponent<PincherController>().CurrentGraspCenter();
        }

        //Comprobacion para ver si echa el cubo fuera de la zona inicial
        
        if ((cube.transform.localPosition.z < (cubeStartingpos.z - 0.1f) || cube.transform.localPosition.z > (cubeStartingpos.z + 0.1f) || cube.transform.localPosition.x < (cubeStartingpos.x  - 0.1) || cube.transform.localPosition.x > (cubeStartingpos.x + 0.1)) && !cubeIsGripped)
        {
            SetReward(-1f);
            Finish();
        }


        
        



        //Comprobar que la pieza se suelta dentro del area del cubo
        
        //Se calcula lis limites, y si se suelta dentro de este area se da una recompensa
        float lowX = goalBox.transform.localPosition.x - (goal.GetComponent<MeshRenderer>().bounds.size.x / 2);
        float upX = goalBox.transform.localPosition.x + (goal.GetComponent<MeshRenderer>().bounds.size.x / 2);
        float lowZ = goalBox.transform.localPosition.z - (goal.GetComponent<MeshRenderer>().bounds.size.z / 2);
        float upZ = goalBox.transform.localPosition.z + (goal.GetComponent<MeshRenderer>().bounds.size.z / 2);

        
        if (!cubeIsReleased && cubeIsGripped && pincherController.grip < 0.4f && cube.transform.localPosition.x > lowX && cube.transform.localPosition.x < upX && cube.transform.localPosition.z > lowZ && cube.transform.localPosition.z < upZ)
        {
            Debug.Log("Cube released in the area! NUEVO");
            cubeIsReleased = true;
            SetReward(rewards.rewardReleaseInArea);
        }

        //Comprobamos que el cubo ha tocado el area verde de la caja
        if (goal.GetComponent<Goal>().goal || cube.GetComponent<TargetTouchDetector>().hasTouchedTarget)
        {
            Debug.Log("Finished!");
            SetReward(rewards.rewardFinish);
            Finish();
        }
        

        if(cubeIsGripped && cube.transform.localPosition.x < 2.5f && !midCheckpoint)
        {
            midCheckpoint = true;
            AddReward(+1f);
            Debug.Log("Ha pasado la mitad!");
        }

        bool a = tableTouchDetector1.hasTouchedTable;
        bool b = tableTouchDetector2.hasTouchedTable;

        //Si se suelta el cubo y cae en la mesa se penaliza
        if (a || b || cube.transform.localPosition.y < 2.9f)
        {
            cubeIsGripped = false;
            SetReward(rewards.rewardCubeReleased);
            Finish();
        }
        

        AddReward(-1f / MaxStep);

    }

    
    // Controles manuales
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        for (int i = 0; i < robotController.joints.Length; i++)
        {
            float inputVal = Input.GetAxis(robotController.joints[i].inputAxis);
            switch (Mathf.RoundToInt(inputVal))
            {
                case -1:

                    discreteActions[i] = 2;
                    break;
                case 0:
                    discreteActions[i] = 1;
                    break;

                case +1:
                    discreteActions[i] = 0;
                    break;
            }
        }


        float pincherInput = Input.GetAxis("Fingers");


        switch (Mathf.RoundToInt(pincherInput))
        {
            case -1:

                discreteActions[7] = 2;
                break;
            case 0:
                discreteActions[7] = 1;
                break;

            case +1:
                discreteActions[7] = 0;
                break;
        }



    }

    public bool estaAgarrado()
    {

        bool cuboEnPinza = cube.GetComponent<Collider>().bounds.Contains(endEffector.GetComponent<PincherController>().CurrentGraspCenter());
        bool pinzasCerrandas = pincherController.grip > 0.5f;
        bool collidersTocanPieza = pincherController.fingerA.GetComponent<FingerTouch>().isTouchingCube && pincherController.fingerB.GetComponent<FingerTouch>().isTouchingCube;
        bool piezaEnElAire = cube.transform.localPosition.y > 3.125f;

        return cuboEnPinza && pinzasCerrandas && collidersTocanPieza && piezaEnElAire && !cubeIsGripped;
    }

    private bool estaAgarradoConstante()
    {
        bool cuboEnPinza = cube.GetComponent<Collider>().bounds.Contains(endEffector.GetComponent<PincherController>().CurrentGraspCenter());
        bool pinzasCerrandas = pincherController.grip > 0.5f;
        bool collidersTocanPieza = pincherController.fingerA.GetComponent<FingerTouch>().isTouchingCube && pincherController.fingerB.GetComponent<FingerTouch>().isTouchingCube;
        bool piezaEnElAire = cube.transform.localPosition.y > 3.125f;

        return cuboEnPinza && pinzasCerrandas && collidersTocanPieza && piezaEnElAire;
    }


    // HELPERS

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Carro")
        {
            Finish();
        }
    }

    private void Finish()
    {
        carro.GetComponent<AgenteCarro>().Finish();
        EndEpisode();
    }

    //Objeto para guardar recomensas
    public class SaveRewards
    {
        public bool randomBox = false;
        public bool randomCube = false;
        public float rewardGrip = 0.5f;
        public float rewardReleaseInArea = 0.5f;
        public float rewardFinish = 0.5f;
        public float rewardCubeKnockedOff = -1f;
        public float rewardCubeReleased = -1f;
        public float rewardTime = -1f;


    }

    //Direcciones
    static public RotationDirection ActionIndexToRotationDirection(int actionIndex)
    {
        return (RotationDirection)(actionIndex - 1);
    }

    static GripState GripStateForInput(float input)
    {
        if (input > 0)
        {
            return GripState.Closing;
        }
        else if (input < 0)
        {
            return GripState.Opening;
        }
        else
        {
            return GripState.Fixed;
        }
    }

    static RotationDirection GetRotationDirection(float inputVal)
    {
        if (inputVal > 0)
        {
            return RotationDirection.Positive;
        }
        else if (inputVal < 0)
        {
            return RotationDirection.Negative;
        }
        else
        {
            return RotationDirection.None;
        }
    }

    //Funciones para normalizar
    public float normalizarValor(float valorRecibido, float valorMaximo, float valorMinimo)
    {
        float valorNormalizado;

        valorNormalizado = (valorRecibido - valorMinimo) / (valorMaximo - valorMinimo);

        if (valorNormalizado <= 0)
        {
            valorNormalizado = 0;

        }else if (valorNormalizado >= 1)
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


