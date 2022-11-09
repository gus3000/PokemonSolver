﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using PokemonSolver.MapData;
using PokemonSolver.Memory;

namespace PokemonSolver.Algoritm
{
    public class AStar
    {
        private readonly MapData.MapData _mapData;

        public AStar(MapData.MapData mapData)
        {
            _mapData = mapData;
        }


        public Node<Position>? resolve(Position start, Position goal)
        {
            Utils.Log("starting AStar", true);
            const int MAX_CALC = 1000;
            int calc = 0;

            var explored = new List<Node<Position>>();
            var toExplore = new Queue<Node<Position>>();

            var currentNode = new Node<Position>(start, null);

            do
            {
                Utils.Log($"Checking node {currentNode.Debug()}", true);
                if (Equals(currentNode.State, goal))
                {
                    Utils.Log($"found path after {calc} nodes examined", true);
                    return currentNode;
                }

                foreach (var p in currentNode.State.Neighbours())
                {
                    if (p.X < 0 || p.X >= _mapData.Width || p.Y < 0 || p.Y >= _mapData.Height)
                        continue;

                    var tile = _mapData.GetTile(p.X, p.Y);
                    //TODO check permissions of tile

                    if (!explored.Exists(node => node.State == p))
                        toExplore.Enqueue(new Node<Position>(p, currentNode));
                }

                explored.Add(currentNode);

                toExplore = new Queue<Node<Position>>(toExplore.OrderBy(n => n.State.MinimumDistance(goal)));

                if (calc++ > MAX_CALC)
                {
                    Utils.Log($"made more than {MAX_CALC} operations, exiting...", true);
                    break;
                }

                currentNode = toExplore.Dequeue();
            } while (toExplore.Count > 0);


            return null;
        }
    }
}