using System;
using System.Collections.Generic;
using PokemonSolver.Debug;
using PokemonSolver.MapData;

namespace PokemonSolver.Algoritm
{
    public class Node<T> where T : IShortStringable
    {
        public Node<T>? Parent { get; }
        public List<Node<T>> Children { get; }
        public T State { get; }

        public Node(T state, Node<T>? parent)
        {
            State = state;
            Parent = parent;
            Children = new List<Node<T>>();
        }
        public List<Node<T>> Ancestors(bool includeSelf = false)
        {
            var ancestors = new List<Node<T>>();
            var p = includeSelf ? this : Parent;
            while (p != null)
            {
                ancestors.Add(p);
                p = p.Parent;
            }
            return ancestors;
        }
        
        public int Depth(int acc = 0)
        {
            if (Parent == null)
                return acc;
            return Parent.Depth(acc + 1);
        }

        public string Debug()
        {
            if (Parent == null)
                return State.ToShortString();
            return Parent.Debug() + "\n -> " + State.ToShortString();
        }
    }
}