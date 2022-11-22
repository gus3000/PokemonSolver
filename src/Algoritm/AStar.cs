using System;
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
            //FIXME bug going from 0,8 to 2,2
            Utils.Log("starting AStar", true);
            const int MAX_CALC = 10000;
            int calc = 0;

            var explored = new List<Node<Position>>();
            var toExplore = new Queue<Node<Position>>();

            var currentNode = new Node<Position>(start, null);

            do
            {
                // Utils.Log($"Checking node {currentNode.Debug()}", true);
                if (Equals(currentNode.State, goal))
                {
                    Utils.Log($"found path after {calc} nodes examined", true);
                    return currentNode;
                }

                foreach (var p in currentNode.State.Neighbours())
                {
                    if (p.X >= _mapData.Width || p.Y >= _mapData.Height)
                        continue;

                    var tile = _mapData.GetTile(p.X, p.Y);
                    if (!tile.canWalk())
                        continue;

                    if (!explored.Exists(node => Equals(node.State, p)))
                        toExplore.Enqueue(new Node<Position>(p, currentNode));
                }

                explored.Add(currentNode);

                toExplore = new Queue<Node<Position>>(toExplore.OrderBy(n => n.Depth() + n.State.MinimumDistance(goal)));

                if (calc++ > MAX_CALC)
                {
                    Utils.Log($"made more than {MAX_CALC} operations, exiting...", true);
                    break;
                }

                if (toExplore.Count == 0)
                    break;
                
                currentNode = toExplore.Dequeue();
            } while (toExplore.Count > 0);


            return null;
        }
    }
}