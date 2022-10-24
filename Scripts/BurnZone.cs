﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class BurnZone : MonoBehaviour, IDropHandler
{
    public AudioSource burnAudio = null;
    public void OnDrop(PointerEventData eventData)
    {
        if (GameController.instance.playerTurn)
        {
            GameObject obj = eventData.pointerDrag;
            Card card = obj.GetComponent<Card>();
            if (card != null)
            {
                PlayBurnSound();
                GameController.instance.playersHand.RemoveCard(card);
                GameController.instance.NextPlayersTurn();
            }
            return;
        }
    }

    internal void PlayBurnSound()
    {
        burnAudio.Play();
    }

}
