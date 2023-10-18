using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Linq;
[CanEditMultipleObjects]
[CustomPropertyDrawer(typeof(EditorButton))]
public class EditorButtonDrawer : PropertyDrawer  {

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!ShouldDraw(property))
            return 0f;
        return base.GetPropertyHeight(property, label);
    }
    // Draw the property inside the given rect
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {

        // First get the attribute since it contains the range for the slider
        EditorButton eD = attribute as EditorButton;
        if (!ShouldDraw(property))
            return;

        Rect buttonRect = new Rect(position.x + (position.width - 300f) * 0.5f,
            position.y, 300f, position.height);
        if(GUI.Button(buttonRect, eD.buttonLabel)) {
            var obj = property.serializedObject.targetObject;
            var type = obj.GetType();

            var method = type.GetMethod(eD.callMethod);
            method.Invoke(obj, null);
            if(property.serializedObject.isEditingMultipleObjects)
            {
                GameObject[] objs = Selection.gameObjects;
                foreach(GameObject selectedObj in objs)
                {
                    Component c = selectedObj.GetComponent(type);
                    if(c && c != obj)
                    {
                        method.Invoke(c, null);
                    }
                }
            }

        }

    }

    bool ShouldDraw(SerializedProperty property)
    {
        EditorButton eD = attribute as EditorButton;

        if (string.IsNullOrEmpty(eD.dontShowVar))
        {
            return true;
        }
        
        bool show = true;
        var relProperty = property.serializedObject.FindProperty(eD.dontShowVar);
        if (relProperty != null)
        {
            show = relProperty.intValue == eD.dontShowVarVal;
            if (eD.exclude)
                show = !show;
        }

        if (relProperty == null)
        {
            show = false;
        }
        return show;
    }
}
