using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Unity.Mathematics;


[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour
{
    public TileType tileType = TileType.Normal;

    public int xIndex;
    public int yIndex;

    private Board board;

    private SpriteRenderer m_sprite;

    public int breakableValue = 0;

    public Sprite[] breakableSprites;
    public Color normalColor;

    private void Awake()
    {
        m_sprite = FindObjectOfType<SpriteRenderer>();
    }

    public void Init(int x, int y, Board board)
    {
        this.xIndex = x;
        this.yIndex = y;
        this.board = board;

        if (tileType == TileType.Breakable)
        {
            if (breakableSprites[breakableValue] != null)

            {
                m_sprite.sprite = breakableSprites[breakableValue];
            }
        }
    }


    void MouseDown()
    {
        if(board!=null){
        board.ClickTile(this);
        }
    }

    void MouseUp()
    {
        if (board != null)
        {
            board.RelaseTile();
        }
    }

    void MouseEnter()
    {
        if (board != null)
        {
            board.DragTile(this);
        }
    }

    public void BreakTile()
    {
        if (tileType == TileType.Breakable)
        {
            return;
        }
        StartCoroutine(BreakTileRoutine());
    }

    public IEnumerator BreakTileRoutine()
    {
        breakableValue = math.clamp(breakableValue--, 0, breakableValue);
        //
        yield return new WaitForSeconds(0.4f);
        //
        if (breakableSprites[breakableValue] != null)
        {
            m_sprite.sprite = breakableSprites[breakableValue];
        }
        //
        if (breakableValue == 0)
        {
            tileType = TileType.Normal;
            m_sprite.color=normalColor;
        }
    }

   }