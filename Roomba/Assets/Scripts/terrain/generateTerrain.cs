using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generateTerrain : MonoBehaviour
{
    public GameObject tree;
    public GameObject bench;
    public GameObject pavement;
    public GameObject l;
    public GameObject roomba;
    public GameObject waste;
    GameObject cube;
    Controller a;

    private float xSpace = 2.6f;
    private float zSpace = 3.5f;
    private float x;
    private float z;
    //public int columnLength = 7;
    //public int rowLength = 9;

    private List<Transform> wasteList;
    private List<GameObject> roombaList;

    string file = System.IO.File.ReadAllText("Assets/Scripts/terrain/terrain.txt");


    private void Awake()
    {
        wasteList = new List<Transform>();
        roombaList = new List<GameObject>();
        x = transform.position.x;
        z = transform.position.z;
        print(file);
        spawn(file);
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Funcion para instanciar los prefabs en forma de malla de tamaño columnLength x rowLength (puedes variarla como quieras)
    public void spawn(string file)
    {
        string code = divideCode(file);
        int columnLength = getN(file, 1);
        int rowLength = getN(file, 2);
        int nRoombas = getN(file, 3);
        int nWaste = getN(file, 4);

        cube = createCube();
        cube.transform.localScale = new Vector3(columnLength * xSpace, 4, rowLength * zSpace);
        createBorders(columnLength * xSpace, rowLength * zSpace);

        print(columnLength * xSpace);
        print(rowLength * zSpace);


        for (int i = 0; i < columnLength * rowLength; i++)
        {
            // Leer el string
            switch (code[i])
            {
                // a Instanciar arboles
                case 'a':
                    var iTree = Instantiate(tree, new Vector3(x + xSpace * (i % columnLength), 0, z + zSpace * (i / columnLength)), Quaternion.identity);
                    makeChild(iTree, cube);
                    break;

                // b Instanciar banco facing left
                case 'b':
                    var iBench = Instantiate(bench, new Vector3(x + xSpace * (i % columnLength), 0, z + zSpace * (i / columnLength)), Quaternion.identity);
                    makeChild(iBench, cube);
                    break;
                // B Instanciar banco facing right
                case 'B':
                    var iBench2 = Instantiate(bench, new Vector3(x + xSpace * (i % columnLength), 0, z + zSpace * (i / columnLength)), Quaternion.Euler(new Vector3(0, 180, 0)));
                    makeChild(iBench2, cube);
                    break;
                // c Instanciar banco facing front
                case 'c':
                    var iBench3 = Instantiate(bench, new Vector3(x + xSpace * (i % columnLength), 0, z + zSpace * (i / columnLength)), Quaternion.Euler(new Vector3(0, 90, 0)));
                    makeChild(iBench3, cube);
                    break;
                // C Instanciar banco facing back
                case 'C':
                    var iBench4 = Instantiate(bench, new Vector3(x + xSpace * (i % columnLength), 0, z + zSpace * (i / columnLength)), Quaternion.Euler(new Vector3(0, -90, 0)));
                    makeChild(iBench4, cube);
                    break;
                // i Instanciar camino facing right
                case 'i':
                    var iPavement = Instantiate(pavement, new Vector3(x + xSpace * (i % columnLength), 0, z + zSpace * (i / columnLength)), Quaternion.identity);
                    makeChild(iPavement, cube);
                    break;
                // I Instanciar camino facing front
                case 'I':
                    var iPavement2 = Instantiate(pavement, new Vector3(x + xSpace * (i % columnLength), 0, z + zSpace * (i / columnLength)), Quaternion.Euler(new Vector3(0, 90, 0)));
                    makeChild(iPavement2, cube);
                    break;
                // l Instanciar codo (actualmente deshabilitado)
                case 'l':
                    var iL = Instantiate(l, new Vector3(x + xSpace * (i % columnLength), 0, z + zSpace * (i / columnLength)), Quaternion.identity);
                    makeChild(iL, cube);
                    break;
            }

        }

        //Instanciar cantidad nRoombas de roombas
        for (int i = 0; i < nRoombas; i++)
        {
            GameObject iRoomba = Instantiate(roomba, new Vector3(x + Random.Range(3, 15), 0.1f, z + Random.Range(3, 15)), Quaternion.identity);
            var emptyObject = new GameObject();
            makeChild(emptyObject, cube);
            makeChild(iRoomba, emptyObject);
            roombaList.Add(iRoomba);
        }

        createWaste(nWaste);


    }

    //Cortar string de spawn
    public string divideCode(string file)
    {
        string[] code = file.Split();
        print(code[0]);
        return code[0];
    }
    //Conseguir numero de txt
    public int getN(string file, int i)
    {
        string[] code = file.Split();
        string[] s = code[i].Split(':');
        int n = int.Parse(s[1]);
        print(n);

        return n;
    }
    // Cubo transparente para agrupar los objetos de cada spawn
    public GameObject createCube()
    {
        cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.GetComponent<BoxCollider>().enabled = false;
        cube.GetComponent<BoxCollider>().isTrigger = false;
        var cubeRender = cube.GetComponent<Renderer>();
        cubeRender.material.SetColor("_Color", Color.clear);
        cubeRender.material.EnableKeyword("_ALPHATEST_ON");

        cube.transform.position = new Vector3(x + 8, 2, z + 13.5f);
        cube.tag = "Border";


        return cube;
    }

    // Bordes para que las roombas no salgan
    // Se recomienda mirar y/o ajustar si al cambiar de layout no coincide perfectamente

    public void createBorders(float columnLength, float rowLength)
    {
        //List<GameObject> borders = new List<GameObject>();
        GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject top = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject bottom = GameObject.CreatePrimitive(PrimitiveType.Cube);

        left.transform.position = new Vector3(x - 2, 0.5f, z + rowLength/2 -2);
        left.transform.localScale = new Vector3(1, 1, 35);
        left.tag = "Border";

        right.transform.position = new Vector3(x + columnLength, 0.5f, z + rowLength/2 - 2);
        right.transform.localScale = new Vector3(1, 1, 35);
        right.tag = "Border";

        top.transform.position = new Vector3(x + columnLength/2 -1, 0.5f, z - columnLength /6);
        top.transform.localScale = new Vector3(19, 1, 1);
        top.tag = "Border";

        bottom.transform.position = new Vector3(x + columnLength/2 - 1, 0.5f, z + rowLength);
        bottom.transform.localScale = new Vector3(19, 1, 1);
        bottom.tag = "Border";
        //return borders;
    }

    public void createWaste(int nWaste)
    {
        //if nWaste 
        for (int i = nWaste; i > 0; i--)
        {
            var iWaste = instantiateWaste();
            makeChild(iWaste, cube);
            wasteList.Add(iWaste.transform);
        }
    }

    public GameObject instantiateWaste()
    {  
        return Instantiate(waste, new Vector3(x + Random.Range(3, 15), 0.3f, z + Random.Range(3, 15)), Quaternion.identity);
       
    }

    public void makeChild(GameObject son, GameObject parent)
    {
        son.transform.parent = parent.transform;
    }
}
