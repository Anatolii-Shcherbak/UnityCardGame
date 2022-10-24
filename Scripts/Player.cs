using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : MonoBehaviour, IDropHandler
{
    public Image playerImage = null;
    public Image mirrorimage = null;
    public Image healthNumberImage = null;
    public Image glowImage = null;

    public int maxHealth = 5;
    public int health = 5;
    public int mana = 1;

    public bool isPlayer;
    public bool isFire;

    public GameObject[] manaBalls = new GameObject[5];

    private Animator animator = null;

    public AudioSource DealAudio = null;
    public AudioSource HealAudio = null;
    public AudioSource MirrorAudio = null;
    public AudioSource smashAudio = null;
  
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        UpdateHealth();
        UpdateManaBalls();
    }

    internal void PlayHitAnim()
    {
        if (animator != null)
            animator.SetTrigger("Hit");
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!GameController.instance.isPlayabale)
            return;
        GameObject obj = eventData.pointerDrag;
        if(obj!=null)
        {
            Card card = obj.GetComponent<Card>();
            if(card!=null)
            {
                GameController.instance.UseCard(card, this, GameController.instance.playersHand);
            }
        }
    }

    internal void UpdateHealth()
    {
        if (health>=0 && health< GameController.instance.healthNumbers.Length)
        {
            healthNumberImage.sprite = GameController.instance.healthNumbers[health];
        }
        else
        {
            Debug.LogWarning("Health is not a vaild number, " + health.ToString());
        }
    }    

    internal  void SetMirror(bool on)
    {
        mirrorimage.gameObject.SetActive(on);
    }

    internal bool hasMirror()
    {
        return mirrorimage.gameObject.activeInHierarchy;
    }

    internal void UpdateManaBalls()
    {
        for(int m=0; m<5; m++)
        {
            if (mana > m)
                manaBalls[m].SetActive(true);
            else
                manaBalls[m].SetActive(false);
        }
    }
    internal void PlayMirrorSound()
    {
        MirrorAudio.Play();
    }

    internal void PlayHealSound()
    {
        HealAudio.Play();
    }

    internal void PlayDealSound()
    {
       DealAudio.Play();
    }

    internal void PlaySmashSound()
    {
        smashAudio.Play();
    }


}
