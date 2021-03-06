﻿// Copyright (c) William Humphreys.
// Licensed under the MIT license. See licence file in the project root for full license information.

using System;

namespace Minesweeper.Models
{
    public class Game
    {
        private int _rowCount;
        private int _columnCount;
        private int _mineCount;

        private int _activeFlags;

        public int Timer { get; set; }

        public int FlagScore
        {
            get { return _mineCount - _activeFlags; }

            set { _activeFlags = _activeFlags + value; }
        }

        public GameState State { get; set; }

        public Tile[,] GameGrid { get; }

        public Game(int rowCount, int columnCount, int mineCount)
        {
            _rowCount    = rowCount;
            _columnCount = columnCount;
            _mineCount   = mineCount;

            // Create a grid of tile objects.
            GameGrid = new Tile[_rowCount, _columnCount];
        }

        public void Start()
        {
            State = GameState.Start;

            Timer = 0;
            _activeFlags = 0;

            InitialiseGameGrid();
        }

        private void InitialiseGameGrid()
        {
            for (int row = 0; row < _rowCount; row++)
            {
                for (int column = 0; column < _columnCount; column++)
                {
                    GameGrid[row, column] = new Tile
                    {
                        IsFlag         = false,
                        IsMine         = false,
                        IsExplodedMine = false,
                        Value          = 0,
                        IsRevealed     = false
                    };
                }
            }
        }

        public void RevealTile(int row, int column)
        {
            // The game grid doesnt contain any mines on the
            // first selection as it makes the game unfair so
            // they are generated after the first tile is opened.

            if (State == GameState.Start)
            {
                AddMinesToGameGrid(row, column);

                AddNumberOfSurroundingMinesToGameGrid();
            }

            if (GameGrid[row, column].IsFlag)
            {
                return;
            }
            else if (GameGrid[row, column].IsMine)
            {
                State = GameState.Lost;
                GameGrid[row, column].IsExplodedMine = true;
                RevealAllTiles();
                return;
            }
            else if (GameGrid[row, column].Value > 0)
            {
                GameGrid[row, column].IsRevealed = true;
            }
            else
            {
                GameGrid[row, column].IsRevealed = true;
                ExpandEmptyTile(row, column);
            }

            if (IsGameWon())
            {
                State = GameState.Won;
                RevealAllTiles();
            }
        }

        private void RevealAllTiles()
        {
            for (int row = 0; row < _rowCount; row++)
            {
                for (int column = 0; column < _columnCount; column++)
                {
                    GameGrid[row, column].IsRevealed = true;
                }
            }
        }

        /// <summary>
        /// The game of minesweeper is won at the point when all tiles 
        /// that are not mines are clicked open.
        /// 
        /// When there are (_rowcount * _columnCount) - _mineCount 
        /// non mine tiles that are revealed the game is won.
        /// </summary>
        /// <returns></returns>
        private bool IsGameWon()
        {
            int revealedTileCount = 0;

            for (int row = 0; row < _rowCount; row++)
            {
                for (int column = 0; column < _columnCount; column++)
                {
                    if (GameGrid[row, column].IsRevealed && !GameGrid[row, column].IsMine)
                    {
                        revealedTileCount++;
                    }
                }
            }

            return revealedTileCount == (_rowCount * _columnCount) - _mineCount ? true : false;
        }

        /// <summary>
        /// Recursively reveal empty tiles from a specific tile at [row,column].
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        private void ExpandEmptyTile(int row, int column)
        {
            for (int r = row - 1; r <= row + 1; ++r)
            {
                for (int c = column - 1; c <= column + 1; ++c)
                {
                    if (IsInsideGameGrid(r, c) && !GameGrid[r, c].IsRevealed && !GameGrid[r, c].IsFlag)
                    {
                        GameGrid[r, c].IsRevealed = true;

                        if (GameGrid[r, c].Value == 0)
                        {
                            ExpandEmptyTile(r, c);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add mines to random tiles in the game grid.
        /// </summary>
        /// <param name="startRow"></param>
        /// <param name="startColumn"></param>
        private void AddMinesToGameGrid(int startRow, int startColumn)
        {
            Random random = new Random();

            int placed = 0;

            while (placed < _mineCount)
            {
                int row    = random.Next() % _rowCount;
                int column = random.Next() % _columnCount;

                // We dont want to place a bomb on the tile
                // that is opened first.
                if (row != startRow && column != startColumn)
                {
                    if (!GameGrid[row, column].IsMine)
                    {
                        GameGrid[row, column].IsMine = true;
                        placed++;
                    }
                }
            }
        }

        /// <summary>
        /// Calculate the number of mines that surround each tile 
        /// for every tile in the game grid.
        /// </summary>
        private void AddNumberOfSurroundingMinesToGameGrid()
        {
            for (int row = 0; row < _rowCount; row++)
            {
                for (int column = 0; column < _columnCount; column++)
                {
                    if (IsInsideGameGrid(row, column) && !GameGrid[row, column].IsMine)
                    {
                        GameGrid[row, column].Value = GetNumberOfSurroundingMines(row, column);
                    }
                }
            }
        }

        /// <summary>
        /// Count the number of mines surrounding any tile within the grid.
        /// 
        /// Each tile that isnt on any edge position of the grid will have eight
        /// surrounding tiles. Tiles at any edge position will have less so we need
        /// to ignore any tiles that are outside the edge positions.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        private int GetNumberOfSurroundingMines(int row, int column)
        {
            int mineCount = 0;

            for (int r = row - 1; r <= row + 1; r++)
            {
                for (int c = column - 1; c <= column + 1; c++)
                {
                    if (IsInsideGameGrid(r, c) && GameGrid[r, c].IsMine)
                    {
                        mineCount++;
                    }
                }
            }

            return mineCount;
        }
        
        /// <summary>
        /// Check a tiles [row,column] position to verify that it is
        /// a valid position within the game grid.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns>True, if the tile is inside the game grid.</returns>
        private bool IsInsideGameGrid(int row, int column)
        {
            return row >= 0 && column >= 0 && row < _rowCount && column < _columnCount;
        }
    }
}
