using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public GameObject TBlockPrefab;
    public GameObject OBlockPrefab;
    public GameObject JBlockPrefab;
    public GameObject LBlockPrefab;
    public GameObject IBlockPrefab;
    public GameObject SBlockPrefab;
    public GameObject ZBlockPrefab;
    public GameObject LinesClearedValue;
    public GameObject GameOverText;
    public AudioSource BgmAudioSource;
    public AudioSource LineClearAudioSource;

    private GameObject liveBlock;
    private bool gameStarted;
    private List<GameObject> settledBlocks;
    private int linesCleared = 0;
    private readonly int BOARD_WIDTH = 10;
    private readonly int MAX_SETTLED_Y = 19;
    private readonly Vector3 BLOCK_SPAWN_POSITION = new Vector3(0.5f, 20.5f, 10);

    // Start is called before the first frame update
    void Start()
    {
        gameStarted = false;
        settledBlocks = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        // Start game
        if ((Input.GetKeyDown(KeyCode.Return) || OVRInput.GetDown(OVRInput.RawButton.RIndexTrigger)) && gameStarted == false) 
        {
            gameStarted = true;
            BgmAudioSource.Play();
            GameOverText.SetActive(false);
            InstantiateRandomBlock();
        }

        if (gameStarted)
        {
            TryGameOver();

            if (liveBlock)
            {
                liveBlock.GetComponent<GameBlockController>().TrySettle(settledBlocks);

                // Settle block
                if (liveBlock.GetComponent<GameBlockController>().isSettled) 
                {
                    liveBlock.GetComponent<GameBlockController>().SnapRotation();

                    for (int i = 0; i < liveBlock.transform.childCount - 1; i++)
                    {
                        settledBlocks.Add(liveBlock.transform.GetChild(i).gameObject);
                    }

                    ClearCompletedLines();
                    InstantiateRandomBlock();
                }
            }
        }
    }

    private void ClearCompletedLines()
    {
        // Count lines into dictionary
        Dictionary<int, List<Transform>> blockLines = new Dictionary<int, List<Transform>>();
        for (int i = 0; i < settledBlocks.Count; i++)
        {
            Transform subBlock = settledBlocks[i].transform;
            int subBlockY = (int) Mathf.Floor(subBlock.position.y);
            if (!blockLines.ContainsKey(subBlockY))
            {
                blockLines.Add(subBlockY, new List<Transform>());
            }
            blockLines[subBlockY].Add(subBlock);
        }
        settledBlocks.Clear();

        // Delete completed lines
        IDictionaryEnumerator dictEnum = blockLines.GetEnumerator();
        while (dictEnum.MoveNext())
        {
            if ((dictEnum.Value as List<Transform>).Count >= BOARD_WIDTH)
            {
                LineClearAudioSource.Play();
                linesCleared++;
                LinesClearedValue.GetComponent<UnityEngine.UI.Text>().text = linesCleared.ToString();

                foreach (Transform block in (dictEnum.Value as List<Transform>))
                {
                    Destroy(block.gameObject);
                }

                // Shift above blocks down
                foreach (int key in blockLines.Keys)
                {
                    if (key > (int) dictEnum.Key)
                    {
                        foreach (Transform block in blockLines[key])
                        {
                            block.Translate(new Vector3(0, -1, 0), Space.World);
                        }
                    }
                }
            }
            else 
            {
                // Add processed lines back to list
                foreach (Transform block in (dictEnum.Value as List<Transform>))
                {
                    settledBlocks.Add(block.gameObject);
                }
            }
        }
    }

    private void TryGameOver()
    {
        for (int i = 0; i < settledBlocks.Count; i++)
        {
            Transform subBlock = settledBlocks[i].transform;
            int subBlockY = (int) Mathf.Floor(subBlock.position.y);
            if (subBlockY >= MAX_SETTLED_Y)
            {
                gameStarted = false;
                BgmAudioSource.Stop();
                GameOverText.SetActive(true);
                liveBlock.GetComponent<GameBlockController>().isSettled = true;
            }
        }
    }

    private void InstantiateRandomBlock()
    {
        switch (Mathf.RoundToInt(Random.Range(-0.49f, 6.49f)))
        {
            case 0:
                liveBlock = Instantiate(TBlockPrefab, BLOCK_SPAWN_POSITION, Quaternion.identity) as GameObject;
                break;
            case 1:
                liveBlock = Instantiate(OBlockPrefab, BLOCK_SPAWN_POSITION, Quaternion.identity) as GameObject;
                break;
            case 2:
                liveBlock = Instantiate(JBlockPrefab, BLOCK_SPAWN_POSITION, Quaternion.identity) as GameObject;
                break;
            case 3:
                liveBlock = Instantiate(LBlockPrefab, BLOCK_SPAWN_POSITION, Quaternion.identity) as GameObject;
                break;
            case 4:
                liveBlock = Instantiate(IBlockPrefab, BLOCK_SPAWN_POSITION, Quaternion.identity) as GameObject;
                break;
            case 5:
                liveBlock = Instantiate(SBlockPrefab, BLOCK_SPAWN_POSITION, Quaternion.identity) as GameObject;
                break;
            default:
            case 6:
                liveBlock = Instantiate(ZBlockPrefab, BLOCK_SPAWN_POSITION, Quaternion.identity) as GameObject;
                break;
        }
    }
}
