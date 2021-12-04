using System;
using System.Collections.Generic;
using UnityEngine;

namespace MainGame
{
    public class MazeGenerator : MonoBehaviour
    {
        public struct Cell
        {
            public bool visited;
            public GameObject north; // 1
            public GameObject east;  // 2
            public GameObject west;  // 3
            public GameObject south; // 4
        }

        public GameObject wall;
        public float wallLength = 2;
        private int xSize = (int)ConstantsList.BOARDWIDTH / 2;
        private int ySize = (int)ConstantsList.BOARDHEIGHT / 2;
        private int yPos = 2;

        private Vector3 _initialPos;
        private GameObject _wallHolder;
        private Cell[] _cells;

        private int _currentCellIndex;
        private int _currentNeighborIndex;
        private int _totalCells;
        private int visitedCells = 0;
        private bool startBuilding = false;
        private List<int> lastCells = new List<int>();
        private int _backingUp;
        private int wallToBreak;

        private void Start()
        {
            
        }

        private void OnDestroy()
        {
            _cells = new Cell[0];
            Array.Clear(_cells, 0, _cells.Length);
            lastCells.Clear();
        }

        public void CreateWalls()
        {
            _wallHolder = new GameObject();
            _wallHolder.name = "Maze";
            _initialPos = new Vector3((-xSize / 2) + wallLength / 2, yPos, (-ySize / 2) + wallLength /2);
            Vector3 myPos = _initialPos;
            GameObject tempWall;

            // for x axis
            for (int i = 0; i < ySize; i++)
            {
                for (int j = 0; j <= xSize; j++)
                {
                    myPos = new Vector3(_initialPos.x + (j * wallLength) - wallLength / 2,
                                        yPos,
                                        _initialPos.z + (i * wallLength) - wallLength / 2);
                    tempWall = Instantiate(wall, myPos, Quaternion.identity);
                    tempWall.transform.SetParent(_wallHolder.transform);
                }
            }

            // for y axis
            for (int i = 0; i <= ySize; i++)
            {
                for (int j = 0; j < xSize; j++)
                {
                    myPos = new Vector3(_initialPos.x + (j * wallLength),
                                        yPos,
                                        _initialPos.z + (i * wallLength) - wallLength);
                    tempWall = Instantiate(wall, myPos, Quaternion.Euler(0f, 90f, 0f));
                    tempWall.transform.SetParent(_wallHolder.transform);
                }
            }

            CreateCells();
        }

        private void CreateCells()
        {
            _totalCells = xSize * ySize;
            int children = _wallHolder.transform.childCount;
            GameObject[] allWalls = new GameObject[children];
            _cells = new Cell[xSize * ySize];

            // get all children
            for (int i = 0; i < children; i++)
            {
                allWalls[i] = _wallHolder.transform.GetChild(i).gameObject;
            }

            int eastWestProcess = 0;
            int childProcess = 0;
            int termCount = 0;
            // assign walls to the cells
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i] = new Cell();
                _cells[i].east = allWalls[eastWestProcess];
                _cells[i].south = allWalls[childProcess + (xSize + 1) * ySize];

                if (termCount == xSize)
                {
                    eastWestProcess+=2;
                    termCount = 0;
                }
                else eastWestProcess++;

                termCount++;
                childProcess++;

                _cells[i].west = allWalls[eastWestProcess];
                _cells[i].north = allWalls[(childProcess + (xSize + 1) * ySize) + xSize - 1];
            }

            CreateMaze();
        }

        private void CreateMaze()
        {
            int count = 0;
            while (visitedCells < _totalCells)
            {
                if (startBuilding)
                {
                    GetNeighbors();
                    if (_cells[_currentNeighborIndex].visited == false && _cells[_currentCellIndex].visited == true)
                    {
                        BreakWall();
                        _cells[_currentNeighborIndex].visited = true;
                        visitedCells++;
                        lastCells.Add(_currentCellIndex);
                        _currentCellIndex = _currentNeighborIndex;
                        if (lastCells.Count > 0)
                            _backingUp = lastCells.Count - 1;
                    }
                }
                else
                {
                    _currentCellIndex = UnityEngine.Random.Range(0, _totalCells);
                    _cells[_currentCellIndex].visited = true;
                    //visitedCells++;
                    startBuilding = true;
                }

                // open start and end
                if (_currentCellIndex == 3)
                    Destroy(_cells[_currentCellIndex].south);
                if (_currentCellIndex == 80)
                    Destroy(_cells[_currentCellIndex].north);
                if (_currentCellIndex == 20)
                    Destroy(_cells[_currentCellIndex].east);
                if (_currentCellIndex == 55)
                    Destroy(_cells[_currentCellIndex].east);

                // to prevent data overflow
                count++;
                if (count >= _totalCells * 80)
                    visitedCells = _totalCells;
            }
        }

        private void BreakWall()
        {
            switch (wallToBreak)
            {
                case 1:
                    Destroy(_cells[_currentCellIndex].north);
                    break;
                case 2:
                    Destroy(_cells[_currentCellIndex].east);
                    break;
                case 3:
                    Destroy(_cells[_currentCellIndex].west);
                    break;
                case 4:
                    Destroy(_cells[_currentCellIndex].south);
                    break;
            }
        }

        private void GetNeighbors()
        {
            int length = 0;
            int[] neighbors = new int[4];
            int[] connectingWalls = new int[4];
            int check = 0;

            check = (_currentCellIndex + 1) / xSize;
            check--;
            check *= xSize;
            check += xSize;

            // west wall
            if (_currentCellIndex + 1 < _totalCells && (_currentCellIndex + 1) != check)
            {
                if (_cells[_currentCellIndex + 1].visited == false)
                {
                    neighbors[length] = _currentCellIndex + 1;
                    connectingWalls[length] = 3;
                    length++;
                }
            }

            // east wall
            if (_currentCellIndex - 1 >= 0 && (_currentCellIndex + 1) != check)
            {
                if (_cells[_currentCellIndex - 1].visited == false)
                {
                    neighbors[length] = _currentCellIndex - 1;
                    connectingWalls[length] = 2;
                    length++;
                }
            }

            // north wall
            if (_currentCellIndex + xSize < _totalCells)
            {
                if (_cells[_currentCellIndex + xSize].visited == false)
                {
                    neighbors[length] = _currentCellIndex + xSize;
                    connectingWalls[length] = 1;
                    length++;
                }
            }

            // south wall
            if (_currentCellIndex - xSize >= 0)
            {
                if (_cells[_currentCellIndex - xSize].visited == false)
                {
                    neighbors[length] = _currentCellIndex - xSize;
                    connectingWalls[length] = 4;
                    length++;
                }
            }

            if (length != 0)
            {
                int chosenOne = UnityEngine.Random.Range(0, length);
                _currentNeighborIndex = neighbors[chosenOne];
                wallToBreak = connectingWalls[chosenOne];
            }
            else if (_backingUp > 0)
            {
                _currentCellIndex = lastCells[_backingUp];
                _backingUp--;
            }

        }
    }
}
