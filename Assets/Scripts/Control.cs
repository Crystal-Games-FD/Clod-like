using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using FUtils;

namespace Control {
    public enum CtxErrorKind {
        NoAction,
        NoKey,
        DuplicateKey,
    }
    public static class FMouse {
        public enum Kind {
            Left,
            Right,
            Middle,
            Back,
            Forward,
        };
        public static ButtonControl As(this Mouse mouse, Kind kind) {
            switch (kind) {
                case Kind.Left:    return mouse.leftButton;
                case Kind.Right:   return mouse.rightButton;
                case Kind.Middle:  return mouse.middleButton;
                case Kind.Back:    return mouse.backButton;
                case Kind.Forward: return mouse.forwardButton;
                default:           break;
            }
            return null;
        }
    }
    public class FKey
    {
        public enum Kind {
            None,
            Key,
            Mouse,
        };
        public enum Duration {
            None,
            Click,
            Hold,
        };
        public Kind     kind;
        public Duration dur;
        // union
        public Key key;
        public FMouse.Kind mouse;
        public FKey() {
            kind = Kind.None;
        }
        public FKey(Key key, Duration dur = Duration.Hold) {
            kind = Kind.Key;
            this.dur = dur;
            this.key = key;
        }
        public FKey(FMouse.Kind mouse, Duration dur = Duration.Click) {
            kind = Kind.Mouse;
            this.dur   = dur;
            this.mouse = mouse;
        }
    }
    public class Ctx<TAction> where TAction : Enum
    {
        private readonly Dictionary<TAction, List<FKey>> actions;
        public Ctx() {
            actions = new();
        }
        public void Map(TAction action, out FError<CtxErrorKind> err, params FKey[] keys) {
            err = null;
            if (keys.Length <= 0) {
                err = new(CtxErrorKind.NoAction, "No action were provided");
                return;
            }
            if (!actions.ContainsKey(action)) {
                actions[action] = new List<FKey>();
            }
            foreach (var key in keys) {
                if (actions[action].Contains(key)) {
                    err = new(CtxErrorKind.DuplicateKey, "Duplicate keycode inserted");
                    return;
                }
                actions[action].Add(key);
            }
        }
        public void Map(TAction action, params FKey[] keys) {
            Map(action, out var err, keys);
            if (err != null) DevUtils.Unreachable(err.message);
        }
        public bool Is(TAction action, out FError<CtxErrorKind> err) {
            err = null;
            if (!actions.ContainsKey(action)) {
                err = new(CtxErrorKind.NoAction, "Not listening for this event");
                return false;
            }
            List<FKey> keys = actions[action];
            foreach (var fkey in keys) {
                switch (fkey.kind) {
                    case FKey.Kind.None: break;
                    case FKey.Kind.Key:
                        switch (fkey.dur) {
                            case FKey.Duration.Click:
                                foreach (var kb in InputSystem.devices.OfType<Keyboard>()) {
                                    if (kb[fkey.key].wasPressedThisFrame) return true;
                                }
                                break;
                            case FKey.Duration.Hold:
                                foreach (var kb in InputSystem.devices.OfType<Keyboard>()) {
                                    if (kb[fkey.key].isPressed) return true;
                                }
                                break;
                            case FKey.Duration.None: break;
                            default: DevUtils.Assert(false, "unreachable"); break;
                        }
                        break;
                    case FKey.Kind.Mouse:
                        switch (fkey.dur) {
                            case FKey.Duration.Click:
                                foreach (var mouse in InputSystem.devices.OfType<Mouse>()) {
                                    ButtonControl key = mouse.As(fkey.mouse);
                                    DevUtils.Assert(key != null, "FMouse.Kind");
                                    if (key.wasPressedThisFrame) return true;
                                }
                                break;
                            case FKey.Duration.Hold:
                                foreach (var mouse in InputSystem.devices.OfType<Mouse>()) {
                                    ButtonControl key = mouse.As(fkey.mouse);
                                    DevUtils.Assert(key != null, "FMouse.Kind");
                                    if (key.isPressed) return true;
                                }
                                break;
                        }
                        break;
                    default: DevUtils.Unreachable(); break;
                }
            }
            return false;
        }
        public bool Is(TAction action) {
            bool result = Is(action, out var err);
            if (err != null) DevUtils.Unreachable(err.message);
            return result;
        }
        public Ctx<TAction> Clone(out FError<CtxErrorKind> err) {
            var ctx = new Ctx<TAction>();
            err = null;
            foreach (var action in actions.Keys) {
                var keys = actions[action].ToArray();
                if (keys.Length <= 0) continue;
                ctx.Map(action, out err, keys);
                if (err != null) return null;
            }
            return ctx;
        }
        public Ctx<TAction> Clone() {
            var ctx = Clone(out var err);
            if (err != null) DevUtils.Unreachable(err.message);
            return ctx;
        }
    }
    public enum DefaultAction {
        None,
        M1,
        M2,
        Left,
        Right,
        Up,
        Down,
        Run,
        Jump,
        Pause,
        Exit,
    };
    public class DControl : MonoBehaviour
    {
       private static readonly Ctx<DefaultAction> defaultCtx = new();
       public static Ctx<DefaultAction> CloneDefaultCtx() {
           return defaultCtx.Clone();
       }
       [RuntimeInitializeOnLoadMethod]
       private static void OnGameLoad()
       {
            defaultCtx.Map(DefaultAction.M1,    new FKey(FMouse.Kind.Left));
            defaultCtx.Map(DefaultAction.M2,    new FKey(FMouse.Kind.Right));

            defaultCtx.Map(DefaultAction.Left,  new(Key.A), new(Key.LeftArrow));
            defaultCtx.Map(DefaultAction.Right, new(Key.D), new(Key.RightArrow));
            defaultCtx.Map(DefaultAction.Up,    new(Key.W), new(Key.UpArrow));
            defaultCtx.Map(DefaultAction.Down,  new(Key.S), new(Key.DownArrow));

            defaultCtx.Map(DefaultAction.Run,   new FKey(Key.LeftShift));
            defaultCtx.Map(DefaultAction.Jump,  new FKey(Key.Space,  FKey.Duration.Click));
            defaultCtx.Map(DefaultAction.Pause, new FKey(Key.P,      FKey.Duration.Click));
            defaultCtx.Map(DefaultAction.Exit,  new FKey(Key.Escape, FKey.Duration.Click));
       }
    }   
}
