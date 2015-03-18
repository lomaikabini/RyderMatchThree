using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class WizardEditor : MonoBehaviour {

	public InputField healthField;
	public InputField slimeField;
	public InputField toothField;
	public Text nameTx;
	[HideInInspector]
	public int id;
	[HideInInspector]
	public int slimePeriod = 0;
	[HideInInspector]
	public int toothPeriod = 0;
	[HideInInspector]
	public int health  = 0;
	[HideInInspector]
	public int resistId = -1;
	public List<Toggle> toggleGroup;

	void Start()
	{
		InputField.SubmitEvent submitEvent = new InputField.SubmitEvent();
		submitEvent.AddListener(submitHealth);
		healthField.onEndEdit = submitEvent;

		InputField.SubmitEvent submitEvent2 = new InputField.SubmitEvent();
		submitEvent2.AddListener(submitSlimePeriod);
		slimeField.onEndEdit = submitEvent;

		InputField.SubmitEvent submitEvent3 = new InputField.SubmitEvent();
		submitEvent3.AddListener(submitToothPeriod);
		toothField.onEndEdit = submitEvent;
	}

	void submitToothPeriod (string arg)
	{
		if (!arg.Equals (""))
			toothPeriod = int.Parse (arg);
		else
			toothPeriod = 0;
	}

	void submitSlimePeriod (string arg)
	{
		if (!arg.Equals (""))
			slimePeriod = int.Parse (arg);
		else
			slimePeriod = 0;
	}

	void submitHealth (string arg)
	{
		if (!arg.Equals (""))
			health = int.Parse (arg);
		else
			health = 0;
	}
	public void SetName(int number)
	{
		id = number;
		nameTx.text = "Wizard " + id.ToString ();
	}

	public void OnToggleChange()
	{
		foreach(Toggle tg in toggleGroup)
		{
			if(tg.isOn)
			{
				resistId = int.Parse(tg.gameObject.name);
			}
		}
	}

}
