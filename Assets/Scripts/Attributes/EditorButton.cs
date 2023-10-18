using UnityEngine;
using System.Collections;

public class EditorButton : PropertyAttribute {

    public string buttonLabel;
    public string callMethod;
	public string dontShowVar;
	public int dontShowVarVal;
	public bool exclude;
	public EditorButton(string buttonLabel, string callMethod, string dontShowIfVariable = "", int dontShowVarInt = -1, bool excludeVal = false) {
        this.buttonLabel = buttonLabel;
        this.callMethod = callMethod;
		this.dontShowVar = dontShowIfVariable;
		dontShowVarVal = dontShowVarInt;
		exclude = excludeVal;
    }
}
