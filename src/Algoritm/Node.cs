using System;
using System.Collections.Generic;
using PokemonSolver.MapData;

namespace PokemonSolver.Algoritm
{
    public class Node<T>
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

        public string Debug()
        {
            if (Parent == null)
                return State.ToString();
            return Parent.Debug() + " -> " + State.ToString();
        }
    }
}