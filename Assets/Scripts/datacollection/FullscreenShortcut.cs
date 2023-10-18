using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[InitializeOnLoad]
static class FullscreenShortcut
{
    static FullscreenShortcut()
    {
        EditorApplication.update += Update;
    }

    static void Update()
    {
        if (EditorApplication.isPlaying && ShouldToggleMaximize())
        {
            EditorWindow.focusedWindow.maximized = !EditorWindow.focusedWindow.maximized;
        }
    }

    private static bool ShouldToggleMaximize()
    {
        return Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.LeftShift);
    }
}
#endif