using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Channels;
using System.Threading.Tasks;
using HtmlAgilityPack;

// Task is to crawl the data of every match and every goal of the match. I follow these steps:

// 1. Load the div of the site where all the matchdays are

// 2. All data are in the td of the table so extract the tds

// 3. Fetch the names of the clubs of every game and the goals in a List<Tuple<string, string, int, int>>

// 4. Export clubnames for database and scrape the game data from the website

// 5. Loop through the Tuple and compare the name of the database array with the tuple names. Create a new List<Tuple<int,int,int,int>>
// with the indexes of the clubs from the database, the matchday is the ((index of the game in the tuple + 1) / 8 with cuting + 1)

// 6. Loop through the new Tuple and create an Insert with the data in the format (GameId, HomeId, GuestId, Matchday, Season, GoalHome, GoalGuest)
// which filled with tuple vars looks like this
// Insert into TGame values (GameId, Tuple.Item1, Tuple.Item2, (Tuple[Tuple.GetIndexOf(Tuple)] + 1) / 8 + 1, '2020/2021', Tuple.Item3, Tuple.Item4), (...);

// 7. Create a export the insert in a file called InsertAllTGameDataFromCsWebcrawler.sql

// 8. Load the div code of the goals via the url pattern https://www.weltfussball.de/spielbericht/bundesliga-2020-2021-{club1 with - for spaces}-{club2 with -}/
// and a loop over the frist Tuple with match data with the names of clubs

// 9. Fish for the td which contains playername and goalminute

// 10. Create a List<Tuple<string, int>> with this data

// 11. Load playernames from database

// 12. Compare names and save in a new tuple List<Tuple<int,int,int>> with item1 = index player, item2 = goalminute, item 3 = gameid

// 13. Create the Insert with the data like this: Insert into TGoal (PlayerId, GameMinute, GameId), (...);

// 14. Export the Insert as InsertAllTGoalsFromCSWebcrawler.sql

// 15. Copy the two Inserts in my SQLFootballDatabase Program and test in Database


// Generate folder for gernerated *.sql files

string relPath = "../../../SQL-Files";
string aPath = Path.GetFullPath(relPath);

Directory.CreateDirectory(aPath);

string season = "2020/2021";



using (HttpClient c = new())
{
    // Step 1
    var url = "https://www.weltfussball.de/alle_spiele/bundesliga-2020-2021/";
    string htmltext = await c.GetStringAsync(url);
    int start = htmltext.IndexOf(@"<th colspan=""7""><a href=""/spielplan/bundesliga-2020-2021-spieltag/1/"">1. Spieltag</a></th>");
    htmltext = htmltext.Substring(start);
    int end = htmltext.IndexOf("</table>");
    htmltext = htmltext.Substring(0, end + 8);

    List<string> list = new List<string>();


    // Step 2

    // 2.1 Get the tds

    while ((start = htmltext.IndexOf("<td ")) != -1 && (end = htmltext.IndexOf("</td>")) != -1)
    {
        string s = htmltext.Substring(start, end - start + 5);
        list.Add(s);
        htmltext = htmltext.Substring(end + 5);

        start = htmltext.IndexOf("<td ");
        end = htmltext.IndexOf("</td>");
    }

    // 2.2 Get the <a>s for the teams

    List<string> listAllTeams = new();

    list.ForEach(s =>
    {
        string s2 = string.Empty;
        if ((start = s.IndexOf(@"href=""/teams/")) != -1) // Escape char will not be counted in string.length!
        {
            s2 = s.Substring(start + 13);
            s2 = s2.Substring(0, s2.IndexOf('/'));

            listAllTeams.Add(s2); // This List now contains the teams with xx-xx as in the url needed, but with doubles

        }

    });

    // Step 3

    // Tuple for the games in format teamHome+teamGuest goalHome goalGuest

    List<Tuple<string, int, int>> listGames = new();
    string s3 = string.Empty;

    listAllTeams.ForEach(s =>
    {

        if (listGames.Count < 306)
        {
            if (s3 == string.Empty)
            {
                s3 = s;
            }
            else
            {
                s3 += $"+{s}";
                listGames.Add(new(s3, -1, -1));
                s3 = string.Empty;
            }
        }
    });



    // Set listAllTeams with the clubs only one time

    listAllTeams = listAllTeams.Distinct().ToList();

    Console.WriteLine("All Teams \n");
    listAllTeams.ForEach(s => Console.WriteLine(s.Replace('-', ' ')));
    Console.WriteLine("-------------------");
    Console.WriteLine();

    // Get all players

    // Format Fname, Lname, clubid, gametime, season

    List<Tuple<string, string, int, string, string>> lPlayers = new();

    foreach (string item in listAllTeams)
    {
        url = $"https://www.weltfussball.de/team_einsaetze/{item}/bundesliga-2020-2021/nach-minuten/";

        htmltext = await c.GetStringAsync(url);

        HtmlDocument doc = new HtmlDocument();

        doc.LoadHtml(htmltext);

        var tab = doc.DocumentNode.SelectNodes(".//table").ToList().Find(x => x.InnerHtml.Contains("spieleinsatz.gif"));

        var tds = tab.SelectNodes(".//td").ToList();

        tds = tds.FindAll(t => tds.IndexOf(t) % 10 < 2);

        tds.RemoveRange(0, 2);

        int i = 0;
        string clubIndex = $"{listAllTeams.IndexOf(item).ToString().PadLeft(3)} club ";

        int counterPlayer = 0;


        string[] name = default;

        foreach (HtmlNode n in tds)
        {

            if (++i % 2 != 0)
            {
                name = n.InnerText.Split(' ');

                if (name.Length > 2)
                {
                    string[] marker = name;

                    name = new string[2];
                    name[0] = marker[0] + " " + marker[1];
                    name[1] = marker[2];
                }
                else if (name.Length == 1)
                {
                    name = new[] { name[0], "noname" };
                }
            }
            else
            {
                int minutes = Convert.ToInt32(n.InnerText.Replace("'", "").Replace('-', '0'));


                lPlayers.Add(new(name[0], name[1], Convert.ToInt32(minutes), item, season));
                Console.WriteLine(clubIndex + name[0].PadRight(20) + name[1].PadRight(20) + minutes.ToString().PadLeft(4) + season.PadLeft(11));
                clubIndex = $"{listAllTeams.IndexOf(item).ToString().PadLeft(3)} club ";
                counterPlayer++;
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Count player: {counterPlayer}");
        Console.WriteLine();

    }

    string clubNow = string.Empty;
    string clubMarker = string.Empty;

    using (StreamWriter w = new("../../../SQL-Files/InsertTPlayers.sql"))
    {
        foreach (Tuple<string, string, int, string, string> playerSet in lPlayers)
        {
            string sql = string.Empty;
            clubNow = playerSet.Item4;

            if (clubNow != clubMarker)
            {
                clubMarker = clubNow;

                w.WriteLine("");
                w.WriteLine($"-- {playerSet.Item4}");
                w.WriteLine();
                w.WriteLine();

            }

            sql += $"INSERT INTO TPlayer (FirstName, LastName, ClubId) SELECT '{playerSet.Item1}', '{playerSet.Item2}',  (SELECT ClubId FROM TClub WHERE ClubOnlineName = '{playerSet.Item4}') " +
                   $"WHERE NOT EXISTS (SELECT 1 FROM TPlayer WHERE LOWER(FirstName) = '{playerSet.Item1}' AND LOWER(LastName) = '{playerSet.Item2}');";



            w.WriteLine(sql);
        }
    }



    // 4. Export clubnames for database

    using (StreamWriter w = new("../../../SQL-Files/clubOnlineNames.sql", false))
    {

        int counter = 1;

        listAllTeams.ForEach(s =>
        {
            string sql = string.Empty;
            sql = $"UPDATE TClub SET ClubOnlineName = '{s}' WHERE ClubID = {counter++};";
            w.WriteLine(sql);
        });
    }

    // scrap gamedata from website

    List<Tuple<int, int>> gameData = new();

    list.ForEach(td =>
    {
        if ((start = td.IndexOf("title=\"Spielschema ")) != -1)
        {
            string substr = td.Substring(start + 19);
            start = substr.IndexOf('>');
            end = substr.IndexOf('(');
            substr = substr.Substring(start + 1, end - start - 1);

            string[] astr = substr.Split(':');

            gameData.Add(new(Convert.ToInt32(astr[0]), Convert.ToInt32(astr[1])));
        }
    });

    // 5. and 6. Merge club name with game data

    for (int i = 0; i < listGames.Count; i++)
    {
        listGames[i] = new(listGames[i].Item1, gameData[i].Item1, gameData[i].Item2);
    }

    Console.WriteLine();
    Console.WriteLine("All Games \n");
    listGames.ForEach(g =>
    {
        string[] s3 = g.Item1.Replace('-', ' ').Split('+');
        Console.WriteLine($"HomeClub: {s3[0].PadRight(25)} GuestClub: {s3[1].PadRight(25)} HomeGoal: {g.Item2 + "".PadLeft(3)} GuestGoal: {g.Item3 + "".PadLeft(3)}");

    });

    Console.WriteLine("-------------------");

    // 7. Create the insert cmd 

    using (StreamWriter w = new("../../../SQL-Files/InsertGames.sql", false))
    {
        string sql = string.Empty;

        int counter = 1;

        listGames.ForEach(g =>
        {
            string[] clubNamesSplit = g.Item1.Split('+');
            w.WriteLine($"INSERT INTO TGame (HomeId, GuestId, MatchDay, Season) VALUES ((Select ClubId FROM TClub WHERE ClubOnlineName = '{clubNamesSplit[0]}'),(SELECT ClubId FROM TClub WHERE ClubOnlineName = '{clubNamesSplit[1]}'),{counter / 8 + 1},'2020/2021');");
            counter++;
        });

    }

    // 8. Access url for goal data

    string gamedate = string.Empty;

    Console.WriteLine();
    Console.WriteLine("All Goals: ");
    Console.WriteLine();

    int gameCount = 0;
    int gamedayMarker = 0;
    int gameDay = 0;
    int goalCounter = 0;
    int goalDayCounter = 0;

    using (StreamWriter w = new("../../../SQL-Files/InsertTGoals.sql"))
    {

        foreach (Tuple<string, int, int> game in listGames)
        {
            string[] clubs = game.Item1.Split('+');

            url = $"https://www.weltfussball.de/spielbericht/bundesliga-2020-2021-{clubs[0]}-{clubs[1]}/";

            htmltext = await c.GetStringAsync(url);

            HtmlDocument doc = new();
            doc.LoadHtml(htmltext);

            var tables = doc.DocumentNode.SelectNodes("//table");

            var table = tables.ToList().Find(t => t.InnerText.Contains("Tore"));

            var trs = table.SelectNodes(".//tr");


            gameDay = gameCount / 9 + 1;

            gameCount++;

            if (gamedayMarker != gameDay)
            {
                Console.WriteLine();

                if (goalDayCounter != 0)
                    Console.WriteLine($"Goals od the gameday: {goalDayCounter}");

                goalDayCounter = 0;

                Console.WriteLine("-----");

                Console.WriteLine($"{gameDay}. Gameday");

                Console.WriteLine();

                gamedayMarker = gameDay;
            }


            trs.ToList().ForEach(t =>
            {
                if (t.InnerText.Contains(':'))
                {
                    goalCounter++;
                    goalDayCounter++;
                    var tds = t.ChildNodes;
                    var aas = t.SelectNodes(".//a");

                    string input = tds[3].InnerText.Split("&nbsp")[0];

                    // Zerlegen Sie den String bei ' / '
                    string[] parts = input.Contains('/') ? input.Split(" / ") : input.Split('(');

                 

                    // Zerlegen Sie den ersten Teil bei Leerzeichen
                    string[] nameAndTime = parts[0].Split(' ');

                    string name = string.Join(" ", nameAndTime, 0, nameAndTime.Length - 1).Replace("\n", "");
                    string time = nameAndTime.ToList().Find(s => s.Contains('.') && char.IsDigit(s[s.Length - 2])).TrimEnd('.');
                    string shotType = parts.Length > 1 ? parts[1] : "";

                    // Remove if numbers is in string

                    if (name.Any(x => char.IsDigit(x)))
                    {

                        for (int i = 0; i < name.Length; i++)
                        {
                            if (!char.IsLetter(name[i]) && name[i] != ' ')
                            {
                                name = name.Remove(i);
                            }
                        }
                    }

                    if (shotType.Contains("dir. Freisto&szlig;"))
                    {
                        shotType = "Freistoss";
                    }
                    else if(shotType.ToLower().Contains("elfmeter"))
                    {
                        shotType = "Elfmeter";
                    }
                    else if(shotType.Contains("Eigentor"))
                    {
                        shotType = "Eigentor";
                    }



                    Tuple<int, string, int, string> tu = new(gameDay, name, Convert.ToInt32(time), shotType);

                    string sql = $"INSERT INTO TGOAL (PlayerId, MatchDay, GameMinute, GameId, Penalty, FreeKick, OwnGoal) VALUES " +
                $"((SELECT PlayerId FROM TPlayer where CONCAT(FirstName, ' ', Lastname) = '{tu.Item2}'  OR FirstName = '{tu.Item2}' AND LastName = 'noname'), {tu.Item1}, {tu.Item3}, {gameCount}, {tu.Item4.ToLower() == "elfmeter"}, {tu.Item4.ToLower() == "freistoss"}, {tu.Item4.ToLower() == "eigentor"});";

                    w.WriteLine(sql);


                    Console.WriteLine(tu.Item2.ToString().PadLeft(25) + " " + tu.Item3.ToString().PadLeft(3) + " " + tu.Item4 + "".PadLeft(25));
                }

            });

        }

    }


    Console.WriteLine();
    Console.WriteLine("-----");
    Console.WriteLine($"Goals Total: {goalCounter}");


















}
