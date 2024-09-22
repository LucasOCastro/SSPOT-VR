﻿using Photon.Pun;
using UnityEngine;

namespace SSpot.Grids
{
    [RequireComponent(typeof(Grid))]
    public class LevelGrid : MonoBehaviourPun
    {
        [SerializeField] private int gridSize = 10;

        public int GridSize => gridSize;

        public Grid InternalGrid { get; private set; }

        private Node[][] _nodes;

        public Node this[int x, int y] => _nodes[x][y];
        public Node this[Vector2Int xy] => this[xy.x, xy.y];
        public Node this[Vector3Int xyz] => this[xyz.x, xyz.y];

        public Vector2Int WorldToCell(Vector3 worldPos)
        {
            var res = InternalGrid.WorldToCell(worldPos);
            return new(res.x, res.y);
        }

        public Vector3 CellToWorld(Vector2Int cell) => InternalGrid.CellToWorld(new(cell.x, cell.y, 0));

        public Vector3 GetCellCenterWorld(Vector2Int cell) => InternalGrid.GetCellCenterWorld(new(cell.x, cell.y, 0));

        public bool InGrid(int coord) => coord >= 0 && coord < gridSize;

        public bool InGrid(Vector2Int cell) => InGrid(cell.x) && InGrid(cell.y);

        public bool InGrid(Vector3 worldPos) => InGrid(WorldToCell(worldPos));
        
        
        private void Awake()
        {
            InternalGrid = GetComponent<Grid>();
            
            _nodes = new Node[gridSize][];
            for (int i = 0; i < gridSize; i++)
            {
                _nodes[i] = new Node[gridSize];
                for (int j = 0; j < gridSize; j++)
                {
                    _nodes[i][j] = new();
                }
            }
            
            foreach (var obj in GetComponentsInChildren<ILevelGridObject>())
            {
                obj.Grid = this;
                this[obj.GridPosition].Objects.Add(obj);
            }
        }

        public void ChangeNode(ILevelGridObject obj, Vector2Int target)
        {
            this[obj.GridPosition].Objects.Remove(obj);
            this[target].Objects.Add(obj);
            obj.GridPosition = target;
            obj.GameObject.transform.position = GetCellCenterWorld(target);
        }
    }
}