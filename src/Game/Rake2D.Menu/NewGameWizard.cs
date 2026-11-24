using Microsoft.Xna.Framework;
using Rake2D.JnG;

namespace Rake2D.Menu;

public class NewGameWizard
{
	public enum WizardStateEnum
	{
		Hidden,
		Hiding,
		Showing,
		NewGameDialog,
		NewPlayerDialog_1,
		NewPlayerDialog_2
	}

	public WizardStateEnum State;

	public NewGameDialog NewGameDialog;

	public NewPlayerDialog NewPlayerDialog1;

	public NewPlayerDialog NewPlayerDialog2;

	public int PlayerCount;

	public GameModeEnum GameMode;

	public byte Alpha;

	private MainMenu _mainMenu;

	private ShopMenu _shopMenu;

	public bool IsKeyInputFree
	{
		get
		{
			if (NewPlayerDialog1.IsKeyInputFree)
			{
				return NewPlayerDialog2.IsKeyInputFree;
			}
			return false;
		}
	}

	public NewGameWizard(ShopMenu shopMenu, MainMenu mainMenu)
	{
		_mainMenu = mainMenu;
		_shopMenu = shopMenu;
		NewGameDialog = new NewGameDialog(shopMenu, mainMenu, this);
		NewPlayerDialog1 = new NewPlayerDialog(shopMenu, mainMenu, this);
		NewPlayerDialog2 = new NewPlayerDialog(shopMenu, mainMenu, this);
	}

	public void SetTexts()
	{
		NewGameDialog.SetTexts();
		NewPlayerDialog1.SetTexts();
		NewPlayerDialog2.SetTexts();
	}

	public void Show()
	{
		PlayerCount = 1;
		GameMode = GameModeEnum.Standard;
		State = WizardStateEnum.Showing;
		NewGameDialog.Show();
		NewPlayerDialog1.SetState(ShopMenuState.Hidden);
		NewPlayerDialog2.SetState(ShopMenuState.Hidden);
		NewPlayerDialog1.HeroName = TextBundle.GetText("default_player_name_1");
		NewPlayerDialog1.FaceNumber = 0;
		NewPlayerDialog2.HeroName = TextBundle.GetText("default_player_name_2");
		NewPlayerDialog2.FaceNumber = 1;
	}

	public void Hide()
	{
		NewGameDialog.Hide();
		NewPlayerDialog1.Hide();
		NewPlayerDialog2.Hide();
		State = WizardStateEnum.Hiding;
	}

	public void OnOKButtonPressed(Button btn)
	{
		switch (State)
		{
		case WizardStateEnum.NewGameDialog:
			State = WizardStateEnum.NewPlayerDialog_1;
			NewGameDialog.Hide();
			NewPlayerDialog1.Show();
			Alpha = byte.MaxValue;
			break;
		case WizardStateEnum.NewPlayerDialog_1:
			State = WizardStateEnum.NewPlayerDialog_2;
			NewPlayerDialog1.Hide();
			if (PlayerCount > 1)
			{
				NewPlayerDialog2.Show();
			}
			else
			{
				CreateNewGame();
			}
			break;
		case WizardStateEnum.NewPlayerDialog_2:
			CreateNewGame();
			break;
		}
	}

	public void OnCancelButtonPressed(Button btn)
	{
		switch (State)
		{
		case WizardStateEnum.NewGameDialog:
			Hide();
			break;
		case WizardStateEnum.NewPlayerDialog_1:
			State = WizardStateEnum.NewGameDialog;
			NewPlayerDialog1.Hide();
			NewGameDialog.Show();
			break;
		case WizardStateEnum.NewPlayerDialog_2:
			State = WizardStateEnum.NewPlayerDialog_1;
			NewPlayerDialog2.Hide();
			NewPlayerDialog1.Show();
			break;
		}
	}

	private void CreateNewGame()
	{
		Vector2[] array = new Vector2[_mainMenu.MyHeroArray.Length];
		for (int i = 0; i < _mainMenu.MyHeroArray.Length; i++)
		{
			Hero hero = _mainMenu.MyHeroArray[i];
			hero.AutopilotStateX = HeroAutopilotStateXEnum.OutOfGame;
			array[i] = hero.LastPosition;
		}
		Hide();
		GameJnG.GameMode = GameMode;
		GameJnG.SetDifficulty(1);
		Campaign.HideAllSecretLevels();
		Hero[] array2 = new Hero[PlayerCount];
		for (int j = 0; j < PlayerCount; j++)
		{
			Hero hero2 = (array2[j] = new Hero(j));
			hero2.CreateDefaultShip();
			if (array.Length > j)
			{
				hero2.LastPosition = array[j];
			}
			else
			{
				hero2.LastPosition = array[0];
			}
			if (j == 0)
			{
				hero2.PlayerName = NewPlayerDialog1.PlayerName;
				hero2.FaceNumber = NewPlayerDialog1.FaceNumber;
			}
			else
			{
				hero2.PlayerName = NewPlayerDialog2.PlayerName;
				hero2.FaceNumber = NewPlayerDialog2.FaceNumber;
			}
		}
		TotalStats.Recalculate();
		GameJnG.SaveGameVersion = SaveGame.SAVE_GAME_VERSION;
		GameJnG.SetCheater(isCheater: false);
		SaveGame.SaveDefaultSavegame(array2, _mainMenu.ProfileSelector.EditedProfile + 1);
		_mainMenu.ProfileSelector.ReloadSavegameList();
		SaveGameInfo saveGameInfo = _mainMenu.ProfileSelector.GetSaveGameInfo(_mainMenu.ProfileSelector.EditedProfile);
		if (saveGameInfo != null)
		{
			saveGameInfo.MyObject.ChangePartState("change_to_full");
			GameJnG.SaveGameNumber = saveGameInfo.Number;
		}
	}

	public void Update(float elapsedTime)
	{
		if (NewGameDialog.State != ShopMenuState.Hidden)
		{
			NewGameDialog.Update(elapsedTime);
		}
		if (NewPlayerDialog1.State != ShopMenuState.Hidden)
		{
			NewPlayerDialog1.Update(elapsedTime);
		}
		if (NewPlayerDialog2.State != ShopMenuState.Hidden)
		{
			NewPlayerDialog2.Update(elapsedTime);
		}
		switch (State)
		{
		case WizardStateEnum.Showing:
			Alpha = NewGameDialog.Alpha;
			if (Alpha >= byte.MaxValue)
			{
				State = WizardStateEnum.NewGameDialog;
			}
			break;
		case WizardStateEnum.Hiding:
			Alpha = NewGameDialog.Alpha;
			if (Alpha <= 0)
			{
				State = WizardStateEnum.Hidden;
			}
			break;
		case WizardStateEnum.NewGameDialog:
			Alpha = byte.MaxValue;
			break;
		case WizardStateEnum.NewPlayerDialog_1:
			Alpha = byte.MaxValue;
			break;
		case WizardStateEnum.NewPlayerDialog_2:
			Alpha = byte.MaxValue;
			break;
		}
	}

	public void Draw()
	{
		if (NewGameDialog.State != ShopMenuState.Hidden)
		{
			NewGameDialog.Draw();
		}
		if (NewPlayerDialog1.State != ShopMenuState.Hidden)
		{
			NewPlayerDialog1.Draw();
		}
		if (NewPlayerDialog2.State != ShopMenuState.Hidden)
		{
			NewPlayerDialog2.Draw();
		}
	}
}
