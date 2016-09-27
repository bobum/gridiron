using System.Collections.Generic;
using Newtonsoft.Json;

namespace DomainObjects
{
    public class Penalty
    {
        public PenaltyNames Name { get; set; }
        public float Odds { get; set; }
        public float Home { get; set; }
        public float Away { get; set; }
    }

    public static class Penalties
    {
        public static List<Penalty> List { get; set; }

        static Penalties()
        {
            string penalties =
                @"[{'Name':'OffensiveHolding','Odds':0.01900018,'Home':0.51,'Away':0.49},
                    {'Name':'FalseStart','Odds':0.01554800,'Home':0.53,'Away':0.47},
                    {'Name':'DefensivePassInterference','Odds':0.00640367,'Home':0.48,'Away':0.52},
                    {'Name':'UnnecessaryRoughness','Odds':0.00621920,'Home':0.54,'Away':0.46},
                    {'Name':'DefensiveHolding','Odds':0.00600838,'Home':0.53,'Away':0.47},
                    {'Name':'DefensiveOffside','Odds':0.00469075,'Home':0.55,'Away':0.45},
                    {'Name':'NeutralZoneInfraction','Odds':0.00419005,'Home':0.33,'Away':0.67},
                    {'Name':'DelayofGame','Odds':0.00400559,'Home':0.34,'Away':0.66},
                    {'Name':'IllegalBlockAbovetheWaist','Odds':0.00339948,'Home':0.53,'Away':0.47},
                    {'Name':'IllegalUseofHands','Odds':0.00313595,'Home':0.57,'Away':0.43},
                    {'Name':'OffensivePassInterference','Odds':0.00274066,'Home':0.56,'Away':0.44},
                    {'Name':'FaceMask15Yards','Odds':0.00266161,'Home':0.45,'Away':0.55},
                    {'Name':'RoughingthePasser','Odds':0.00268796,'Home':0.44,'Away':0.56},
                    {'Name':'UnsportsmanlikeConduct','Odds':0.00229267,'Home':0.31,'Away':0.69},
                    {'Name':'IllegalContact','Odds':0.00163386,'Home':0.37,'Away':0.63},
                    {'Name':'IllegalFormation','Odds':0.00163386,'Home':0.5,'Away':0.5},
                    {'Name':'Defensive12OnField','Odds':0.00137033,'Home':0.46,'Away':0.54},
                    {'Name':'Encroachment','Odds':0.00115951,'Home':0.36,'Away':0.64},
                    {'Name':'IntentionalGrounding','Odds':0.00089599,'Home':0.62,'Away':0.38},
                    {'Name':'IllegalShift','Odds':0.00084328,'Home':0.47,'Away':0.53},
                    {'Name':'Taunting','Odds':0.00050070,'Home':0.58,'Away':0.42},
                    {'Name':'IneligibleDownfieldPass','Odds':0.00044799,'Home':0.41,'Away':0.59},
                    {'Name':'OffsideonFreeKick','Odds':0.00042164,'Home':0.56,'Away':0.44},
                    {'Name':'ChopBlock','Odds':0.00042164,'Home':0.5,'Away':0.5},
                    {'Name':'PlayerOutofBoundsonPunt','Odds':0.00034258,'Home':0.62,'Away':0.38},
                    {'Name':'RunningIntotheKicker','Odds':0.00034258,'Home':0.38,'Away':0.62},
                    {'Name':'HorseCollarTackle','Odds':0.00036894,'Home':0.36,'Away':0.64},
                    {'Name':'IllegalMotion','Odds':0.00031623,'Home':0.42,'Away':0.58},
                    {'Name':'Tripping','Odds':0.00028988,'Home':0.45,'Away':0.55},
                    {'Name':'Offensive12OnField','Odds':0.00018447,'Home':0.29,'Away':0.71},
                    {'Name':'IllegalSubstitution','Odds':0.00021082,'Home':0.25,'Away':0.75},
                    {'Name':'PersonalFoul','Odds':0.00023717,'Home':0.89,'Away':0.11},
                    {'Name':'IneligibleDownfieldKick','Odds':0.00023717,'Home':0.22,'Away':0.78},
                    {'Name':'IllegalForwardPass','Odds':0.00023717,'Home':0.67,'Away':0.33},
                    {'Name':'Clipping','Odds':0.00021082,'Home':0.63,'Away':0.37},
                    {'Name':'IllegalBlindsideBlock','Odds':0.00021082,'Home':0.63,'Away':0.37},
                    {'Name':'DefensiveDelayofGame','Odds':0.00015812,'Home':0.5,'Away':0.5},
                    {'Name':'IllegalTouchPass','Odds':0.00015812,'Home':0.33,'Away':0.67},
                    {'Name':'FairCatchInterference','Odds':0.00015812,'Home':0.17,'Away':0.83},
                    {'Name':'OffensiveOffside','Odds':0.00010541,'Home':0,'Away':1},
                    {'Name':'IllegalTouchKick','Odds':0.00005271,'Home':0,'Away':1},
                    {'Name':'LowBlock','Odds':0.00007906,'Home':0.67,'Away':0.33},
                    {'Name':'IllegalPeelback','Odds':0.00005271,'Home':0,'Away':1},
                    {'Name':'Leaping','Odds':0.00007906,'Home':0.67,'Away':0.33},
                    {'Name':'RoughingtheKicker','Odds':0.00005271,'Home':0.5,'Away':0.5},
                    {'Name':'IllegalCrackback','Odds':0.00010541,'Home':0.75,'Away':0.25},
                    {'Name':'InvalidFairCatchSignal','Odds':0.00005271,'Home':0.5,'Away':0.5},
                    {'Name':'Disqualification','Odds':0.00007906,'Home':0.33,'Away':0.67},
                    {'Name':'InterferencewithOpportunitytoCatch','Odds':0.00007906,'Home':0.67,'Away':0.33},
                    {'Name':'Leverage','Odds':0.00002635,'Home':0,'Away':1},
                    {'Name':'NoPenalty','Odds':0,'Home':0,'Away':0}]";

            List = new List<Penalty>(JsonConvert.DeserializeObject<List<Penalty>>(penalties));
        }
    }

    public enum PenaltyNames
    {
        OffensiveHolding,
        FalseStart,
        DefensivePassInterference,
        UnnecessaryRoughness,
        DefensiveHolding,
        DefensiveOffside,
        NeutralZoneInfraction,
        DelayofGame,
        IllegalBlockAbovetheWaist,
        IllegalUseofHands,
        OffensivePassInterference,
        FaceMask15Yards,
        RoughingthePasser,
        UnsportsmanlikeConduct,
        IllegalContact,
        IllegalFormation,
        Defensive12OnField,
        Encroachment,
        IntentionalGrounding,
        IllegalShift,
        Taunting,
        IneligibleDownfieldPass,
        OffsideonFreeKick,
        ChopBlock,
        PlayerOutofBoundsonPunt,
        RunningIntotheKicker,
        HorseCollarTackle,
        IllegalMotion,
        Tripping,
        Offensive12OnField,
        IllegalSubstitution,
        PersonalFoul,
        IneligibleDownfieldKick,
        IllegalForwardPass,
        Clipping,
        IllegalBlindsideBlock,
        DefensiveDelayofGame,
        IllegalTouchPass,
        FairCatchInterference,
        OffensiveOffside,
        IllegalTouchKick,
        LowBlock,
        IllegalPeelback,
        Leaping,
        RoughingtheKicker,
        IllegalCrackback,
        InvalidFairCatchSignal,
        Disqualification,
        InterferencewithOpportunitytoCatch,
        Leverage,
        NoPenalty
    }
}