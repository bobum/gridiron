using System.Linq;
using DomainObjects.Helpers;
using DomainObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class TeamsDepthChartTests
    {
        [TestMethod]
        public void OffenseDepthChart_QB_ShouldBeHighestPassing()
        {
            var teams = new Teams();
            var qbList = teams.HomeTeam.OffenseDepthChart.Chart[Positions.QB];
            Assert.IsNotNull(qbList);
            Assert.IsTrue(qbList.Count > 0);

            var expectedQB = teams.HomeTeam.Players
                .Where(p => p.Position == Positions.QB)
                .OrderByDescending(p => p.Passing)
                .First();

            Assert.AreEqual(expectedQB.Number, qbList[0].Number);
        }

        [TestMethod]
        public void DefenseDepthChart_LB_ShouldBeTopThreeBySkill()
        {
            var teams = new Teams();
            var lbList = teams.HomeTeam.DefenseDepthChart.Chart[Positions.LB];
            Assert.IsNotNull(lbList);
            Assert.AreEqual(3, lbList.Count);

            var expectedLBs = teams.HomeTeam.Players
                .Where(p => p.Position == Positions.LB)
                .OrderByDescending(p => p.Tackling + p.Coverage)
                .Take(3)
                .Select(p => p.Number)
                .ToList();

            var actualLBs = lbList.Select(p => p.Number).ToList();
            CollectionAssert.AreEqual(expectedLBs, actualLBs);
        }

        [TestMethod]
        public void FieldGoalOffenseDepthChart_K_ShouldBeTopKicker()
        {
            var teams = new Teams();
            var kList = teams.HomeTeam.FieldGoalOffenseDepthChart.Chart[Positions.K];
            Assert.IsNotNull(kList);
            Assert.AreEqual(1, kList.Count);

            var expectedKicker = teams.HomeTeam.Players
                .Where(p => p.Position == Positions.K)
                .OrderByDescending(p => p.Kicking)
                .First();

            Assert.AreEqual(expectedKicker.Number, kList[0].Number);
        }

        [TestMethod]
        public void PuntOffenseDepthChart_Holder_ShouldBeTopQB()
        {
            var teams = new Teams();
            var holderList = teams.HomeTeam.PuntOffenseDepthChart.Chart[Positions.H];
            Assert.IsNotNull(holderList);
            Assert.AreEqual(1, holderList.Count);

            var expectedHolder = teams.HomeTeam.Players
                .Where(p => p.Position == Positions.QB)
                .OrderByDescending(p => p.Passing)
                .First();

            Assert.AreEqual(expectedHolder.Number, holderList[0].Number);
        }
    }
}
