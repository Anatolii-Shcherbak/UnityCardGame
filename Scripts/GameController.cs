using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    static public GameController instance = null;

    public Deck playerDeck = new Deck();
    public Deck enemyDeck = new Deck();

    public Hand playersHand = new Hand();
    public Hand enemyHand = new Hand();

    public Player player = null;
    public Player enemy = null;

    public List<CardData> cards = new List<CardData>();

    public Sprite[] healthNumbers = new Sprite[10];
    public Sprite[] damageNumbers = new Sprite[10];

    public GameObject cardPrefab = null;
    public Canvas canvas = null;

    public bool isPlayabale = false;

    public GameObject effectFromLeftPrefab = null;
    public GameObject effectFromRightPrefab = null;

    public Sprite fireBallImage = null;
    public Sprite IceBallImage = null;
    public Sprite multifireBallImage = null;
    public Sprite multiIceBallImage = null;
    public Sprite fireAndIceBallImage = null;
    public Sprite DestructBallImage = null;

    public bool playerTurn = true;

    public Text turnText = null;

    public Image enemySkipTurn = null;

    public Sprite FireDemon = null;
    public Sprite IceDemon = null;
    public Text scoreText = null;
    public int playerScore = 0;
    public int playersKill = 0;
    public AudioSource PlayerdieAudio = null;
    public AudioSource EnemydieAudio = null;

    private void Awake()
    {
        instance = this;
        SetUpEnemy();

        playerDeck.Create();
        enemyDeck.Create();

        StartCoroutine(DealHands());
    }
    public void Quit()
    {
        SceneManager.LoadScene(0);
    }

    public void SkipTurn()
    {
        if(playerTurn && isPlayabale)
        NextPlayersTurn();
    }

    internal IEnumerator DealHands()
    {
        yield return new WaitForSeconds(1);
        for (int t=0; t<3; t++)
        {
            playerDeck.DealCard(playersHand);
            enemyDeck.DealCard(enemyHand);
            yield return new WaitForSeconds(1);
        }
        isPlayabale = true;
    }    

    internal bool UseCard(Card card, Player usingOnPlayer, Hand fromHand)
    {
        if (!CardValid(card, usingOnPlayer, fromHand))
            return false;

        isPlayabale = false;

        Castcard(card, usingOnPlayer, fromHand);

        player.glowImage.gameObject.SetActive(false); 
        enemy.glowImage.gameObject.SetActive(false);

        fromHand.RemoveCard(card);

        return false;
    }

    internal bool CardValid(Card cardBeingPlayed, Player usingOnPlayer, Hand fromHand)
    {
        bool valid = false;
        if (cardBeingPlayed == null)
            return false;

        if (fromHand.isPlayers)
        {
            if(cardBeingPlayed.cardData.cost<=player.mana)
            {
                if (usingOnPlayer.isPlayer && cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;
                if (!usingOnPlayer.isPlayer && !cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;

            }
        }
        else
        {
            if (cardBeingPlayed.cardData.cost <= enemy.mana)
            {
                if (!usingOnPlayer.isPlayer && cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;
                if (usingOnPlayer.isPlayer && !cardBeingPlayed.cardData.isDefenseCard)
                    valid = true;

            }
        }
        return valid;
    }

    internal void Castcard(Card card, Player usingOnPlayer, Hand fromHand)
    {
        if(card.cardData.isMirrorCard)
        {
            usingOnPlayer.SetMirror(true);
            usingOnPlayer.PlayMirrorSound();
            NextPlayersTurn();
            isPlayabale = true;
        }
        else
        {
            if (card.cardData.isDefenseCard)
            {
                usingOnPlayer.health += card.cardData.damage;
                usingOnPlayer.PlayHealSound();

                if (usingOnPlayer.health > usingOnPlayer.maxHealth)
                    usingOnPlayer.health = usingOnPlayer.maxHealth;

                UpdateHealths();

                StartCoroutine(CastHealEffect(usingOnPlayer));
                     
            }
            else // Attack card
            {
                CastAttackEffect(card, usingOnPlayer);
            }

            if (fromHand.isPlayers)
                playerScore += card.cardData.damage;

            UpdateScore();
        }
        if(fromHand.isPlayers)
        {
            GameController.instance.player.mana -= card.cardData.cost;
            GameController.instance.player.UpdateManaBalls();
        }
        else
        {
            GameController.instance.enemy.mana -= card.cardData.cost;
            GameController.instance.enemy.UpdateManaBalls();
        }
    }

    private IEnumerator CastHealEffect(Player usingOnplayer)
    {
        yield return new WaitForSeconds(0.5f);
        NextPlayersTurn();
        isPlayabale = true;
    }

    internal void CastAttackEffect(Card card, Player usingOnPlayer)
    {
        GameObject effectGo = null;
        if (usingOnPlayer.isPlayer)
            effectGo = Instantiate(effectFromRightPrefab, canvas.gameObject.transform);
        else
            effectGo = Instantiate(effectFromLeftPrefab, canvas.gameObject.transform);

        Effect effect = effectGo.GetComponent<Effect>();
        if(effect)
        {
            effect.targetPlayer = usingOnPlayer;
            effect.sourceCard = card; 

            switch(card.cardData.damageType)
            {
                case CardData.DamageType.Fire:    
                        if (card.cardData.isMulti)
                        effect.effectImage.sprite = multifireBallImage;
                    else
                        effect.effectImage.sprite = fireBallImage;
                    effect.PlayFireSound();
                    break;

                case CardData.DamageType.Destruct:
                    if(card.cardData.isDestruct)
                        effect.effectImage.sprite = DestructBallImage;
                    effect.PlayBoomSound();
                    break;

                case CardData.DamageType.Ice:
                    if (card.cardData.isMulti)
                        effect.effectImage.sprite = multiIceBallImage;
                    else
                        effect.effectImage.sprite = IceBallImage;
                    effect.PlayIcrSound();
                    break;

                case CardData.DamageType.Both:
                        effect.effectImage.sprite = fireAndIceBallImage;
                    effect.PlayFireSound();
                    effect.PlayIcrSound();
                    break;
            }
        }
    }

    internal void UpdateHealths()
    {
        player.UpdateHealth();
        enemy.UpdateHealth();

        if(player.health<= 0)
        {
            StartCoroutine(GameOver());
        }

        if (enemy.health<=0)
        {
            playersKill++;
            playerScore += 100;
            UpdateScore();
            StartCoroutine(NewEnemy());
        }
    }
    private IEnumerator NewEnemy()
    {
        enemy.gameObject.SetActive(false);

        enemyHand.ClearHand();
        yield return new WaitForSeconds(0.75f);
        SetUpEnemy();

        enemy.gameObject.SetActive(true);
        StartCoroutine(DealHands());
    }

    private void SetUpEnemy()
    {
        enemy.mana = 0;
        enemy.health = 5;
        enemy.UpdateHealth();
        enemy.isFire = true;
        if (UnityEngine.Random.Range(0, 2) == 1)
            enemy.isFire = false;
        if (enemy.isFire)
            enemy.playerImage.sprite = FireDemon;
        else
            enemy.playerImage.sprite = IceDemon;
    }
    private IEnumerator GameOver()
    {
        yield return new WaitForSeconds(1);
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }
    internal void NextPlayersTurn()
    {
        playerTurn = !playerTurn;
        bool enemyIsDead = false;
        if (playerTurn)
        {
            if (player.mana < 5)
                player.mana++;
        }
        else
        {
            if (enemy.health > 0)
            {
                if (enemy.mana < 5)
                    enemy.mana++;

            }

            else
                enemyIsDead = true;
        }



        if (enemyIsDead)
        {
            playerTurn = !playerTurn;
            if (player.mana < 5)
                player.mana++;
        }

        else
        {
            SetTurnText();
            if (!playerTurn)
                MonstersTurn();

        }
        player.UpdateManaBalls();
        enemy.UpdateManaBalls();
    }
    internal void SetTurnText()
    {
        if (playerTurn)
        {
            turnText.text = "Merlin's Turn";
        }
        else
        {
            turnText.text = "Demon's Turn";
        }
    }

    private void MonstersTurn()
    {
        Card card = AIChooseCard();
        StartCoroutine(MonsterCastCard(card));
    }

    private Card AIChooseCard()
    {
        List<Card> available = new List<Card>();
        for(int i=0; i<3; i++)
        {
            if (CardValid(enemyHand.cards[i], enemy, enemyHand))
                available.Add(enemyHand.cards[i]);
            else if (CardValid(enemyHand.cards[i], player, enemyHand))
                available.Add(enemyHand.cards[i]);
        }
        if(available.Count ==0)
        {
            NextPlayersTurn();
            return null;
        }
        int choice = UnityEngine.Random.Range(0, available.Count);
        return available[choice];
    }

    private IEnumerator MonsterCastCard(Card card)
    {
        yield return new WaitForSeconds(0.5f);

        if(card)
        {
            TurnCard(card);

            yield return new WaitForSeconds(2);
            if (card.cardData.isDefenseCard)
                UseCard(card, enemy, enemyHand);
            else
                UseCard(card, player, enemyHand);

            yield return new WaitForSeconds(1);
            enemyDeck.DealCard(enemyHand);
            yield return new WaitForSeconds(1);
        }
        else
        {
            enemySkipTurn.gameObject.SetActive(true);
            yield return new WaitForSeconds(1);
            enemySkipTurn.gameObject.SetActive(false);
        }
    }
    internal void TurnCard(Card card)
    {
        Animator animator = card.GetComponentInChildren<Animator>();
        if (animator)
        {
            animator.SetTrigger("Flip");
        }
        else
            Debug.LogError("No Animator found");
    }

    private void UpdateScore()
    {
        scoreText.text = "Demons killed: " + playersKill.ToString() + ".  Score: " + playerScore.ToString();
    }

    internal void PlayPlayerDieSound()
    {
        PlayerdieAudio.Play();
    }

    internal void PlayEnemyDieSound()
    {
       EnemydieAudio.Play();
    }
}
