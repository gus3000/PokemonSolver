using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using PokemonSolver.Interaction;
using PokemonSolver.Mapping;
using PokemonSolver.Memory;
using Priority_Queue;

namespace PokemonSolver.Algoritm
{
    public class AStar
    {
        private readonly int _maxQueueSize = 100000;

        public AStar()
        {
        }

        public void Bench()
        {
            var start = OverworldEngine.GetInstance().GetCurrentPosition();
            var end = new Position(start.MapBank, start.MapIndex, start.X + 2, start.Y, start.Direction, start.Altitude);
            var node = Resolve(start, end);
            Utils.Log($"node : {node.Debug()}");
        }

        public Node<Map>? MapConSearch(Position start, Position goal)
        {
            var root = new Node<Map>(OverworldEngine.GetInstance().GetMap(start), null);
            var explored = new List<Node<Map>>();
            var toExplore = new Queue<Node<Map>>();
            toExplore.Enqueue(root);
            Node<Map>? node = null;
            while (toExplore.Count > 0)
            {
                node = toExplore.Dequeue();
                if (node.State.Bank == goal.MapBank && node.State.MapIndex == goal.MapIndex)
                    break;

                explored.Add(node);
                foreach (var connection in node.State.Connections)
                {
                    if (!explored.Exists((n => n.State.Bank == connection.MapBank && n.State.MapIndex == connection.MapIndex)))
                        toExplore.Enqueue(new Node<Map>(OverworldEngine.GetInstance().GetMap(connection), node));
                }
            }

            Utils.Log($"map path : {node?.Debug()}", true);

            return node;
        }

        private int ScoreFromMaps(Position p, Node<Map> mapsOnPath)
        {
            var currentMap = OverworldEngine.GetInstance().GetMap(p);
            var penalty = 0;
            foreach (var ancestor in mapsOnPath.Ancestors(true))
            {
                // Utils.Log($"Comparing ({ancestor.State.Bank},{ancestor.State.MapIndex}) and ({goalMap.Bank},{goalMap.MapIndex})", true);
                if (ancestor.State.Bank == currentMap.Bank && ancestor.State.MapIndex == currentMap.MapIndex)
                    break;
                penalty += 1000;
            }

            return penalty;
        }

        private Map? NextMapInPath(Position p, Node<Map> mapsOnPath)
        {
            var currentMap = OverworldEngine.GetInstance().GetMap(p);
            Node<Map>? previous = null;
            foreach (var ancestor in mapsOnPath.Ancestors(true))
            {
                if (ancestor.State.Bank == currentMap.Bank && ancestor.State.MapIndex == currentMap.MapIndex)
                    return previous?.State;
                previous = ancestor;
            }

            return null;
        }

        public Node<Position>? Resolve(Position start, Position goal)
        {
            Utils.Log("starting AStar", true);

            var mapsOnPath = MapConSearch(start, goal);
            if (mapsOnPath == null)
            {
                Utils.Log($"Unable to find a map path to go from {start} to {goal}", true);
                return null;
            }

            //check if goal is walkable
            var goalMap = OverworldEngine.GetInstance().GetMap(goal);
            if (!goalMap.MapData.GetTile(goal.X, goal.Y).Walkable)
            {
                Utils.Log($"{goal} is not a walkable tile", true);
                return null;
            }

            var ancestorsDebug = mapsOnPath.Ancestors(true);
            foreach (var anc in ancestorsDebug)
            {
                Utils.Log(anc.Debug(), true);
            }

            var calc = 0;

            var explored = new List<Node<Position>>();
            // var toExplore = new SimplePriorityQueue<Node<Position>>();
            var toExplore = new FastPriorityQueue<Node<Position>>(_maxQueueSize);

            var currentNode = new Node<Position>(start, null);


            // var existsWatch = new Stopwatch();
            // var heuristicWatch = new Stopwatch();
            // var nextMapWatch = new Stopwatch();
            // var scoreWatch = new Stopwatch();
            do
            {
                // Utils.Log(
                // $"Checking node (dist {currentNode.Depth()},{currentNode.State.MinimumDistance(goal, NextMapInPath(currentNode.State, mapsOnPath))},{ScoreFromMaps(currentNode.State, mapsOnPath)}) {currentNode.State}, next map = {NextMapInPath(currentNode.State, mapsOnPath)?.Name}",
                // true);
                if (Equals(currentNode.State, goal))
                {
                    Utils.Log($"found path after {calc} nodes examined", true);
                    return currentNode;
                }

                foreach (var p in currentNode.State.Children())
                {
                    // existsWatch.Start();
                    if (!explored.Exists(node => Equals(node.State, p)))
                    {
                        var n = new Node<Position>(p, currentNode);
                        var heuristic = n.Depth() + n.State.MinimumDistance(goal, NextMapInPath(n.State, mapsOnPath)) + ScoreFromMaps(n.State, mapsOnPath);
                        toExplore.Enqueue(n, heuristic);
                        // toExplore.Enqueue(new Node<Position>(p, currentNode));
                    }
                    // existsWatch.Stop();
                }

                explored.Add(currentNode);

                if (toExplore.Count == 0)
                    break;

                if (calc++ >= 50000)
                {
                    Utils.Log($"limit of {calc} reached, exiting...", true);
                    return null;
                }

                currentNode = toExplore.Dequeue();
            } while (toExplore.Count > 0);

            Utils.Log("Everything explored, no path found ; exiting...", true);
            return null;
        }
    }
}