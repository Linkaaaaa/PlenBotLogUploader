﻿using Newtonsoft.Json;
using PlenBotLogUploader.DpsReport.ExtraJson;
using PlenBotLogUploader.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PlenBotLogUploader.DpsReport;

internal sealed class DpsReportJsonExtraJson
{
    [JsonProperty("eliteInsightsVersion")]
    internal string EliteInsightsVersion { get; set; }

    [JsonProperty("recordedBy")]
    internal string RecordedBy { get; set; }

    [JsonProperty("recordedAccountBy")]
    internal string RecordedByAccountName { get; set; }

    [JsonProperty("timeStartStd")]
    internal DateTime TimeStart { get; set; }

    [JsonProperty("timeEndStd")]
    internal DateTime TimeEnd { get; set; }

    [JsonProperty("duration")]
    internal string Duration { get; set; }

    [JsonProperty("durationMs")]
    internal ulong DurationMs { get; set; }

    [JsonProperty("success")]
    internal bool Succcess { get; set; }

    [JsonProperty("triggerID")]
    internal int TriggerId { get; set; }

    [JsonProperty("fightName")]
    internal string FightName { get; set; }

    [JsonProperty("gw2Build")]
    internal ulong GameBuild { get; set; }

    [JsonProperty("fightIcon")]
    internal string FightIcon { get; set; }

    [JsonProperty("isCM")]
    internal bool IsCm { get; set; }

    [JsonProperty("isLegendaryCM")]
    internal bool IsLegendaryCm { get; set; }

    [JsonProperty("targets")]
    internal Target[] Targets { get; set; }

    [JsonProperty("players")]
    internal Player[] Players { get; set; }

    [JsonProperty("phases")]
    internal Phase[] Phases { get; set; }

    [JsonProperty("logErrors")]
    internal string[] LogErrors { get; set; }

    private List<Target> GetTargetsByIndex(List<int> indexes)
    {
        var result = new List<Target>();
        foreach (var targetIndex in indexes.AsSpan())
        {
            result.Add(Targets[targetIndex]);
        }
        return result;
    }

    internal Dictionary<Player, int> GetPlayerTargetDps()
    {
        var dict = new Dictionary<Player, int>();
        foreach (var player in Players.AsSpan())
        {
            var damage = player.DpsTargets
                .Select(x => x[0].Dps)
                .Sum();
            dict.Add(player, damage);
        }
        return dict;
    }

    private Phase GetLastNonBreakbarPhase()
    {
        Phase lastNonBreakbarPhase = null;
        foreach (var phase in Phases.AsSpan())
        {
            if (!phase.BreakbarPhase)
            {
                lastNonBreakbarPhase = phase;
            }
        }
        return lastNonBreakbarPhase ?? Phases[0];
    }

    internal string GetLastPhaseName() => GetLastNonBreakbarPhase().Name ?? "Unknown phase";

    internal string GetLastPhaseTargets()
    {
        var lastPhase = GetLastNonBreakbarPhase();
        if (lastPhase is null)
        {
            return string.Empty;
        }
        var resultTargetTexts = new List<string>();
        var blockingTargets = lastPhase.GetBlockingTargets();
        if (blockingTargets.Count != 0)
        {
            var blockingTargetList = GetTargetsByIndex(blockingTargets);
            foreach (var target in blockingTargetList.AsSpan())
            {
                resultTargetTexts.Add($"{target.Name} - {Math.Round(100 - target.HealthPercentBurned, 2)}%");
            }
            return string.Join(" | ", resultTargetTexts);
        }
        var mainTargets = lastPhase.GetMainTargets();
        var mainTargetList = GetTargetsByIndex(mainTargets);
        if (mainTargets.Count == 0)
        {
            return string.Empty;
        }
        foreach (var target in mainTargetList.AsSpan())
        {
            resultTargetTexts.Add($"{target.Name} - {Math.Round(100 - target.HealthPercentBurned, 2)}%");
        }
        return string.Join(" | ", resultTargetTexts);
    }
}
