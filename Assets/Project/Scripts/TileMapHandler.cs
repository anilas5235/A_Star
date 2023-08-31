using System;
using Project.Scripts.General;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Project.Scripts
{
    public class TileMapHandler : Singleton<TileMapHandler>
    {
        private Tilemap myTilemap;
        [SerializeField] private Tile defaultTile;
        [SerializeField] private Tile redTile,greenTile, blackTile, blueTile;
        [HideInInspector] public Vector2Int size = new Vector2Int(10,10);
        private Camera gameCam;
        
        public enum Tiles
        {
            Default,
            Red,
            Green,
            Black,
            Blue,
        }

        protected override void Awake()
        {
            base.Awake();
            gameCam = Camera.main;
            myTilemap = GetComponent<Tilemap>();
        }

        public void ResetField()
        {
            float width = size.x / gameCam.aspect *.5f+1;
            float height = size.y *.5f+1;
            
            gameCam.orthographicSize = width > height ? width:height;
            gameCam.transform.position = new Vector3(size.x / 2f, size.y / 2f, -10);
            myTilemap.ClearAllTiles();
            FillBox(Vector3Int.zero,defaultTile,0,size.x,0,size.y);
        }

        private void FillBox(Vector3Int center, TileBase tileType, int startX, int endX, int startY, int endY)
        {
            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    myTilemap.SetTile(center+new Vector3Int(x,y,0),tileType);
                }
            }
        }

        public void SetTileColor(Vector3Int pos,Tiles type)
        {
            switch (type)
            {
                case Tiles.Default: myTilemap.SetTile(pos,defaultTile);
                    break;
                case Tiles.Red: myTilemap.SetTile(pos,redTile);
                    break;
                case Tiles.Green: myTilemap.SetTile(pos,greenTile);
                    break;
                case Tiles.Black: myTilemap.SetTile(pos,blackTile);
                    break;
                case Tiles.Blue: myTilemap.SetTile(pos,blueTile);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

        }
    }
}
