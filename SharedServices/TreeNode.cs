using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace BruSoftware.SharedServices;

/// <summary>
/// Thanks to Ronnie Overby at https://stackoverflow.com/questions/66893/tree-data-structure-in-c-sharp/2012855#2012855
/// </summary>
/// <typeparam name="T"></typeparam>
public class TreeNode<T>
{
    private readonly List<TreeNode<T>> _children = new();
    private readonly TreeNode<T> _parent;
    private readonly List<TreeNode<T>> _ancestors = new();

    public TreeNode(T value, string name)
    {
        Name = name;
        Value = value;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public string Name { get; }

    public TreeNode<T> this[int i] => _children[i];

    public TreeNode<T> Parent
    {
        get => _parent;
        private init
        {
            _parent = value;
            if (_ancestors.Count == 0)
            {
                // This is the first child. Set the list of _ancestors
                var parent = _parent;
                while (parent != null)
                {
                    _ancestors.Add(parent);
                    parent = parent._parent;
                }
            }
        }
    }

    public T Value { get; }

    /// <summary>
    /// <c>true</c> for a child which is the same as a Parent somewhere up the branch
    /// </summary>
    public bool IsCircularReference { get; private set; }

    public ReadOnlyCollection<TreeNode<T>> Children => _children.AsReadOnly();

    public ReadOnlyCollection<TreeNode<T>> Ancestors => _ancestors.AsReadOnly();

    public TreeNode<T> AddChild(T value, string name)
    {
        var node = new TreeNode<T>(value, name) { Parent = this };
        if (_ancestors.Select(x => x.Value).Contains(node.Value))
        {
            node.IsCircularReference = true;
        }
        _children.Add(node);
        return node;
    }

    public bool RemoveChild(TreeNode<T> node)
    {
        return _children.Remove(node);
    }

    public void Traverse(Action<T> action)
    {
        var isCircularReference = IsCircularReference;
        action(Value);
        foreach (var child in _children)
        {
            child.Traverse(action);
        }
        if (isCircularReference)
        {
            // Stop circular reference
        }
    }

    public void TraverseExceptCircularReference(Action<T> action)
    {
        if (IsCircularReference)
        {
            return;
        }
        action(Value);
        foreach (var child in _children)
        {
            child.TraverseExceptCircularReference(action);
        }
    }

    /// <summary>
    /// Flattened nodes in the tree
    /// </summary>
    /// <returns></returns>
    public IEnumerable<TreeNode<T>> FlattenNodes()
    {
        return new[] { this }.Concat(_children.SelectMany(x => x.FlattenNodes()));
    }

    /// <summary>
    /// Flattened values in the tree
    /// </summary>
    /// <returns></returns>
    public IEnumerable<T> FlattenValues()
    {
        return new[] { Value }.Concat(_children.SelectMany(x => x.FlattenValues()));
    }

    /// <summary>
    /// Return the first node equal to value in the tree, starting at the top of the tree
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="SharedServicesException">Thrown when the node cannot be found</exception>
    public TreeNode<T> FindNode(T value)
    {
        // Move to the top level parent
        var node = _ancestors.Count == 0 ? this : _ancestors.Last();
        var result = FindNodeRecursive(node, value);
        return result;
    }

    private static TreeNode<T> FindNodeRecursive(TreeNode<T> node, T value)
    {
        if (node.Value.Equals(value))
        {
            return node;
        }
        // Then check it and every child
        foreach (var child in node.Children)
        {
            if (child.Value.Equals(value))
            {
                return child;
            }
        }
        return null;
    }

    public override string ToString()
    {
        var result = $"{Name}";
        if (IsCircularReference)
        {
            result += " (CircRef)";
        }
        if (Parent == null)
        {
            result += " no Parent";
        }
        else
        {
            result += $" under {Parent.Name}";
        }
        if (_children.Count > 0)
        {
            var childStr = _children.Count == 1 ? "child" : "children";
            result += $" with {_children.Count:N0} {childStr}";
        }
        if (IsCircularReference)
        {
            var str = _ancestors.Count == 1 ? "parent" : "ancestors";
            result += $" Ends circular reference branch with {_ancestors.Count:N0} {str}.";
        }
        return result;
    }
}