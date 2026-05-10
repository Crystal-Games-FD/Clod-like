using System;
using UnityEngine;

public class Room : MonoBehaviour
{
    [Serializable]
    public class Side {
        public enum Kind {
            Wall,
            Door,
            Any,
        };
        [Header("Properties")]
        [SerializeField] public Kind kind;
        public Side() {
            kind = Kind.Any;
        }
    }
    [Header("Properties")]
    [SerializeField] private Zone.Kind kind = Zone.Kind.None;
    [SerializeField] private Side left  = new();
    [SerializeField] private Side right = new();
    [SerializeField] private Side up    = new();
    [SerializeField] private Side down  = new();
    private Side[] sides; 
    void Start()
    {
        sides = new Side[]{
            left,
            right,
            up,
            down,
        };
    }
}
