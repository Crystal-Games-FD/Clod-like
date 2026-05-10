using System;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace FUtils {
    public class FError<TError> where TError : Enum {
        public TError error;
        public string message;
        public FError(TError error, string message) {
            this.error = error;
            this.message = message;
        }
    }
    public class DevUtils {
        [System.Diagnostics.Conditional("UNITY_ASSERTIONS")]
        public static void Assert(bool condition, string message = "Assertion failed") {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (!condition) {
                Debug.LogError(message);
                Exit();
            }
#endif
        }
        [DoesNotReturn]
        public static void Unreachable(string message = "unreachable") {
            Assert(false, message);
        }
        [DoesNotReturn]
        public static void Exit() {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
