using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    //Make avaliable in the editor
    [SerializeField] int mapSize = 11;
    [SerializeField] float unitHeightOffset = 1.5f;
    
    public static GameManager instance;
	public GameObject TilePreFab;
	public GameObject PlayerCharacterPreFab;
	public GameObject NonPlayerCharacterPreFab;

    //Create tile and char lists
    //List<List<Tile>> map = new List<List<Tile>>();
    [SerializeField] List<Tile> map;
	List<Character> characters = new List<Character>();
    Character activeCharacter;
	int characterIndex = 0;


    public int MapSize
    {
        get
        {
            return mapSize;
        }
    }

    // Use this for initialization
    void Awake(){
		instance = this;		
	}
	void Start () {
		//generateMap();
		generateCharacters();
        activeCharacter = characters[characterIndex];
	}
	
	// Update is called once per frame
	void Update () {
		characters[characterIndex].TurnUpdate();
	}
	public void nextTurn(){
		if (characterIndex + 1 < characters.Count){
			characterIndex++;
		} else {
			characterIndex = 0;
		}
        activeCharacter = characters[characterIndex];
	}

	public void moveCurrentPlayer(Tile destinationTile) {
        activeCharacter.MoveToTile(destinationTile);
		//characters[charIndex].moveDestination = destinationTile.transform.position + 1.5f * Vector3.up;
	}
	//void generateMap(){
	//	map = new List<List<Tile>>();
	//	for (int i = 0; i < MapSize; i++) {
	//		List <Tile> row = new List <Tile>();
	//		//This loop creates the grid by iterating through i and j coords
	//		for (int j = 0; j < MapSize; j++) {
 //               //Place the tile into the 3d space, y will always be zero
 //               //Tile tile = ((GameObject)Instantiate(TilePreFab, new Vector3(i-Mathf.Floor(mapSize/2), 0, -j+Mathf.Floor(mapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<Tile>();			
 //               Tile tile = ((GameObject)Instantiate(TilePreFab, new Vector3(i - MapSize, 0, -j + MapSize), Quaternion.Euler(new Vector3()))).GetComponent<Tile>();
 //               tile.gridPosition = new Vector2(i,j);
	//			row.Add(tile);
	//		}
	//		map.Add(row);
	//	}
	//}

	//this will create the characters on the map for this level
	void generateCharacters(){
		PlayerCharacter character;
		NonPlayerCharacter npc;

		character = ((GameObject)Instantiate(PlayerCharacterPreFab, new Vector3(0-Mathf.Floor(MapSize/2), 1.5f, 0+Mathf.Floor(MapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<PlayerCharacter>();			
		characters.Add(character);

        //Removed Extra characters for clarity
		
        //character = ((GameObject)Instantiate(PlayerCharacterPreFab, new Vector3((mapSize-1)-Mathf.Floor(mapSize/2), 1.5f, -(mapSize-1)+Mathf.Floor(mapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<PlayerCharacter>();			
		//characters.Add(character);

		//character = ((GameObject)Instantiate(PlayerCharacterPreFab, new Vector3(4-Mathf.Floor(mapSize/2), 1.5f, -4+Mathf.Floor(mapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<PlayerCharacter>();			
		//characters.Add(character);

		//npc = ((GameObject)Instantiate(NonPlayerCharacterPreFab, new Vector3(12-Mathf.Floor(mapSize/2), 1.5f, -4+Mathf.Floor(mapSize/2)), Quaternion.Euler(new Vector3()))).GetComponent<NonPlayerCharacter>();			
		//characters.Add(npc);
	}

    public List<Tile> GetPossibleMoves(Character character)
    {
        List<Tile> possibleMoves = new List<Tile>();
        //Queue<Tile>
        int movementRange = character.GetMovementRange();



        return possibleMoves;
    }
}
