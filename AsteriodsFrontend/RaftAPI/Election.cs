﻿using System.Timers;

namespace RaftElection;

public enum State { Follower, Candidate, Leader, Unhealthy }

public class Election
{
    private readonly HttpClient httpClient;

    public Guid NodeId { get; set; }
    public State CurrentState { get; set; }
    public int CurrentTerm { get; set; }
    public int LogIndex { get; set; }
    public Guid CurrentLeader { get; set; }
    public List<string> Urls { get; set; }

    public int timer;
    private readonly Random random = new();
    private readonly static List<Election> ListOfAllNodes = [];
    private readonly static Dictionary<Guid, (int, Guid)> Votes = [];
    private readonly object lockObject = new object();
    private readonly ILogger<Election> logger;
    private Dictionary<string, (string, int)> logDict = [];
    private readonly System.Timers.Timer startTimer;


    public Election(List<string> urls, ILogger<Election> logger)
    {
        Console.WriteLine("made it to the election node");
        this.httpClient = new HttpClient();

        //set id
        NodeId = Guid.NewGuid();
        //everyone starts as a follower
        CurrentState = State.Follower;
        //starting term 0
        CurrentTerm = 0;
        LogIndex = 0;
        //add the current node to the static list
        ListOfAllNodes.Add(this);
        //set timers
        ResetTimers();
        Urls = urls;
        this.logger = logger;

        startTimer = new System.Timers.Timer(timer);
        startTimer.Elapsed += CheckState;
        startTimer.Start();
    }

    public Election(ILogger<Election> logger)
    {
        this.httpClient = new HttpClient();
        //set id
        NodeId = Guid.NewGuid();
        //everyone starts as a follower
        CurrentState = State.Follower;
        //starting term 0
        CurrentTerm = 0;
        LogIndex = 0;
        //add the current node to the static list
        ListOfAllNodes.Add(this);
        //set timers
        ResetTimers();
        this.logger = logger;
    }

    public bool LogToFile(string key, string value)
    {
        //lock (lockObject)
        //{
        //    string fileName = $"{NodeId}.log";
        //    System.IO.File.AppendAllText(fileName, $"{DateTime.Now}: {message}\n");
        //}
        try
        {
            LogIndex = LogIndex + 1;
            logDict[key] = (value, LogIndex);
            logger.LogInformation($"{DateTime.Now}: value:{value}, logIndex: {LogIndex}");
            return true;
        }
        catch { return false; }
    }

    public void ResetTimers()
    {
        timer = random.Next(300, 800);
    }
    public void CheckState(object? sender, ElapsedEventArgs e)
    {
        startTimer.Stop();
        Console.WriteLine("started in check state");
        CheckWhatToDoWithTheStateAsync().Wait();
        //while (true)
        //{
        //    CheckWhatToDoWithTheStateAsync();
        //}
    }


    public async Task CheckWhatToDoWithTheStateAsync()
    {
        while (true)
        {
            Console.WriteLine("In check what to do with the state");
            Thread.Sleep(timer);
            switch (CurrentState)
            {
                case State.Follower:
                    CurrentState = State.Candidate;
                    break;
                case State.Candidate:
                    await StartAnElectionAsync();
                    break;
                case State.Leader:
                    await SendOutHeartbeatAsync("regular heartbeat", "-1", NodeId);
                    break;
            }
            Console.WriteLine($"current state {CurrentState}");
        }
    }

    public async Task StartAnElectionAsync()
    {
        Console.WriteLine("Started an election");
        //increase the term 
        CurrentTerm++;
        //current node votes for themself
        int voteCount = 0;
        //record the votes

        foreach (var nodeUrl in Urls)
        {
            bool Voted = false;
            try
            {
                // Remove any unwanted characters from the URL, such as double quotes
                var cleanNodeUrl = nodeUrl.Trim('"');

                Console.WriteLine($"Attempting to get votes from node: {cleanNodeUrl}");
                var fullUrl = $"{cleanNodeUrl}/Node/getVotes/{NodeId}/{CurrentTerm}";
                Console.WriteLine($"Full URL: {fullUrl}");

                Voted = await httpClient.GetFromJsonAsync<bool>(fullUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting votes from node: {nodeUrl}, Error: {ex.Message}");
            }

            Console.WriteLine($"vote {Voted}");
            if (Voted)
            {
                voteCount++;
            }
            if (voteCount >= Urls.Count() / 2 + 1)
            {
                CurrentState = State.Leader;
                logger.LogInformation($"{NodeId} is the leader for term {CurrentTerm}");
                //LogToFile($"{NodeId} is the leader for term {CurrentTerm}");
                await SendOutHeartbeatAsync("election ended", "-1", NodeId);
                return;
            }

        }
    }

    public async Task<int> SendOutHeartbeatAsync(string key, string value, Guid CurrentLeader)
    {
        Console.WriteLine($"Got a heart beat from {CurrentLeader}");
        int success = 0;
        foreach (var nodes in Urls)
        {
            try
            {
                var node = nodes.Trim('"');
                if (node != null)
                {
                    var beat = new HeartbeatInfo()
                    {
                        CurrentTerm = CurrentTerm,
                        LeaderId = CurrentLeader,
                        Value = value,
                        key = key,
                    };

                    var response = await httpClient.GetFromJsonAsync<bool>($"{node}/Node/heartbeat/from/{beat}");

                    if (response)
                    {
                        success++;
                    }

                    ResetTimers();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        return success;
    }

    public bool VoteForTheCurrentTerm(int term, Guid CandidateId)
    {
        lock (lockObject)
        {
            try
            {
                var checkVote = Votes[NodeId];
                if (term > checkVote.Item1)
                {
                    Votes[NodeId] = (term, CandidateId);
                    logger.LogInformation($"{NodeId} voted for {CandidateId} on term {term}");
                    //LogToFile($"{NodeId} voted for {CandidateId} on term {term}");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Votes[NodeId] = (term, CandidateId);
                logger.LogInformation($"{NodeId} voted for {CandidateId} on term {term}");
                //LogToFile($"{NodeId} voted for {CandidateId} on term {term}");
                return true;
            }
        }
    }

    public (string?, int?) EventualGet(string key)
    {
        if (logDict.TryGetValue(key, out var value))
        {
            return value;
        }
        return (null, null);
    }

    public async Task<(string?, int?)> StrongGetAsync(string key)
    {
        int leaderInt = 0;
        foreach (var node in Urls)
        {
            var nodes = node.Trim('"');

            var SameLeader = await httpClient.GetFromJsonAsync<bool>($"{nodes}/Node/compareleader/{NodeId}");
            if (SameLeader)
            {
                leaderInt++;
            }
        }
        if (leaderInt >= (Urls.Count() / 2 + 1))
        {
            if (logDict.TryGetValue(key, out var value))
            {
                return value;
            }
            else
                return (null, null);
        }
        else
        {
            return (null, null);
        }

    }

    public bool CompareVersionAndSwap(string key, int expectedIndex, string newValue)
    {
        if (logDict.TryGetValue(key, out var value) && value.Item2 <= expectedIndex)
        {
            logDict[key] = (newValue, expectedIndex);
            return true;
        }
        else { return false; }

        //if (CurrentState != State.Leader)
        //{
        //    return false;
        //}

        //LogToFile($"term:{CurrentTerm} command:{value}");

        //var nodesResponseCount = SendOutHeartbeat(value.ToString(), NodeId);

        //if (nodesResponseCount + 1 >= ListOfAllNodes.Count() / 2 + 1)
        //{
        //    logDict[key] = (value, CurrentTerm);
        //    return true;
        //}
        //else { return false; }
    }

    public async Task<bool> WriteAsync(string key, string value)
    {
        if (CurrentState != State.Leader)
        {
            return false;
        }

        //LogToFile($"term:{CurrentTerm} command:{value}");
        logger.LogInformation($"term:{CurrentTerm} command:{value}");

        var nodesResponseCount = await SendOutHeartbeatAsync(key, value, NodeId);

        if (nodesResponseCount + 1 >= Urls.Count() / 2 + 1)
        {
            LogIndex = LogIndex + 1;
            logDict[key] = (value, LogIndex);
            return true;
        }
        else { return false; }
    }



    public static void MarkNodesUnhealthy(int count)
    {
        for (int i = 1; i <= count; i++)
        {
            ListOfAllNodes[i].CurrentState = State.Unhealthy;
        }
    }

    public static void MarkNodesHealthy(int count)
    {
        for (int i = 1; i <= count; i++)
        {
            ListOfAllNodes[i].CurrentState = State.Follower;
        }
    }

    public static void ClearListForTestingPurpose()
    {
        ListOfAllNodes.Clear();
    }

    public static List<Election> GetTheListofnodes()
    {
        return ListOfAllNodes;
    }

    public static Dictionary<Guid, (int, Guid)> GetTheListofVotes()
    {
        return Votes;
    }

    public void SetUrlsForTests()
    {
        Urls = new List<string>();
        foreach (var n in ListOfAllNodes)
        {
            Urls.Add(n.NodeId.ToString());
        }
    }


}

public class HeartbeatInfo
{
    public Guid LeaderId { get; set; }
    public string Value { get; set; }
    public string key { get; set; }
    public int CurrentTerm { get; set; }
}

public class SwapInfo
{
    public string Key { get; set; }
    public int ExpectedIndex { get; set; }
    public string NewValue { get; set; }
}

public class KeyValue
{
    public string key { get; set; }
    public string value { get; set; }
}

public class Product
{
    public string ProductItem { get; set; }
    public int Quanity { get; set; }
    public double Cost { get; set; }
}

public class Cart
{
    public Guid OrderId { get; set; }
    public string Username { get; set; }
    public List<Product> ShoppingItems { get; set; } = new();
}

public class LogInfo
{
    public string Value { get; set; }
    public int LogIndex { get; set; }
}

public class PendingOrders
{
    public Guid ProcessorId { get; set; }
    public Guid OrderId { get; set; }
    public string Status { get; set; }
    public int StatusIndex { get; set; }
    public Cart? OrderInfo { get; set; }
    public int OrderInfoIndex { get; set; }
    public string username { get; set; }
}
