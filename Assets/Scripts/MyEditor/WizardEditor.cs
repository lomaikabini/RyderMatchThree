using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class WizardEditor : MonoBehaviour {

	public InputField healthField;
	public InputField slimeField;
	public InputField toothField;
	public InputField jumperField;
	public Text nameTx;
	public WizardEssence wizardConfig = new WizardEssence();
	public List<Toggle> toggleGroup;

	void Start()
	{
		InputField.SubmitEvent submitEvent = new InputField.SubmitEvent();
		submitEvent.AddListener(submitHealth);
		healthField.onEndEdit = submitEvent;

		InputField.SubmitEvent submitEvent2 = new InputField.SubmitEvent();
		submitEvent2.AddListener(submitSlimePeriod);
		slimeField.onEndEdit = submitEvent2;

		InputField.SubmitEvent submitEvent3 = new InputField.SubmitEvent();
		submitEvent3.AddListener(submitToothPeriod);
		toothField.onEndEdit = submitEvent3;

		InputField.SubmitEvent submitEvent4 = new InputField.SubmitEvent ();
		submitEvent4.AddListener (submitJumperPeriod);
		jumperField.onEndEdit = submitEvent4;
	}

	void submitJumperPeriod (string arg)
	{
		if (!arg.Equals (""))
			wizardConfig.jumpPeriond = int.Parse (arg);
		else
			wizardConfig.jumpPeriond = 0;
	}

	void submitToothPeriod (string arg)
	{
		if (!arg.Equals (""))
			wizardConfig.toothPeriod = int.Parse (arg);
		else
			wizardConfig.toothPeriod = 0;
	}

	void submitSlimePeriod (string arg)
	{
		if (!arg.Equals (""))
			wizardConfig.slimePeriod = int.Parse (arg);
		else
			wizardConfig.slimePeriod = 0;
	}

	void submitHealth (string arg)
	{
		if (!arg.Equals (""))
			wizardConfig.health = int.Parse (arg);
		else
			wizardConfig.health = 0;
	}
	public void SetName(int number)
	{
		wizardConfig.id = number;
		nameTx.text = wizardConfig.id.ToString ();
	}

	public void OnToggleChange()
	{
		foreach(Toggle tg in toggleGroup)
		{
			if(tg.isOn)
			{
				wizardConfig.resistId = int.Parse(tg.gameObject.name);
			}
		}
	}

}
