using System;
using System.Activities;
using ActivityLibrary;
using DomainObjects;
using DomainObjects.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UnitTestProject.Helpers;


namespace UnitTestProject
{
    [TestClass]
    public class DomainObjectTests
    {
        [TestMethod]
        public void DomainObject_GeneratorMakesARandomNumber()
        {
            CryptoRandom rng = new CryptoRandom();
            var nextInt = rng.Next();
            Assert.IsInstanceOfType(nextInt, typeof(Int32));

            var nextUnder10 = rng.Next(10);
            Assert.IsTrue(nextUnder10 < 10);

            var nextBetween18And22 = rng.Next(18, 22);
            Assert.IsTrue(nextBetween18And22 >= 18 && nextBetween18And22 < 22);

            var nextDouble = rng.NextDouble();
            Assert.IsInstanceOfType(nextDouble, typeof(Double));
        }

        [TestMethod]
        public void DomainObject_PenaltiesHasPenalties()
        {
            Assert.IsTrue(Penalties.List.Count > 0);
        }

        [TestMethod]
        public void DomainObject_FirstHalf_CreatesProperQuarters()
        {
            var half = new FirstHalf();

            Assert.IsTrue(half.Quarters[0].QuarterType == QuarterType.First);
            Assert.IsTrue(half.Quarters[1].QuarterType == QuarterType.Second);

        }

        [TestMethod]
        public void DomainObject_SecondHalf_CreatesProperQuarters()
        {
            var half = new SecondHalf();

            Assert.IsTrue(half.Quarters[0].QuarterType == QuarterType.Third);
            Assert.IsTrue(half.Quarters[1].QuarterType == QuarterType.Fourth);

        }

        [TestMethod]
        public void DomainObject_Quarter_LimitsMinTimeRemaining()
        {
            var quarter = new Quarter(QuarterType.First);
            quarter.TimeRemaining -= 1000;

            Assert.AreEqual(quarter.TimeRemaining, 0);

        }

        [TestMethod]
        public void DomainObject_Quarter_LimitsMaxTimeRemaining()
        {
            var quarter = new Quarter(QuarterType.First);
            quarter.TimeRemaining += 10;

            Assert.AreEqual(quarter.TimeRemaining, 900);

        }

        [TestMethod]
        public void DomainObject_Quarter_SetsProperTimeRemaining()
        {
            var quarter = new Quarter(QuarterType.First);
            quarter.TimeRemaining -= 20;

            Assert.AreEqual(quarter.TimeRemaining, 880);

        }
    }
}