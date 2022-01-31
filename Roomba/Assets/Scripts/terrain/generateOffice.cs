using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class generateOffice : MonoBehaviour
{
    public GameObject setup;
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

    string file = System.IO.File.ReadAllText("Assets/Scripts/terrain/office.txt");


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


        print(columnLength * xSpace);
        print(rowLength * zSpace);


        for (int i = 0; i < columnLength * rowLength; i++)
        {
            // Leer el string
            switch (code[i])
            {
                // a Instanciar arboles
                case 'a':
                    var iTree = Instantiate(setup, new Vector3(x + xSpace * (i % columnLength), 0, z + zSpace * (i / columnLength)), Quaternion.identity);
                    break;
                case 'b':
                    break;
               
            }

        }

        //Instanciar cantidad nRoombas de roombas
        for (int i = 0; i < nRoombas; i++)
        {
            GameObject iRoomba = Instantiate(roomba, new Vector3(x + Random.Range(-2, 2), 0.1f, z + Random.Range(-2, 2)), Quaternion.identity);
            iRoomba.transform.localScale = new Vector3 (0.3f, 0.3f, 0.3f);
            //makeChild(iRoomba, cube);
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


    public void createWaste(int nWaste)
    {
        //if nWaste 
        for (int i = nWaste; i > 0; i--)
        {
            var iWaste = instantiateWaste();
            iWaste.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            //makeChild(iWaste, cube);
            wasteList.Add(iWaste.transform);
        }
    }

    public GameObject instantiateWaste()
    {
        return Instantiate(waste, new Vector3(x + Random.Range(-2, 3), 0.3f, z + Random.Range(0, 10)), Quaternion.identity);

    }

    public void makeChild(GameObject son, GameObject parent)
    {
        son.transform.parent = parent.transform;
    }
}
