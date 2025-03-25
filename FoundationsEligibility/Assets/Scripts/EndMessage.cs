using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EndMessage : MonoBehaviour
{

	[SerializeField]
	private TMP_Text _playerMessage = null;

	public void OnGameEnded(int winner)
	{
        _playerMessage.text = winner == 0 ? "Tie" : winner == 2 ? "AI wins" : "Player wins";
    }
}
