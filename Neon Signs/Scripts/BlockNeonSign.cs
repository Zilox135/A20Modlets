using System;
using UnityEngine;

public class BlockNeonSign : BlockPoweredLight
{
	static bool showDebugLog = false;
	public Vector3i blockPos;
	
	public string LitObject()
	{
		string litObjectName = @"DefaultLitObjectName";
		if (this.Properties.Values.ContainsKey("LightObject"))
		{
			litObjectName = this.Properties.Values["LightObject"].Replace(@"DefaultLitObjectName", @"DefaultLitObjectName");
		}
		else
		litObjectName = "LitObjects";
		return litObjectName;
	}
	
	public static void DebugMsg(string msg)
	{
		if(showDebugLog)
		{
			Debug.Log(msg);
		}
	}
	
	public override bool OnBlockActivated(int _indexInBlockActivationCommands, WorldBase _world, int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		switch (_indexInBlockActivationCommands)
		{
		case 0:
		{
			TileEntityPoweredBlock tileEntityPoweredBlock = (TileEntityPoweredBlock)_world.GetTileEntity(_cIdx, _blockPos);
			if (!_world.IsEditor() && tileEntityPoweredBlock != null)
			{
				tileEntityPoweredBlock.IsToggled = !tileEntityPoweredBlock.IsToggled;
			}
			break;
		}
		case 1:
			TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
			return true;
		}
		return false;
	}
	
	public override void OnBlockEntityTransformBeforeActivated(WorldBase _world, Vector3i _blockPos, int _cIdx, BlockValue _blockValue, BlockEntityData _ebcd)
	{
		this.shape.OnBlockEntityTransformBeforeActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
		if(_ebcd != null && _ebcd.bHasTransform)
		{
			GameObject gameObject = _ebcd.transform.gameObject;
			if(gameObject != null)
			{
				GameObject litSignObject = _ebcd.transform.Find(LitObject()).gameObject;
				if(litSignObject != null)
				{
					NeonSignControl neonSignScript = gameObject.GetComponent<NeonSignControl>();
					if(neonSignScript == null)
					{
						neonSignScript = gameObject.AddComponent<NeonSignControl>();
					}
					if(neonSignScript != null)
					{
						neonSignScript.enabled = false;
						neonSignScript.blockNeonSign = this;
						neonSignScript.blockPos = _blockPos;
						neonSignScript.cIdx = _cIdx;
						neonSignScript.litSignObject = litSignObject;
						neonSignScript.litSignObject.SetActive(false);
						neonSignScript.enabled = true;
						neonSignScript.SetUp(this);
					}
					
				}
			}
		}
	}
	
	public static bool isBlockPoweredUp(Vector3i _blockPos, int _clrIdx)
	{
		WorldBase world = GameManager.Instance.World;
		TileEntityPoweredBlock tileEntityPoweredBlock = (TileEntityPoweredBlock)world.GetTileEntity(_clrIdx, _blockPos);
		if (!world.IsEditor() && tileEntityPoweredBlock != null)
		{
			if(tileEntityPoweredBlock.IsPowered && tileEntityPoweredBlock.IsToggled)
			{
				return true;
			}
			else
				return false;
		}
		return false;
	}
	
	private BlockActivationCommand[] cmds = new BlockActivationCommand[2]
	{
		new BlockActivationCommand("light", "electric_switch", _enabled: true),
		new BlockActivationCommand("take", "hand", _enabled: false)
	};
	
	public void TakeItemWithTimer(int _cIdx, Vector3i _blockPos, BlockValue _blockValue, EntityAlive _player)
	{
		if (_blockValue.damage > 0)
		{
			GameManager.ShowTooltip(_player as EntityPlayerLocal, Localization.Get("ttRepairBeforePickup"), "ui_denied");
			return;
		}
		LocalPlayerUI playerUI = (_player as EntityPlayerLocal).PlayerUI;
		playerUI.windowManager.Open("timer", _bModal: true);
		XUiC_Timer childByType = playerUI.xui.GetChildByType<XUiC_Timer>();
		TimerEventData timerEventData = new TimerEventData();
		timerEventData.Data = new object[4] { _cIdx, _blockValue, _blockPos, _player };
		timerEventData.Event += EventData_EventAlt;
		childByType.SetTimer(TakeDelay, timerEventData);
	}
	
	private void EventData_EventAlt(TimerEventData timerData)
	{
		World world = GameManager.Instance.World;
		object[] obj = (object[])timerData.Data;
		int clrIdx = (int)obj[0];
		BlockValue blockValue = (BlockValue)obj[1];
		Vector3i vector3i = (Vector3i)obj[2];
		BlockValue block = world.GetBlock(vector3i);
		EntityPlayerLocal entityPlayerLocal = obj[3] as EntityPlayerLocal;
		if (block.damage > 0)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttRepairBeforePickup"), "ui_denied");
			return;
		}
		if (block.type != blockValue.type)
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttBlockMissingPickup"), "ui_denied");
			return;
		}
		TileEntityPowered tileEntityPowered = world.GetTileEntity(clrIdx, vector3i) as TileEntityPowered;
		if (tileEntityPowered != null && tileEntityPowered.IsUserAccessing())
		{
			GameManager.ShowTooltip(entityPlayerLocal, Localization.Get("ttCantPickupInUse"), "ui_denied");
			return;
		}
		LocalPlayerUI uIForPlayer = LocalPlayerUI.GetUIForPlayer(entityPlayerLocal);
		HandleTakeInternalItems(tileEntityPowered, uIForPlayer);
		ItemStack itemStack = new ItemStack(block.ToItemValue(), 1);
		Block thisBlock = Block.list[block.type];
		if (thisBlock.Properties.Values.ContainsKey("TakeAltBlock"))
		{
			string altBlockName = thisBlock.Properties.Values["TakeAltBlock"];
			if(!String.IsNullOrEmpty(altBlockName))
			{
				ItemValue altBlockValue = ItemClass.GetItem(altBlockName, false);
				itemStack = new ItemStack(altBlockValue, 1);
			}
		}
		if (!uIForPlayer.xui.PlayerInventory.AddItem(itemStack))
		{
			uIForPlayer.xui.PlayerInventory.DropItem(itemStack);
		}
		world.SetBlockRPC(clrIdx, vector3i, BlockValue.Air);
	}
}