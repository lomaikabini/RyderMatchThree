using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BubbleEditorDamage : MonoBehaviour {

	public Image img;
	public InputField input;
	[HideInInspector]
	public FieldItem.Type type;
	public Item.ItemType itemType;
	public Cell.Type cellType;
	public bool isGoal =  false;
	public bool isCell = false;
	void Start()
	{
		InputField.SubmitEvent submitEvent = new InputField.SubmitEvent();
		submitEvent.AddListener(SubmitDamage);
		input.onEndEdit = submitEvent;
	}

	public void SubmitDamage(string count)
	{
		if(!isGoal)
			MyEditor.instance.OnDamageChanged (this, int.Parse (count));
		else
		{
			MyEditor.instance.OnGoalChanged(this, int.Parse (count));
		}
	}
}
