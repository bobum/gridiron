﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainObjects;
using Newtonsoft.Json;

namespace UnitTestProject
{
    public class Teams
    {
        public Team HomeTeam { get; set; }
        public Team VisitorTeam { get; set; }

        public Teams()
        {
//            string json = @"{
            //  'Name': 'Bad Boys',
            //  'ReleaseDate': '1995-4-7T00:00:00',
            //  'Genres': [
            //    'Action',
            //    'Comedy'
            //  ]
            //}";
//
//            Movie m = JsonConvert.DeserializeObject<Movie>(json);
//
//            string name = m.Name;

            string homeJson =
                @"[{'number':23,'lastName':'Alford ','firstName':'Robert','position':'CB','height':'5-10','weight':186,'age':27,'exp':'4','college':'Southeastern Louisiana'},
                    { 'number':37,'lastName':'Allen ','firstName':'Ricardo','position':'S','height':'5-9','weight':186,'age':24,'exp':'2','college':'Purdue'},
                    { 'number':95,'lastName':'Babineaux ','firstName':'Jonathan','position':'DT','height':'6-2','weight':300,'age':34,'exp':'12','college':'Iowa'},
                    { 'number':44,'lastName':'Beasley ','firstName':'Vic','position':'OLB','height':'6-3','weight':246,'age':24,'exp':'2','college':'Clemson'},
                    { 'number':5,'lastName':'Bosher ','firstName':'Matt','position':'P','height':'6-0','weight':208,'age':28,'exp':'6','college':'Miami (Fla.)'},
                    { 'number':3,'lastName':'Bryant ','firstName':'Matt','position':'K','height':'5-9','weight':203,'age':41,'exp':'15','college':'Baylor'},
                    { 'number':59,'lastName':'Campbell ','firstName':'De\'Vondre','position':'OLB','height':'6 - 4','weight':232,'age':23,'exp':'0','college':'Minnesota'},
                    { 'number':65,'lastName':'Chester ','firstName':'Chris','position':'G','height':'6-3','weight':303,'age':33,'exp':'11','college':'Oklahoma'},
                    { 'number':99,'lastName':'Clayborn ','firstName':'Adrian','position':'DE','height':'6-3','weight':280,'age':28,'exp':'6','college':'Iowa'},
                    { 'number':26,'lastName':'Coleman ','firstName':'Tevin','position':'RB','height':'6-1','weight':210,'age':23,'exp':'2','college':'Indiana'},
                    { 'number':76,'lastName':'Compton ','firstName':'Tom','position':'T','height':'6-5','weight':308,'age':27,'exp':'4','college':'South Dakota'},
                    { 'number':42,'lastName':'DiMarco ','firstName':'Patrick','position':'FB','height':'6-1','weight':234,'age':27,'exp':'5','college':'South Carolina'},
                    { 'number':24,'lastName':'Freeman ','firstName':'Devonta','position':'RB','height':'5-8','weight':206,'age':24,'exp':'3','college':'Florida State'},
                    { 'number':93,'lastName':'Freeney ','firstName':'Dwight','position':'DE','height':'6-1','weight':268,'age':36,'exp':'14','college':'Syracuse'},
                    { 'number':18,'lastName':'Gabriel ','firstName':'Taylor','position':'WR','height':'5-8','weight':167,'age':25,'exp':'3','college':'Abilene Christian'},
                    { 'number':63,'lastName':'Garland ','firstName':'Ben','position':'G','height':'6-5','weight':308,'age':28,'exp':'3','college':'Air Force'},
                    { 'number':38,'lastName':'Goldson ','firstName':'Dashon','position':'FS','height':'6-2','weight':200,'age':32,'exp':'9','college':'Washington'},
                    { 'number':29,'lastName':'Goodwin ','firstName':'C.J.','position':'CB','height':'6-3','weight':190,'age':26,'exp':'1','college':'California (PA)'},
                    { 'number':77,'lastName':'Hageman ','firstName':'Ra\'Shede','position':'DT','height':'6 - 6','weight':318,'age':26,'exp':'3','college':'Minnesota'},
                    { 'number':16,'lastName':'Hardy ','firstName':'Justin','position':'WR','height':'5-10','weight':192,'age':24,'exp':'2','college':'East Carolina'},
                    { 'number':47,'lastName':'Harris ','firstName':'Josh','position':'LS','height':'6-1','weight':224,'age':27,'exp':'5','college':'Auburn'},
                    { 'number':81,'lastName':'Hooper ','firstName':'Austin','position':'TE','height':'6-4','weight':248,'age':21,'exp':'0','college':'Stanford'},
                    { 'number':36,'lastName':'Ishmael ','firstName':'Kemal','position':'S','height':'6-0','weight':206,'age':25,'exp':'4','college':'Central Florida'},
                    { 'number':94,'lastName':'Jackson ','firstName':'Tyson','position':'DE','height':'6-4','weight':296,'age':30,'exp':'8','college':'LSU'},
                    { 'number':97,'lastName':'Jarrett ','firstName':'Grady','position':'DT','height':'6-0','weight':305,'age':23,'exp':'2','college':'Clemson'},
                    { 'number':45,'lastName':'Jones ','firstName':'Deion','position':'LB','height':'6-1','weight':222,'age':21,'exp':'0','college':'LSU'},
                    { 'number':11,'lastName':'Jones ','firstName':'Julio','position':'WR','height':'6-3','weight':220,'age':27,'exp':'6','college':'Alabama'},
                    { 'number':67,'lastName':'Levitre ','firstName':'Andy','position':'G','height':'6-2','weight':303,'age':30,'exp':'8','college':'Oregon State'},
                    { 'number':51,'lastName':'Mack ','firstName':'Alex','position':'C','height':'6-4','weight':311,'age':30,'exp':'8','college':'California'},
                    { 'number':70,'lastName':'Matthews ','firstName':'Jake','position':'T','height':'6-5','weight':305,'age':24,'exp':'3','college':'Texas A&M'},
                    { 'number':22,'lastName':'Neal ','firstName':'Keanu','position':'S','height':'6-0','weight':211,'age':21,'exp':'0','college':'Florida'},
                    { 'number':82,'lastName':'Perkins ','firstName':'Joshua','position':'TE','height':'6-4','weight':227,'age':23,'exp':'0','college':'Washington'},
                    { 'number':68,'lastName':'Person ','firstName':'Mike','position':'G','height':'6-4','weight':300,'age':28,'exp':'3','college':'Montana State'},
                    { 'number':34,'lastName':'Poole ','firstName':'Brian','position':'S','height':'5-10','weight':211,'age':23,'exp':'0','college':'Florida'},
                    { 'number':50,'lastName':'Reed ','firstName':'Brooks','position':'OLB','height':'6-3','weight':254,'age':29,'exp':'6','college':'Arizona'},
                    { 'number':53,'lastName':'Reynolds ','firstName':'LaRoy','position':'LB','height':'6-1','weight':240,'age':25,'exp':'4','college':'Virginia'},
                    { 'number':19,'lastName':'Robinson ','firstName':'Aldrick','position':'WR','height':'5-10','weight':187,'age':28,'exp':'4','college':'Southern Methodist'},
                    { 'number':2,'lastName':'Ryan ','firstName':'Matt','position':'QB','height':'6-4','weight':217,'age':31,'exp':'9','college':'Boston College'},
                    { 'number':12,'lastName':'Sanu ','firstName':'Mohamed','position':'WR','height':'6-2','weight':210,'age':27,'exp':'5','college':'Rutgers'},
                    { 'number':8,'lastName':'Schaub ','firstName':'Matt','position':'QB','height':'6-6','weight':245,'age':35,'exp':'13','college':'Virginia'},
                    { 'number':54,'lastName':'Schofield ','firstName':'O\'Brien','position':'OLB','height':'6 - 3','weight':242,'age':29,'exp':'6','college':'Wisconsin'},
                    { 'number':73,'lastName':'Schraeder ','firstName':'Ryan','position':'T','height':'6-7','weight':300,'age':28,'exp':'4','college':'Valdosta State'},
                    { 'number':71,'lastName':'Schweitzer ','firstName':'Wes','position':'G','height':'6-5','weight':314,'age':23,'exp':'0','college':'San Jose State'},
                    { 'number':90,'lastName':'Shelby ','firstName':'Derrick','position':'DE','height':'6-2','weight':280,'age':27,'exp':'5','college':'Utah'},
                    { 'number':83,'lastName':'Tamme ','firstName':'Jacob','position':'TE','height':'6-3','weight':230,'age':31,'exp':'9','college':'Kentucky'},
                    { 'number':27,'lastName':'Therezie ','firstName':'Robenson','position':'S','height':'5-9','weight':212,'age':25,'exp':'2','college':'Auburn'},
                    { 'number':80,'lastName':'Toilolo ','firstName':'Levine','position':'TE','height':'6-8','weight':265,'age':25,'exp':'4','college':'Stanford'},
                    { 'number':21,'lastName':'Trufant ','firstName':'Desmond','position':'CB','height':'6-0','weight':190,'age':26,'exp':'4','college':'Washington'},
                    { 'number':91,'lastName':'Upshaw ','firstName':'Courtney','position':'OLB','height':'6-2','weight':272,'age':26,'exp':'5','college':'Alabama'},
                    { 'number':56,'lastName':'Weatherspoon ','firstName':'Sean','position':'LB','height':'6-2','weight':244,'age':28,'exp':'6','college':'Missouri'},
                    { 'number':41,'lastName':'Wheeler ','firstName':'Philip','position':'LB','height':'6-2','weight':245,'age':31,'exp':'9','college':'Georgia Tech'},
                    { 'number':55,'lastName':'Worrilow ','firstName':'Paul','position':'LB','height':'6-0','weight':230,'age':26,'exp':'4','college':'Delaware'}]";
            HomeTeam = new Team {Players = new List<Player>(JsonConvert.DeserializeObject<List<Player>>(homeJson)) };

            var visitorJson =
                @"[{'number':17,'lastName':'Agholor','firstName':'Nelson','position':'WR','height':'6-0','weight':198,'age':23,'exp':2,'college':'USC'},
                    {'number':94,'lastName':'Allen','firstName':'Beau','position':'DT','height':'6-3','weight':327,'age':24,'exp':3,'college':'Wisconsin'},
                    {'number':68,'lastName':'Andrews','firstName':'Josh','position':'C','height':'6-2','weight':311,'age':25,'exp':2,'college':'Oregon State'},
                    {'number':76,'lastName':'Barbre','firstName':'Allen','position':'G','height':'6-4','weight':310,'age':32,'exp':9,'college':'Missouri Southern State'},
                    {'number':34,'lastName':'Barner','firstName':'Kenjon','position':'RB','height':'5-9','weight':195,'age':27,'exp':3,'college':'Oregon'},
                    {'number':98,'lastName':'Barwin','firstName':'Connor','position':'DE','height':'6-4','weight':264,'age':29,'exp':8,'college':'Cincinnati'},
                    {'number':53,'lastName':'Bradham','firstName':'Nigel','position':'LB','height':'6-2','weight':241,'age':27,'exp':5,'college':'Florida State'},
                    {'number':56,'lastName':'Braman','firstName':'Bryan','position':'DE','height':'6-5','weight':241,'age':29,'exp':6,'college':'West Texas A&M'},
                    {'number':79,'lastName':'Brooks','firstName':'Brandon','position':'G','height':'6-5','weight':335,'age':27,'exp':5,'college':'Miami (Ohio)'},
                    {'number':33,'lastName':'Brooks','firstName':'Ron','position':'CB','height':'5-10','weight':190,'age':27,'exp':5,'college':'LSU'},
                    {'number':29,'lastName':'Brooks','firstName':'Terrence','position':'S','height':'5-11','weight':200,'age':24,'exp':3,'college':'Florida State'},
                    {'number':47,'lastName':'Burton','firstName':'Trey','position':'TE','height':'6-3','weight':235,'age':24,'exp':3,'college':'Florida'},
                    {'number':22,'lastName':'Carroll','firstName':'Nolan','position':'CB','height':'6-1','weight':205,'age':29,'exp':7,'college':'Maryland'},
                    {'number':87,'lastName':'Celek','firstName':'Brent','position':'TE','height':'6-4','weight':255,'age':31,'exp':10,'college':'Cincinnati'},
                    {'number':91,'lastName':'Cox','firstName':'Fletcher','position':'DT','height':'6-4','weight':310,'age':25,'exp':5,'college':'Mississippi State'},
                    {'number':75,'lastName':'Curry','firstName':'Vinny','position':'DE','height':'6-3','weight':279,'age':28,'exp':5,'college':'Marshall'},
                    {'number':10,'lastName':'Daniel','firstName':'Chase','position':'QB','height':'6-0','weight':225,'age':29,'exp':8,'college':'Missouri'},
                    {'number':46,'lastName':'Dorenbos','firstName':'Jon','position':'LS','height':'6-0','weight':250,'age':36,'exp':14,'college':'UTEP'},
                    {'number':86,'lastName':'Ertz','firstName':'Zach','position':'TE','height':'6-5','weight':250,'age':25,'exp':4,'college':'Stanford'},
                    {'number':52,'lastName':'Goode','firstName':'Najee','position':'LB','height':'6-0','weight':244,'age':27,'exp':5,'college':'West Virginia'},
                    {'number':69,'lastName':'Gordon','firstName':'Dillon','position':'G','height':'6-4','weight':322,'age':23,'exp':0,'college':'LSU'},
                    {'number':55,'lastName':'Graham','firstName':'Brandon','position':'DE','height':'6-2','weight':265,'age':28,'exp':7,'college':'Michigan'},
                    {'number':18,'lastName':'Green-Beckham','firstName':'Dorial','position':'WR','height':'6-5','weight':237,'age':23,'exp':2,'college':'Oklahoma'},
                    {'number':54,'lastName':'Grugier-Hill','firstName':'Kamu','position':'LB','height':'6-2','weight':220,'age':22,'exp':0,'college':'Eastern Illinois'},
                    {'number':58,'lastName':'Hicks','firstName':'Jordan','position':'LB','height':'6-1','weight':236,'age':24,'exp':2,'college':'Texas'},
                    {'number':13,'lastName':'Huff','firstName':'Josh','position':'WR','height':'5-11','weight':206,'age':24,'exp':3,'college':'Oregon'},
                    {'number':27,'lastName':'Jenkins','firstName':'Malcolm','position':'S','height':'6-0','weight':204,'age':28,'exp':8,'college':'Ohio State'},
                    {'number':65,'lastName':'Johnson','firstName':'Lane','position':'T','height':'6-6','weight':317,'age':26,'exp':4,'college':'Oklahoma'},
                    {'number':8,'lastName':'Jones','firstName':'Donnie','position':'P','height':'6-2','weight':221,'age':36,'exp':13,'college':'LSU'},
                    {'number':62,'lastName':'Kelce','firstName':'Jason','position':'C','height':'6-3','weight':295,'age':28,'exp':6,'college':'Cincinnati'},
                    {'number':95,'lastName':'Kendricks','firstName':'Mychal','position':'LB','height':'6-0','weight':240,'age':25,'exp':5,'college':'California'},
                    {'number':96,'lastName':'Logan','firstName':'Bennie','position':'DT','height':'6-2','weight':315,'age':26,'exp':4,'college':'LSU'},
                    {'number':42,'lastName':'Maragos','firstName':'Chris','position':'S','height':'5-10','weight':200,'age':29,'exp':7,'college':'Wisconsin'},
                    {'number':24,'lastName':'Mathews','firstName':'Ryan','position':'RB','height':'6-0','weight':220,'age':28,'exp':7,'college':'Fresno State'},
                    {'number':81,'lastName':'Matthews','firstName':'Jordan','position':'WR','height':'6-3','weight':212,'age':24,'exp':3,'college':'Vanderbilt'},
                    {'number':21,'lastName':'McKelvin','firstName':'Leodis','position':'CB','height':'5-10','weight':185,'age':31,'exp':9,'college':'Troy'},
                    {'number':23,'lastName':'McLeod','firstName':'Rodney','position':'S','height':'5-10','weight':195,'age':26,'exp':5,'college':'Virginia'},
                    {'number':51,'lastName':'Means','firstName':'Steven','position':'DE','height':'6-3','weight':263,'age':26,'exp':2,'college':'Buffalo'},
                    {'number':31,'lastName':'Mills','firstName':'Jalen','position':'CB','height':'6-0','weight':196,'age':22,'exp':0,'college':'LSU'},
                    {'number':71,'lastName':'Peters','firstName':'Jason','position':'T','height':'6-4','weight':328,'age':34,'exp':13,'college':'Arkansas'},
                    {'number':73,'lastName':'Seumalo','firstName':'Isaac','position':'G','height':'6-4','weight':303,'age':22,'exp':0,'college':'Oregon State'},
                    {'number':28,'lastName':'Smallwood','firstName':'Wendell','position':'RB','height':'5-10','weight':208,'age':22,'exp':0,'college':'West Virginia'},
                    {'number':90,'lastName':'Smith','firstName':'Marcus','position':'DE','height':'6-3','weight':251,'age':24,'exp':3,'college':'Louisville'},
                    {'number':43,'lastName':'Sproles','firstName':'Darren','position':'RB','height':'5-6','weight':190,'age':33,'exp':12,'college':'Kansas State'},
                    {'number':6,'lastName':'Sturgis','firstName':'Caleb','position':'K','height':'5-9','weight':192,'age':27,'exp':4,'college':'Florida'},
                    {'number':64,'lastName':'Tobin','firstName':'Matt','position':'G','height':'6-6','weight':290,'age':26,'exp':4,'college':'Iowa'},
                    {'number':16,'lastName':'Treggs','firstName':'Bryce','position':'WR','height':'6-0','weight':185,'age':22,'exp':0,'college':'California'},
                    {'number':50,'lastName':'Tulloch','firstName':'Stephen','position':'LB','height':'5-11','weight':245,'age':31,'exp':11,'college':'N.C. State'},
                    {'number':97,'lastName':'Vaeao','firstName':'Destiny','position':'DT','height':'6-4','weight':299,'age':22,'exp':0,'college':'Washington State'},
                    {'number':72,'lastName':'Vaitai','firstName':'Halapoulivaati','position':'T','height':'6-6','weight':315,'age':23,'exp':0,'college':'TCU'},
                    {'number':26,'lastName':'Watkins','firstName':'Jaylen','position':'S','height':'5-11','weight':194,'age':24,'exp':2,'college':'Florida'},
                    {'number':11,'lastName':'Wentz','firstName':'Carson','position':'QB','height':'6-5','weight':237,'age':23,'exp':0,'college':'North Dakota State'},
                    {'number':61,'lastName':'Wisniewski','firstName':'Stefen','position':'G','height':'6-3','weight':305,'age':27,'exp':6,'college':'Penn State'}]";
            VisitorTeam = new Team { Players = new List<Player>(JsonConvert.DeserializeObject<List<Player>>(visitorJson)) };
        }
    }
}
