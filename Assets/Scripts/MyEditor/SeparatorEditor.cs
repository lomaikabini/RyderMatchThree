using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SeparatorEditor : MonoBehaviour,IPointerClickHandler {

	public static  Separator.Sprites[] sprites;
	public Image img;
	public Text tx;
	public bool isMenu = false;
	[HideInInspector]
	public RectTransform rectTransform;
	public SeparatorEssence separatorConfig = new SeparatorEssence ();

	public void OnPointerClick (PointerEventData eventData)
	{
		if (isMenu)
			MyEditor.instance.OnMenuSeparatorClick (this);
	}

	public void SetType(Separator.Type t,Separator.DestroyType dT, float size = -1, int health = 1)
	{
		separatorConfig.type = t;
		separatorConfig.destroyType = dT;
		Separator.Sprites kit = GetKitByType (dT);
		separatorConfig.lives = health;
		img.sprite = kit.sprites [kit.sprites.Length - separatorConfig.lives];
		if(size != -1)
			rectTransform.sizeDelta = new Vector2 (rectTransform.sizeDelta.x, size);
	}
	public Separator.Sprites GetKitByType (Separator.DestroyType dt)
	{
		for(int i = 0; i < sprites.Length; i++)
		{
			if(sprites[i].destroyType == dt) return sprites[i];
		}
		return default(Separator.Sprites);
	}
	void Awake () 
	{
		rectTransform = GetComponent<RectTransform> ();
	}
}
