using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node
{
    public Tile[] board = new Tile[Constants.NumTiles];
    public Node parent;
    public List<Node> childList = new List<Node>();
    public int type;//Constants.MIN o Constants.MAX
    public double utility;
    public double alfa;
    public double beta;

    public Node(Tile[] tiles)
    {
        for (int i = 0; i < tiles.Length; i++)
        {
            this.board[i] = new Tile();
            this.board[i].value = tiles[i].value;
        }

    }    

}

public class Player : MonoBehaviour
{
    public int turn;    
    private BoardManager boardManager;

    void Start()
    {
        boardManager = GameObject.FindGameObjectWithTag("BoardManager").GetComponent<BoardManager>();
    }
    /*
     * Entrada: Dado un tablero
     * Salida: Posición donde mueve  
     */
    public int SelectTile(Tile[] board)
    {        
             
        //Generamos el nodo raíz del árbol (MAX)
        Node root = new Node(board);
        root.type = Constants.MAX;

        //Generamos primer nivel de nodos hijos
        List<int> selectableTiles = boardManager.FindSelectableTiles(board, turn);


        foreach (int s in selectableTiles)
        {
            //Creo un nuevo nodo hijo con el tablero padre
            Node n = new Node(root.board);
            //Lo añadimos a la lista de nodos hijo
            root.childList.Add(n);
            //Enlazo con su padre
            n.parent = root;
            //En nivel 1, los hijos son MIN
            n.type = Constants.MIN;
            //Aplico un movimiento, generando un nuevo tablero con ese movimiento
            boardManager.Move(n.board, s, turn);
        }

        /**
         *  //Selecciono un movimiento aleatorio. Esto habrá que modificarlo para elegir el mejor movimiento según MINIMAX
         * int movimiento = Random.Range(0, selectableTiles.Count);
         * Segundo nivel
         * Para cada Nodo en la lista de hijos:
         *  -   tendremos un estado de la partida con unas fichas en unos movimientos concretos que obtenemos mediante findselectables 
         *  -   habrá que realizar todo lo anterior:
         *      - Creo un nuevo nodo hijo con el tablero padre
         *      - Lo añadimos a la lista de nodos hijo
         *      - Enlazo con su padre
         *      - En nivel 1, los hijos son MIN
         *      - Aplico un movimiento, generando un nuevo tablero con ese movimiento
         *  -   Calcular su utilidad en referente a piezas que hayan en el tablero y la relacion  a piezas del tablero de cada jugador y posibles futuras
         *  
         */
        foreach (Node nodeFather in root.childList)
        {
            // casillas disponibles en cada hijo en su tablero creado 1 
            //NEGRAS
            List<int> tilesAvailableLevel2 = boardManager.FindSelectableTiles(nodeFather.board, -1 * turn);
            foreach (short tile in tilesAvailableLevel2)
            {
                Node newNodeChild = new Node(nodeFather.board);
                //Lo añadimos a la lista de nodos hijo
               nodeFather.childList.Add(newNodeChild);
                //Enlazo con su padrepiezasJugador
                newNodeChild.parent =nodeFather;
                //En nivel 1, los hijos son MIN
                newNodeChild.type = Constants.MAX;
                //Aplico un movimiento, generando un nuevo tablero con ese movimiento
                boardManager.Move(newNodeChild.board, tile, -1 * turn);
                //si queremos imprimir el nodo generado (tablero hijo)
                boardManager.PrintBoard(newNodeChild.board);
                //Calculamos el valor de la función de utilidad del nodo
                int playerTiles = boardManager.CountPieces(newNodeChild.board, -1 * turn);
                // Cuenta de piezas que lleva la IA
                int iaTiles = boardManager.CountPieces(newNodeChild.board, turn);
                //Sitios disponibles para las piezas de su turno nivel 3
                List<int> tilesAvailableLevel3 = boardManager.FindSelectableTiles(newNodeChild.board, turn);
                // piezas que se cambian entre jugadores
                HashSet<int> turnTile = new HashSet<int>();
                //accedemos a cada ficha disponible 
                foreach (short tileAvailableLevel3 in tilesAvailableLevel3)
                {
                    //accedemos a cada ficha disponible 
                    List<int> _turnTileS = boardManager.FindSwappablePieces(newNodeChild.board, tileAvailableLevel3, turn);
                    // añadimos cada ficha disponible al hashshet como es un hashset no hace falta filtrar repetidos por si acaso ni nada
                    foreach (short swapElement in _turnTileS)
                    {
                        turnTile.Add(swapElement);
                    }
                }
                //calculamos la utilidad de cada nodo como la cuenta disponible en ese nivel 3 y las posibles tiles que realizan un turn de "boss" cambian de pertenecer a uno y la diferencia entre nuestras fichas ia y player
                newNodeChild.utility =  (tilesAvailableLevel3.Count + turnTile.Count + 3*(iaTiles - playerTiles ));
                Debug.Log("player: " + playerTiles + " IA: " + iaTiles + " utility: " + newNodeChild.utility + " casilla: " + tile + " available: " + tilesAvailableLevel3.Count + " turn: " + turnTile.Count);
            }
        }

        //Busqueda del  mejor valor de los hijos en sus padres 
        foreach (Node nodeFather in root.childList)
        {
            double valueMax = 1000;
            double valueMin = -1000;
            //realizamos una busqueda comparando la utilidad de cada hijo 
            foreach (Node newNodeChild in nodeFather.childList)
            {
                if (newNodeChild.utility < valueMax)
                {
                    valueMax = newNodeChild.utility;
                }
                else { }
            }
            // The best children in the hood level 2 
            nodeFather.utility = valueMax;

            if (nodeFather.utility > valueMin)
            {
                nodeFather.parent.utility = nodeFather.utility;
                valueMin = nodeFather.utility;
            }
            else
            {
                //Aqui debemos de ver si esta rama se poda o 
            }
        }

        // mejor movimiento que debe realizar la IA vs tableros del padre actualizado//  utilidad hijo igual a la de su padre  == entonces viene de el
        // Padre 0  Hijo 1||-1 mas indicado movimiento
        foreach (Node nodeFather in root.childList)
        {
            if (nodeFather.utility == nodeFather.parent.utility)
            {
                for (int i = 0; i < Constants.NumTiles; i++)
                {
                    // Padre 0  Hijo 1||-1
                    if (nodeFather.parent.board[i].value == 0 )
                    {
                        if (nodeFather.board[i].value != 0)
                        {
                            Debug.Log("Jugada a " + i);
                            return selectableTiles[i];
                        }
                        else { }
                    }
                    else { }
                }
            }
            else { }
        }
        return 0;
    }
}