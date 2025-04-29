using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CellView : MonoBehaviour
{
    public ICellData BackingData
    { get; private set; }

    [SerializeField] SpriteRenderer tileSpriteRenderer;
    [Header ("Barrier")]
    [SerializeField] Sprite barrierSprite;

    [Header("Forest")]
    [SerializeField] List<Sprite> forestSprite;

    [Header("Mountain")]
    [SerializeField] List<Sprite> mountainSprite;

    [Header("Desert")]
    [SerializeField] List<Sprite> desertSprite;

    [Header("SnowPlains")]
    [SerializeField] List<Sprite> snowSprite;

    [Header("Swamp")]
    [SerializeField] List<Sprite> swampSprite;

    [Header("Ocean")]
    [SerializeField] Sprite oceanSprite;

    [Header("Lava")]
    [SerializeField] Sprite lavaSprite;

    public void SetData(ICellData cell)
    {
        BackingData = cell;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        switch(BackingData.Cost)
        {
            case -1:
                {
                    tileSpriteRenderer.sprite = barrierSprite; // barrier
                    break;
                }
            case 1:
                {
                    int i = Random.Range(0, forestSprite.Count);
                    tileSpriteRenderer.sprite = forestSprite[i]; // grasslands
                    break;
                }
            case 2:
                {
                    int i = Random.Range(0, mountainSprite.Count);

                    tileSpriteRenderer.sprite = mountainSprite[i]; // mountain
                    break;
                }
            case 3:
                {
                    int i = Random.Range(0, desertSprite.Count);

                    tileSpriteRenderer.sprite = desertSprite[i]; // desert
                    break;
                }
            case 4:
                {
                    int i = Random.Range(0, snowSprite.Count);

                    tileSpriteRenderer.sprite = snowSprite[i]; // snowplains
                    break;
                }
            case 5:
                {
                    int i = Random.Range(0, swampSprite.Count);

                    tileSpriteRenderer.sprite = swampSprite[i]; // swamp
                    break;
                }
            case 6:
                {
                    tileSpriteRenderer.sprite = oceanSprite; // ocean
                    break;
                }
            case 7:
                {
                    tileSpriteRenderer.sprite = lavaSprite; // lava
                    break;
                }

        }

        transform.position = BackingData.Coordinate;
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        if (BackingData.Cost < 0)
            return;

        foreach (var conn in BackingData.Neighbours)
        {
            if (conn == null || conn.Cost < 0)
                continue;

            var start = new Vector3(BackingData.Coordinate.x + 0.5f, BackingData.Coordinate.y + 0.5f);
            var end = new Vector3(conn.Coordinate.x + 0.5f, conn.Coordinate.y + 0.5f);
            Debug.DrawLine(start, end, new Color(0.0f, 0.5f, 0.0f, 1.0f));
        }
    }
}