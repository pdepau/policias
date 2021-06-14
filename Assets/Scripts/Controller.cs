using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour
{
    //GameObjects
    public GameObject board;
    public GameObject[] cops = new GameObject[2];
    public GameObject robber;
    public Text rounds;
    public Text finalMessage;
    public Button playAgainButton;

    //Otras variables
    Tile[] tiles = new Tile[Constants.NumTiles];
    private int roundCount = 0;
    private int state;
    private int clickedTile = -1;
    private int clickedCop = 0;
                    
    void Start()
    {        
        InitTiles();
        InitAdjacencyLists();
        state = Constants.Init;
    }
        
    //Rellenamos el array de casillas y posicionamos las fichas
    void InitTiles()
    {
        for (int fil = 0; fil < Constants.TilesPerRow; fil++)
        {
            GameObject rowchild = board.transform.GetChild(fil).gameObject;            

            for (int col = 0; col < Constants.TilesPerRow; col++)
            {
                GameObject tilechild = rowchild.transform.GetChild(col).gameObject;                
                tiles[fil * Constants.TilesPerRow + col] = tilechild.GetComponent<Tile>();                         
            }
        }
                
        cops[0].GetComponent<CopMove>().currentTile=Constants.InitialCop0;
        cops[1].GetComponent<CopMove>().currentTile=Constants.InitialCop1;
        robber.GetComponent<RobberMove>().currentTile=Constants.InitialRobber;           
    }

    public void InitAdjacencyLists()
    {
        //Matriz de adyacencia
        int[,] matriu = new int[Constants.NumTiles, Constants.NumTiles];

        //TODO: Inicializar matriz a 0's
        for(int i = 0; i <= Constants.NumTiles - 1; i++)
        {
            for(int j=0;j<=Constants.NumTiles-1; j++)
            {
                matriu[i, j] = 0;
            }
        }

        //TODO: Para cada posición, rellenar con 1's las casillas adyacentes (arriba, abajo, izquierda y derecha)
        for(int fila = 0; fila <= Constants.NumTiles - 1; fila++)
        {
            for(int columna=0; columna <= Constants.NumTiles-1; columna++)
            {
                if (Mathf.Abs(fila - columna) == 1 || Mathf.Abs(fila - columna) == 8)
                {
                    matriu[fila, columna] = 1;
                }
                if(fila%8==0 && fila-columna==1)
                {
                    matriu[fila, columna] = 0;
                }
                if((fila+1)%8==0 && fila - columna == -1){
                    matriu[fila, columna] = 0;
                }
            }
        }

        //TODO: Rellenar la lista "adjacency" de cada casilla con los índices de sus casillas adyacentes
        for (int a=0;a<=Constants.NumTiles-1;a++)
        {
            for(int b = 0; b <= Constants.NumTiles - 1; b++)
            {
                if (matriu[a, b] == 1)
                {
                    tiles[a].adjacency.Add(b);

                }
            }
        }
    }

    //Reseteamos cada casilla: color, padre, distancia y visitada
    public void ResetTiles()
    {        
        foreach (Tile tile in tiles)
        {
            tile.Reset();
        }
    }

    public void ClickOnCop(int cop_id)
    {
        switch (state)
        {
            case Constants.Init:
            case Constants.CopSelected:                
                clickedCop = cop_id;
                clickedTile = cops[cop_id].GetComponent<CopMove>().currentTile;
                tiles[clickedTile].current = true;

                ResetTiles();
                FindSelectableTiles(true);

                state = Constants.CopSelected;                
                break;            
        }
    }

    public void ClickOnTile(int t)
    {                     
        clickedTile = t;

        switch (state)
        {            
            case Constants.CopSelected:
                //Si es una casilla roja, nos movemos
                if (tiles[clickedTile].selectable)
                {                  
                    cops[clickedCop].GetComponent<CopMove>().MoveToTile(tiles[clickedTile]);
                    cops[clickedCop].GetComponent<CopMove>().currentTile=tiles[clickedTile].numTile;
                    tiles[clickedTile].current = true;   
                    
                    state = Constants.TileSelected;
                }                
                break;
            case Constants.TileSelected:
                state = Constants.Init;
                break;
            case Constants.RobberTurn:
                state = Constants.Init;
                break;
        }
    }

    public void FinishTurn()
    {
        switch (state)
        {            
            case Constants.TileSelected:
                ResetTiles();

                state = Constants.RobberTurn;
                RobberTurn();
                break;
            case Constants.RobberTurn:                
                ResetTiles();
                IncreaseRoundCount();
                if (roundCount <= Constants.MaxRounds)
                    state = Constants.Init;
                else
                    EndGame(false);
                break;
        }

    }

    public void RobberTurn()
    {
        clickedTile = robber.GetComponent<RobberMove>().currentTile;
        tiles[clickedTile].current = true;
        FindSelectableTiles(false);

        /*TODO: Cambia el código de abajo para hacer lo siguiente
        - Elegimos una casilla aleatoria entre las seleccionables que puede ir el caco
        - Movemos al caco a esa casilla
        - Actualizamos la variable currentTile del caco a la nueva casilla
        */
        List<int> alcanzables = new List<int>();
        for(int i = 0; i <= Constants.NumTiles - 1; i++) {
            Debug.Log("entro" + i);
            Debug.Log(tiles[i].selectable);
            if (tiles[i].selectable == true) 
            {
                alcanzables.Add(tiles[i].numTile);
                Debug.Log(alcanzables.Count);
            }
        }
        int num = Random.Range(0,alcanzables.Count-1);
        Debug.Log(num);
        robber.GetComponent<RobberMove>().MoveToTile(tiles[alcanzables[num]]);
        robber.GetComponent<RobberMove>().currentTile = tiles[alcanzables[num]].numTile;
    }

    public void EndGame(bool end)
    {
        if(end)
            finalMessage.text = "You Win!";
        else
            finalMessage.text = "You Lose!";
        playAgainButton.interactable = true;
        state = Constants.End;
    }

    public void PlayAgain()
    {
        cops[0].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop0]);
        cops[1].GetComponent<CopMove>().Restart(tiles[Constants.InitialCop1]);
        robber.GetComponent<RobberMove>().Restart(tiles[Constants.InitialRobber]);
                
        ResetTiles();

        playAgainButton.interactable = false;
        finalMessage.text = "";
        roundCount = 0;
        rounds.text = "Rounds: ";

        state = Constants.Restarting;
    }

    public void InitGame()
    {
        state = Constants.Init;
         
    }

    public void IncreaseRoundCount()
    {
        roundCount++;
        rounds.text = "Rounds: " + roundCount;
    }

    public void FindSelectableTiles(bool cop)
    {
                 
        int indexcurrentTile;
 
        if (cop==true)
            indexcurrentTile = cops[clickedCop].GetComponent<CopMove>().currentTile;
        else
            indexcurrentTile = robber.GetComponent<RobberMove>().currentTile;

        //La ponemos rosa porque acabamos de hacer un reset
        tiles[indexcurrentTile].current = true;

        //Cola para el BFS
        Queue<Tile> nodes = new Queue<Tile>();

        for(int i=0; i <= tiles.Length-1; i++)
        {
            tiles[i].distance = 3;
        }

        tiles[indexcurrentTile].distance = 0;
        tiles[indexcurrentTile].visited = true;
        nodes.Enqueue(tiles[indexcurrentTile]);

        while (nodes.Count > 0)
        {
            Tile auxiliar = nodes.Dequeue();
            foreach (int adyacente in auxiliar.adjacency)
            {
                if (tiles[adyacente].visited == false)
                {
                    bool libre = true;
                    if (cops[0].GetComponent<CopMove>().currentTile == adyacente)
                    {
                        libre = false;
                    }
                    if (cops[1].GetComponent<CopMove>().currentTile == adyacente)
                    {
                        libre = false;
                    }
                    if (libre == false)
                    {
                        tiles[adyacente].distance = 3;
                        tiles[adyacente].visited = true;
                    }
                    else
                    {
                        tiles[adyacente].distance = auxiliar.distance + 1;
                        tiles[adyacente].visited = true;
                        nodes.Enqueue(tiles[adyacente]);
                    }
                }
            }
        }
        for(int i=0; i < Constants.NumTiles; i++)
        {
            if (tiles[i].distance <= 2)
            {
                tiles[i].selectable = true;
            }
        }
    }
    
   
    

    

   

       
}
