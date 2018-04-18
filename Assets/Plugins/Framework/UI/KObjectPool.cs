// ***********************************************************************
// Assembly         : Unity
// Author           : Kimch
// Created          : 
//
// Last Modified By : Kimch
// Last Modified On : 
// ***********************************************************************
// <copyright file= "ListPool" company=""></copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// From UGUI
/// </summary>
public static class KListPool<T>
{
    // Object pool to avoid allocations.
    private static readonly KObjectPool<List<T>> _ListPool = new KObjectPool<List<T>>(null, l => l.Clear());

    public static List<T> Get()
    {
        return _ListPool.Get();
    }

    public static void Release(List<T> toRelease)
    {
        _ListPool.Release(toRelease);
    }
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="T"></typeparam>
public static class KHashSetPool<T>
{
    // Object pool to avoid allocations.
    private static readonly KObjectPool<HashSet<T>> _HashSetPool = new KObjectPool<HashSet<T>>(null, s => s.Clear());

    public static HashSet<T> Get()
    {
        return _HashSetPool.Get();
    }

    public static void Release(HashSet<T> toRelease)
    {
        _HashSetPool.Release(toRelease);
    }
}

/// <summary>
/// From UGUI
/// </summary>
/// <typeparam name="T"></typeparam>
public class KObjectPool<T> where T : new()
{
    #region Field

    private readonly Stack<T> _objectStack = new Stack<T>();
    private readonly Action<T> _actionOnGet;
    private readonly Action<T> _actionOnRelease;

    #endregion

    #region Property

    public int countAll
    {
        get;
        private set;
    }

    public int countActive
    {
        get { return countAll - countInactive; }
    }

    public int countInactive
    {
        get { return _objectStack.Count; }
    }

    #endregion

    public KObjectPool(Action<T> actionOnGet, Action<T> actionOnRelease)
    {
        _actionOnGet = actionOnGet;
        _actionOnRelease = actionOnRelease;
    }

    public T Get()
    {
        T element;
        if (_objectStack.Count == 0)
        {
            element = new T();
            countAll++;
        }
        else
        {
            element = _objectStack.Pop();
        }

        if (_actionOnGet != null)
        {
            _actionOnGet(element);
        }
        return element;
    }

    public void Release(T element)
    {
        if (_objectStack.Count > 0 && ReferenceEquals(_objectStack.Peek(), element))
        {
            Debug.LogError("Internal error. Trying to destroy object that is already released to pool.");
        }
        if (_actionOnRelease != null)
        {
            _actionOnRelease(element);
        }
        _objectStack.Push(element);
    }
}
