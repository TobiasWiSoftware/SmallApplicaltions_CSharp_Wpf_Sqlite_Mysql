﻿using System;
using System.Net.Http;
using System.Runtime.Intrinsics.Arm;
using System.Threading.Channels;
using System.Threading.Tasks;

// Task is to crawl the data of every match and every goal of the match. I follow these steps:

// 1. Load the div of the site where all the matchdays are

// 2. All data are in the td of the table so extract the tds

// 3. Fetch the names of the clubs of every game and the goals in a List<Tuple<string, string, int, int>>

// 4. Read all club names from the database and map in an array

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


using (HttpClient c = new())
{
    // Step 1
    var url = "https://www.weltfussball.de/alle_spiele/bundesliga-2020-2021/";
    string text = await c.GetStringAsync(url);
    int start = text.IndexOf(@"<th colspan=""7""><a href=""/spielplan/bundesliga-2020-2021-spieltag/1/"">1. Spieltag</a></th>");
    text = text.Substring(start);
    int end = text.IndexOf("</table>");
    text = text.Substring(0, end + 8);

    List<string> list = new List<string>();


    // Step 2

    // 2.1 Get the tds

    while ((start = text.IndexOf("<td ")) != -1 && (end = text.IndexOf("</td>")) != -1)
    {
        string s = text.Substring(start, end - start + 5);
        list.Add(s);
        text = text.Substring(end + 5);

        start = text.IndexOf("<td ");
        end = text.IndexOf("</td>");
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

    Console.WriteLine();
    Console.WriteLine("All Games \n");
    listGames.ForEach(g =>
    {
        string[] s3 = g.Item1.Replace('-', ' ').Split('+');
        Console.WriteLine($"HomeClub: {s3[0].PadRight(25)} GuestClub: {s3[1].PadRight(25)} HomeGoal: {g.Item2 + "".PadLeft(3)} GuestGoal: {g.Item3 + "".PadLeft(3)}");

    });

}
