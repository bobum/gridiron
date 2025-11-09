using DomainObjects;
using DomainObjects.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StateLibrary.Calculators;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class CalculatorTests
    {
        #region TeamPowerCalculator Tests

        [TestMethod]
        public void TeamPowerCalculator_PassBlockingPower_CalculatesAverage()
        {
            // Arrange
            var players = new List<Player>
            {
                new Player { Position = Positions.C, Blocking = 80 },
                new Player { Position = Positions.G, Blocking = 70 },
                new Player { Position = Positions.G, Blocking = 90 },
                new Player { Position = Positions.T, Blocking = 85 },
                new Player { Position = Positions.T, Blocking = 75 }
            };

            // Act
            var power = TeamPowerCalculator.CalculatePassBlockingPower(players);

            // Assert
            Assert.AreEqual(80.0, power); // (80+70+90+85+75)/5 = 80
        }

        [TestMethod]
        public void TeamPowerCalculator_PassBlockingPower_IncludesTEsAndRBs()
        {
            // Arrange
            var players = new List<Player>
            {
                new Player { Position = Positions.C, Blocking = 80 },
                new Player { Position = Positions.TE, Blocking = 60 },
                new Player { Position = Positions.RB, Blocking = 40 },
                new Player { Position = Positions.FB, Blocking = 70 }
            };

            // Act
            var power = TeamPowerCalculator.CalculatePassBlockingPower(players);

            // Assert
            Assert.AreEqual(62.5, power); // (80+60+40+70)/4 = 62.5
        }

        [TestMethod]
        public void TeamPowerCalculator_PassBlockingPower_NoBlockers_ReturnsDefault()
        {
            // Arrange
            var players = new List<Player>
            {
                new Player { Position = Positions.QB, Blocking = 10 },
                new Player { Position = Positions.WR, Blocking = 20 }
            };

            // Act
            var power = TeamPowerCalculator.CalculatePassBlockingPower(players);

            // Assert
            Assert.AreEqual(50.0, power); // Default power
        }

        [TestMethod]
        public void TeamPowerCalculator_PassRushPower_CalculatesComposite()
        {
            // Arrange
            var players = new List<Player>
            {
                new Player { Position = Positions.DE, Tackling = 90, Speed = 85, Strength = 80 }, // (90+85+80)/3 = 85
                new Player { Position = Positions.DT, Tackling = 85, Speed = 70, Strength = 95 }  // (85+70+95)/3 = 83.33
            };

            // Act
            var power = TeamPowerCalculator.CalculatePassRushPower(players);

            // Assert
            Assert.AreEqual(84.166666666666671, power, 0.01); // (85 + 83.33)/2 = 84.165
        }

        [TestMethod]
        public void TeamPowerCalculator_PassRushPower_IncludesLinebackers()
        {
            // Arrange
            var players = new List<Player>
            {
                new Player { Position = Positions.LB, Tackling = 80, Speed = 75, Strength = 70 },
                new Player { Position = Positions.OLB, Tackling = 85, Speed = 80, Strength = 75 }
            };

            // Act
            var power = TeamPowerCalculator.CalculatePassRushPower(players);

            // Assert
            Assert.IsTrue(power > 0); // Should calculate from LBs
        }

        [TestMethod]
        public void TeamPowerCalculator_RunBlockingPower_ExcludesRBs()
        {
            // Arrange - RBs block in pass protection but not run blocking
            var players = new List<Player>
            {
                new Player { Position = Positions.C, Blocking = 80 },
                new Player { Position = Positions.RB, Blocking = 40 }, // Should be excluded
                new Player { Position = Positions.FB, Blocking = 70 }  // Should be included
            };

            // Act
            var power = TeamPowerCalculator.CalculateRunBlockingPower(players);

            // Assert
            Assert.AreEqual(75.0, power); // (80+70)/2, RB not included
        }

        [TestMethod]
        public void TeamPowerCalculator_RunDefensePower_CalculatesComposite()
        {
            // Arrange
            var players = new List<Player>
            {
                new Player { Position = Positions.DT, Tackling = 90, Strength = 95, Speed = 60 },
                new Player { Position = Positions.LB, Tackling = 85, Strength = 75, Speed = 80 }
            };

            // Act
            var power = TeamPowerCalculator.CalculateRunDefensePower(players);

            // Assert - (90+95+60)/3 = 81.67, (85+75+80)/3 = 80, average = 80.83
            Assert.AreEqual(80.833333333333329, power, 0.01);
        }

        [TestMethod]
        public void TeamPowerCalculator_CoveragePower_IncludesDBsAndLBs()
        {
            // Arrange
            var players = new List<Player>
            {
                new Player { Position = Positions.CB, Coverage = 85, Speed = 90, Awareness = 80 },
                new Player { Position = Positions.S, Coverage = 80, Speed = 85, Awareness = 85 },
                new Player { Position = Positions.FS, Coverage = 82, Speed = 88, Awareness = 90 },
                new Player { Position = Positions.LB, Coverage = 70, Speed = 75, Awareness = 80 }
            };

            // Act
            var power = TeamPowerCalculator.CalculateCoveragePower(players);

            // Assert - Should include all 4 players
            Assert.IsTrue(power > 70 && power < 90);
        }

        [TestMethod]
        public void TeamPowerCalculator_CoveragePower_NoCoverage_ReturnsDefault()
        {
            // Arrange
            var players = new List<Player>
            {
                new Player { Position = Positions.DE, Coverage = 30, Speed = 80, Awareness = 60 }
            };

            // Act
            var power = TeamPowerCalculator.CalculateCoveragePower(players);

            // Assert
            Assert.AreEqual(50.0, power); // Default when no coverage players
        }

        #endregion

        #region LineBattleCalculator Tests

        [TestMethod]
        public void LineBattleCalculator_StandardRush_EvenMatchup_ReturnsBasePressure()
        {
            // Arrange - 4 rushers (standard), equal skills
            var offense = new List<Player>
            {
                new Player { Position = Positions.C, Blocking = 70 },
                new Player { Position = Positions.G, Blocking = 70 },
                new Player { Position = Positions.G, Blocking = 70 },
                new Player { Position = Positions.T, Blocking = 70 },
                new Player { Position = Positions.T, Blocking = 70 }
            };

            var defense = new List<Player>
            {
                new Player { Position = Positions.DE, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DE, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DT, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DT, Tackling = 70, Speed = 70, Strength = 70 }
            };

            // Act
            var pressure = LineBattleCalculator.CalculateDPressureFactor(offense, defense, isPassPlay: true);

            // Assert - Should be near 1.0 (base pressure)
            Assert.AreEqual(1.0, pressure, 0.1);
        }

        [TestMethod]
        public void LineBattleCalculator_ThreeManRush_LowPressure()
        {
            // Arrange - Only 3 rushers (prevent defense)
            var offense = new List<Player>
            {
                new Player { Position = Positions.C, Blocking = 70 },
                new Player { Position = Positions.G, Blocking = 70 },
                new Player { Position = Positions.T, Blocking = 70 }
            };

            var defense = new List<Player>
            {
                new Player { Position = Positions.DE, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DT, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DT, Tackling = 70, Speed = 70, Strength = 70 }
            };

            // Act
            var pressure = LineBattleCalculator.CalculateDPressureFactor(offense, defense, isPassPlay: true);

            // Assert - Should be less than 1.0 (1 fewer rusher = -0.15)
            Assert.IsTrue(pressure < 1.0);
            Assert.IsTrue(pressure > 0.7); // Around 0.85
        }

        [TestMethod]
        public void LineBattleCalculator_Blitz_HighPressure()
        {
            // Arrange - 6 rushers (blitz: 4 DL + 2 LB)
            var offense = new List<Player>
            {
                new Player { Position = Positions.C, Blocking = 70 },
                new Player { Position = Positions.G, Blocking = 70 },
                new Player { Position = Positions.T, Blocking = 70 }
            };

            var defense = new List<Player>
            {
                new Player { Position = Positions.DE, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DE, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DT, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DT, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.LB, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.OLB, Tackling = 70, Speed = 70, Strength = 70 }
            };

            // Act
            var pressure = LineBattleCalculator.CalculateDPressureFactor(offense, defense, isPassPlay: true);

            // Assert - Should be greater than 1.0 (2 extra rushers = +0.30)
            Assert.IsTrue(pressure > 1.0);
            Assert.IsTrue(pressure < 1.5); // Around 1.30
        }

        [TestMethod]
        public void LineBattleCalculator_StrongOLine_WeakDLine_LowPressure()
        {
            // Arrange - Elite O-Line (90) vs weak D-Line (50)
            var offense = new List<Player>
            {
                new Player { Position = Positions.C, Blocking = 90 },
                new Player { Position = Positions.G, Blocking = 90 },
                new Player { Position = Positions.T, Blocking = 90 }
            };

            var defense = new List<Player>
            {
                new Player { Position = Positions.DE, Tackling = 50, Speed = 50, Strength = 50 },
                new Player { Position = Positions.DT, Tackling = 50, Speed = 50, Strength = 50 },
                new Player { Position = Positions.DT, Tackling = 50, Speed = 50, Strength = 50 },
                new Player { Position = Positions.DT, Tackling = 50, Speed = 50, Strength = 50 }
            };

            // Act
            var pressure = LineBattleCalculator.CalculateDPressureFactor(offense, defense, isPassPlay: true);

            // Assert - Should be well below 1.0 (skill differential -40 = -0.40)
            Assert.IsTrue(pressure < 0.8);
        }

        [TestMethod]
        public void LineBattleCalculator_WeakOLine_StrongDLine_HighPressure()
        {
            // Arrange - Weak O-Line (50) vs elite D-Line (90)
            var offense = new List<Player>
            {
                new Player { Position = Positions.C, Blocking = 50 },
                new Player { Position = Positions.G, Blocking = 50 },
                new Player { Position = Positions.T, Blocking = 50 }
            };

            var defense = new List<Player>
            {
                new Player { Position = Positions.DE, Tackling = 90, Speed = 90, Strength = 90 },
                new Player { Position = Positions.DT, Tackling = 90, Speed = 90, Strength = 90 },
                new Player { Position = Positions.DT, Tackling = 90, Speed = 90, Strength = 90 },
                new Player { Position = Positions.DT, Tackling = 90, Speed = 90, Strength = 90 }
            };

            // Act
            var pressure = LineBattleCalculator.CalculateDPressureFactor(offense, defense, isPassPlay: true);

            // Assert - Should be well above 1.0 (skill differential +40 = +0.40)
            Assert.IsTrue(pressure > 1.2);
        }

        [TestMethod]
        public void LineBattleCalculator_ExtremeBlitz_ClampsAtMax()
        {
            // Arrange - 8 rushers with elite skills
            var offense = new List<Player>
            {
                new Player { Position = Positions.C, Blocking = 50 },
                new Player { Position = Positions.G, Blocking = 50 },
                new Player { Position = Positions.T, Blocking = 50 }
            };

            var defense = new List<Player>
            {
                new Player { Position = Positions.DE, Tackling = 95, Speed = 95, Strength = 95 },
                new Player { Position = Positions.DE, Tackling = 95, Speed = 95, Strength = 95 },
                new Player { Position = Positions.DT, Tackling = 95, Speed = 95, Strength = 95 },
                new Player { Position = Positions.DT, Tackling = 95, Speed = 95, Strength = 95 },
                new Player { Position = Positions.LB, Tackling = 95, Speed = 95, Strength = 95 },
                new Player { Position = Positions.LB, Tackling = 95, Speed = 95, Strength = 95 },
                new Player { Position = Positions.OLB, Tackling = 95, Speed = 95, Strength = 95 },
                new Player { Position = Positions.OLB, Tackling = 95, Speed = 95, Strength = 95 }
            };

            // Act
            var pressure = LineBattleCalculator.CalculateDPressureFactor(offense, defense, isPassPlay: true);

            // Assert - Should be clamped at or below MAX_PRESSURE (2.5)
            Assert.IsTrue(pressure <= 2.5);
            Assert.IsTrue(pressure >= 2.0); // Should be high
        }

        [TestMethod]
        public void LineBattleCalculator_PreventDefense_ClampsAtMin()
        {
            // Arrange - 2 rushers with weak skills vs elite O-Line
            var offense = new List<Player>
            {
                new Player { Position = Positions.C, Blocking = 95 },
                new Player { Position = Positions.G, Blocking = 95 },
                new Player { Position = Positions.G, Blocking = 95 },
                new Player { Position = Positions.T, Blocking = 95 },
                new Player { Position = Positions.T, Blocking = 95 }
            };

            var defense = new List<Player>
            {
                new Player { Position = Positions.DE, Tackling = 40, Speed = 40, Strength = 40 },
                new Player { Position = Positions.DT, Tackling = 40, Speed = 40, Strength = 40 }
            };

            // Act
            var pressure = LineBattleCalculator.CalculateDPressureFactor(offense, defense, isPassPlay: true);

            // Assert - Should be clamped at or above MIN_PRESSURE (0.0)
            Assert.IsTrue(pressure >= 0.0);
            Assert.IsTrue(pressure < 0.5); // Should be very low
        }

        [TestMethod]
        public void LineBattleCalculator_PassVsRun_UsesDifferentCalculations()
        {
            // Arrange - Same players, different mode
            var offense = new List<Player>
            {
                new Player { Position = Positions.C, Blocking = 70 },
                new Player { Position = Positions.G, Blocking = 70 },
                new Player { Position = Positions.T, Blocking = 70 },
                new Player { Position = Positions.RB, Blocking = 60 } // RB helps pass block, but lowers average
            };

            var defense = new List<Player>
            {
                new Player { Position = Positions.DE, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.DT, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.LB, Tackling = 70, Speed = 70, Strength = 70 },
                new Player { Position = Positions.LB, Tackling = 70, Speed = 70, Strength = 70 }
            };

            // Act
            var passPressure = LineBattleCalculator.CalculateDPressureFactor(offense, defense, isPassPlay: true);
            var runPressure = LineBattleCalculator.CalculateDPressureFactor(offense, defense, isPassPlay: false);

            // Assert - Pass should have slightly higher pressure because RB's lower blocking (60) 
            // reduces the average from 70 to 67.5, making offense weaker
            Assert.IsTrue(passPressure > runPressure);
        }

        #endregion
    }
}
