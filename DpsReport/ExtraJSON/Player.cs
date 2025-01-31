﻿using Newtonsoft.Json;

namespace PlenBotLogUploader.DpsReport.ExtraJson;

internal sealed class Player
{
    [JsonProperty("account")]
    internal string Account { get; set; }

    [JsonProperty("group")]
    internal int Group { get; set; }

    [JsonProperty("hasCommanderTag")]
    internal bool IsCommander { get; set; }

    [JsonProperty("profession")]
    internal string Profession { get; set; }

    internal string ProfessionShort
    {
        get
        {
            return Profession switch
            {
                "Soulbeast" => "Slb",
                "Specter" => "Spec",
                "Bladesworn" => "BS",
                "Spellbreaker" => "Spb",
                "Engineer" => "Engi",
                "Dragonhunter" => "DH",
                "Holosmith" => "Holo",
                "Willbender" => "WB",
                "Mechanist" => "Mech",
                "Virtuoso" => "Virt",
                "Firebrand" => "FB",
                _ => !string.IsNullOrWhiteSpace(Profession) && Profession.Length > 2 ? Profession[..3] : "",
            };
        }
    }

    [JsonProperty("friendlyNPC")]
    internal bool FriendlyNpc { get; set; }

    [JsonProperty("notInSquad")]
    internal bool NotInSquad { get; set; }

    [JsonProperty("support")]
    internal PlayerSupport[] Support { get; set; }

    [JsonProperty("name")]
    internal string Name { get; set; }

    [JsonProperty("dpsAll")]
    internal DpsAll[] DpsAll { get; set; }

    [JsonProperty("dpsTargets")]
    internal DpsTarget[][] DpsTargets { get; set; }

    [JsonProperty("statsTargets")]
    internal StatsTarget[][] StatsTargets { get; set; }

    [JsonProperty("statsAll")]
    internal StatsAll[] StatsAll { get; set; }

    [JsonProperty("defenses")]
    internal Defenses[] Defenses { get; set; }

    [JsonProperty("extHealingStats")]
    internal StatsHealing StatsHealing { get; set; }

    [JsonProperty("extBarrierStats")]
    internal StatsBarrier StatsBarrier { get; set; }
}
