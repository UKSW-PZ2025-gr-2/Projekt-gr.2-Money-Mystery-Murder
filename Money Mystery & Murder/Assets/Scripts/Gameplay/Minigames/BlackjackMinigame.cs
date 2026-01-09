using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Classic Blackjack minigame where player bets money to try to beat the dealer.
/// Player must choose bet amount (minimum 1), then plays blackjack.
/// Win: get 2x bet. Lose/Bust: lose bet. Push (tie): get bet back.
/// Inherits from MinigameBase and integrates with Player.
/// </summary>
public class BlackjackMinigame : MinigameBase
{
    [Header("Blackjack Settings")]
    [SerializeField] private int minBet = 1;
    [SerializeField] private int defaultBet = 10;
    [SerializeField] private int maxBet = 1000;
    
    [Header("UI References")]
    [SerializeField] private GameObject uiPanel;
    [SerializeField] private TMP_Text playerCardsText;
    [SerializeField] private TMP_Text dealerCardsText;
    [SerializeField] private TMP_Text playerScoreText;
    [SerializeField] private TMP_Text dealerScoreText;
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text betAmountText;
    [SerializeField] private Button hitButton;
    [SerializeField] private Button standButton;
    [SerializeField] private Button betIncreaseButton;
    [SerializeField] private Button betDecreaseButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private GameObject bettingPanel;
    [SerializeField] private GameObject gamePanel;
    
    [Header("Card Display Settings")]
    [SerializeField] private float cardSpacing = 1.5f;
    [SerializeField] private float verticalOffset = 2f;
    [SerializeField] private Vector3 cardScale = new Vector3(3f, 3f, 3f);
    [SerializeField] private string sortingLayerName = "Default";
    [SerializeField] private int sortingOrder = 10;
    
    [Header("Card Sprites")]
    [SerializeField] private Sprite[] cardSprites; // 0-12: Ace through King for different suits
    [SerializeField] private Sprite cardBackSprite;
    
    // Game state
    private List<int> playerCards = new List<int>();
    private List<int> dealerCards = new List<int>();
    private int currentBet;
    private bool isPlayerTurn;
    private bool gameInProgress;
    private bool roundEnded;
    private GameObject playerCardsRoot;
    private GameObject dealerCardsRoot;
    private List<SpriteRenderer> playerCardRenderers = new List<SpriteRenderer>();
    private List<SpriteRenderer> dealerCardRenderers = new List<SpriteRenderer>();
    
    void Awake()
    {
        currentBet = defaultBet;
        
        // Setup button listeners - musi być w Awake żeby działać nawet gdy obiekt wyłączony
        if (hitButton != null)
            hitButton.onClick.AddListener(OnHitClicked);
        if (standButton != null)
            standButton.onClick.AddListener(OnStandClicked);
        if (betIncreaseButton != null)
            betIncreaseButton.onClick.AddListener(OnIncreaseBet);
        if (betDecreaseButton != null)
            betDecreaseButton.onClick.AddListener(OnDecreaseBet);
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnStartGameClicked);
    }
    
    void Start()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);
            
        UpdateBetDisplay();
    }
    
    protected override void OnStartGame()
    {
        if (uiPanel != null)
            uiPanel.SetActive(true);
            
        // Show betting panel first
        ShowBettingPanel();
        Debug.Log("[BlackjackMinigame] Started - choose your bet.");
    }
    
    protected override void OnEndGame()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);
            
        CleanupCards();
        Debug.Log("[BlackjackMinigame] Ended.");
    }
    
    private void ShowBettingPanel()
    {
        if (bettingPanel != null)
            bettingPanel.SetActive(true);
        if (gamePanel != null)
            gamePanel.SetActive(false);
            
        // Reset message
        if (messageText != null)
            messageText.text = "";
            
        // Clear cards from previous round
        CleanupCards();
        playerCards.Clear();
        dealerCards.Clear();
        
        // Clamp bet to player's balance
        if (ActivatingPlayer != null)
        {
            int maxAffordable = ActivatingPlayer.Balance;
            if (currentBet > maxAffordable)
                currentBet = Mathf.Max(minBet, maxAffordable);
        }
        
        UpdateBetDisplay();
    }
    
    private void ShowGamePanel()
    {
        if (bettingPanel != null)
            bettingPanel.SetActive(false);
        if (gamePanel != null)
            gamePanel.SetActive(true);
    }
    
    public void OnStartGameClicked()
    {
        if (ActivatingPlayer == null) return;
        
        // Check if player can afford bet
        if (ActivatingPlayer.Balance < currentBet)
        {
            if (messageText != null)
                messageText.text = "Not enough money!";
            return;
        }
        
        // Deduct bet
        ActivatingPlayer.SpendBalance(currentBet, false);
        Debug.Log($"[BlackjackMinigame] Player bet {currentBet}. Remaining balance: {ActivatingPlayer.Balance}");
        
        // Start actual game
        StartBlackjackRound();
    }
    
    private void StartBlackjackRound()
    {
        ShowGamePanel();
        
        // Clear previous round data
        playerCards.Clear();
        dealerCards.Clear();
        CleanupCards();
        
        gameInProgress = true;
        isPlayerTurn = true;
        roundEnded = false;
        
        // Clear message
        if (messageText != null)
            messageText.text = "";
        
        // Deal initial cards
        playerCards.Add(DrawCard());
        dealerCards.Add(DrawCard());
        playerCards.Add(DrawCard());
        dealerCards.Add(DrawCard());
        
        UpdateCardDisplay();
        UpdateUI();
        
        // Check for blackjack
        if (CalculateHandValue(playerCards) == 21)
        {
            // Player blackjack!
            OnStandClicked(); // Auto-stand
        }
    }
    
    public void OnHitClicked()
    {
        if (!isPlayerTurn || !gameInProgress) return;
        
        playerCards.Add(DrawCard());
        UpdateCardDisplay();
        UpdateUI();
        
        int playerValue = CalculateHandValue(playerCards);
        if (playerValue > 21)
        {
            // Bust!
            EndRound(false, true);
        }
        else if (playerValue == 21)
        {
            // Auto-stand on 21
            OnStandClicked();
        }
    }
    
    public void OnStandClicked()
    {
        if (!isPlayerTurn || !gameInProgress) return;
        
        isPlayerTurn = false;
        
        // Dealer's turn
        PlayDealerTurn();
    }
    
    private void PlayDealerTurn()
    {
        // Dealer draws until 17 or higher
        while (CalculateHandValue(dealerCards) < 17)
        {
            dealerCards.Add(DrawCard());
        }
        
        UpdateCardDisplay();
        UpdateUI();
        
        // Determine winner
        int playerValue = CalculateHandValue(playerCards);
        int dealerValue = CalculateHandValue(dealerCards);
        
        if (dealerValue > 21)
        {
            // Dealer bust - player wins
            EndRound(true, false);
        }
        else if (playerValue > dealerValue)
        {
            // Player wins
            EndRound(true, false);
        }
        else if (playerValue < dealerValue)
        {
            // Dealer wins
            EndRound(false, false);
        }
        else
        {
            // Push (tie) - return bet
            EndRound(false, false, true);
        }
    }
    
    private void EndRound(bool playerWon, bool playerBust, bool push = false)
    {
        gameInProgress = false;
        roundEnded = true;
        
        if (ActivatingPlayer != null)
        {
            if (push)
            {
                // Return bet
                ActivatingPlayer.AddBalance(currentBet);
                if (messageText != null)
                    messageText.text = $"PUSH! Bet returned: ${currentBet}\n\n[SPACE] Play Again | [ESC] Exit";
                Debug.Log($"[BlackjackMinigame] Push - returned {currentBet}");
            }
            else if (playerWon)
            {
                // Win 2x bet
                int winnings = currentBet * 2;
                ActivatingPlayer.AddBalance(winnings);
                if (messageText != null)
                    messageText.text = $"YOU WIN! +${winnings}\n\n[SPACE] Play Again | [ESC] Exit";
                Debug.Log($"[BlackjackMinigame] Player won {winnings}");
            }
            else
            {
                // Lost bet (already deducted)
                if (playerBust)
                {
                    if (messageText != null)
                        messageText.text = $"BUST! Lost ${currentBet}\n\n[SPACE] Play Again | [ESC] Exit";
                }
                else
                {
                    if (messageText != null)
                        messageText.text = $"DEALER WINS! Lost ${currentBet}\n\n[SPACE] Play Again | [ESC] Exit";
                }
                Debug.Log($"[BlackjackMinigame] Player lost {currentBet}");
            }
        }
        
        // Disable buttons
        if (hitButton != null)
            hitButton.interactable = false;
        if (standButton != null)
            standButton.interactable = false;
    }
    
    private int DrawCard()
    {
        // Returns 1-13 (Ace through King)
        return Random.Range(1, 14);
    }
    
    private int CalculateHandValue(List<int> cards)
    {
        int total = 0;
        int aces = 0;
        
        foreach (int card in cards)
        {
            if (card == 1) // Ace
            {
                aces++;
                total += 11;
            }
            else if (card >= 10) // 10, J, Q, K
            {
                total += 10;
            }
            else
            {
                total += card;
            }
        }
        
        // Adjust for aces
        while (total > 21 && aces > 0)
        {
            total -= 10; // Convert ace from 11 to 1
            aces--;
        }
        
        return total;
    }
    
    private void UpdateUI()
    {
        // Update scores
        if (playerScoreText != null)
            playerScoreText.text = $"Player: {CalculateHandValue(playerCards)}";
            
        if (dealerScoreText != null)
        {
            if (isPlayerTurn)
            {
                // Hide dealer's second card value
                int visibleValue = dealerCards.Count > 0 ? GetCardValue(dealerCards[0]) : 0;
                dealerScoreText.text = $"Dealer: {visibleValue} + ?";
            }
            else
            {
                dealerScoreText.text = $"Dealer: {CalculateHandValue(dealerCards)}";
            }
        }
        
        // Update card lists text
        if (playerCardsText != null)
            playerCardsText.text = GetHandString(playerCards);
            
        if (dealerCardsText != null)
        {
            if (isPlayerTurn && dealerCards.Count > 1)
            {
                dealerCardsText.text = GetCardName(dealerCards[0]) + ", [Hidden]";
            }
            else
            {
                dealerCardsText.text = GetHandString(dealerCards);
            }
        }
        
        // Button states
        if (hitButton != null)
            hitButton.interactable = isPlayerTurn && gameInProgress;
        if (standButton != null)
            standButton.interactable = isPlayerTurn && gameInProgress;
    }
    
    private int GetCardValue(int card)
    {
        if (card == 1) return 11; // Ace (before adjustment)
        if (card >= 10) return 10;
        return card;
    }
    
    private string GetCardName(int card)
    {
        return card switch
        {
            1 => "A",
            11 => "J",
            12 => "Q",
            13 => "K",
            _ => card.ToString()
        };
    }
    
    private string GetHandString(List<int> cards)
    {
        string result = "";
        for (int i = 0; i < cards.Count; i++)
        {
            result += GetCardName(cards[i]);
            if (i < cards.Count - 1)
                result += ", ";
        }
        return result;
    }
    
    private void UpdateBetDisplay()
    {
        if (betAmountText != null)
            betAmountText.text = $"Bet: ${currentBet}";
    }
    
    public void OnIncreaseBet()
    {
        currentBet += 10;
        if (currentBet > maxBet)
            currentBet = maxBet;
        if (ActivatingPlayer != null && currentBet > ActivatingPlayer.Balance)
            currentBet = ActivatingPlayer.Balance;
        UpdateBetDisplay();
    }
    
    public void OnDecreaseBet()
    {
        currentBet -= 10;
        if (currentBet < minBet)
            currentBet = minBet;
        UpdateBetDisplay();
    }
    
    private void UpdateCardDisplay()
    {
        // Cleanup old cards
        CleanupCards();
        
        // Create player cards
        playerCardsRoot = new GameObject("PlayerCards");
        playerCardsRoot.transform.SetParent(transform, false);
        playerCardsRoot.transform.localPosition = new Vector3(0, verticalOffset, 0);
        
        for (int i = 0; i < playerCards.Count; i++)
        {
            var cardObj = new GameObject($"PlayerCard_{i}");
            cardObj.transform.SetParent(playerCardsRoot.transform, false);
            cardObj.transform.localPosition = new Vector3(i * cardSpacing - (playerCards.Count - 1) * cardSpacing / 2f, 0, 0);
            cardObj.transform.localScale = cardScale;
            
            var sr = cardObj.AddComponent<SpriteRenderer>();
            sr.sprite = GetCardSprite(playerCards[i]);
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;
            
            playerCardRenderers.Add(sr);
        }
        
        // Create dealer cards
        dealerCardsRoot = new GameObject("DealerCards");
        dealerCardsRoot.transform.SetParent(transform, false);
        dealerCardsRoot.transform.localPosition = new Vector3(0, verticalOffset + 2f, 0);
        
        for (int i = 0; i < dealerCards.Count; i++)
        {
            var cardObj = new GameObject($"DealerCard_{i}");
            cardObj.transform.SetParent(dealerCardsRoot.transform, false);
            cardObj.transform.localPosition = new Vector3(i * cardSpacing - (dealerCards.Count - 1) * cardSpacing / 2f, 0, 0);
            cardObj.transform.localScale = cardScale;
            
            var sr = cardObj.AddComponent<SpriteRenderer>();
            
            // Hide second card if player's turn
            if (i == 1 && isPlayerTurn)
            {
                sr.sprite = cardBackSprite;
            }
            else
            {
                sr.sprite = GetCardSprite(dealerCards[i]);
            }
            
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;
            
            dealerCardRenderers.Add(sr);
        }
    }
    
    private Sprite GetCardSprite(int cardValue)
    {
        if (cardSprites == null || cardSprites.Length == 0)
            return null;
            
        // Simple mapping - you can improve this with suits
        int index = (cardValue - 1) % cardSprites.Length;
        return cardSprites[index];
    }
    
    private void CleanupCards()
    {
        if (playerCardsRoot != null)
        {
            Destroy(playerCardsRoot);
            playerCardsRoot = null;
        }
        if (dealerCardsRoot != null)
        {
            Destroy(dealerCardsRoot);
            dealerCardsRoot = null;
        }
        
        playerCardRenderers.Clear();
        dealerCardRenderers.Clear();
    }
    
    void Update()
    {
        if (!IsRunning) return;
        
        var k = Keyboard.current;
        if (k == null) return;
        
        // Escape to close
        if (k.escapeKey.wasPressedThisFrame)
        {
            EndGame();
            return;
        }
        
        // Round ended - wait for play again or exit
        if (roundEnded)
        {
            if (k.spaceKey.wasPressedThisFrame)
            {
                // Play again - reset everything
                roundEnded = false;
                gameInProgress = false;
                isPlayerTurn = false;
                CleanupCards();
                playerCards.Clear();
                dealerCards.Clear();
                ShowBettingPanel();
            }
            return;
        }
        
        // Betting phase controls
        if (bettingPanel != null && bettingPanel.activeSelf)
        {
            if (k.lKey.wasPressedThisFrame)
            {
                OnIncreaseBet();
            }
            if (k.jKey.wasPressedThisFrame)
            {
                OnDecreaseBet();
            }
            if (k.spaceKey.wasPressedThisFrame)
            {
                OnStartGameClicked();
            }
        }
        
        // Game phase controls
        if (gamePanel != null && gamePanel.activeSelf && gameInProgress)
        {
            if (k.hKey.wasPressedThisFrame)
            {
                OnHitClicked();
            }
            if (k.sKey.wasPressedThisFrame)
            {
                OnStandClicked();
            }
        }
    }
    
    protected override int GetStartCost() => 0; // Bet is handled separately
    protected override bool AllowNegativeBalanceOnStart() => false;
}
