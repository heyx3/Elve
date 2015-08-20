using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using Assert = UnityEngine.Assertions.Assert;


/// <summary>
/// A pop-up context menu that appears when the player clicks on something.
/// </summary>
/// <typeparam name="ContextData">
/// The type of context associated with this menu.
/// </typeparam>
/// <remarks>
/// This script requires the object to have a Text component that acts as a blueprint
/// for the items in this menu as well as an Image component that acts as a blueprint
/// for the invisible buttons on each item in this menu.
/// </remarks>
[RequireComponent(typeof(Text))]
public class ContextMenu<ContextData> : MonoBehaviour
{
	/// <summary>
	/// Helper method for "Start()" that gets around some issues with lambdas inside loops.
	/// </summary>
	private void AddListener(Button b, IContextMenuItem<ContextData> item)
	{
		b.onClick.AddListener(() => { item.OnSelected(Context, MyTransform.position); });
	}


	public Camera UICam = null;
	public RectTransform BackgroundImage = null;

	public float AppearAnimLength = 0.1f;

	public Color ItemHoverColor = new Color(0.6235f, 0.6235f, 0.6235f, 0.185f),
				 ItemPressedColor = new Color(0.4f, 0.4f, 0.4f, 0.185f);

	public Vector2 Border = new Vector2(5.0f, 8.0f);
	
	/// <summary>
	/// The items that this menu can display. Initialized by the ContextMenuInitializer component.
	/// </summary>
	[NonSerialized]
	public List<IContextMenuItem<ContextData>> Items;


	public Transform MyTransform { get; private set; }
	public ContextData Context { get; private set; }


	private Vector2 targetScale;
	private float elapsedTime;
	
	private List<Transform> itemObjs = new List<Transform>();
	private Vector2 itemSize;


	void Awake()
	{
		MyTransform = transform;
		targetScale = MyTransform.transform.localScale;

		Assert.IsNotNull(UICam);
		Assert.IsNotNull(BackgroundImage);

		ContextMenuInitializer<ContextData> init = GetComponent<ContextMenuInitializer<ContextData>>();
		Assert.IsNotNull(init, "Must have a context menu initializer component on object '" +
								gameObject.name + "'!");
		Items = init.GetAllItems();
		Destroy(init);

		Text myText = GetComponent<Text>();
		Rect textRect = myText.rectTransform.rect;
		itemSize = new Vector2(textRect.width, textRect.height);

		//Generate an item in the menu for each possible entry.
		itemObjs = new List<Transform>();
		itemObjs.AddRange(Items.Select(item =>
			{
				string text = item.Text;
				Transform tr = new GameObject(text).transform;
				tr.parent = MyTransform;


				//Give the item button functionality.
				Image i = tr.gameObject.AddComponent<Image>();
				tr = i.transform;
				i.type = Image.Type.Simple;
				i.material.mainTexture = Texture2D.whiteTexture;
				i.rectTransform.sizeDelta = itemSize;

				Button b = tr.gameObject.AddComponent<Button>();
				b.interactable = true;
				b.transition = Selectable.Transition.ColorTint;
				b.targetGraphic = i;
				ColorBlock cols = new ColorBlock();
				cols.normalColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
				cols.highlightedColor = ItemHoverColor;
				cols.pressedColor = ItemPressedColor;
				cols.colorMultiplier = 1.0f;
				cols.fadeDuration = 0.1f;
				b.colors = cols;
				AddListener(b, item);


				//Create the text label as a child of the button.
				
				Transform textTr = new GameObject("Text").transform;
				textTr.parent = tr;
				textTr.localPosition = Vector3.zero;
				textTr.localScale = Vector3.one;

				Text t = textTr.gameObject.AddComponent<Text>();
				textTr = t.transform;
				t.font = myText.font;
				t.fontStyle = myText.fontStyle;
				t.fontSize = myText.fontSize;
				t.lineSpacing = myText.lineSpacing;
				t.supportRichText = myText.supportRichText;
				t.alignment = myText.alignment;
				t.horizontalOverflow = myText.horizontalOverflow;
				t.verticalOverflow = myText.verticalOverflow;
				t.resizeTextForBestFit = myText.resizeTextForBestFit;
				t.resizeTextMinSize = myText.resizeTextMinSize;
				t.resizeTextMaxSize = myText.resizeTextMaxSize;
				t.material = myText.material;
				t.color = myText.color;
				t.text = text;
				t.rectTransform.sizeDelta = itemSize;

				return tr;
			}));

		//Disable the text item blueprint; we don't need it anymore.
		myText.enabled = false;
	}

	/// <summary>
	/// Activates this popup menu with the given context.
	/// If there are no available options, the menu de-activates itself and this method returns false.
	/// Otherwise, it returns true.
	/// </summary>
	public bool SetUp(ContextData context, Vector2 screenPos)
	{
		gameObject.SetActive(true);

		MyTransform.position = new Vector3(screenPos.x, screenPos.y, MyTransform.position.z);

		Context = context;

		//Enable/position the applicable items.
		int nOptions = 0;
		float dir = (screenPos.y > Screen.height * 0.5f) ? -1.0f : 1.0f;
		Vector2 startPos = screenPos + new Vector2(Border.x + (itemSize.x * 0.5f),
												  dir * (Border.y + (itemSize.y * 0.5f)));
		Vector2 nextPos = startPos;
		for (int i = 0; i < Items.Count; ++i)
		{
			if (Items[i].IsItemAvailable(Context))
			{
				itemObjs[i].gameObject.SetActive(true);
				itemObjs[i].position = new Vector3(nextPos.x, nextPos.y, itemObjs[i].position.z);

				nextPos.y += dir * itemSize.y;

				nOptions += 1;
			}
			else
			{
				itemObjs[i].gameObject.SetActive(false);
			}
		}

		if (nOptions == 0)
		{
			gameObject.SetActive(false);
			return false;
		}
		else
		{
			float finalY = nextPos.y - (dir * itemSize.y) + (0.5f * itemSize.y * dir) + (dir * Border.y);
			Rect r = new Rect(screenPos.x,
							  Mathf.Min(screenPos.y, finalY),
							  itemSize.x + Border.x + Border.x,
							  Mathf.Abs(finalY - screenPos.y));
			BackgroundImage.position = r.center;
			BackgroundImage.sizeDelta = r.size;
		}

		return true;
	}

	void OnEnable()
	{
		elapsedTime = 0.0f;
	}
	void LateUpdate()
	{
		MyTransform.localScale = Vector3.Lerp(new Vector3(0.0000001f, 0.0000001f, 1.0f),
											  targetScale,
											  Mathf.Clamp01(elapsedTime / AppearAnimLength));
	}
	void Update()
	{
		elapsedTime += Time.deltaTime;
	}
}