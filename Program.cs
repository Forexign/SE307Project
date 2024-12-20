using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;

public class Player
{
    private string name;
    private int balance;
    private int position;
    private int money;
    private List<Tile> properties = new List<Tile>();
    private bool inJail;
    private int turnsInJail;
    private bool jailfree = false;

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    public int Balance
    {
        get { return balance; }
        set { balance = value; }
    }

    public int Position
    {
        get { return position; }
        set { position = value; }
    }

    public int Money
    {
        get { return money; }
        set { money = value; }
    }

    public List<Tile> Properties
    {
        get { return properties; }
        set { properties = value; }
    }

    public bool InJail
    {
        get { return inJail; }
        set { inJail = value; }
    }

    public int TurnsInJail
    {
        get { return turnsInJail; }
        set { turnsInJail = value; }
    }

    // Getter method for jailfree
    public bool GetJailFree()
    {
        return jailfree;
    }

    public void SetJailFree(bool value)
    {
        jailfree = value;
    }

    public void GoToJail()
    {
        Console.WriteLine($"{Name} goes to jail!");
        Position = 10;
        InJail = true;
        TurnsInJail = 2;
    }

    public bool CanAfford(int amount)
    {
        return Balance >= amount;
    }

    public void Pay(int amount)
    {
        if (CanAfford(amount))
        {
            Balance -= amount;
            Console.WriteLine($"{Name} paid {amount}Ꝟ.");
        }
        else
        {
            Console.WriteLine($"{Name} cannot afford to pay {amount}Ꝟ.");
        }
    }
}



public abstract class Tile
{
    public string Name { get; set; }
    public Player Owner { get; set; }
    public virtual void PerformAction(Player player, int diceSum)
    {

    }
}

public class StartTile : Tile
{
    public StartTile(string name)
    {
        Name = name;
    }

    public override void PerformAction(Player player, int diceSum)
    {
        // controlled in the main 
    }
}

public class GoToJailTile : Tile
{
    public GoToJailTile(string name)
    {
        Name = name;
    }

    public override void PerformAction(Player player, int diceSum)
    {

        if (player.GetJailFree() == false)
        {
            Console.WriteLine($"{player.Name} landed on {Name}! Goes to jail immediately.");

            // Send the player to the jail
            player.GoToJail();
        }
        else
        {
            Console.WriteLine($"{player.Name} has jail free card");
            player.SetJailFree(false);
        }
    }
}


public class RealEstateTile : Tile
{
    public int Tier { get; private set; }
    public int BuyLandCost { get; private set; }
    public int BuildHouseCost { get; private set; }
    public int[] RentTable { get; private set; }

    private int houseCount;
    private int hotelCost;

    public RealEstateTile(string name, int tier, int buyLandCost, int buildHouseCost, int[] rentTable)
    {
        Name = name;
        Tier = tier;
        SetCosts(tier, buyLandCost, buildHouseCost);
        RentTable = rentTable;
        houseCount = 0;
    }

    private void SetCosts(int tier, int buyLandCost, int buildHouseCost)
    {
        hotelCost = buildHouseCost * 5;

        switch (tier)
        {
            case 1:
                BuyLandCost = 60;
                BuildHouseCost = 50;
                break;
            case 2:
                BuyLandCost = 120;
                BuildHouseCost = 50;
                break;
            case 3:
                BuyLandCost = 160;
                BuildHouseCost = 100;
                break;
            case 4:
                BuyLandCost = 200;
                BuildHouseCost = 100;
                break;
            case 5:
                BuyLandCost = 240;
                BuildHouseCost = 150;
                break;
            case 6:
                BuyLandCost = 280;
                BuildHouseCost = 150;
                break;
            case 7:
                BuyLandCost = 320;
                BuildHouseCost = 200;
                break;
            case 8:
                BuyLandCost = 400;
                BuildHouseCost = 200;
                break;
            default:
                throw new ArgumentException("Invalid tier");
        }
    }

    public int GetHouseCount()
    {
        return houseCount;
    }

    public override void PerformAction(Player player, int diceSum)
    {
        if (Owner == null)
        {
            // Tile doesn't have an owner, allow the player to buy it
            Console.WriteLine($"{player.Name} landed on {Name}. It's available for purchase at a cost of {BuyLandCost}Ꝟ.");

            Console.Write($"Do you want to buy {Name}? (y/n): ");
            string response = Console.ReadLine();

            if (response.ToLower() == "y")
            {
                BuyProperty(player, BuyLandCost);
            }
            else
            {
                Console.WriteLine($"{player.Name} decided not to buy {Name}.");
            }
        }
        else if (Owner == player)
        {
            // The player already owns the tile, allow them to build houses/hotel
            Console.WriteLine($"{player.Name} owns {Name}. What do you want to do?");
            Console.WriteLine("1. Build House");
            Console.WriteLine("2. Build Hotel");
            Console.WriteLine("3. Do nothing");

            int choice;
            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 3)
            {
                Console.Write("Invalid input. Choose 1, 2, or 3: ");
            }

            switch (choice)
            {
                case 1:
                    BuildHouse(player);
                    break;
                case 2:
                    if (houseCount < 4)
                    {
                        Console.WriteLine($"{player.Name} cannot upgrade to a hotel yet. Build more houses first.");
                    }
                    else
                    {
                        UpgradeToHotel(player);
                        houseCount = 5;
                    }
                    break;

            }
        }
        else
        {
            int rent = (houseCount < RentTable.Length) ? RentTable[houseCount] : RentTable[RentTable.Length - 1];
            Console.WriteLine($"{player.Name} landed on {Name}, owned by {Owner.Name}. Paying rent: {rent}Ꝟ.");

            //if cant pay rent
            if (player.Balance < rent)
            {
                Program.kill(player);
            }
            else
            {
                player.Pay(rent);
                Owner.Balance += rent;
            }

        }
    }

    public static void ReturnProperties(Player player)
    {
        Console.WriteLine($"{player.Name} doesn't have enough money and must return all properties.");

        foreach (var property in player.Properties.ToList())
        {
            property.Owner = null;
            player.Properties.Remove(property);
        }
    }

    private void UpgradeToHotel(Player player)
    {
        if (player.Balance >= hotelCost && houseCount == 4)
        {
            player.Pay(hotelCost);
            houseCount = 4; // Upgrade to a hotel
            Console.WriteLine($"{player.Name} built a hotel on {Name}.");
        }
        else
        {
            Console.WriteLine($"{player.Name} cannot afford to build a hotel or has not built 5 houses yet.");

        }
    }

    private void BuildHouse(Player player)
    {
        int maxHouses = 4; // Maximum number of houses allowed before uprgading to a hotel

        if (houseCount < maxHouses && player.CanAfford(BuildHouseCost))
        {
            Console.Write($"{player.Name}, how many houses would you like to build on {Name}? Enter a number (1-{maxHouses - houseCount}): ");

            if (int.TryParse(Console.ReadLine(), out int housesToBuild) && housesToBuild >= 1 && housesToBuild <= maxHouses - houseCount)
            {
                int totalCost = BuildHouseCost * housesToBuild;

                if (player.CanAfford(totalCost))
                {
                    player.Pay(totalCost);
                    houseCount += housesToBuild;

                    Console.WriteLine($"{player.Name} built {housesToBuild} houses on {Name}.");
                }
                else
                {
                    Console.WriteLine($"{player.Name} doesn't have enough money to build houses.");
                }
            }
            else
            {
                Console.WriteLine("Invalid input. Please enter a valid number of houses to build.");
            }
        }
        else if (houseCount == maxHouses)
        {
            Console.WriteLine($"{player.Name} has reached the maximum number of houses. Consider upgrading to a hotel.");
        }
        else
        {
            Console.WriteLine($"{player.Name} cannot afford to build houses or has reached the maximum limit.");
        }
    }
    private void BuyProperty(Player player, int cost)
    {
        if (player.Balance >= cost)
        {
            Console.WriteLine($"{player.Name} bought {Name} for {cost}Ꝟ.");
            player.Balance -= cost;
            Owner = player;
            player.Properties.Add(this);
        }
        else
        {
            Console.WriteLine($"{player.Name} doesn't have enough money to buy {Name}.");
        }
    }
}

public class TrainStationTile : Tile
{
    public int Cost { get; set; }
    public int Rent1Station { get; set; }
    public int Rent2Stations { get; set; }
    public int Rent3Stations { get; set; }
    public int Rent4Stations { get; set; }

    public TrainStationTile(string name, int cost, int rent1Station, int rent2Stations, int rent3Stations, int rent4Stations)
    {
        Name = name;
        Cost = cost;
        Rent1Station = rent1Station;
        Rent2Stations = rent2Stations;
        Rent3Stations = rent3Stations;
        Rent4Stations = rent4Stations;
    }

    public override void PerformAction(Player player, int diceSum)
    {
        if (Owner == null)
        {
            Console.WriteLine($"{player.Name} can buy the train station for {Cost}Ꝟ. Do you want to buy it? (Y/N)");

            if (Console.ReadLine().ToUpper() == "Y")
            {
                BuyProperty(player);
            }
            else
            {
                Console.WriteLine($"{player.Name} decided not to buy {Name}.");
            }
        }
        else if (Owner != player)
        {
            int ownerStations = GetOwnerStationsCount();

            int rentAmount = 0;

            switch (ownerStations)
            {
                case 1:
                    rentAmount = Rent1Station;
                    break;
                case 2:
                    rentAmount = Rent2Stations;
                    break;
                case 3:
                    rentAmount = Rent3Stations;
                    break;
                case 4:
                    rentAmount = Rent4Stations;
                    break;
            }

            Console.WriteLine($"{player.Name} has to pay {Owner.Name} {rentAmount}Ꝟ as rent for the train station.");
            player.Balance -= rentAmount;
            Owner.Balance += rentAmount;
        }
    }

    private int GetOwnerStationsCount()
    {
        return Owner.Properties.Count(tile => tile is TrainStationTile);
    }

    private void BuyProperty(Player player)
    {
        if (player.Balance >= Cost)
        {
            Console.WriteLine($"{player.Name} bought the train station for {Cost}Ꝟ.");
            player.Balance -= Cost;
            Owner = player;
            player.Properties.Add(this);
        }
        else
        {
            Console.WriteLine($"{player.Name} doesn't have enough money to buy the train station.");
        }
    }
}

public class UtilityTile : Tile
{
    public int Cost { get; set; }

    public UtilityTile(string name, int cost)
    {
        Name = name;
        Cost = cost;
    }

    public override void PerformAction(Player player, int diceSum)
    {
        if (Owner == null)
        {
            Console.WriteLine($"{player.Name} can buy {Name} for {Cost}Ꝟ. Do you want to buy it? (Y/N)");

            if (Console.ReadLine().ToUpper() == "Y")
            {
                BuyProperty(player);
            }
            else
            {
                Console.WriteLine($"{player.Name} decided not to buy {Name}.");
            }
        }
        else if (Owner != player)
        {
            int rentAmount = CalculateRent(diceSum);

            Console.WriteLine($"{player.Name} has to pay {Owner.Name} {rentAmount}Ꝟ as rent for {Name}.");
            player.Balance -= rentAmount;
            Owner.Balance += rentAmount;
        }
    }

    private void BuyProperty(Player player)
    {
        if (player.Balance >= Cost)
        {
            Console.WriteLine($"{player.Name} bought {Name} for {Cost}Ꝟ.");
            player.Balance -= Cost;
            Owner = player;
            player.Properties.Add(this);
        }
        else
        {
            Console.WriteLine($"{player.Name} doesn't have enough money to buy {Name}.");
        }
    }

    private int CalculateRent(int diceSum)
    {
        int ownerUtilities = Owner.Properties.Count(tile => tile is UtilityTile);

        if (ownerUtilities == 1 || ownerUtilities == 2)
        {
            return diceSum * 5;
        }
        else if (ownerUtilities == 2)
        {
            return diceSum * 10;
        }

        return 0;
    }
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
        if (player.Balance < 0)
        {
            Program.kill(player);
        }
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

        if (player.Balance < 0)
        {
            Program.kill(player);
        }
    }
}

public class JailTile : Tile
{
    public JailTile(string name)
    {
        Name = name;
    }

    public override void PerformAction(Player player, int diceSum)
    {
        Console.WriteLine($"{player.Name} landed on {Name}");

        // Check if the player is in jail
        if (player.GetJailFree() == false)
        {
            if (player.InJail)
            {
                Console.WriteLine($"{player.Name} is in jail. Nothing to do here but wait out the turns.");

                // Reduce the remaining turns in jail
                player.TurnsInJail--;

                if (player.TurnsInJail == 0)
                {
                    Console.WriteLine($"{player.Name} is released from jail!");
                    player.InJail = false;
                }
            }
        }
        else
        {
            Console.WriteLine($"{player.Name} has jail free card");
            player.SetJailFree(false);
        }
    }
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

        // Simulating a random card from the chanceTile.
        Random random = new Random();
        int cardNumber = random.Next(1, 9);

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
                PayEachPlayer(player, 50, Program.players);
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
        Console.WriteLine($"Placing {amount}Ꝟ on the board.");
        if (player.Balance >= amount)
        {
            player.Balance -= amount;
            boardMoney += amount;  // Add the amount to the boardMoney variable

            int ownerHousesAndHotelsCount = GetOwnerHousesAndHotelsCount(player);
            boardMoney += ownerHousesAndHotelsCount * 25;  // Add money for houses
            boardMoney += (ownerHousesAndHotelsCount / 5) * 100;  // Add money for hotels
        }
        else
        {
            // Player doesn't have enough money scenario.
            Console.WriteLine($"{player.Name} doesn't have enough money to place {amount}Ꝟ on the board.");
            Console.WriteLine($"{player.Name} has a negative balance and is out of the game!");
            Program.players.Remove(player);
            RealEstateTile.ReturnProperties(player);
        }
    }

    private void PlaceMoneyForHousesAndHotels(Player player)
    {
        int totalMoney = player.Properties.OfType<RealEstateTile>().Sum(t => t.GetHouseCount() * 25);
        totalMoney += player.Properties.OfType<RealEstateTile>().Count(t => t.GetHouseCount() == 5) * 100;

        Console.WriteLine($"Placing {totalMoney}Ꝟ on the board for houses and hotels.");
    }

    private void TravelToNearestTrainStation(Player player)
    {
        int nearestTrainStationIndex = FindNearestTrainStation(player);

        if (nearestTrainStationIndex != -1)
        {
            Console.WriteLine($"{player.Name} is traveling to the nearest train station.");
            player.Position = nearestTrainStationIndex;

            Program.tiles[nearestTrainStationIndex].PerformAction(player, 0);

            // Collect 200Ꝟ if passing through the beginning tile
            if (nearestTrainStationIndex < player.Position)
            {
                Console.WriteLine($"{player.Name} passed through the beginning tile and collected 200Ꝟ.");
                player.Balance += 200;
            }
        }
        else
        {
            Console.WriteLine($"{player.Name} couldn't find any train stations on the board.");
        }
    }

    private void GoBackTiles(Player player, int numberOfTiles)
    {
        Console.WriteLine($"{player.Name} is going back {numberOfTiles} tiles.");
        player.Position = (player.Position - numberOfTiles + Program.tiles.Count) % Program.tiles.Count;
    }

    private void GetOutOfJail(Player player)
    {
        player.SetJailFree(true);
        Console.WriteLine($"{player.Name} has jail free card");
    }

    private void PayEachPlayer(Player payingPlayer, int amount, List<Player> allPlayers)
    {
        Console.WriteLine($"{payingPlayer.Name} has to pay each player {amount}Ꝟ.");

        int totalAmount = (allPlayers.Count - 1) * amount;

        if (payingPlayer.Balance < totalAmount)
        {
            Program.kill(payingPlayer);
            return;
        }

        foreach (Player otherPlayers in allPlayers)
        {
            if (otherPlayers != payingPlayer)
            {
                Console.WriteLine($"{payingPlayer.Name} pays {otherPlayers.Name} {amount}Ꝟ.");
                payingPlayer.Balance -= amount;
                otherPlayers.Balance += amount;
            }
        }

    }

    private int GetOwnerHousesAndHotelsCount(Player player)
    {
        // Assume for simplicity that hotels are equivalent to 5 houses
        int houses = 0;
        if (player != null)
        {
            houses = player.Properties.OfType<RealEstateTile>().Count(t => t.GetHouseCount() == 5);
        }
        return houses;
    }

    // Method to get the total money placed on the board
    public static int GetBoardMoney()
    {
        return boardMoney;
    }

    // Method to find the nearest train station
    private int FindNearestTrainStation(Player player)
    {
        int currentIndex = player.Position;
        int forwardIndex = currentIndex;
        int backwardIndex = currentIndex;

        while (true)
        {
            forwardIndex = (forwardIndex + 1) % Program.tiles.Count;
            backwardIndex = (backwardIndex - 1 + Program.tiles.Count) % Program.tiles.Count;

            if (Program.tiles[forwardIndex] is TrainStationTile)
            {
                return forwardIndex;
            }

            if (Program.tiles[backwardIndex] is TrainStationTile)
            {
                return backwardIndex;
            }

            if (forwardIndex == currentIndex && backwardIndex == currentIndex)
            {
                return -1;
            }
        }
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

        // Random card from community chest 
        Random random = new Random();
        int cardNumber = random.Next(1, 10);
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
                TravelToStart(player);
                break;
            case 8:
                TravelToJail(player);
                break;
            case 9:
                CollectFromEachPlayer(player, 100, Program.players);
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
        Console.WriteLine($"Placing {amount}Ꝟ on the board.");

        if (player.Balance >= amount)
        {
            player.Balance -= amount;
            AddMoneyToBoard(amount);  // Add the amount to the boardMoney variable in the ChanceTile class
        }
        else
        {
            Console.WriteLine($"{player.Name} doesn't have enough money to place {amount}Ꝟ on the board.");
        }
    }

    private void AddMoneyToBoard(int amount)
    {
        Console.WriteLine($"Placing {amount}Ꝟ on the board.");

        // Implement logic to place money on the board (example: add it to the boardMoney variable)
        ChanceTile.boardMoney += amount;
    }

    private void PlaceMoneyForHousesAndHotels(Player player)
    {
        int totalMoney = player.Properties.OfType<RealEstateTile>().Sum(t => t.GetHouseCount() * 40);
        totalMoney += player.Properties.OfType<RealEstateTile>().Count(t => t.GetHouseCount() == 5) * 115;

        Console.WriteLine($"Placing {totalMoney}Ꝟ on the board for houses and hotels.");
        // Implement logic to place money on the board (not specified in your original code)
    }

    private void TravelToNearestUtility(Player player)
    {
        int nearestUtilityIndex = FindNearestUtility(player);

        if (nearestUtilityIndex != -1)
        {
            Console.WriteLine($"{player.Name} is traveling to the nearest utility.");
            player.Position = nearestUtilityIndex;


            // Collect 200Ꝟ if passing through the beginning tile
            if (nearestUtilityIndex < player.Position)
            {
                Console.WriteLine($"{player.Name} passed through the beginning tile and collected 200Ꝟ.");
                player.Balance += 200;
            }
        }
        else
        {
            Console.WriteLine($"{player.Name} couldn't find any utilities on the board.");
        }
    }

    private void GoBackTiles(Player player, int numberOfTiles)
    {
        Console.WriteLine($"{player.Name} is going back {numberOfTiles} tiles.");
        player.Position = (player.Position - numberOfTiles + Program.tiles.Count) % Program.tiles.Count;
    }

    private void TravelToJail(Player player)
    {
        Console.WriteLine($"{player.Name} is traveling to jail.");
        player.Position = Program.tiles.FindIndex(tile => tile is JailTile);
    }

    private void TravelToStart(Player player)
    {
        Console.WriteLine($"{player.Name} is traveling to jail.");
        //player.Position = Program.tiles.FindIndex(tile => tile is StartTile);
        player.Position = 0;
    }

    private void CollectFromEachPlayer(Player collectingPlayer, int amount, List<Player> allPlayers)
    {
        Console.WriteLine($"{collectingPlayer.Name} is collecting {amount}Ꝟ from each player.");

        foreach (Player otherPlayer in allPlayers)
        {
            if (otherPlayer != collectingPlayer)
            {
                Console.WriteLine($"{collectingPlayer.Name} collects {amount}Ꝟ from {otherPlayer.Name}.");
                collectingPlayer.Balance += amount;
                otherPlayer.Balance -= amount;
            }
        }

        // Check if any player has a negative balance after the collection
        foreach (Player player in allPlayers)
        {
            if (player.Balance < 0)
            {
                Program.kill(player);
            }
        }
    }


    private int FindNearestUtility(Player player)
    {
        int currentIndex = player.Position;
        int forwardIndex = currentIndex;
        int backwardIndex = currentIndex;

        while (true)
        {
            forwardIndex = (forwardIndex + 1) % Program.tiles.Count;
            backwardIndex = (backwardIndex - 1 + Program.tiles.Count) % Program.tiles.Count;

            if (Program.tiles[forwardIndex] is UtilityTile)
            {
                return forwardIndex;
            }

            if (Program.tiles[backwardIndex] is UtilityTile)
            {
                return backwardIndex;
            }

            // Prevent infinite loop if there are no utilities on the board
            if (forwardIndex == currentIndex && backwardIndex == currentIndex)
            {
                return -1;
            }
        }
    }
}

public class FreeParkingTile : Tile
{
    public FreeParkingTile(string name)
    {
        Name = name;
    }

    public override void PerformAction(Player player, int diceSum)
    {
        Console.WriteLine($"{player.Name} landed on Free Parking!");

        // Collect the money placed on the board
        int moneyOnBoard = ChanceTile.GetBoardMoney();
        Console.WriteLine($"{player.Name} collected {moneyOnBoard}Ꝟ from Free Parking.");
        player.Balance += moneyOnBoard;

        // Reset the money on the board
        ChanceTile.boardMoney = 0;
    }
}


public class Program
{

    static void DisplayPropertiesAndBalances(List<Player> allPlayers)
    {
        Console.WriteLine("Properties and Balances:");

        foreach (Player player in allPlayers)
        {
            Console.WriteLine($"{player.Name} (Balance: {player.Balance})");

            foreach (Tile property in player.Properties)
            {
                Console.Write($"  - {property.Name}");

                if (property is RealEstateTile realEstateTile)
                {
                    int houseCount = realEstateTile.GetHouseCount();
                    if (houseCount > 0)
                    {
                        Console.Write($" (Houses: {houseCount})");
                    }
                    else if (houseCount == 5)
                    {
                        Console.Write(" (Hotel)");
                    }
                }

                Console.WriteLine();
            }

            Console.WriteLine();
        }
    }
    public static void kill(Player player)
    {

        Console.WriteLine($"{player.Name} has a negative balance and is out of the game!");
        players.Remove(player);
        RealEstateTile.ReturnProperties(player);

    }

    static void DisplayBoard(List<Player> allPlayers)
    {
        Console.WriteLine("Board View:");

        for (int i = 0; i < tiles.Count; i++)
        {
            string tileInfo = $"{tiles.ElementAt(i).Name}";
            List<Player> owners = players.Where(p => p.Properties.Contains(tiles[i])).ToList();

            if (tiles.ElementAt(i) is RealEstateTile realEstateTile)
            {
                int houseCount = realEstateTile.GetHouseCount();
                if (houseCount == 5)
                {
                    tileInfo += " (Hotel)";
                }
                else if (houseCount > 0)
                {
                    tileInfo += $" (Houses: {houseCount})";
                }
            }

            if (owners.Count > 0)
            {
                tileInfo += "  Owned by: ";
                foreach (var owner in owners)
                {
                    tileInfo += $"{owner.Name}, ";
                }
                // Remove the trailing comma and space
                tileInfo = tileInfo.Remove(tileInfo.Length - 2);
            }

            // Display player names on the current tile
            List<Player> playersOnTile = allPlayers.Where(p => p.Position % tiles.Count == i).ToList();
            if (playersOnTile.Count > 0)
            {
                tileInfo += "  Players: ";
                foreach (var playerOnTile in playersOnTile)
                {
                    tileInfo += $"{playerOnTile.Name}, ";
                }
                // Remove the trailing comma and space
                tileInfo = tileInfo.Remove(tileInfo.Length - 2);
            }

            Console.WriteLine($"- {tileInfo}");
        }

        Console.WriteLine();

        foreach (Player player in allPlayers)
        {
            Tile playerTile = tiles[player.Position % tiles.Count];
            string playerInfo = $"{player.Name} (Balance: {player.Balance}, Position: {player.Position}, Tile: {playerTile.Name})";

            Console.WriteLine(playerInfo);
        }

        Console.WriteLine();
    }





    public static List<Player> players = new List<Player>();


    public static List<Tile> tiles = new List<Tile>
{
    new StartTile("Start"),
    new RealEstateTile("Buy Land (Tier 1 - 1)", 1, 60, 50, new int[] { 4, 20, 60, 180, 320, 450}),
    new CommunityChestTile("Community Chest"),
    new RealEstateTile("Buy Land (Tier 1 - 2)", 1, 60, 50, new int[] { 4, 20, 60, 180, 320, 450 }),
    new IncomeTaxTile("Income Tax", 200),
    new TrainStationTile("Train Station", 100, 50, 100, 150, 200),
    new RealEstateTile("Buy Land (Tier 2 - 1)", 2, 120, 50, new int[] { 8, 40, 100, 300, 450, 600 }),
    new ChanceTile("Chance"),
    new RealEstateTile("Buy Land (Tier 2 - 2 )", 2, 120, 50, new int[] { 8, 40, 100, 300, 450, 600 }),
    new RealEstateTile("Buy Land (Tier 2 - 3)", 2, 120, 50, new int[] { 8, 40, 100, 300, 450, 600 }),
    new JailTile("The Jail"),
    new RealEstateTile("Buy Land (Tier 3 - 1)", 3, 160, 100, new int[] { 12, 60, 180, 500, 700, 900 }),
    new UtilityTile("Electric Company", 100),
    new RealEstateTile("Buy Land (Tier 3 - 2)", 3, 160, 100, new int[] { 12, 60, 180, 500, 700, 900 }),
    new RealEstateTile("Buy Land (Tier 3 - 3)", 3, 160, 100, new int[] { 12, 60, 180, 500, 700, 900 }),
    new TrainStationTile("Train Station", 100, 50, 100, 150, 200),
    new RealEstateTile("Buy Land (Tier 4 - 1)", 4, 200, 100, new int[] { 16, 80, 220, 600, 800, 1000 }),
    new CommunityChestTile("Community Chest"),
    new RealEstateTile("Buy Land (Tier 4 - 2)", 4, 200, 100, new int[] { 16, 80, 220, 600, 800, 1000 }),
    new RealEstateTile("Buy Land (Tier 4 - 3)", 4, 200, 100, new int[] { 16, 80, 220, 600, 800, 1000 }),
    new FreeParkingTile("Free Parking"),
    new RealEstateTile("Buy Land (Tier 5 - 1)", 5, 240, 150, new int[] { 20, 100, 300, 750, 925, 1100 }),
    new ChanceTile("Chance"),
    new RealEstateTile("Buy Land (Tier 5 - 2)", 5, 240, 150, new int[] { 20, 100, 300, 750, 925, 1100 }),
    new RealEstateTile("Buy Land (Tier 5 - 3)", 5, 240, 150, new int[] { 20, 100, 300, 750, 925, 1100 }),
    new TrainStationTile("Train Station", 100, 50, 100, 150, 200),
    new RealEstateTile("Buy Land (Tier 6 - 1)", 6, 280, 150, new int[] { 24, 120, 360, 850, 1025, 1200 }),
    new RealEstateTile("Buy Land (Tier 6 - 2)", 6, 280, 150, new int[] { 24, 120, 360, 850, 1025, 1200 }),
    new UtilityTile("Water Works", 100),
    new RealEstateTile("Buy Land (Tier 6 - 3)", 6, 280, 150, new int[] { 24, 120, 360, 850, 1025, 1200 }),
    new GoToJailTile("Go to Jail"),
    new RealEstateTile("Buy Land (Tier 7 - 1)", 7, 320, 200, new int[] { 28, 150, 450, 1000, 1200, 1400 }),
    new RealEstateTile("Buy Land (Tier 7 - 2)", 7, 320, 200, new int[] { 28, 150, 450, 1000, 1200, 1400 }),
    new CommunityChestTile("Community Chest"),
    new RealEstateTile("Buy Land (Tier 7 - 3)", 7, 320, 200, new int[] { 28, 150, 450, 1000, 1200, 1400 }),
    new TrainStationTile("Train Station", 100, 50, 100, 150, 200),
    new ChanceTile("Chance"),
    new RealEstateTile("Buy Land (Tier 8 - 1)", 8, 320, 200, new int[] { 50, 200, 600, 1400, 1700, 2000 }),
    new LuxuryTaxTile("Luxury Tax", 150),
    new RealEstateTile("Buy Land (Tier 8 - 2)", 8, 400, 200, new int[] { 50, 200, 600, 1400, 1700, 2000 })
};

    static void Main()
    {

        // Ask for the number of players
        Console.Write("Enter the number of players (2-4): ");
        int numberOfPlayers;

        while (!int.TryParse(Console.ReadLine(), out numberOfPlayers) || numberOfPlayers < 2 || numberOfPlayers > 4)
        {
            Console.Write("Invalid input. Enter a number between 2 and 4: ");
        }

        // Create players and tiles based on the user input

        for (int i = 1; i <= numberOfPlayers; i++)
        {
            Console.Write($"Enter the name for Player {i}: ");
            string playerName = Console.ReadLine();
            players.Add(new Player { Name = playerName, Balance = 200, Position = 0 });
        }


        Console.WriteLine($"Starting the game with {numberOfPlayers} players!");

        // Determine the starting player by rolling a die
        Random random = new Random();
        int startingPlayerIndex = random.Next(0, numberOfPlayers);
        Player currentPlayer = players[startingPlayerIndex];

        Console.WriteLine($"Starting player: {currentPlayer.Name}");

        // Start the game loop
        while (players.Count > 1)
        {

            if (currentPlayer.Balance < 0)
            {
                kill(currentPlayer);
            }

            DisplayBoard(players);
            Console.WriteLine($"It's {currentPlayer.Name}'s turn. Do you want to view properties and balances before rolling the dice? (y/n)");
            string input = Console.ReadLine();

            if (input.ToLower() == "y")
            {
                // Display properties and balances on demand
                DisplayPropertiesAndBalances(players);
            }
            else if (input.ToLower() != "n")
            {
                Console.WriteLine($"Invalid input. Press Enter to roll the dice.");
            }

            Console.WriteLine($"Press Enter to roll the dice.");
            Console.ReadLine();

            if (currentPlayer.InJail)
            {
                Console.WriteLine($"{currentPlayer.Name} is in jail. Skipping turn.");

                // Reduce the remaining turns in jail
                currentPlayer.TurnsInJail--;

                if (currentPlayer.TurnsInJail == 0)
                {
                    Console.WriteLine($"{currentPlayer.Name} is released from jail!");
                    currentPlayer.InJail = false;
                }
            }
            else
            {
                // rolling two dice
                int dice1 = random.Next(1, 6);
                int dice2 = random.Next(1, 6);
                int sum = dice1 + dice2;

                Console.WriteLine($"{currentPlayer.Name} rolled a {dice1} and a {dice2}. Total: {sum} Currency {currentPlayer.Balance}");

                // Update the player's position based on the sum of the dice
                int newPosition = (currentPlayer.Position + sum) % tiles.Count;



                if (newPosition < currentPlayer.Position)
                {
                    Console.WriteLine($"{currentPlayer.Name} passed Start! Collect $200.");
                    currentPlayer.Balance += 200;
                    Console.WriteLine($"Updated Balance for {currentPlayer.Name}: ${currentPlayer.Balance}");
                }


                // Get the tile where the player landed
                Tile landedTile = tiles[newPosition];
                currentPlayer.Position = newPosition;
                Console.WriteLine($"{currentPlayer.Name} landed on {landedTile.Name}");

                // Check if the player is in jail
                if (landedTile is JailTile)
                {
                    currentPlayer.GoToJail();
                }
                else
                {
                    currentPlayer.Position = newPosition;
                    landedTile.PerformAction(currentPlayer, sum);
                }
            }

            // Move to the next player
            int nextPlayerIndex = (players.IndexOf(currentPlayer) + 1) % players.Count;
            currentPlayer = players[nextPlayerIndex];


        }

        // End of the game loop
        Console.WriteLine($"Game over! {players[0].Name} is the winner!");
    }
}
