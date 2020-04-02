using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeaFightLogic.Supplementary;

namespace SeaFightLogic
{
    public class GameField
    {
        private Cell[,] cells;
        private readonly int quadrantSideLength;

        public GameField(int quadrantSideLength)
        {
            cells = new Cell[quadrantSideLength * 2, quadrantSideLength * 2];
            this.quadrantSideLength = quadrantSideLength;

            for (int i = 0; i <= cells.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= cells.GetUpperBound(1); j++)
                {
                    cells[i, j] = new Cell();
                }
            }
        }

        private CartesianCoordinate ConvertArrIndexToCartesianCoords(int line, int column)
        {
            int x = -1;
            int y = -1;
            int quadrant = -1;

            // Get quadrant value
            if (column >= quadrantSideLength)
            {
                quadrant = line >= quadrantSideLength ? 3 : 0;
            }
            else
            {
                quadrant = line >= quadrantSideLength ? 2 : 1;
            }

            // Get X and Y values
            switch (quadrant)
            {
                case 0:
                    x = column - quadrantSideLength;
                    y = quadrantSideLength - line - 1;
                    break;
                case 1:
                    x = quadrantSideLength - column - 1;
                    y = quadrantSideLength - line - 1;
                    break;
                case 2:
                    x = quadrantSideLength - column - 1;
                    y = line - quadrantSideLength;
                    break;
                case 3:
                    x = column - quadrantSideLength;
                    y = line - quadrantSideLength;
                    break;
            }

            CartesianCoordinate cartesianCoordinate = new CartesianCoordinate(x, y, quadrant);
            return cartesianCoordinate;
        }

        private void ConvertCartesianCoordsToArrIndex(CartesianCoordinate cartCoord, ref int str, ref int column)
        {
            // Convert cartesian coordinate to 2dim-array index
            switch (cartCoord.Quadrant)
            {
                case 0:
                    column = cartCoord.X + quadrantSideLength;
                    str = quadrantSideLength - cartCoord.Y - 1;
                    break;
                case 1:
                    column = quadrantSideLength - cartCoord.X - 1;
                    str = quadrantSideLength - cartCoord.Y - 1;
                    break;
                case 2:
                    column = quadrantSideLength - cartCoord.X - 1;
                    str = cartCoord.Y + quadrantSideLength;
                    break;
                case 3:
                    column = cartCoord.X + quadrantSideLength;
                    str = cartCoord.Y + quadrantSideLength;
                    break;
            }
        }

        public Cell this[int x, int y, int quadrant]
        {
            get
            {
                if (quadrant < 0 || quadrant > 3)
                {
                    throw new ArgumentOutOfRangeException("Quadrant value must be from 0 to 3");
                }
                if (x < 0 || x > quadrantSideLength)
                {
                    throw new ArgumentOutOfRangeException($"value of X must be from 0 to {quadrantSideLength - 1}");
                }
                if (y < 0 || y > quadrantSideLength)
                {
                    throw new ArgumentOutOfRangeException($"value of Y must be from 0 to {quadrantSideLength - 1}");
                }

                int line = -1;
                int column = -1;
                CartesianCoordinate cartesianCoord = new CartesianCoordinate(x, y, quadrant);
                ConvertCartesianCoordsToArrIndex(cartesianCoord, ref line, ref column);

                return cells[line, column];
            }
        }

        private bool IsCellsLineFree(int line, int column, int size, Direction direction, int shift =0)
        {
            bool IsCellsLineFree = true;
            int i = 0;

            try
            {
                switch (direction)
                {
                    case Direction.North:
                        while (i < size)
                        {
                            if (cells[line - i - shift, column].IsOccupied == true)
                            {
                                IsCellsLineFree = false;
                                break;
                            }
                            i++;
                        }

                        break;

                    case Direction.West:
                        while (i < size)
                        {
                            if (cells[line, column - i - shift].IsOccupied == true)
                            {
                                IsCellsLineFree = false;
                                break;
                            }
                            i++;
                        }
                        break;
                    case Direction.South:
                        while (i < size)
                        {
                            if (cells[line + i + shift, column].IsOccupied == true)
                            {
                                IsCellsLineFree = false;
                                break;
                            }
                            i++;
                        }
                        break;
                    case Direction.East:
                        while (i < size)
                        {
                            if (cells[line, column + i + shift].IsOccupied == true)
                            {
                                IsCellsLineFree = false;
                                break;
                            }
                            i++;
                        }
                        break;
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                ex.Data.Add("UserMessage", "Part of the ship goes beyond the field bounds");
                throw;
            }

            return IsCellsLineFree;
        }

        public void AddShip(Ship ship,
                            int quadrant, int x, int y,
                            Direction direction)
        {
            if (quadrant < 0 || quadrant > 3)
            {
                throw new ArgumentOutOfRangeException("Value of quadrant must be integer value from 0 to 3");
            }
            if (x < 0 || x > quadrantSideLength - 1 || y < 0 || y > quadrantSideLength - 1)
            {
                throw new ArgumentOutOfRangeException($"X and Y must be integer values from 0 to {quadrantSideLength}");
            }

            CartesianCoordinate headCartesianCoord = new CartesianCoordinate(x, y, quadrant);
            ship.Direction = direction;
            ship.ShipAction += this.OnShipAction;
            ship.ShipMovement += this.OnShipMovement;
            int line = -1;
            int column = -1;

            ConvertCartesianCoordsToArrIndex(headCartesianCoord, ref line, ref column);

            Direction reverseDirection = 0;

            // Check reverse direction
            switch (direction)
            {
                case Direction.North:
                    reverseDirection = Direction.South;
                    break;
                case Direction.South:
                    reverseDirection = Direction.North;
                    break;
                case Direction.West:
                    reverseDirection = Direction.East;
                    break;
                case Direction.East:
                    reverseDirection = Direction.West;
                    break;
            }

            bool isCellsLineFree = IsCellsLineFree(line, column, ship.Size, reverseDirection);

            if (isCellsLineFree)
            {
                ship.Head = headCartesianCoord;

                int i = 0;
                switch (direction)
                {
                    case Direction.North:
                        while (i < ship.Size)
                        {
                            cells[line + i, column].IsOccupied = true;
                            cells[line + i, column].Ship = ship;
                            i++;
                        }
                        break;

                    case Direction.West:
                        while (i < ship.Size)
                        {
                            cells[line, column + i].IsOccupied = true;
                            cells[line, column + i].Ship = ship;
                            i++;
                        }
                        break;

                    case Direction.South:
                        while (i < ship.Size)
                        {
                            cells[line - i, column].IsOccupied = true;
                            cells[line - i, column].Ship = ship;
                            i++;
                        }
                        break;

                    case Direction.East:
                        while (i < ship.Size)
                        {
                            cells[line, column - i].IsOccupied = true;
                            cells[line, column - i].Ship = ship;
                            i++;
                        }
                        break;
                }
            }
            else
            {
                throw new Exception("Error! Cannot place ship. Some of the cells are occupied by another ship!");
            }
        }

        private void GetTargetIndex(Ship sender, ShipActionEventArgs eventArgs,
                                  ref int targetArrLine, ref int targetArrColumn)
        {
            int senderHeadLine = -1;
            int senderHeadColumn = -1;

            ConvertCartesianCoordsToArrIndex(sender.Head, ref senderHeadLine, ref senderHeadColumn);
            switch (sender.Direction)
            {
                case Direction.North:
                    {
                        targetArrLine = senderHeadLine - eventArgs.ActionDistance;
                        targetArrColumn = senderHeadColumn;
                        break;
                    }
                case Direction.West:
                    {
                        targetArrLine = senderHeadLine;
                        targetArrColumn = senderHeadColumn - eventArgs.ActionDistance;
                        break;
                    }
                case Direction.South:
                    {

                        targetArrLine = senderHeadLine + eventArgs.ActionDistance;
                        targetArrColumn = senderHeadColumn;
                        break;
                    }
                case Direction.East:
                    {
                        targetArrLine = senderHeadLine;
                        targetArrColumn = senderHeadColumn + eventArgs.ActionDistance;
                        break;
                    }
            }
        }

        // Method for listening events of Ship
        private void OnShipAction(Ship sender, ShipActionEventArgs eventArgs)
        {
            // Get target cell
            int targetArrLine = -1;
            int targetArrColumn = -1;
            GetTargetIndex(sender, eventArgs, ref targetArrLine, ref targetArrColumn);
            Cell targetCell = cells[targetArrLine, targetArrColumn];

            // If there is a ship in the target cell
            if (targetCell.IsOccupied)
            {
                Ship targetShip = targetCell.Ship;

                // Get array index of Target Head
                int targetHeadArrLine = -1;
                int targetHeadArrColumn = -1;
                ConvertCartesianCoordsToArrIndex(targetShip.Head, ref targetHeadArrLine, ref targetHeadArrColumn);

                // Get  index of  affected cell of a ship in a ship-state array.
                int shipStateArrCell = -1;
                switch (targetShip.Direction)
                {
                    case Direction.North:
                        shipStateArrCell = targetArrLine - targetHeadArrLine;
                        break;
                    case Direction.West:
                        shipStateArrCell = targetArrColumn - targetHeadArrColumn;
                        break;
                    case Direction.South:
                        shipStateArrCell = targetHeadArrLine - targetArrLine;
                        break;
                    case Direction.East:
                        shipStateArrCell = targetHeadArrColumn - targetArrColumn;
                        break;
                }

                //Change a cell in a ship-state array according to an action of a sender
                if (eventArgs.ShipAction == ShipActionType.Shoot)
                {
                    targetShip.ShipCells[shipStateArrCell] = false;
                }
                else if (eventArgs.ShipAction == ShipActionType.Heal)
                {
                    targetShip.ShipCells[shipStateArrCell] = true;
                }

                // if quantity of 'true' cells in ShipCells = 0, remove ship from field
            }
        }

        public void OnShipMovement(Ship sender, ShipMovementEventArgs eventArgs)
        {
            // Check if there free cells to move on
            int shipHeadLine = -1;
            int shipHeadColumn = -1;
            ConvertCartesianCoordsToArrIndex(sender.Head, ref shipHeadLine, ref shipHeadColumn);
            bool isCellsLineFree = IsCellsLineFree(shipHeadLine, shipHeadColumn, eventArgs.Speed, sender.Direction, 1);

            if(isCellsLineFree == false)
            {
                throw new Exception("Cannot move ship on this direction");
            }

            //Cleaning up cells before ship occupies another cells
            RemoveShipFromField(sender);

            int newShipHeadLine = shipHeadLine;
            int newShipHeadColumn = shipHeadColumn;

            int i = 0;

            // Shifting ship to new position
            switch (sender.Direction)
            {
                case Direction.North:
                    while (i < sender.Size)
                    {
                        cells[shipHeadLine - eventArgs.Speed + i, shipHeadColumn].IsOccupied = true;
                        cells[shipHeadLine - eventArgs.Speed + i, shipHeadColumn].Ship = sender;
                        i++;
                    }
                    newShipHeadLine = shipHeadLine - eventArgs.Speed;
                    break;

                case Direction.West:
                    while (i < sender.Size)
                    {
                        cells[shipHeadLine, shipHeadColumn - eventArgs.Speed+i].IsOccupied = true;
                        cells[shipHeadLine, shipHeadColumn - eventArgs.Speed+i].Ship = sender;
                        i++;
                    }
                    newShipHeadColumn = shipHeadColumn - eventArgs.Speed;
                    break;

                case Direction.South:
                    while (i < sender.Size)
                    {
                        cells[shipHeadLine + eventArgs.Speed-i, shipHeadColumn].IsOccupied = true;
                        cells[shipHeadLine + eventArgs.Speed-i, shipHeadColumn].Ship = sender;
                        i++;
                    }
                    newShipHeadLine = shipHeadLine + eventArgs.Speed;
                    break;

                case Direction.East:
                    while (i < sender.Size)
                    {
                        cells[shipHeadLine, shipHeadColumn + eventArgs.Speed-i].IsOccupied = true;
                        cells[shipHeadLine, shipHeadColumn + eventArgs.Speed-i].Ship = sender;
                        i++;
                    }
                    newShipHeadColumn = shipHeadColumn + eventArgs.Speed;
                    break;
            }

            // Assign new Head Coordinate
            CartesianCoordinate newHeadCoord = this.ConvertArrIndexToCartesianCoords(newShipHeadLine, newShipHeadColumn);
            sender.Head = newHeadCoord;
        }

        private void RemoveShipFromField(Ship ship)
        {
            // Get arr index of head coordinate
            CartesianCoordinate shipHeadCoords = ship.Head;
            int headArrLine = -1;
            int headArrColumn = -1;
            ConvertCartesianCoordsToArrIndex(shipHeadCoords, ref headArrLine, ref headArrColumn);

            int i = 0;
            switch (ship.Direction)
            {
                case Direction.North:
                    while (i < ship.Size)
                    {
                        cells[headArrLine + i, headArrColumn].IsOccupied = false;
                        cells[headArrLine + i, headArrColumn].Ship = null;
                        i++;
                    }
                    break;

                case Direction.West:
                    while (i < ship.Size)
                    {
                        cells[headArrLine, headArrColumn + i].IsOccupied = false;
                        cells[headArrLine, headArrColumn + i].Ship = null;
                        i++;
                    }
                    break;

                case Direction.South:
                    while (i < ship.Size)
                    {
                        cells[headArrLine - i, headArrColumn].IsOccupied = false;
                        cells[headArrLine - i, headArrColumn].Ship = null;
                        i++;
                    }
                    break;

                case Direction.East:
                    while (i < ship.Size)
                    {
                        cells[headArrLine, headArrColumn - i].IsOccupied = false;
                        cells[headArrLine, headArrColumn - i].Ship = null;
                        i++;
                    }
                    break;
            }
        }

    }
}
