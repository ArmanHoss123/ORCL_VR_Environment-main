using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class ShowOnVariable : PropertyAttribute {
    public string variableName;
    public int value;
    public bool exceptValue;
    public bool maskValue;
    public bool useNext;

	public string[] variableNames;
	public int[] combineValues;
	public bool isAnd;

    public bool localValue = false;

    public System.Type thisType;

    public object[] objParams;


    public ShowOnVariable(string variableName, int value, bool exceptValue = false, bool maskValue = false)
    {
        this.variableName = variableName;
        this.value = value;
        this.exceptValue = exceptValue;
        this.maskValue = maskValue;
    }

    public ShowOnVariable(bool useNext, string variableName, int value, bool exceptValue = false, bool maskValue = false)
    {
        this.useNext = useNext;
        this.variableName = variableName;
        this.value = value;
        this.exceptValue = exceptValue;
        this.maskValue = maskValue;
    }

	public ShowOnVariable(string[] varNames, int[] varValues, bool andValue = true)
    {
		variableNames = varNames;
		combineValues = varValues;
		isAnd = andValue;
    }

    public ShowOnVariable(bool doAnd, bool includeExclude, params object[] varParams)
    {
        objParams = varParams;
		exceptValue = includeExclude;
        isAnd = doAnd;
    }
    
    public ShowOnVariable(string variable, System.Type type)
    {
        this.variableName = variable;
        thisType = type;
    }
}

	
