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
    /// <summary>
    /// Minimum bet amount allowed for a round.
    /// </summary>
    [SerializeField] private int minBet = 1;
    
    /// <summary>
    /// Default bet amount when starting the minigame.
    /// </summary>
    [SerializeField] private int defaultBet = 10;
    
    /// <summary>
    /// Maximum bet amount allowed for a round.
    /// </summary>
    [SerializeField] private int maxBet = 1000;
    
    [Header("UI References")]
    /// <summary>
    /// Main UI panel container for the blackjack interface.
    /// </summary>
    [SerializeField] private GameObject uiPanel;
    
    /// <summary>
    /// Text component displaying the player's cards.
    /// </summary>
    [SerializeField] private TMP_Text playerCardsText;
    
    /// <summary>
    /// Text component displaying the dealer's cards.
    /// </summary>
    [SerializeField] private TMP_Text dealerCardsText;
    
    /// <summary>
    /// Text component displaying the player's current score.
    /// </summary>
    [SerializeField] private TMP_Text playerScoreText;
    
    /// <summary>
    /// Text component displaying the dealer's current score.
    /// </summary>
    [SerializeField] private TMP_Text dealerScoreText;
    
    /// <summary>
    /// Text component for displaying game messages (win, lose, bust, etc.).
    /// </summary>
    [SerializeField] private TMP_Text messageText;
    
    /// <summary>
    /// Text component displaying the current bet amount.
    /// </summary>
    [SerializeField] private TMP_Text betAmountText;
    
    /// <summary>
    /// Button to request another card (hit).
    /// </summary>
    [SerializeField] private Button hitButton;
    
    /// <summary>
    /// Button to end the player's turn (stand).
    /// </summary>
    [SerializeField] private Button standButton;
    
    /// <summary>
    /// Button to increase the bet amount.
    /// </summary>
    [SerializeField] private Button betIncreaseButton;
    
    /// <summary>
    /// Button to decrease the bet amount.
    /// </summary>
    [SerializeField] private Button betDecreaseButton;
    
    /// <summary>
    /// Button to start a new blackjack round with the current bet.
    /// </summary>
    [SerializeField] private Button startGameButton;
    
    /// <summary>
    /// Panel displayed during the betting phase.
    /// </summary>
    [SerializeField] private GameObject bettingPanel;
    
    /// <summary>
    /// Panel displayed during active gameplay.
    /// </summary>
    [SerializeField] private GameObject gamePanel;
    
    [Header("Hint Texts")]
    /// <summary>
    /// Text hint for the increase bet keybinding.
    /// </summary>
    [SerializeField] private TMP_Text hintIncreaseText;
    
    /// <summary>
    /// Text hint for the decrease bet keybinding.
    /// </summary>
    [SerializeField] private TMP_Text hintDecreaseText;
    
    /// <summary>
    /// Text hint for the start game keybinding.
    /// </summary>
    [SerializeField] private TMP_Text hintStartText;
    
    /// <summary>
    /// Text hint for the hit keybinding.
    /// </summary>
    [SerializeField] private TMP_Text hintHitText;
    
    /// <summary>
    /// Text hint for the stand keybinding.
    /// </summary>
    [SerializeField] private TMP_Text hintStandText;
    
    [Header("Card Display Settings")]
    /// <summary>
    /// Horizontal spacing between cards in world space.
    /// </summary>
    [SerializeField] private float cardSpacing = 1.5f;
    
    /// <summary>
    /// Vertical offset for card positioning.
    /// </summary>
    [SerializeField] private float verticalOffset = 2f;
    
    /// <summary>
    /// Scale of the card sprites in world space.
    /// </summary>
    [SerializeField] private Vector3 cardScale = new Vector3(3f, 3f, 3f);
    
    /// <summary>
    /// Sorting layer name for card sprite renderers.
    /// </summary>
    [SerializeField] private string sortingLayerName = "Default";
    
    /// <summary>
    /// Sorting order for card sprite renderers.
    /// </summary>
    [SerializeField] private int sortingOrder = 10;
    
    [Header("Card Sprites")]
    /// <summary>
    /// Array of card sprites (0-12: Ace through King for different suits).
    /// </summary>
    [SerializeField] private Sprite[] cardSprites;
    
    /// <summary>
    /// Sprite used for face-down cards (hidden dealer card).
    /// </summary>
    [SerializeField] private Sprite cardBackSprite;
    
    /// <summary>
    /// List of card values in the player's hand.
    /// </summary>
    private List<int> playerCards = new List<int>();
    
    /// <summary>
    /// List of card values in the dealer's hand.
    /// </summary>
    private List<int> dealerCards = new List<int>();
    
    /// <summary>
    /// The amount of money bet on the current round.
    /// </summary>
    private int currentBet;
    
    /// <summary>
    /// Whether it is currently the player's turn.
    /// </summary>
    private bool isPlayerTurn;
    
    /// <summary>
    /// Whether a game round is currently in progress.
    /// </summary>
    private bool gameInProgress;
    
    /// <summary>
    /// Whether the current round has ended and is awaiting player input.
    /// </summary>
    private bool roundEnded;
    
    /// <summary>
    /// Root GameObject container for player card sprites.
    /// </summary>
    private GameObject playerCardsRoot;
    
    /// <summary>
    /// Root GameObject container for dealer card sprites.
    /// </summary>
    private GameObject dealerCardsRoot;
    
    /// <summary>
    /// List of sprite renderers for player cards.
    /// </summary>
    private List<SpriteRenderer> playerCardRenderers = new List<SpriteRenderer>();
    
    /// <summary>
    /// List of sprite renderers for dealer cards.
    /// </summary>
    private List<SpriteRenderer> dealerCardRenderers = new List<SpriteRenderer>();
    
    /// <summary>
    /// Unity lifecycle method called when the script instance is being loaded.
    /// Sets up button listeners and initializes default bet.
    /// </summary>
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
    
    /// <summary>
    /// Unity lifecycle method called before the first frame update.
    /// Initializes UI state.
    /// </summary>
    void Start()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);
            
        UpdateBetDisplay();
    }
    
    /// <summary>
    /// Unity lifecycle method called when the object becomes enabled and active.
    /// Updates keybinding hints and subscribes to keybinding change events.
    /// </summary>
    void OnEnable()
    {
        UpdateHints();
        
        // Subscribe to key changes
        if (KeyBindings.Instance != null)
            KeyBindings.Instance.OnKeysChanged += UpdateHints;
    }
    
    /// <summary>
    /// Unity lifecycle method called when the behaviour becomes disabled.
    /// Unsubscribes from keybinding change events.
    /// </summary>
    void OnDisable()
    {
        if (KeyBindings.Instance != null)
            KeyBindings.Instance.OnKeysChanged -= UpdateHints;
    }
    
    /// <summary>
    /// Unity lifecycle method called when the MonoBehaviour will be destroyed.
    /// Ensures cleanup of event subscriptions.
    /// </summary>
    void OnDestroy()
    {
        if (KeyBindings.Instance != null)
            KeyBindings.Instance.OnKeysChanged -= UpdateHints;
    }
    
    /// <summary>
    /// Updates all keybinding hint texts with current key mappings.
    /// Retries if KeyBindings instance is not yet available.
    /// </summary>
    private void UpdateHints()
    {
        if (KeyBindings.Instance == null)
        {
            // Retry after a frame if KeyBindings not loaded yet
            StartCoroutine(RetryUpdateHints());
            return;
        }
        
        if (hintIncreaseText != null)
            hintIncreaseText.text = $"[{KeyBindings.Instance.BlackjackIncreaseBet}] +10";
        if (hintDecreaseText != null)
            hintDecreaseText.text = $"[{KeyBindings.Instance.BlackjackDecreaseBet}] -10";
        if (hintStartText != null)
            hintStartText.text = $"[{KeyBindings.Instance.BlackjackStart}] START";
        if (hintHitText != null)
            hintHitText.text = $"[{KeyBindings.Instance.BlackjackHit}] HIT";
        if (hintStandText != null)
            hintStandText.text = $"[{KeyBindings.Instance.BlackjackStand}] STAND";
    }
    
    /// <summary>
    /// Coroutine that retries updating hints after a short delay.
    /// Used when KeyBindings instance is not immediately available.
    /// </summary>
    /// <returns>Enumerator for coroutine execution.</returns>
    private System.Collections.IEnumerator RetryUpdateHints()
    {
        yield return new WaitForSeconds(0.1f);
        UpdateHints();
    }
    
    /// <summary>
    /// Called when the minigame starts. Shows the UI and betting panel.
    /// Overrides MinigameBase method.
    /// </summary>
    protected override void OnStartGame()
    {
        if (uiPanel != null)
            uiPanel.SetActive(true);
            
        // Show betting panel first
        ShowBettingPanel();
        Debug.Log("[BlackjackMinigame] Started - choose your bet.");
    }
    
    /// <summary>
    /// Called when the minigame ends. Hides the UI and cleans up card objects.
    /// Overrides MinigameBase method.
    /// </summary>
    protected override void OnEndGame()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);
            
        CleanupCards();
        Debug.Log("[BlackjackMinigame] Ended.");
    }
    
    /// <summary>
    /// Displays the betting panel and prepares for a new betting phase.
    /// Clears previous round data and clamps bet to player's available balance.
    /// </summary>
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
    
    /// <summary>
    /// Displays the game panel and hides the betting panel.
    /// </summary>
    private void ShowGamePanel()
    {
        if (bettingPanel != null)
            bettingPanel.SetActive(false);
        if (gamePanel != null)
            gamePanel.SetActive(true);
    }
    
    /// <summary>
    /// Handler for the start game button click.
    /// Validates player balance and deducts the bet before starting a round.
    /// </summary>
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
    
    /// <summary>
    /// Initializes and starts a new blackjack round.
    /// Deals initial cards (2 to player, 2 to dealer) and checks for immediate blackjack.
    /// </summary>
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
    
    /// <summary>
    /// Handler for the hit button click.
    /// Draws an additional card for the player and checks for bust or 21.
    /// </summary>
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
    
    /// <summary>
    /// Handler for the stand button click.
    /// Ends the player's turn and initiates the dealer's turn.
    /// </summary>
    public void OnStandClicked()
    {
        if (!isPlayerTurn || !gameInProgress) return;
        
        isPlayerTurn = false;
        
        // Dealer's turn
        PlayDealerTurn();
    }
    
    /// <summary>
    /// Executes the dealer's turn following standard blackjack rules.
    /// Dealer draws cards until reaching 17 or higher, then determines the winner.
    /// </summary>
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
    
    /// <summary>
    /// Ends the current round and handles payouts based on the result.
    /// </summary>
    /// <param name="playerWon">Whether the player won the round.</param>
    /// <param name="playerBust">Whether the player busted.</param>
    /// <param name="push">Whether the round was a push (tie).</param>
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
    
    /// <summary>
    /// Draws a random card from the deck.
    /// </summary>
    /// <returns>Card value between 1 (Ace) and 13 (King).</returns>
    private int DrawCard()
    {
        // Returns 1-13 (Ace through King)
        return Random.Range(1, 14);
    }
    
    /// <summary>
    /// Calculates the total value of a hand following blackjack rules.
    /// Aces count as 11 or 1, face cards count as 10.
    /// </summary>
    /// <param name="cards">List of card values in the hand.</param>
    /// <returns>Total hand value with aces optimally valued.</returns>
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
    
    /// <summary>
    /// Updates all UI elements to reflect the current game state.
    /// Shows or hides dealer's cards based on whose turn it is.
    /// </summary>
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
    
    /// <summary>
    /// Gets the point value of a single card (before ace adjustment).
    /// </summary>
    /// <param name="card">Card value (1-13).</param>
    /// <returns>Point value of the card.</returns>
    private int GetCardValue(int card)
    {
        if (card == 1) return 11; // Ace (before adjustment)
        if (card >= 10) return 10;
        return card;
    }
    
    /// <summary>
    /// Converts a card value to its display name.
    /// </summary>
    /// <param name="card">Card value (1-13).</param>
    /// <returns>Display name (A, 2-10, J, Q, K).</returns>
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
    
    /// <summary>
    /// Creates a comma-separated string representation of a hand.
    /// </summary>
    /// <param name="cards">List of card values.</param>
    /// <returns>String representation of the hand (e.g., "A, 10, 5").</returns>
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
    
    /// <summary>
    /// Updates the bet amount display text.
    /// </summary>
    private void UpdateBetDisplay()
    {
        if (betAmountText != null)
            betAmountText.text = $"Bet: ${currentBet}";
    }
    
    /// <summary>
    /// Handler for the increase bet button.
    /// Increases bet by 10, capped at maximum bet and player balance.
    /// </summary>
    public void OnIncreaseBet()
    {
        currentBet += 10;
        if (currentBet > maxBet)
            currentBet = maxBet;
        if (ActivatingPlayer != null && currentBet > ActivatingPlayer.Balance)
            currentBet = ActivatingPlayer.Balance;
        UpdateBetDisplay();
    }
    
    /// <summary>
    /// Handler for the decrease bet button.
    /// Decreases bet by 10, floored at minimum bet.
    /// </summary>
    public void OnDecreaseBet()
    {
        currentBet -= 10;
        if (currentBet < minBet)
            currentBet = minBet;
        UpdateBetDisplay();
    }
    
    /// <summary>
    /// Creates and displays card sprites for both player and dealer hands.
    /// Hides the dealer's second card during the player's turn.
    /// </summary>
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
    
    /// <summary>
    /// Gets the sprite for a specific card value.
    /// </summary>
    /// <param name="cardValue">Card value (1-13).</param>
    /// <returns>Sprite for the card, or null if not available.</returns>
    private Sprite GetCardSprite(int cardValue)
    {
        if (cardSprites == null || cardSprites.Length == 0)
            return null;
            
        // Simple mapping - you can improve this with suits
        int index = (cardValue - 1) % cardSprites.Length;
        return cardSprites[index];
    }
    
    /// <summary>
    /// Destroys all card GameObjects and clears sprite renderer lists.
    /// </summary>
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
    
    /// <summary>
    /// Unity lifecycle method called every frame.
    /// Handles keyboard input for minigame controls.
    /// </summary>
    void Update()
    {
        if (!IsRunning) return;
        
        var k = Keyboard.current;
        if (k == null) return;
        
        var bindings = KeyBindings.Instance;
        
        // Escape to close
        if (k.escapeKey.wasPressedThisFrame)
        {
            EndGame();
            return;
        }
        
        // Round ended - wait for play again or exit
        if (roundEnded)
        {
            if (bindings != null)
            {
                var startKey = k[bindings.BlackjackStart];
                if (startKey != null && startKey.wasPressedThisFrame)
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
            }
            return;
        }
        
        // Betting phase controls
        if (bettingPanel != null && bettingPanel.activeSelf)
        {
            if (bindings != null)
            {
                var increaseKey = k[bindings.BlackjackIncreaseBet];
                var decreaseKey = k[bindings.BlackjackDecreaseBet];
                var startKey = k[bindings.BlackjackStart];
                
                if (increaseKey != null && increaseKey.wasPressedThisFrame)
                {
                    OnIncreaseBet();
                }
                if (decreaseKey != null && decreaseKey.wasPressedThisFrame)
                {
                    OnDecreaseBet();
                }
                if (startKey != null && startKey.wasPressedThisFrame)
                {
                    OnStartGameClicked();
                }
            }
        }
        
        // Game phase controls
        if (gamePanel != null && gamePanel.activeSelf && gameInProgress)
        {
            if (bindings != null)
            {
                var hitKey = k[bindings.BlackjackHit];
                var standKey = k[bindings.BlackjackStand];
                
                if (hitKey != null && hitKey.wasPressedThisFrame)
                {
                    OnHitClicked();
                }
                if (standKey != null && standKey.wasPressedThisFrame)
                {
                    OnStandClicked();
                }
            }
        }
    }
    
    /// <summary>
    /// Gets the cost to start the minigame. Always returns 0 as bet is handled separately.
    /// Overrides MinigameBase method.
    /// </summary>
    /// <returns>Cost to start (always 0).</returns>
    protected override int GetStartCost() => 0;
    
    /// <summary>
    /// Determines if negative balance is allowed when starting.
    /// Overrides MinigameBase method.
    /// </summary>
    /// <returns>Always returns false.</returns>
    protected override bool AllowNegativeBalanceOnStart() => false;
}
