using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class BossPatternValidation
{
    static BossPatternValidation()
    {
        EditorApplication.playModeStateChanged += state =>
        {
            if (state != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool("BossValidation", false)) return;
            SessionState.SetBool("BossValidation", false);
            new GameObject("Boss Validation").AddComponent<BossPatternValidationRunner>();
        };
    }
    [MenuItem("Tools/Boss Patterns/Validate In Play Mode")]
    public static void Run()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode first.");
        SessionState.SetBool("BossValidation", true);
        EditorApplication.isPlaying = true;
    }
}

