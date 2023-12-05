using System;
using System.Collections.Generic;
using System.Linq;

public class Player
{
    public string Name { get; set; }
    public int Balance { get; set; }
    public int Position { get; set; }
    public List<Tile> Tiles { get; set; } = new List<Tile>();


}

public class Tile
{
    public string Name { get; set; }
    public Player Owner { get; set; }

    public virtual void PerformAction(Player player, int diceSum)
    {

    }
}

public class StartTile : Tile
{

}

public class GoToJailTile : Tile
{

}

public class RealEstateTile : Tile
{

}



public class TrainStationTile : Tile
{


    public override void PerformAction(Player player, int diceSum)
    {

    }

    public class UtilityTile : Tile
    {

    }

    public class IncomeTaxTile : Tile
    {
        public int TaxAmount { get; set; }

        public IncomeTaxTile(string name, int taxAmount)
        {
            Name = name;
            TaxAmount = taxAmount;
        }

        public override void PerformAction(Player player, int diceSum)
        {
            Console.WriteLine($"{player.Name} must pay {TaxAmount}Ꝟ for Income Tax.");
            player.Balance -= TaxAmount;
        }
    }

    public class LuxuryTaxTile : Tile
    {
        public int TaxAmount { get; set; }

        public LuxuryTaxTile(string name, int taxAmount)
        {
            Name = name;
            TaxAmount = taxAmount;
        }

        public override void PerformAction(Player player, int diceSum)
        {
            Console.WriteLine($"{player.Name} must pay {TaxAmount}Ꝟ for Luxury Tax.");
            player.Balance -= TaxAmount;
        }
    }

    public class JailTile : Tile
    {

    }



    public class ChanceTile : Tile
    {
        // Define a static variable to store money placed on the board
        public static int boardMoney = 0;

        public ChanceTile(string name)
        {
            Name = name;
        }

        public override void PerformAction(Player player, int diceSum)
        {
            Console.WriteLine($"{player.Name} landed on Chance! Drawing a Chance card...");

            // Simulate drawing a random Chance card
            Random random = new Random();
            int cardNumber = random.Next(1, 9);

            // Perform the action based on the drawn card
            switch (cardNumber)
            {
                case 1:
                    CollectMoney(player, 150);
                    break;
                case 2:
                    CollectMoney(player, 50);
                    break;
                case 3:
                    PlaceMoneyOnBoard(player, 150);
                    break;
                case 4:
                    PlaceMoneyForHousesAndHotels(player);
                    break;
                case 5:
                    TravelToNearestTrainStation(player);
                    break;
                case 6:
                    GoBackTiles(player, 3);
                    break;
                case 7:
                    GetOutOfJail(player);
                    break;
                case 8:
                    PayEachPlayer(player, 50);
                    break;
            }
        }



        private void CollectMoney(Player player, int amount)
        {
            Console.WriteLine($"{player.Name} collected {amount}Ꝟ.");
            player.Balance += amount;
        }

        private void PlaceMoneyOnBoard(Player player, int amount)
        {

        }

        private void PlaceMoneyForHousesAndHotels(Player player)
        {

        }

        private void TravelToNearestTrainStation(Player player)
        {
        }

        private void GoBackTiles(Player player, int numberOfTiles)
        {

        }

        private void GetOutOfJail(Player player)
        {

        }

        private void PayEachPlayer(Player player, int amount)
        {

        }


    }

    public class CommunityChestTile : Tile
    {
        public CommunityChestTile(string name)
        {
            Name = name;
        }

        public override void PerformAction(Player player, int diceSum)
        {
            Console.WriteLine($"{player.Name} landed on Community Chest! Drawing a Community Chest card...");

            // Simulate drawing a random Community Chest card
            Random random = new Random();
            int cardNumber = random.Next(1, 9);

            // Perform the action based on the drawn card
            DrawCard(player, cardNumber);
        }

        private void DrawCard(Player player, int cardNumber)
        {
            switch (cardNumber)
            {
                case 1:
                    CollectMoney(player, 200);
                    break;
                case 2:
                    CollectMoney(player, 100);
                    break;
                case 3:
                    PlaceMoneyOnBoard(player, 100);
                    break;
                case 4:
                    PlaceMoneyForHousesAndHotels(player);
                    break;
                case 5:
                    TravelToNearestUtility(player);
                    break;
                case 6:
                    GoBackTiles(player, 1);
                    break;
                case 7:
                    TravelToJail(player);
                    break;
                case 8:
                    PayEachPlayer(player, 100);
                    break;
            }
        }

        private void CollectMoney(Player player, int amount)
        {
            Console.WriteLine($"{player.Name} collected {amount}Ꝟ.");
            player.Balance += amount;
        }


        private void PlaceMoneyOnBoard(Player player, int amount)
        {

        }

        private void AddMoneyToBoard(int amount)
        {

        }

        private void PlaceMoneyForHousesAndHotels(Player player)
        {

        }

        private void TravelToNearestUtility(Player player)
        {

        }

        private void GoBackTiles(Player player, int numberOfTiles)
        {

        }

        private void TravelToJail(Player player)
        {

        }

        private void PayEachPlayer(Player player, int amount)
        {

        }


    }

    public class FreeParkingTile : Tile
    {

    }

    public class Program
    {



        public static void DisplayBoardState()
        {

        }

        public static void DisplayPropertiesAndBalances()
        {

        }

        public static List<Tile> tiles = new List<Tile>
        {
            // Add tiles
        };

        static void Main()
        {



        }
    }
}