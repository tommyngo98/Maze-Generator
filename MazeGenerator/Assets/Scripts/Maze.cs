using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Maze : MonoBehaviour
{
    [System.Serializable]
    //Cell class consisting of 4 walls
    public class Cell
    {
        public bool visited;
        public GameObject topWall; //3
        public GameObject bottomWall; //4 
        public GameObject leftWall; //2
        public GameObject rightWall; //1
    }

    public Text infoDisplay; //Displays the information for the game
    public Text newTimeDisplay; //Displays the time needed for the current round
    public Text oldTimeDisplay; //Displays the time needed for the previous round

    public Material startColor; //Color for the start position
    public Material targetColor; //Color for the end position

    public GameObject playerBall; //The Player

    public GameObject wall; //Single wall object 
    private GameObject wallDirectory; //Used as a register for all walls.
    private GameObject[] walls; //Stores all walls from the maze

    public Text widthValue; //Width value displayed on the slider
    public Text heightValue; //Height value displayed on the slider

    private int width; //Width of the Maze
    private int height; //Height of the Maze
    public float wallLength = 1; //Length of a wall

    private Vector3 startPos; //Position where you start to build the maze (bottom left corner of the Maze)

    public Cell[] cells; //Stores all cells from the maze
    private int totalCells; //Total amount of cells in the maze
    public int currentCell;

    private int visitedCells; //Amount of visited cells

    private bool startedBuilding; //Check if thre is a current cell instantiated
    private int currentNeighbour; //Index used for the cells[] array
    private List<int> lastCells; //Stores the path of the cells if you need to track back
    private int backtrack; 
    private int wallToBreak;

    //Create the scaffold for the Maze
    void Start()
    {
        //Reset the values
        currentCell = 0;
        visitedCells = 0;
        startedBuilding = false;

        //Parse the slider values to int in order to compute with them
        width = int.Parse(widthValue.text);
        height = int.Parse(heightValue.text);

        //Set the Player to the start position
        playerBall.transform.position = new Vector3(-width / 2, 0.0f, -height / 2 - wallLength / 2);
        playerBall.SetActive(true);

        infoDisplay.text = "Direct the ball to the top right corner! Press Enter to start the Time!";
 
        startPos = new Vector3(-width / 2, 0.0f, -height / 2);
        Vector3 currentPos;
        GameObject currentWall;
        wallDirectory = new GameObject();
        wallDirectory.name = "Maze";

        //Create the vertical walls on the X Axis
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x <= width; x++)
            {
                //Set the tag for the right wall of the top right cell
                if (x == width && y == height -1)
                {
                    currentPos = new Vector3(startPos.x + x * wallLength - wallLength / 2, 0.0f, startPos.z + y * wallLength - wallLength / 2);
                    currentWall = Instantiate(wall, currentPos, Quaternion.identity) as GameObject;
                    currentWall.tag = "Target";
                    currentWall.transform.parent = wallDirectory.transform;
                }
                else
                {
                    currentPos = new Vector3(startPos.x + x * wallLength - wallLength / 2, 0.0f, startPos.z + y * wallLength - wallLength / 2);
                    currentWall = Instantiate(wall, currentPos, Quaternion.identity) as GameObject;
                    currentWall.transform.parent = wallDirectory.transform;
                }
            }
        }

        //Create the horizontal walls on the Y Axis 
        for (int y = 0; y <= height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //Set the tag for the top wall of the top right cell
                if (y == height && x == width - 1)
                {
                    currentPos = new Vector3(startPos.x + x * wallLength, 0.0f, startPos.z + y * wallLength - wallLength);
                    currentWall = Instantiate(wall, currentPos, Quaternion.Euler(0.0f, 90.0f, 0.0f)) as GameObject;
                    currentWall.tag = "Target";
                    currentWall.transform.parent = wallDirectory.transform;
                }
                else
                {
                    currentPos = new Vector3(startPos.x + x * wallLength, 0.0f, startPos.z + y * wallLength - wallLength);
                    currentWall = Instantiate(wall, currentPos, Quaternion.Euler(0.0f, 90.0f, 0.0f)) as GameObject;
                    currentWall.transform.parent = wallDirectory.transform;
                }

            }
        }

        CreateCell();
    }

    //Creat the cells for the maze
    void CreateCell()
    {
        lastCells = new List<int>();
        lastCells.Clear();

        //Create a Cell Array
        totalCells = width * height;
        cells = new Cell[totalCells];


        // Create a Wall Array
        int wallAmount = wallDirectory.transform.childCount;
        walls = new GameObject[wallAmount];

        int leftRight = 0;
        int northSouth = 0;
        int border = 0;

        // Fill the Wall Array with all the walls
        for (int i = 0; i < wallAmount; i++)
        {
            walls[i] = wallDirectory.transform.GetChild(i).gameObject;
        }

        // Create the cells with the correct walls for the maze and storing them in the Cell Array
        for (int i = 0; i < cells.Length; i++)
        {
            // Making sure that the right wall of the last Cell in a horizontal row is not mistakenly seen as the left wall of the cell from the row above
            if (border == width)
            {
                leftRight++;
                border = 0;
            }

            cells[i] = new Cell();
            cells[i].leftWall = walls[leftRight];
            
            // The vertical walls get created first so that the first horizontal wall comes just after them
            cells[i].bottomWall = walls[northSouth + (width + 1) * height];

            leftRight++;
            northSouth++;
            border++;

            cells[i].rightWall = walls[leftRight];
            cells[i].topWall = walls[(northSouth + (width + 1) * height) + width - 1];
        }

        // Mark the start point 
        cells[0].leftWall.GetComponent<MeshRenderer>().material = startColor;
        cells[0].bottomWall.GetComponent<MeshRenderer>().material = startColor;

        // Mark the end point
        cells[cells.Length - 1].rightWall.GetComponent<MeshRenderer>().material = targetColor;
        cells[cells.Length - 1].topWall.GetComponent<MeshRenderer>().material = targetColor;

        GenerateMaze();
    }

    // Break the walls and create the maze 
    void GenerateMaze()
    {
        //As long as there are unvisited cells do that:
        while (visitedCells < totalCells)
        {
            if (startedBuilding)
            {
                FindNeighbours();
                if (cells[currentNeighbour].visited == false && cells[currentCell].visited == true)
                {
                    BreakWall();

                    //Set the state of the visited neighbour to true
                    cells[currentNeighbour].visited = true;

                    //Increment the value of visited cells
                    visitedCells++;

                    //Add the current visited cell to the path list
                    lastCells.Add(currentCell);

                    //Make the neighbour the current cell 
                    currentCell = currentNeighbour;

                    //If the stack (path) is not empty
                    if (lastCells.Count > 0)
                    {
                        backtrack = lastCells.Count - 1;
                    }
                }
            }
            else
            {
                //Start the building at a random cell
                currentCell = Random.Range(0, totalCells);
                cells[currentCell].visited = true;
                visitedCells++;
                startedBuilding = true;
            }
        }        
    }

    //Break the correct wall
    void BreakWall()
    {
        switch (wallToBreak)
        {
            case 1 :
                Destroy(cells[currentCell].rightWall);
                break;
            case 2:
                Destroy(cells[currentCell].leftWall);
                break;
            case 3:
                Destroy(cells[currentCell].topWall);
                break;
            case 4:
                Destroy(cells[currentCell].bottomWall);
                break;
        }   
    }

    //Find the horizontal and vertical neighbouring cells from the current cell
    void FindNeighbours()
    {
        //Actual amount of neighbours per cell
        int neighbourCount = 0;

        //The array stores the positions of the neighbours
        int[] neighbours = new int[4]; 

        //Each cell wall will get a specific number
        int[] wallNum = new int[4];

        int check = 0;
        check = (currentCell + 1) / width;
        check -= 1;
        check *= width;
        check += width;

        //Right neighbour
        if (currentCell + 1 < totalCells && (currentCell + 1) != check)
        {
            if (cells[currentCell + 1].visited == false)
            {
                neighbours[neighbourCount] = currentCell + 1;
                wallNum[neighbourCount] = 1;
                neighbourCount++;
            }
        }
        //Left neighbour
        if (currentCell - 1 >= 0 && currentCell != check)
        {
            if (cells[currentCell - 1].visited == false)
            {
                neighbours[neighbourCount] = currentCell - 1;
                wallNum[neighbourCount] = 2;
                neighbourCount++;
            }
        }
        //Top neighbour
        if (currentCell + width < totalCells)
        {
            if (cells[currentCell + width].visited == false)
            {
                neighbours[neighbourCount] = currentCell + width;
                wallNum[neighbourCount] = 3;
                neighbourCount++;
            }
        }
        //Bottom neighbour
        if (currentCell - width >= 0)
        {
            if (cells[currentCell - width].visited == false)
            {
                neighbours[neighbourCount] = currentCell - width;
                wallNum[neighbourCount] = 4;
                neighbourCount++;
            }
        }

        //Checks whether there are unvisited neighbours of the cell
        if (neighbourCount != 0)
        {
            //Choose a random neighbour for the next step
            int randomNeighbour = Random.Range(0, neighbourCount);

            //Assign the position of the chosen neighbour to the appropriate variable
            currentNeighbour = neighbours[randomNeighbour];

            //wallToBreak stores the number of wall to break (1 to 4, will be used in the breakWall() method) 
            wallToBreak = wallNum[randomNeighbour];
        }
        else
        {   //As long as there are unvisited cells
            if (backtrack > 0)
            {
                //Go back to the previously visited cell
                currentCell = lastCells[backtrack];
                backtrack--;
            }
        }
    }

    //Regenerate the maze
    public void GenerateNewMaze_Click()
    {
        //Destroy the current Maze before creating a new one
        for (int i = 0; i < walls.Length; i++)
        {
            Destroy(walls[i]);
        }
        Destroy(wallDirectory);

        //Restart the process 
        Start();

        float oldTime = 0;
        if (PlayerPrefs.HasKey("oldTime"))
        {
            oldTime = PlayerPrefs.GetFloat("oldTime");
        }
        newTimeDisplay.text = "New Time: 0.0 sec.";
        oldTimeDisplay.text = string.Format("Old Time: {0,6:0.0} sec.", oldTime);
    }

    public void QuitApplication_OnClick()
    {
            Application.Quit();
    }
}
