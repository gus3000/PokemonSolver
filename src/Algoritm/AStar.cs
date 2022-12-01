using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using BenchmarkDotNet.Attributes;
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

        [Benchmark]
        public Node<Position>? ResolveBenchmark(Position start, Position goal)
        {
            return Resolve(start, goal);
        }
        
        public Node<Position>? Resolve(Position start, Position goal)
        {
            Utils.Log("starting AStar", true);
            Position.SetMapData(_mapData);
            int calc = 0;

            var explored = new List<Node<Position>>();
            var toExplore = new Queue<Node<Position>>();

            var currentNode = new Node<Position>(start, null);

            do
            {
                // Utils.Log($"Checking node (dist {currentNode.Depth()},{currentNode.State.MinimumDistance(goal)}) {currentNode.Debug()}", true);
                if (Equals(currentNode.State, goal))
                {
                    Utils.Log($"found path after {calc} nodes examined", true);
                    return currentNode;
                }

                foreach (var p in currentNode.State.Neighbours())
                {
                    if (!explored.Exists(node => Equals(node.State, p)))
                        toExplore.Enqueue(new Node<Position>(p, currentNode));
                }

                explored.Add(currentNode);

                toExplore = new Queue<Node<Position>>(toExplore.OrderBy(n => n.Depth() + n.State.MinimumDistance(goal)));

                if (toExplore.Count == 0)
                    break;
                
                currentNode = toExplore.Dequeue();
            } while (toExplore.Count > 0);

            Utils.Log("Everything explored, no path found ; exiting...", true);
            return null;
        }
    }
}