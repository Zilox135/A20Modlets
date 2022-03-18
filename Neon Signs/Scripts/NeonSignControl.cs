using System;
using UnityEngine;

public class NeonSignControl : MonoBehaviour 
{
	public int cIdx;
	public Vector3i blockPos;
	public GameObject litSignObject;
	private bool isSignActive;
	private int flashSpeed;
	private DateTime nextStateChangeTime;
	private bool flash;
	private bool flicker;
	public BlockNeonSign blockNeonSign;
	
	public void SetUp(BlockNeonSign block)
	{
		if (block.Properties.Values.ContainsKey("Flash"))
        {
			bool.TryParse(block.Properties.Values["Flash"], out flash);
			if (block.Properties.Values.ContainsKey("FlashSpeed"))
			{
				int.TryParse(block.Properties.Values["FlashSpeed"], out flashSpeed);
			}
			else
			flashSpeed = 1;
        }
		else
		{
			flash = false;
		}
		if(flashSpeed == 0 && flash)
		{
			flicker = true;
		}
		else
		flicker = false;
		GetColorSlots(block);
	}
	
	void Update()
	{
		if(litSignObject == null)
		{
			return;
		}
		if(BlockNeonSign.isBlockPoweredUp(blockPos, cIdx))
		{
			isSignActive = true;
			
		}
		else
		{
			isSignActive = false;
			nextStateChangeTime = default(DateTime);
		}
		if(isSignActive)
		{
			if(litSignObject != null && flicker)
			{
				if(!litSignObject.activeInHierarchy)
				{
					litSignObject.SetActive(true);
				}
				else litSignObject.SetActive(false);
				return;
			}
			if(flash)
			{
				//Flashing
				if(nextStateChangeTime == default(DateTime))
				{
					nextStateChangeTime = DateTime.Now;
				}
				if (DateTime.Now > nextStateChangeTime)
				{
					if(litSignObject != null)
					{
						if(litSignObject.activeInHierarchy)
						{
							litSignObject.SetActive(false);
						}
						else
						{
							litSignObject.SetActive(true);
						}
						nextStateChangeTime = DateTime.Now.AddSeconds(flashSpeed);
					}
				}
			}
			else
			{
				//No Flashing
				if(litSignObject != null && !litSignObject.activeInHierarchy)
				{
					litSignObject.SetActive(true);
				}
			}
		}
		else
		{
			if(litSignObject != null && litSignObject.activeInHierarchy)
			{
				litSignObject.SetActive(false);
			}
		}
	}
	
	private void GetColorSlots(Block block)
	{
		Transform[] colorSlots = litSignObject.GetComponentsInChildren<Transform>();
		foreach (Transform slot in colorSlots)
		{
			if(slot != null)
			{
				// Debug.Log("color slot found");
			}
			GameObject slotObject = slot.gameObject;
			if(slotObject != null)
			{
				if(slotObject.name.Contains("color"))
				{
					if (block.Properties.Values.ContainsKey(slotObject.name))
					{
						Vector3 colorVector = StringParsers.ParseVector3(block.Properties.Values[slotObject.name], 0, -1);
						SetColor(slotObject, colorVector);
					}
				}
			}
			
		}
	}
	
	private void SetColor(GameObject _slotObject, Vector3 _colorVector)
    {
        Color color = new Color(_colorVector.x / 256, _colorVector.y / 256, _colorVector.z / 256);
        // Debug.Log("Vector: " + _colorVector.ToString() + " - Color: " + color.ToString());
        Transform[] children = _slotObject.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            GameObject gameObject = child.gameObject;
			if(gameObject != null)
			{
				Renderer rend = gameObject.GetComponent<Renderer>();
				if(rend != null)
				{
					rend.material.EnableKeyword("_EMISSION");
					rend.material.SetColor("_Emission", color);
				}
			}
        }
    }
}