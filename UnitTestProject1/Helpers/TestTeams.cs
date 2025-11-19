using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using DomainObjects;

namespace UnitTestProject1.Helpers
{
    /// <summary>
    /// Provides test data teams with full rosters for unit testing.
    /// Loads team data from JSON files and builds depth charts.
    /// </summary>
    public static class TestTeams
    {
        private static readonly string TestDataPath = Path.Combine(
   AppDomain.CurrentDomain.BaseDirectory, 
 "TestData"
   );

        /// <summary>
      /// Creates a Teams object with Atlanta Falcons (home) and Philadelphia Eagles (visitor).
      /// Loads player data from JSON files and builds all depth charts.
        /// </summary>
        public static DomainObjects.Helpers.Teams CreateTestTeams()
        {
    var homeTeam = LoadAtlantaFalcons();
      var visitorTeam = LoadPhiladelphiaEagles();

      // Use the parameterized constructor which builds all depth charts
            return new DomainObjects.Helpers.Teams(homeTeam, visitorTeam);
        }

        /// <summary>
     /// Loads the Atlanta Falcons team from JSON file
        /// </summary>
        public static Team LoadAtlantaFalcons()
        {
  string jsonPath = Path.Combine(TestDataPath, "AtlantaFalcons.json");
    
            if (!File.Exists(jsonPath))
   {
      throw new FileNotFoundException(
          $"Atlanta Falcons test data not found at: {jsonPath}. " +
           $"Ensure AtlantaFalcons.json exists in the TestData folder and is set to 'Copy to Output Directory'."
     );
       }

            string json = File.ReadAllText(jsonPath);
         var players = JsonConvert.DeserializeObject<List<Player>>(json);

        return new Team
  {
       City = "Atlanta",
          Name = "Falcons",
        Players = players ?? new List<Player>()
  };
        }

        /// <summary>
        /// Loads the Philadelphia Eagles team from JSON file
        /// </summary>
        public static Team LoadPhiladelphiaEagles()
        {
      string jsonPath = Path.Combine(TestDataPath, "PhiladelphiaEagles.json");
 
            if (!File.Exists(jsonPath))
      {
     throw new FileNotFoundException(
        $"Philadelphia Eagles test data not found at: {jsonPath}. " +
            $"Ensure PhiladelphiaEagles.json exists in the TestData folder and is set to 'Copy to Output Directory'."
       );
         }

     string json = File.ReadAllText(jsonPath);
  var players = JsonConvert.DeserializeObject<List<Player>>(json);

     return new Team
            {
    City = "Philadelphia",
    Name = "Eagles",
     Players = players ?? new List<Player>()
 };
        }

   /// <summary>
        /// Creates a minimal team with just a few players for lightweight testing
        /// </summary>
     public static Team CreateMinimalTeam(string city, string name)
        {
         return new Team
       {
     City = city,
   Name = name,
    Players = new List<Player>
       {
      CreateTestPlayer(Positions.QB, 1, "Test", "Quarterback", 90),
       CreateTestPlayer(Positions.RB, 21, "Test", "Runner", 85),
   CreateTestPlayer(Positions.WR, 81, "Test", "Receiver", 88),
        CreateTestPlayer(Positions.T, 71, "Test", "Tackle", 82),
         CreateTestPlayer(Positions.G, 61, "Test", "Guard", 80),
          CreateTestPlayer(Positions.C, 51, "Test", "Center", 83),
        CreateTestPlayer(Positions.TE, 88, "Test", "TightEnd", 84),
     CreateTestPlayer(Positions.DE, 99, "Test", "End", 86),
         CreateTestPlayer(Positions.DT, 95, "Test", "Tackle", 87),
       CreateTestPlayer(Positions.LB, 54, "Test", "Linebacker", 85),
    CreateTestPlayer(Positions.CB, 22, "Test", "Corner", 89),
         CreateTestPlayer(Positions.S, 32, "Test", "Safety", 84),
  CreateTestPlayer(Positions.K, 3, "Test", "Kicker", 80),
CreateTestPlayer(Positions.P, 5, "Test", "Punter", 78)
 }
     };
        }

   private static Player CreateTestPlayer(Positions position, int number, string firstName, string lastName, int overallSkill)
        {
      return new Player
         {
  Position = position,
    Number = number,
         FirstName = firstName,
        LastName = lastName,
     Height = "6-2",
       Weight = 220,
    Age = 25,
       Exp = 3,
                College = "Test University",
      Speed = overallSkill,
   Strength = overallSkill,
                Agility = overallSkill,
      Awareness = overallSkill,
     Morale = 90,
       Passing = position == Positions.QB ? overallSkill : 10,
      Catching = position == Positions.WR || position == Positions.TE ? overallSkill : 20,
     Rushing = position == Positions.RB ? overallSkill : 20,
    Blocking = position == Positions.T || position == Positions.G || position == Positions.C ? overallSkill : 30,
                Tackling = position == Positions.DE || position == Positions.DT || position == Positions.LB ? overallSkill : 30,
         Coverage = position == Positions.CB || position == Positions.S ? overallSkill : 30,
        Kicking = position == Positions.K || position == Positions.P ? overallSkill : 10,
 Potential = overallSkill + 5,
   Progression = 75,
      Health = 100,
         Discipline = 85
         };
        }
    }
}
