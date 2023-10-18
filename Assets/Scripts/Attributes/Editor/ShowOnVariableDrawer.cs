using UnityEngine;
using System.Collections.Generic;

using UnityEditor;
using System.Linq;
[CustomPropertyDrawer(typeof(ShowOnVariable), true)]
public class ShowOnVariableDrawer : PropertyDrawer {
    SerializedProperty propertyCached;


    // Draw the property inside the given rect
    public override void OnGUI (Rect position, SerializedProperty property, GUIContent label) {
        ShowOnVariable variableProperty = attribute as ShowOnVariable;
        EditorGUI.BeginProperty (position, label, property);

        if (ShouldShowVariable(variableProperty, property))
        {

            EditorGUI.PropertyField(position, property, true);
        }

        EditorGUI.EndProperty ();
    }


    public override float GetPropertyHeight (SerializedProperty property, GUIContent label) {
        ShowOnVariable variableProperty = attribute as ShowOnVariable;
      
		if (ShouldShowVariable(variableProperty, property)) {
        //    Debug.Log("Setting property height: " + EditorGUI.GetPropertyHeight(property, true));
			return EditorGUI.GetPropertyHeight(property, true);
        }
        return 0f;
    }

    SerializedProperty GetRelativeProperty(SerializedProperty property)
    {
        ShowOnVariable variableProperty = attribute as ShowOnVariable;
        if(variableProperty.useNext)
        {
            //if(!cached)
           // {
                SerializedProperty copied = property.Copy();
                while(copied.name != variableProperty.variableName)
                    copied.Next(true);
                propertyCached = copied.Copy();
            //Debug.Log("Found cached property: " + propertyCached.displayName + " val: " + propertyCached.intValue);
            //}

            return propertyCached;
        }

        return property.serializedObject.FindProperty(variableProperty.variableName);
    }

	bool ShouldShowVariable(ShowOnVariable variableProperty, SerializedProperty property)
    {
		bool show = false;

        if(variableProperty.thisType != null)
        {
            var obj = property.objectReferenceValue as System.Object;
            if (obj == null)
                return true;

            return obj.GetType() == variableProperty.thisType;
        }

	   if (variableProperty.variableNames != null || variableProperty.objParams != null)
        {
            show = true;
            int i = 0;
			if (variableProperty.combineValues != null && variableProperty.combineValues.Length != variableProperty.variableNames.Length)
			{
				show = false;
				Debug.LogError("Length mismtach between names and combine values!");
			}
			else
			{
				Dictionary<string, int> varNamesExtraAnd = new Dictionary<string, int>();
				Dictionary<string, int> varNamesExtraOr = new Dictionary<string, int>();
				List<string> varNames = new List<string>();
				List<int> varValues = new List<int>();
				List<bool> exceptValues = new List<bool>();
				if(variableProperty.objParams != null) {
					int incrementer = 0;
					for(int o = 0; o < variableProperty.objParams.Length; o++)
					{
						//Debug.Log("Incrementer is: " + incrementer + " obj is: " + variableProperty.objParams[o].ToString());
						if(incrementer == 0) {
							varNames.Add((string)variableProperty.objParams[o]);
						}
						else if(incrementer == 1) 
						{
							varValues.Add((int)variableProperty.objParams[o]);
							if(!variableProperty.exceptValue) {
							   exceptValues.Add(false);
							   incrementer = -1;
							}
						}
						else 
						{
							exceptValues.Add((bool)variableProperty.objParams[o]);
							incrementer = -1;
						}
						incrementer++;
						
					}
				}
				else {
					varNames = new List<string>(variableProperty.variableNames);
					varValues = new List<int>(variableProperty.combineValues);
					exceptValues = new List<bool>();
					for(int e = 0; e < varNames.Count; e++)
						exceptValues.Add(false);
				}
				

				foreach (string varName in varNames)
				{
					bool useAnd = variableProperty.isAnd;
					string curVarName = varName;
					bool useNot = false;
					if (curVarName.StartsWith("&&"))
					{
						useAnd = true;
						curVarName = varName.Substring(2);
					}
					else if (curVarName.StartsWith("||"))
					{
						useAnd = false;
						curVarName = varName.Substring(2);
					}

					if(curVarName.StartsWith("!")) {
						useNot = true;
						curVarName = varName.Substring(1);
					}
                    
					if(variableProperty.isAnd != useAnd) {
						if(useAnd)
						    varNamesExtraAnd.Add(curVarName, i);
						else
						    varNamesExtraOr.Add(curVarName, i);
						continue;
					}

					var prop = property.serializedObject.FindProperty(curVarName);
					bool equal = prop.intValue == varValues[i];
					if (useNot)
						equal = !equal;
					if(exceptValues[i])
						equal = !equal;

					show = variableProperty.isAnd ? (show && (prop != null && equal)) : (show || (prop != null && equal));
					if (prop == null)
					{
						Debug.LogError("Var name: " + curVarName + " is null");
						show = false;
						break;
					}

					i++;
				}

				foreach(var andVar in varNamesExtraAnd) {
					var prop = property.serializedObject.FindProperty(andVar.Key);
					show = (show && (prop != null && prop.intValue == variableProperty.combineValues[andVar.Value]));
					Debug.Log("checking and var: " + andVar.Key);
					if (prop == null)
                    {
						Debug.LogError("Var name: " + andVar.Key + " is null");
                        show = false;
                        break;
                    }
				}

				foreach (var orVar in varNamesExtraOr)
                {
                    var prop = property.serializedObject.FindProperty(orVar.Key);
                    show = (show || (prop != null && prop.intValue == variableProperty.combineValues[orVar.Value]));

                    if (prop == null)
                    {
                        Debug.LogError("Var name: " + orVar.Key + " is null");
                        show = false;
                        break;
                    }
                }
			}
        }
        else
        {
			
            var relProperty = GetRelativeProperty(property);
            if (relProperty != null)
            {
				if (variableProperty.maskValue)
				{
                    if(relProperty.propertyType == SerializedPropertyType.Generic)
                    {
                        return false;
                    }

					int val1 = variableProperty.value;
					int val2 = relProperty.intValue;
               
					show = val2 == (val2 | val1);
				}
				else
				{
					show = (variableProperty.exceptValue && relProperty.intValue != variableProperty.value)
						|| (!variableProperty.exceptValue && relProperty.intValue == variableProperty.value);
				}
            }

			if (relProperty == null)
			{
				Debug.LogError("Relative property " + variableProperty.variableName + " is null.");
				show = false;
			}
        }

		return show;
    }
}
