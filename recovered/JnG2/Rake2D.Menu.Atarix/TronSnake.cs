using System;
using System.Collections.Generic;
using System.Linq;
using Rake2D.JnG;

namespace Rake2D.Menu.Atarix;

public class TronSnake
{
	private AtarixTron _tron;

	private List<TronRect> _rectList;

	private int _currMoveDirection;

	private int _wantedMoveDirection;

	private int _raiseCount;

	private float _moveTimer;

	public TronSnake(AtarixTron tron)
	{
		_tron = tron;
	}

	public void Build()
	{
		_rectList = new List<TronRect>();
		for (int i = 0; i < 1; i++)
		{
			TronRect tronRect = new TronRect(_tron);
			tronRect.X = AtarixTron.GAME_W / 2;
			tronRect.Y = AtarixTron.GAME_H / 2;
			tronRect.Direction = -1;
			tronRect.Type = TronRect.RectTypeEnum.Snake;
			_rectList.Add(tronRect);
		}
		_raiseCount = 30 + GameJnG.AtarixAutoHackChance;
		_currMoveDirection = -1;
		_wantedMoveDirection = -1;
	}

	public bool IsOnPosition(int x, int y)
	{
		foreach (TronRect rect in _rectList)
		{
			if (rect.X == x && rect.Y == y)
			{
				return true;
			}
		}
		return false;
	}

	public void ChangeMoveDirection(int direction)
	{
		if ((_currMoveDirection < 0 || ((_currMoveDirection == 0 || _currMoveDirection == 2) && (direction == 1 || direction == 3)) || ((_currMoveDirection == 1 || _currMoveDirection == 3) && (direction == 0 || direction == 2))) && _wantedMoveDirection != direction)
		{
			_wantedMoveDirection = direction;
			GameCore.PlaySound("tron_turn");
		}
	}

	public void Update(float elapsedTime)
	{
		if (_wantedMoveDirection < 0 || !Utils.ChangeValueToTarget(ref _moveTimer, 0f, elapsedTime))
		{
			return;
		}
		_currMoveDirection = _wantedMoveDirection;
		_moveTimer = Math.Max(0.06f - 0.025f * ((float)GameJnG.AtarixAutoHackChance / 100f), 0.03333f);
		if (_rectList.Count <= 0)
		{
			return;
		}
		TronRect tronRect = _rectList.Last();
		TronRect tronRect2 = _rectList.First();
		int num = tronRect2.X;
		int num2 = tronRect2.Y;
		switch (_currMoveDirection)
		{
		case 0:
			num2--;
			break;
		case 1:
			num++;
			break;
		case 2:
			num2++;
			break;
		case 3:
			num--;
			break;
		}
		TronRect rectAtPos = _tron.GetRectAtPos(num, num2);
		TronRect.RectTypeEnum rectTypeEnum;
		if (rectAtPos != null)
		{
			_tron.RemovePoint(rectAtPos);
			_raiseCount += 10;
			rectTypeEnum = TronRect.RectTypeEnum.None;
		}
		else
		{
			rectTypeEnum = _tron.CheckPosition(num, num2);
		}
		if (rectTypeEnum == TronRect.RectTypeEnum.None)
		{
			TronRect tronRect3;
			if (_raiseCount > 0)
			{
				_raiseCount--;
				tronRect3 = new TronRect(_tron);
				tronRect3.Type = TronRect.RectTypeEnum.Snake;
			}
			else
			{
				tronRect3 = tronRect;
				_rectList.RemoveAt(_rectList.Count - 1);
			}
			tronRect3.Direction = _currMoveDirection;
			tronRect3.X = num;
			tronRect3.Y = num2;
			_rectList.Insert(0, tronRect3);
		}
		else
		{
			_tron.SetState(AtarixTron.TronStateEnum.Fail);
		}
	}

	public void Draw()
	{
		for (int i = 0; i < _rectList.Count; i++)
		{
			TronRect tronRect = _rectList[i];
			tronRect.ShadowSize = 4;
			tronRect.Frame = 1;
			tronRect.DrawShadow();
		}
		for (int j = 0; j < _rectList.Count; j++)
		{
			TronRect tronRect2 = _rectList[j];
			tronRect2.Frame = Utils.Clamp(j + 1, 1, 6);
			tronRect2.Draw();
		}
	}
}
