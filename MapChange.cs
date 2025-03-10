using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Threading;
using System.Linq;
using System.Threading;

using PRoCon.Core;
using PRoCon.Core.Plugin;
using PRoCon.Core.Plugin.Commands;
using PRoCon.Core.Players;
using PRoCon.Core.Players.Items;
using PRoCon.Core.Battlemap;
using PRoCon.Core.Maps;
using System.Net;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace PRoConEvents
{
    public class MapChange : PRoConPluginAPI, IPRoConPluginInterface
    {
        #region PluginSetup
        public string GetPluginName()
        {
            return "MapChange";
        }
        public string GetPluginVersion()
        {
            return "1.0";
        }
        public string GetPluginAuthor()
        {
            return "sacresful";
        }
        public string GetPluginWebsite()
        {
            return "github.com/sacresful/MapChange";
        }
        public string GetPluginDescription()
        {
            return "On-demand map change plugin for 2v2, 5v5 and 8v8";
        }

        public List<CPluginVariable> GetDisplayPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();
            lstReturn.Add(new CPluginVariable("Voting|Uservote prefix", this.m_strHosVotePrefix.GetType(), this.m_strHosVotePrefix));
            lstReturn.Add(new CPluginVariable("Permissions|RequirePerms", typeof(enumBoolYesNo), this.RequirePerms));
            lstReturn.Add(new CPluginVariable("Options|DefaultRounds", this.m_iDefaultRounds.GetType(), this.m_iDefaultRounds));
            lstReturn.Add(new CPluginVariable("Domination|GamemodeCounterDOM", this.m_iGamemodeCounterDOM.GetType(), this.m_iGamemodeCounterDOM));
            lstReturn.Add(new CPluginVariable("Domination|RoundtimeLimitDOM", this.m_iRoundtimeLimitDOM.GetType(), this.m_iRoundtimeLimitDOM));
            lstReturn.Add(new CPluginVariable("SquadObliteration|Preset", this.m_strPreset.GetType(), this.m_strPreset));
            lstReturn.Add(new CPluginVariable("SquadObliteration|GamemodeCounterSOB", this.m_iGamemodeCounterSOB.GetType(), this.m_iGamemodeCounterSOB));
            lstReturn.Add(new CPluginVariable("SquadObliteration|RoundtimeLimitSOB", this.m_iRoundtimeLimitSOB.GetType(), this.m_iRoundtimeLimitSOB));
            lstReturn.Add(new CPluginVariable("Xtras|Debug Level", this.m_iDebugLevel.GetType(), this.m_iDebugLevel));
            return lstReturn;

        }
        public List<CPluginVariable> GetPluginVariables()
        {
            List<CPluginVariable> lstReturn = new List<CPluginVariable>();
            lstReturn.Add(new CPluginVariable("Uservote prefix", this.m_strHosVotePrefix.GetType(), this.m_strHosVotePrefix));
            lstReturn.Add(new CPluginVariable("RequirePerms", typeof(enumBoolYesNo), this.RequirePerms));
            lstReturn.Add(new CPluginVariable("DefaultRounds", this.m_iDefaultRounds.GetType(), this.m_iDefaultRounds));
            lstReturn.Add(new CPluginVariable("GamemodeCounterDOM", this.m_iGamemodeCounterDOM.GetType(), this.m_iGamemodeCounterDOM));
            lstReturn.Add(new CPluginVariable("RoundtimeLimitDOM", this.m_iRoundtimeLimitDOM.GetType(), this.m_iRoundtimeLimitDOM));
            lstReturn.Add(new CPluginVariable("Preset", this.m_strPreset.GetType(), this.m_strPreset));
            lstReturn.Add(new CPluginVariable("GamemodeCounterSOB", this.m_iGamemodeCounterSOB.GetType(), this.m_iGamemodeCounterSOB));
            lstReturn.Add(new CPluginVariable("RoundtimeLimitSOB", this.m_iRoundtimeLimitSOB.GetType(), this.m_iRoundtimeLimitSOB));
            lstReturn.Add(new CPluginVariable("Debug Level", this.m_iDebugLevel.GetType(), this.m_iDebugLevel));
            return lstReturn;
        }
        public void SetPluginVariable(string strVariable, string strValue)
        {
            int iValue = 0;
            if (strVariable.CompareTo("Uservote prefix") == 0)
            {
                this.m_strHosVotePrefix = strValue;
            }
            else if (strVariable.CompareTo("DefaultRounds") == 0 && int.TryParse(strValue, out iValue) == true)
            {
                this.m_iRoundtimeLimitDOM = iValue;
            }
            else if (strVariable.CompareTo("RequirePerms") == 0 && Enum.IsDefined(typeof(enumBoolYesNo), strValue) == true)
            {
                this.RequirePerms = (enumBoolYesNo)Enum.Parse(typeof(enumBoolYesNo), strValue);
            }
            else if (strVariable.CompareTo("GamemodeCounterDOM") == 0 && int.TryParse(strValue, out iValue) == true)
            {
                this.m_iGamemodeCounterDOM = iValue;
            }
            else if (strVariable.CompareTo("RoundtimeLimitDOM") == 0 && int.TryParse(strValue, out iValue) == true)
            {
                this.m_iRoundtimeLimitDOM = iValue;
            }
            else if (strVariable.CompareTo("Preset") == 0)
            {
                this.m_strPreset = strValue;
            }
            else if (strVariable.CompareTo("GamemodeCounterSOB") == 0 && int.TryParse(strValue, out iValue) == true)
            {
                this.m_iGamemodeCounterSOB = iValue;
            }
            else if (strVariable.CompareTo("RoundtimeLimitSOB") == 0 && int.TryParse(strValue, out iValue) == true)
            {
                this.m_iRoundtimeLimitSOB = iValue;
            }
            else if (strVariable.CompareTo("Debug Level") == 0 && int.TryParse(strValue, out iValue) == true)
            {
                m_iDebugLevel = iValue;
            }
        }
        #endregion

        #region Procon Events
        public void OnPluginLoaded(string strHostName, string strPort, string strPRoConVersion)
        {
            this.m_strHostName = strHostName;
            this.m_strPort = strPort;
            this.m_strPRoConVersion = strPRoConVersion;
            this.RegisterEvents(this.GetType().Name, "OnServerInfo", "OnListPlayers", "OnPlayerLeft", "OnGlobalChat", "OnTeamChat", "OnSquadChat", "OnRoundOver", "OnRestartLevel");
        }
        public void OnPluginEnable()
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bMapChange ^2Enabled!");
            this.ExecuteCommand("procon.protected.send", "serverInfo");
        }
        public void OnPluginDisable()
        {
            this.ExecuteCommand("procon.protected.pluginconsole.write", "^bMapChange ^1Disabled =(");
        }
        public void OnGlobalChat(string speaker, string message)
        {
            ProcessChatMessage(speaker, message);
        }
        public void OnTeamChat(string speaker, string message, int teamId)
        {
            ProcessChatMessage(speaker, message);
        }
        public void OnSquadChat(string speaker, string message, int teamId, int squadId)
        {
            ProcessChatMessage(speaker, message);
        }
        public void OnListPlayers(List<CPlayerInfo> lstPlayers, CPlayerSubset cpsSubset)
        {
            this.CurrPlayerCount = lstPlayers.Count;
            foreach (CPlayerInfo player in lstPlayers)
            {
                m_players.UpdatePlayer(player);
            }
            WritePluginConsole("There are " + m_players.Count + " players in the db.", "Info", 5);
        }
        public void OnPlayerLeft(CPlayerInfo playerInfo)
        {
            m_players.Remove(playerInfo.SoldierName);
            WritePluginConsole(playerInfo.SoldierName + " removed from the db.", "Info", 5);
        }
        public void OnRestartLevel()
        {
            WritePluginConsole("Level Restarted.", "Work", 3);
            this.ExecuteCommand("procon.protected.send", "serverInfo");
        }
        public void OnRoundOver(int winningTeamId)
        {
            WritePluginConsole("Round Over.", "Work", 3);
            this.ExecuteCommand("procon.protected.send", "serverInfo");
        }

        public void OnRunNextLevel()
        {
            WritePluginConsole("Running nextmap.", "Work", 3);
            this.ExecuteCommand("procon.protected.send", "serverInfo");
        }

        public void OnServerInfo(CServerInfo csiServerInfo)
        {
            try
            {
                WritePluginConsole("ServerInfo updating.", "Info", 5);
                this.currentMap = csiServerInfo.Map;
                this.currentGameMode = csiServerInfo.GameMode;
                this.currentTotalRounds = csiServerInfo.CurrentRound;
                this.currentTotalRoundsInString = this.currentTotalRounds.ToString();
                WritePluginConsole("Current player count is: " + csiServerInfo.PlayerCount, "Info", 5);
                WritePluginConsole("Current map is: " + csiServerInfo.Map, "Info", 5);
                WritePluginConsole("Current game mode is: " + csiServerInfo.GameMode, "Info", 5);
                WritePluginConsole("Current number of total rounds is: " + csiServerInfo.TotalRounds, "Info", 5);
            }
            catch (Exception e)
            {
                WritePluginConsole("Exception caught in: OnServerInfo", "Error", 1);
                WritePluginConsole(e.Message, "Error", 1);
            }
        }

        #endregion

        private enumBoolYesNo RequirePerms = enumBoolYesNo.Yes;
        private int m_iDefaultRounds = 2;

        private string m_strHostName;
        private string m_strPort;
        private string m_strPRoConVersion;
        private string m_strHosVotePrefix = @"/";
        private int m_iDebugLevel = 3;

        private int m_iGamemodeCounterDOM = 67;
        private int m_iRoundtimeLimitDOM = 100;

        private string m_strPreset = "infantry";
        private int m_iGamemodeCounterSOB = 100;
        private int m_iRoundtimeLimitSOB = 75;

        private int CurrPlayerCount = 0;

        private string playerVotesInString;

        private string currentMap;
        private string currentGameMode;
        private int currentTotalRounds;
        private string currentTotalRoundsInString;

        private class Players
        {
            private List<Player> m_listPlayers = new List<Player>();

            public void UpdatePlayer(CPlayerInfo player)
            {
                bool updated = false;
                foreach (Player p in m_listPlayers)
                {
                    if (p.SoldierName == player.SoldierName)
                    {
                        p.TeamID = player.TeamID;
                        p.SquadID = player.SquadID;
                        updated = true;
                        break;
                    }
                }
                if (!updated)
                {
                    Add(player.SoldierName, player.TeamID, player.SquadID);
                }
            }

            public Player GetPlayer(string name)
            {
                foreach (Player p in m_listPlayers)
                {
                    if (p.SoldierName == name)
                    {
                        return p;
                    }
                }
                return null;
            }

            public List<Squad> GetNonvotedSquads()
            {
                List<Squad> squads = new List<Squad>();

                foreach (Player p in m_listPlayers)
                {
                    if (p.Vote == -1 && !(p.TeamID == 0 && p.SquadID == 0))
                    {
                        bool found = false;
                        foreach (Squad s in squads)
                        {
                            if (s.TeamID == p.TeamID && s.SquadID == p.SquadID)
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            squads.Add(new Squad(p.TeamID, p.SquadID));
                        }
                    }
                }

                squads.Sort();

                return squads;
            }

            public void Add(string name, int teamId, int squadId)
            {
                if (!this.Contains(name))
                {
                    m_listPlayers.Add(new Player(name, teamId, squadId));
                }
            }

            public void Remove(string name)
            {
                foreach (Player p in m_listPlayers)
                {
                    if (p.SoldierName == name)
                    {
                        m_listPlayers.Remove(p);
                    }
                }
            }

            public int Count
            {
                get { return m_listPlayers.Count; }
            }

            public bool Contains(string name)
            {
                foreach (Player p in m_listPlayers)
                {
                    if (p.SoldierName == name)
                    {
                        return true;
                    }
                }
                return false;
            }

            public class Player
            {
                private string m_Name = "";
                private int m_TeamId = -1;
                private int m_SquadId = -1;
                private int m_Vote = -1;

                public Player(string name, int teamId, int squadId)
                {
                    m_Name = name;
                    m_TeamId = teamId;
                    m_SquadId = squadId;
                    m_Vote = -1;
                }

                public string SoldierName
                {
                    get { return m_Name; }
                    private set { m_Name = value; }
                }

                public int TeamID
                {
                    get { return m_TeamId; }
                    set { m_TeamId = value; }
                }

                public int SquadID
                {
                    get { return m_SquadId; }
                    set { m_SquadId = value; }
                }

                public int Vote
                {
                    get { return m_Vote; }
                    set { m_Vote = value; }
                }

            }
            public class Squad : IComparable<Squad>
            {
                private int m_TeamId = -1;
                private int m_SquadId = -1;
                public Squad(int team, int squad)
                {
                    m_TeamId = team;
                    m_SquadId = squad;
                }

                public int TeamID
                {
                    get { return m_TeamId; }
                }

                public int SquadID
                {
                    get { return m_SquadId; }
                }

                public int CompareTo(Squad other)
                {
                    if (this.TeamID == other.TeamID)
                    {
                        return this.SquadID.CompareTo(other.SquadID);
                    }
                    return this.TeamID.CompareTo(other.TeamID);
                }
            }
        }
        private Players m_players = new Players();

        private Dictionary<string, int> playerVotes = new Dictionary<string, int>();
        private Dictionary<string, string> mappedMaps = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase){
            // Vanilla
            { "Dawnbreaker",  "mp_tremors" },
            { "Dragonvalley", "mp_valley" },
            { "Floodzone",    "mp_flooded" },
            { "Golmud",       "mp_journey" },
            { "Hainan",       "mp_resort" },
            { "Lancang",      "mp_damage" },
            { "Locker",       "mp_prison" },
            { "Paracel",      "mp_naval" },
            { "Rogue",        "mp_dish" },
            { "Siege",        "mp_siege" },
            { "Zavod",        "mp_abandoned" },
            // China Rising
            { "Silk",         "xp1_001" },
            { "Altai",        "xp1_002" },
            { "Guilin",       "xp1_003" },
            { "Dragonpass",   "xp1_004" },
            // Second Assault
            { "Caspian",      "mp_007" },
            { "Oman",         "xp1_002_Oman" },
            { "Firestorm",    "mp_012_Firestorm" },
            { "Metro",        "mp_subway" },
            // Naval Strike
            { "Lost",         "xp2_001"  },
            { "Nansha",       "xp2_002"  },
            { "Mortar",       "xp2_003"  },
            { "Wavebreaker",  "xp2_004"  },
            // Dragon's Teeth
            { "Pearl",        "xp3_Marketplace" },
            { "Propaganda",   "xp3_Propaganda" },
            { "Lumphini",     "xp3_UrbanGarden" },
            { "Sunken",       "xp3_Waterfront" },
            // Final Stand
            { "Whiteout",     "xp4_Arctic" },
            { "Hammerhead",   "xp4_SubBase" },
            { "Hangar21",     "xp4_Titan" },
            { "Karelia",      "xp4_WalkerFactory" }
        };
        private Dictionary<string, string> mappedGamemodes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase){
            { "AS", "AirSuperiority0" },
            { "CTF", "CaptureFlag0" },
            { "CA", "CarrierAssaultSmall0" },
            { "CAL", "CarrierAssaultLarge0" },
            { "CL", "Chainlink0" },
            { "CQS", "ConquestSmall0" },
            { "CQL", "ConquestLarge0" },
            { "DEF", "Elimination0" },
            { "DOM", "Domination0" },
            { "GM", "GunMaster0" },
            { "OB", "Obliteration" },
            { "R", "RushLarge0" },
            { "SDM", "SquadDeathMatch0" },
            { "TDM", "TeamDeathMatch0" },
            { "SOB", "SquadObliteration0" }
        };
        private void WritePluginConsole(string message, string tag, int level)
        {
            if (tag.ToLower() == "error")
            {
                tag = "^8" + tag;
            }
            else if (tag.ToLower() == "work")
            {
                tag = "^4" + tag;
            }
            else
            {
                tag = "^5" + tag;
            }
            string line = "^b[" + this.GetPluginName() + " " + this.GetPluginVersion() + "] " + tag + ": ^0^n" + message;

            if (this.m_iDebugLevel >= level)
            {
                this.ExecuteCommand("procon.protected.pluginconsole.write", line);
            }
            //if (this.m_iDebugLevel >= level)
            //{
            //    this.ExecuteCommand("procon.protected.chat.write", line);
            //}
        }
        private void DisplayListOfMaps()
        {
            List<string> listOfMaps = mappedMaps.Keys.ToList();
            int chunkSize = 11;

            for (int i = 0; i < listOfMaps.Count; i += chunkSize)
            {
                string mapChunk = string.Join(", ", listOfMaps.Skip(i).Take(chunkSize));
                this.ExecuteCommand("procon.protected.send", "admin.say", mapChunk, "all");
            }
        }
        private void DisplayListOfGamemodes()
        {
            List<string> listOfGamemodes = mappedGamemodes.Keys.ToList();

            string linkedGamemodes = string.Join(", ", listOfGamemodes);
            this.ExecuteCommand("procon.protected.send", "admin.say", linkedGamemodes, "all");
        }
        private void DisplayListOfFullGamemodes()
        {
            List<string> listOfFullGamemodes = mappedGamemodes.Values.ToList();
            int chunkSize = 3;

            for (int i = 0; i < listOfFullGamemodes.Count; i += chunkSize)
            {
                string gamemodeChunk = string.Join(", ", listOfFullGamemodes.Skip(i).Take(chunkSize));
                this.ExecuteCommand("procon.protected.send", "admin.say", gamemodeChunk, "all");
            }
        }
        private void SetOptions(string preset, string roundTimeLimit, string gameModeCounter, string soldierHealth, string vehicleSpawn)
        {
            this.ExecuteCommand("procon.protected.send", "vars.preset", preset);
            this.ExecuteCommand("procon.protected.send", "vars.roundTimeLimit", roundTimeLimit);
            this.ExecuteCommand("procon.protected.send", "vars.gameModeCounter", gameModeCounter);
            this.ExecuteCommand("procon.protected.send", "vars.soldierHealth", soldierHealth);
            this.ExecuteCommand("procon.protected.send", "vars.vehicleSpawnAllowed", vehicleSpawn);
        }
        private void ProcessChatMessage(string speaker, string message)
        {
            CPrivileges cpPlayerPrivs = this.GetAccountPrivileges(speaker);

            Match match;
            match = Regex.Match(message, @"^" + m_strHosVotePrefix + @"list$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                WritePluginConsole(speaker + " requested list", "Info", 3);
                DisplayListOfMaps();
                DisplayListOfGamemodes();
                return;
            }

            match = Regex.Match(message, @"^" + m_strHosVotePrefix + @"gamemodes$", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                DisplayListOfFullGamemodes();
                return;
            }

            match = Regex.Match(message, @"^" + m_strHosVotePrefix + @"restart$", RegexOptions.IgnoreCase);
            if (match.Success && cpPlayerPrivs.CanUseMapFunctions)
            {
                this.ExecuteCommand("procon.protected.send", "admin.say", "Try using /frestart", "player", speaker);
                return;
            }

            match = Regex.Match(message, @"^" + m_strHosVotePrefix + @"frestart$", RegexOptions.IgnoreCase);
            if (RequirePerms == enumBoolYesNo.Yes)
            {
                if (match.Success && cpPlayerPrivs.CanUseMapFunctions)
                {
                    this.ExecuteCommand("procon.protected.send", "admin.say", "Restarting round", "all", speaker);
                    this.ExecuteCommand("procon.protected.send", "mapList.restartRound");
                    return;
                }
                else if (match.Success) 
                {
                    this.ExecuteCommand("procon.protected.send", "admin.say", "You do not have enough privilages.", "player", speaker);
                    return;
                }
            } 
            else
            {
                if (match.Success)
                {
                    this.ExecuteCommand("procon.protected.send", "admin.say", "Restarting round", "all", speaker);
                    this.ExecuteCommand("procon.protected.send", "mapList.restartRound");
                    return;
                }
            }

            match = Regex.Match(message, @"^" + m_strHosVotePrefix + @"map\s*(\S*)$", RegexOptions.IgnoreCase);
            if (RequirePerms == enumBoolYesNo.Yes)
            {
                if (match.Success && cpPlayerPrivs.CanUseMapFunctions)
                {
                    string mapName = match.Groups[1].Value;
                    string gameMode = this.currentGameMode;

                    if (string.IsNullOrEmpty(gameMode))
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "Game mode has not been initialized yet.", "speaker", speaker);
                        this.ExecuteCommand("procon.protected.send", "serverInfo");
                        this.ExecuteCommand("procon.protected.send", "admin.say", "Wait a couple seconds before running command again.", "speaker", speaker);
                        return;  
                    }

                    if (string.IsNullOrEmpty(mapName))
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "All parameters (map name, game mode) must be from the /maps /gamemodes.", "player", speaker);
                        return;
                    }

                    if (!mappedMaps.ContainsKey(mapName))
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "Invalid map name. Please use a valid map from the list.", "player", speaker);
                        return;
                    }

                    mappedMaps.TryGetValue(mapName, out string internal_mapName);
                    string internal_gameMode = gameMode;
                    string actualMapName = mappedMaps.FirstOrDefault(x => x.Value == internal_mapName).Key;
                    string actualGameMode = mappedGamemodes.FirstOrDefault(x => x.Value == internal_gameMode).Key;
                    string m_strDefaultRounds = m_iDefaultRounds.ToString();

                    this.ExecuteCommand("procon.protected.send", "mapList.clear");
                    this.ExecuteCommand("procon.protected.send", "mapList.add", internal_mapName, internal_gameMode, "2");
                    this.ExecuteCommand("procon.protected.send", "admin.say", $"Changing map to: {actualMapName} {actualGameMode} {m_strDefaultRounds}", "all", speaker);
                    string m_iGamemodeCounterDOMString = m_iGamemodeCounterDOM.ToString();
                    string m_iRoundtimeLimitDOMString = m_iRoundtimeLimitDOM.ToString();
                    string m_iGamemodeCounterSOBString = m_iGamemodeCounterSOB.ToString();
                    string m_iRoundtimeLimitSOBString = m_iRoundtimeLimitSOB.ToString();

                    if (internal_gameMode == "Domination0")
                    {
                        SetOptions("normal", m_iRoundtimeLimitDOMString, m_iGamemodeCounterDOMString, "100", "false");
                    }
                    else if (internal_gameMode == "SquadObliteration0")
                    {
                        SetOptions(m_strPreset, m_iRoundtimeLimitSOBString, m_iGamemodeCounterSOBString, "100", "false");
                    }
                    else if (internal_gameMode == "ConquestSmall0")
                    {
                        SetOptions("normal", "67", "120", "100", "true");
                    }
                    else if (internal_gameMode == "AirSuperiority0")
                    {
                        SetOptions("normal", "9999999", "9999", "500", "true");
                    }
                    else
                    {
                        SetOptions("normal", "9999999", "9999", "500", "true");
                    }

                    this.ExecuteCommand("procon.protected.send", "mapList.runNextRound");

                    WritePluginConsole(speaker + " has changed the map", "Info", 3);
                    return;
                }
                else if (match.Success)
                {
                    this.ExecuteCommand("procon.protected.send", "admin.say", "You do not have enough privilages.", "player", speaker);
                    return;
                }
            }
            else
            {
                if (match.Success)
                {
                    string mapName = match.Groups[1].Value;
                    string gameMode = this.currentGameMode;

                    if (string.IsNullOrEmpty(gameMode))
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "Game mode has not been initialized yet.", "speaker", speaker);
                        this.ExecuteCommand("procon.protected.send", "serverInfo");
                        this.ExecuteCommand("procon.protected.send", "admin.say", "Wait a couple seconds before running command again.", "speaker", speaker);
                        return;
                    }

                    if (string.IsNullOrEmpty(mapName))
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "All parameters (map name, game mode) must be from the /maps /gamemodes.", "player", speaker);
                        return;
                    }

                    if (!mappedMaps.ContainsKey(mapName))
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "Invalid map name. Please use a valid map from the list.", "player", speaker);
                        return;
                    }

                    mappedMaps.TryGetValue(mapName, out string internal_mapName);
                    string internal_gameMode = gameMode;
                    string actualMapName = mappedMaps.FirstOrDefault(x => x.Value == internal_mapName).Key;
                    string actualGameMode = mappedGamemodes.FirstOrDefault(x => x.Value == internal_gameMode).Key;
                    string m_strDefaultRounds = m_iDefaultRounds.ToString();

                    this.ExecuteCommand("procon.protected.send", "mapList.clear");
                    this.ExecuteCommand("procon.protected.send", "mapList.add", internal_mapName, internal_gameMode, "2");
                    this.ExecuteCommand("procon.protected.send", "admin.say", $"Changing map to: {actualMapName} {actualGameMode} {m_strDefaultRounds}", "all", speaker);
                    string m_iGamemodeCounterDOMString = m_iGamemodeCounterDOM.ToString();
                    string m_iRoundtimeLimitDOMString = m_iRoundtimeLimitDOM.ToString();
                    string m_iGamemodeCounterSOBString = m_iGamemodeCounterSOB.ToString();
                    string m_iRoundtimeLimitSOBString = m_iRoundtimeLimitSOB.ToString();

                    if (internal_gameMode == "Domination0")
                    {
                        SetOptions("normal", m_iRoundtimeLimitDOMString, m_iGamemodeCounterDOMString, "100", "false");
                    }
                    else if (internal_gameMode == "SquadObliteration0")
                    {
                        SetOptions(m_strPreset, m_iRoundtimeLimitSOBString, m_iGamemodeCounterSOBString, "100", "false");
                    }
                    else if (internal_gameMode == "ConquestSmall0")
                    {
                        SetOptions("normal", "67", "120", "100", "true");
                    }
                    else if (internal_gameMode == "AirSuperiority0")
                    {
                        SetOptions("normal", "9999999", "9999", "500", "true");
                    }
                    else
                    {
                        SetOptions("normal", "9999999", "9999", "500", "true");
                    }
                    this.ExecuteCommand("procon.protected.send", "mapList.runNextRound");
                    WritePluginConsole(speaker + " has changed the map", "Info", 3);
                    return;
                }
            }

            match = Regex.Match(message, @"^" + m_strHosVotePrefix + @"map\s*(\S*)\s*(\S*)$", RegexOptions.IgnoreCase);
            if (RequirePerms == enumBoolYesNo.Yes) {
                if (match.Success && cpPlayerPrivs.CanUseMapFunctions)
                {
                    string mapName = match.Groups[1].Value;
                    string gameMode = match.Groups[2].Value;

                    if (string.IsNullOrEmpty(mapName) || string.IsNullOrEmpty(gameMode))
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "All parameters (map name, game mode) must be from the /maps /gamemodes.", "player", speaker);
                        return;
                    }

                    if (!mappedGamemodes.ContainsKey(gameMode))
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "Invalid gamemode. Please use a valid gamemode from the list.", "player", speaker);
                        return;
                    }

                    if (!mappedMaps.ContainsKey(mapName))
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "Invalid map name. Please use a valid map from the list.", "player", speaker);
                        return;
                    }

                    mappedMaps.TryGetValue(mapName, out string internal_mapName);
                    mappedGamemodes.TryGetValue(gameMode, out string internal_gameMode);
                    string actualMapName = mappedMaps.FirstOrDefault(x => x.Value == internal_mapName).Key;
                    string actualGameMode = mappedGamemodes.FirstOrDefault(x => x.Value == internal_gameMode).Key;
                    string m_strDefaultRounds = m_iDefaultRounds.ToString();

                    this.ExecuteCommand("procon.protected.send", "mapList.clear");
                    this.ExecuteCommand("procon.protected.send", "mapList.add", internal_mapName, internal_gameMode, m_strDefaultRounds);
                    this.ExecuteCommand("procon.protected.send", "admin.say", $"Changing map to: {actualMapName} {actualGameMode} {m_strDefaultRounds}", "all", speaker);
                    string m_iGamemodeCounterDOMString = m_iGamemodeCounterDOM.ToString();
                    string m_iRoundtimeLimitDOMString = m_iRoundtimeLimitDOM.ToString();
                    string m_iGamemodeCounterSOBString = m_iGamemodeCounterSOB.ToString();
                    string m_iRoundtimeLimitSOBString = m_iRoundtimeLimitSOB.ToString();

                    if (internal_gameMode == "Domination0")
                    {
                        SetOptions("normal", m_iRoundtimeLimitDOMString, m_iGamemodeCounterDOMString, "100", "false");
                    }
                    else if (internal_gameMode == "SquadObliteration0")
                    {
                        SetOptions(m_strPreset, m_iRoundtimeLimitSOBString, m_iGamemodeCounterSOBString, "100", "false");
                    }
                    else if (internal_gameMode == "ConquestSmall0")
                    {
                        SetOptions("normal", "67", "120", "100", "true");
                    }
                    else if (internal_gameMode == "AirSuperiority0")
                    {
                        SetOptions("normal", "9999999", "9999", "500", "true");
                    }
                    else
                    {
                        SetOptions("normal", "9999999", "9999", "500", "true");
                    }
                    this.ExecuteCommand("procon.protected.send", "mapList.runNextRound");
                    WritePluginConsole(speaker  + " has changed the map", "Info", 3);
                    return;
                }
                else if (match.Success)
                {
                    this.ExecuteCommand("procon.protected.send", "admin.say", "You do not have enough privilages.", "player", speaker);
                    return;
                }
            }
            else
            {
                if (match.Success)
                {
                    string mapName = match.Groups[1].Value;
                    string gameMode = match.Groups[2].Value;

                    if (string.IsNullOrEmpty(mapName) || string.IsNullOrEmpty(gameMode))
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "All parameters (map name, game mode) must be from the /maps /gamemodes.", "player", speaker);
                        return;
                    }

                    if (!mappedGamemodes.ContainsKey(gameMode))
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "Invalid gamemode. Please use a valid gamemode from the list.", "player", speaker);
                        return;
                    }

                    if (!mappedMaps.ContainsKey(mapName))
                    {
                        this.ExecuteCommand("procon.protected.send", "admin.say", "Invalid map name. Please use a valid map from the list.", "player", speaker);
                        return;
                    }

                    mappedMaps.TryGetValue(mapName, out string internal_mapName);
                    mappedGamemodes.TryGetValue(gameMode, out string internal_gameMode);
                    string actualMapName = mappedMaps.FirstOrDefault(x => x.Value == internal_mapName).Key;
                    string actualGameMode = mappedGamemodes.FirstOrDefault(x => x.Value == internal_gameMode).Key;
                    string m_strDefaultRounds = m_iDefaultRounds.ToString();

                    this.ExecuteCommand("procon.protected.send", "mapList.clear");
                    this.ExecuteCommand("procon.protected.send", "mapList.add", internal_mapName, internal_gameMode, "2");
                    this.ExecuteCommand("procon.protected.send", "admin.say", $"Changing map to: {actualMapName} {actualGameMode} {m_strDefaultRounds}", "all", speaker);
                    string m_iGamemodeCounterDOMString = m_iGamemodeCounterDOM.ToString();
                    string m_iRoundtimeLimitDOMString = m_iRoundtimeLimitDOM.ToString();
                    string m_iGamemodeCounterSOBString = m_iGamemodeCounterSOB.ToString();
                    string m_iRoundtimeLimitSOBString = m_iRoundtimeLimitSOB.ToString();

                    if (internal_gameMode == "Domination0")
                    {
                        SetOptions("normal", m_iRoundtimeLimitDOMString, m_iGamemodeCounterDOMString, "100", "false");
                    }
                    else if (internal_gameMode == "SquadObliteration0")
                    {
                        SetOptions(m_strPreset, m_iRoundtimeLimitSOBString, m_iGamemodeCounterSOBString, "100", "false");
                    }
                    else if (internal_gameMode == "ConquestSmall0")
                    {
                        SetOptions("normal", "67", "120", "100", "true");
                    }
                    else if (internal_gameMode == "AirSuperiority0")
                    {
                        SetOptions("normal", "9999999", "9999", "500", "true");
                    }
                    else
                    {
                        SetOptions("normal", "9999999", "9999", "500", "true");
                    }
                    this.ExecuteCommand("procon.protected.send", "mapList.runNextRound");
                    WritePluginConsole(speaker + " has changed the map", "Info", 3);
                    return;
                }
            }

            match = Regex.Match(message, @"" + m_strHosVotePrefix + @"tick\s*(.*)$", RegexOptions.IgnoreCase);
            if (RequirePerms == enumBoolYesNo.Yes)
            {
                if (match.Success && cpPlayerPrivs.CanUseMapFunctions)
                {
                    string hz = match.Groups[1].Value;
                    this.ExecuteCommand("procon.protected.send", "admin.say", "Changing tickrate to: " + hz, "all", speaker);
                    this.ExecuteCommand("procon.protected.send", "vars.OutHighFrequency", hz);
                    this.ExecuteCommand("procon.protected.send", "mapList.runNextRound");
                }
                else if (match.Success)
                {
                    this.ExecuteCommand("procon.protected.send", "admin.say", "You do not have enough privilages.", "player", speaker);
                }
            }
            else
            {
                if (match.Success)
                {
                    string hz = match.Groups[1].Value;
                    this.ExecuteCommand("procon.protected.send", "admin.say", "Changing tickrate to: " + hz, "all", speaker);
                    this.ExecuteCommand("procon.protected.send", "vars.OutHighFrequency", hz);
                    this.ExecuteCommand("procon.protected.send", "mapList.runNextRound");
                }
            }
            /*
            match = Regex.Match(message, @"" + m_strHosVotePrefix + @"factions\s+(\d+)\s+(\d+)", RegexOptions.IgnoreCase);
            if (RequirePerms == enumBoolYesNo.Yes)
            {
                if (match.Success && cpPlayerPrivs.CanUseMapFunctions)
                {
                    string hz = match.Groups[1].Value;
                    this.ExecuteCommand("procon.protected.send", "admin.say", "Changing tickrate to: " + hz, "all", speaker);
                    this.ExecuteCommand("procon.protected.send", "vars.OutHighFrequency", hz);
                    this.ExecuteCommand("procon.protected.send", "mapList.runNextRound");
                }
                else
                {
                    this.ExecuteCommand("procon.protected.send", "admin.say", "You do not have enough privilages.", "player", speaker);
                }
            }
            else
            {
                if (match.Success)
                {
                    string hz = match.Groups[1].Value;
                    this.ExecuteCommand("procon.protected.send", "admin.say", "Changing tickrate to: " + hz, "all", speaker);
                    this.ExecuteCommand("procon.protected.send", "vars.OutHighFrequency", hz);
                    this.ExecuteCommand("procon.protected.send", "mapList.runNextRound");
                }
            }
            */
        }
    }
}
