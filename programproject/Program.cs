// CET137 Assessment 1 - Scenario 1: Number Game
// Author: Pratik Panta
// Description: Console-based game where player and computer get two cards (1-8).
// Goal: Get the lowest score (difference between cards). Played over 3 rounds.
// Player can swap one card per round. Computer swaps until score < 3.
// Logs actions to a file and finds winner by score.
//student ID: bi95sw
using System;
using System.Collections.Generic;
using System.IO;

namespace NumberGame
{
    // Entry point of the application
    class Program
    {
        static void Main(string[] args)
        {
            // Create and start the game
            Game game = new Game();
            game.Start();
        }
    }

    // Represents a card with a value from 1 to 8
    class Card
    {
        public int Value { get; private set; }

        public Card(int value)
        {
            Value = value;
        }

        // Returns the card's value as a string
        public override string ToString() => Value.ToString();
    }

    // Represents a player (either human or computer)
    class Player
    {
        public string Name { get; private set; }          // Player's name
        public List<Card> Hand { get; private set; }      // Player's hand (2 cards)
        public int Score { get; private set; }            // Score = Absolute difference between card values

        public Player(string name)
        {
            Name = name;
            Hand = new List<Card>();
        }

        // Deals two random cards to the player
        public void DealCards(Random rng)
        {
            Hand.Clear();  // Ensure hand is reset
            Hand.Add(new Card(rng.Next(1, 9))); // Card 1
            Hand.Add(new Card(rng.Next(1, 9))); // Card 2
            CalculateScore();
        }

        // Swaps one card at specified index (0 or 1) with a new random card
        public void SwapCard(int index, Random rng)
        {
            if (index >= 0 && index < Hand.Count)
            {
                Hand[index] = new Card(rng.Next(1, 9));
                CalculateScore();
            }
            else
            {
                Console.WriteLine("Invalid card index to swap.");
            }
        }

        // Calculates the score based on absolute difference between the two card values
        public void CalculateScore()
        {
            if (Hand.Count < 2)
            {
                Score = 0;
                return;
            }

            int a = Hand[0].Value;
            int b = Hand[1].Value;
            Score = Math.Abs(a - b);
        }

        // Returns the total value of the two cards (used for tie-breaking)
        public int TotalValue() => Hand[0].Value + Hand[1].Value;

        // Returns a formatted string showing the two cards
        public string DisplayHand() => $"[{Hand[0]}] and [{Hand[1]}]";
    }

    // Manages the game logic, rounds, player interactions, and outcome
    class Game
    {
        private Player? human;           // Human player
        private Player? computer;        // Computer player
        private readonly Random rng;     // Random number generator for dealing/swapping cards
        private int round;               // Current round number (up to 3)
        private readonly string logFilePath = "GameLog.txt"; // Log file path

        public Game()
        {
            rng = new Random();
            round = 1;
        }

        // Starts the game and handles all rounds and interactions
        public void Start()
        {
            Console.Write("Enter your name: ");
            string? name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                name = "Player";
            }
            human = new Player(name);
            computer = new Player("Computer");

            // Start a fresh log file for each game session
            try
            {
                File.WriteAllText(logFilePath, "--- Number Game Log ---\n");
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error initializing log file: " + ex.Message);
            }

            // Loop through 3 rounds
            while (round <= 3)
            {
                Console.Clear();
                Console.WriteLine($"--- Round {round} ---");

                human.DealCards(rng);
                computer.DealCards(rng);

                Log($"Round {round}: {human.Name} was dealt {human.DisplayHand()}.");

                Console.WriteLine($"Your cards: {human.DisplayHand()} (Score: {human.Score})");

                int choice;
                while (true)
                {
                    Console.Write("Do you want to swap a card? (1 = First, 2 = Second, 0 = No): ");
                    string? input = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(input))
                    {
                        input = "0";
                    }

                    if (int.TryParse(input, out choice) && choice >= 0 && choice <= 2)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Invalid input. Please enter 0 (No), 1 (First), or 2 (Second).");
                    }
                }

                if (choice > 0)
                {
                    human.SwapCard(choice - 1, rng);
                    Console.WriteLine($"After swap: {human.DisplayHand()} (Score: {human.Score})");
                    Log($"{human.Name} swapped card {choice} and got {human.DisplayHand()}.");
                }
                else
                {
                    Log($"{human.Name} kept original cards.");
                }

                ComputerLogic();
                round++;

                Console.WriteLine("Press Enter to continue...");
                Console.ReadLine();
            }

            Console.Clear();
            Console.WriteLine("--- Game Over ---");

            Console.WriteLine($"{human.Name}'s hand: {human.DisplayHand()} - Score: {human.Score}");
            Console.WriteLine($"Computer's hand: {computer.DisplayHand()} - Score: {computer.Score}");
            Log($"Final hands -> {human.Name}: {human.DisplayHand()} ({human.Score}), Computer: {computer.DisplayHand()} ({computer.Score})");

            EvaluateWinner();
            SaveFinalScores(); // ✅ Save final scores to a separate text file
        }

        // Computer swaps cards if its score is 3 or more, max 2 attempts
        private void ComputerLogic()
        {
            if (computer == null) return;
            int attempts = 0;
            while (computer.Score >= 3 && attempts < 2)
            {
                int swapIndex = rng.Next(0, 2);
                computer.SwapCard(swapIndex, rng);
                attempts++;
                Log($"Computer swapped card {swapIndex + 1}. Now has {computer.DisplayHand()} (Score: {computer.Score})");
            }

            if (attempts == 0)
                Log("Computer kept original cards.");
        }

        // Compares final scores and prints/logs the result
        private void EvaluateWinner()
        {
            if (human == null || computer == null) return;
            string result = "";

            if (human.Score < computer.Score)
                result = $"{human.Name} wins!";
            else if (human.Score > computer.Score)
                result = "Computer wins!";
            else
            {
                if (human.TotalValue() < computer.TotalValue())
                    result = $"Draw on score, but {human.Name} wins by lower total value!";
                else if (human.TotalValue() > computer.TotalValue())
                    result = "Draw on score, but Computer wins by lower total value!";
                else
                    result = "It's a perfect tie!";
            }

            Console.WriteLine(result);
            Log(result);
        }

        // ✅ Saves the final scores to a separate file
        private void SaveFinalScores()
        {
            if (human == null || computer == null) return;

            string finalScoreText = $"--- Final Score ---\n{human.Name}: {human.Score}\nComputer: {computer.Score}\n";
            try
            {
                File.AppendAllText("FinalScores.txt", finalScoreText + "\n");
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error saving final scores: " + ex.Message);
            }
        }

        // Appends messages to the game log file
        private void Log(string message)
        {
            try
            {
                File.AppendAllText(logFilePath, message + "\n");
            }
            catch (IOException ex)
            {
                Console.WriteLine("Error writing to log file: " + ex.Message);
            }
        }
    }
} // End of namespace
