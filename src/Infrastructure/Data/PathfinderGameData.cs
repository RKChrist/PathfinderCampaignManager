using PathfinderCampaignManager.Domain.Entities.Pathfinder;

namespace PathfinderCampaignManager.Infrastructure.Data;

public static class PathfinderGameData
{
    public static class Ancestries
    {
        public static readonly Dictionary<string, PfAncestry> CoreAncestriesData = new()
        {
            ["human"] = new PfAncestry
            {
                Id = "human",
                Name = "Human",
                Description = "As unpredictable and varied as any of Golarion's peoples, humans have exceptional drive and the capacity to endure and expand. Though many civilizations thrived before humanity rose to prominence, humans have built some of the greatest and the most terrible societies throughout the course of history, and today they are the most populous people in the realms around the Inner Sea.",
                HitPoints = 8,
                Size = "Medium",
                Speeds = new Dictionary<string, int> { ["land"] = 25 },
                AbilityBoosts = new List<string> { "Free", "Free" },
                AbilityFlaws = new List<string>(),
                FreeAbilityBoosts = new List<string> { "Any", "Any" },
                Languages = new List<string> { "Common" },
                AdditionalLanguages = 1,
                Senses = new List<string>(),
                SenseRanges = new Dictionary<string, int>(),
                Traits = new List<string> { "Human", "Humanoid" },
                Features = new List<PfAncestryFeature>(),
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "skilled-heritage", Name = "Skilled Heritage", Description = "Your ingenuity allows you to train in a wide variety of skills. You become trained in one skill of your choice. At 5th level, you become an expert in the chosen skill.", Traits = new List<string>() },
                    new() { Id = "versatile-heritage", Name = "Versatile Heritage", Description = "Humanity's versatility and ambition have fueled its ascendance to be the most common ancestry in most nations throughout the world. Select a general feat for which you meet the prerequisites (as with your ancestry feat, you can select this general feat at any point during character creation).", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "adapted-cantrip", "cooperative-nature", "general-training", "haughty-obstinacy", "natural-ambition", "natural-skill", "unconventional-weaponry" },
                    [5] = new List<string> { "adaptive-adept", "clever-improviser", "cooperative-soul", "incredible-improvisation", "multitalented" },
                    [9] = new List<string> { "clever-gambit", "heroic-presence", "incredible-luck", "multitalented" },
                    [13] = new List<string> { "bounce-back", "incredible-investiture", "stubborn-persistence" },
                    [17] = new List<string> { "heroic-recovery", "incredible-resolve", "natural-one" }
                },
                Source = "Core Rulebook",
                Rarity = "Common"
            },

            ["elf"] = new PfAncestry
            {
                Id = "elf",
                Name = "Elf",
                Description = "As an ancient people, elves have seen great change and have the perspective that can come only from watching the arc of history. After leaving the world in ancient times, they returned to a changed land, and they still struggle to reclaim their ancestral homes, most notably from terrible demons that have invaded parts of their lands. To some, the elves are objects of awe—graceful and beautiful, with immense talent and knowledge. Among themselves, however, the elves place far more importance on personal freedom than on living up to these ideals.",
                HitPoints = 6,
                Size = "Medium",
                Speeds = new Dictionary<string, int> { ["land"] = 30 },
                AbilityBoosts = new List<string> { "Dexterity", "Intelligence", "Free" },
                AbilityFlaws = new List<string> { "Constitution" },
                FreeAbilityBoosts = new List<string> { "Any" },
                Languages = new List<string> { "Common", "Elven" },
                AdditionalLanguages = 0,
                Senses = new List<string> { "Low-Light Vision" },
                SenseRanges = new Dictionary<string, int>(),
                Traits = new List<string> { "Elf", "Humanoid" },
                Features = new List<PfAncestryFeature>(),
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "ancient-elf", Name = "Ancient Elf", Description = "In your long life, you've dabbled in many paths and many styles. Choose a class other than your own. You gain the multiclass dedication feat for that class, even though you don't meet its level prerequisite.", Traits = new List<string>() },
                    new() { Id = "arctic-elf", Name = "Arctic Elf", Description = "You dwell deep in the frozen north and have gained incredible resilience against cold environments, granting you cold resistance equal to half your level (minimum 1). You treat environmental cold effects as if they were one step less extreme (incredible cold becomes extreme, extreme cold becomes severe, and so on).", Traits = new List<string>() },
                    new() { Id = "cavern-elf", Name = "Cavern Elf", Description = "You were born or spent many years in underground tunnels or caverns where light is scarce. You gain darkvision instead of low-light vision.", Traits = new List<string>() },
                    new() { Id = "seer-elf", Name = "Seer Elf", Description = "You have an inborn ability to detect and understand magical phenomena. You can cast the detect magic cantrip as an arcane innate spell at will. A cantrip is heightened to a spell level equal to half your level rounded up.", Traits = new List<string>() },
                    new() { Id = "whisper-elf", Name = "Whisper Elf", Description = "Your ears are finely tuned, able to detect even the slightest whispers of sound. As long as you can hear normally, you can use the Seek action to sense undetected creatures in a 60-foot cone instead of a 30-foot cone. You also gain a +2 circumstance bonus to locate undetected creatures that you could hear within 30 feet with a Seek action.", Traits = new List<string>() },
                    new() { Id = "woodland-elf", Name = "Woodland Elf", Description = "You're adapted to life in the forest or the deep jungle, with an innate familiarity with the flora and fauna of your jungle or forest home. When using Survival to subsist, if you roll a failure, you get a success instead, and if you roll a success, you get a critical success instead.", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "ancestral-longevity", "elven-lore", "elven-weapon-familiarity", "forlorn", "nimble-elf", "otherworldly-magic", "unwavering-mien" },
                    [5] = new List<string> { "ageless-patience", "elven-instincts", "elven-weapon-elegance", "elf-step", "expert-longevity" },
                    [9] = new List<string> { "bonded-item", "elven-weapon-expertise", "forest-stealth", "otherworldly-acumen", "tree-climber" },
                    [13] = new List<string> { "avenge-ally", "elven-weapon-specialization", "magic-rider", "otherworldly-resistance", "universal-longevity" },
                    [17] = new List<string> { "elven-weapon-legend", "elf-reinvigoration", "eternal-longevity" }
                },
                Source = "Core Rulebook",
                Rarity = "Common"
            },

            ["dwarf"] = new PfAncestry
            {
                Id = "dwarf",
                Name = "Dwarf",
                Description = "Dwarves have a well-earned reputation as a stoic and stern people, ensconced within citadels and cities carved from solid rock. While some see them as dour and humorless crafters of stone and metal, dwarves and those who have spent time among them understand their unbridled zeal for their work, caring far more about quality than quantity. To a stranger, a dwarf might seem tight-lipped, but among friends, a dwarf's humor and loyalty know no bounds.",
                HitPoints = 10,
                Size = "Medium",
                Speeds = new Dictionary<string, int> { ["land"] = 20 },
                AbilityBoosts = new List<string> { "Constitution", "Wisdom", "Free" },
                AbilityFlaws = new List<string> { "Charisma" },
                FreeAbilityBoosts = new List<string> { "Any" },
                Languages = new List<string> { "Common", "Dwarven" },
                AdditionalLanguages = 0,
                Senses = new List<string> { "Darkvision" },
                SenseRanges = new Dictionary<string, int> { ["darkvision"] = 60 },
                Traits = new List<string> { "Dwarf", "Humanoid" },
                Features = new List<PfAncestryFeature>
                {
                    new()
                    {
                        Id = "clan-dagger",
                        Name = "Clan Dagger",
                        Description = "You get one clan dagger of your clan for free, as it was passed down through the generations. In addition, you are trained in clan daggers.",
                        Traits = new List<string>()
                    }
                },
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "ancient-blooded", Name = "Ancient-Blooded Dwarf", Description = "Dwarven heroes of old could shrug off their enemies' magic, and some of that resistance manifests in you. You gain the Call on Ancient Blood reaction.", Traits = new List<string>() },
                    new() { Id = "death-warden", Name = "Death Warden Dwarf", Description = "Your ancestors have been tomb guardians for generations, and the power they cultivated to ward off necromancy has passed on to you. If you roll a success on a saving throw against a necromancy effect, you get a critical success instead.", Traits = new List<string>() },
                    new() { Id = "forge-dwarf", Name = "Forge Dwarf", Description = "You have a remarkable adaptation to hot environments from ancestors who inhabited blazing deserts or volcanic chambers beneath the earth. This grants you fire resistance equal to half your level (minimum 1), and you treat environmental heat effects as if they were one step less extreme (incredible heat becomes extreme, extreme heat becomes severe, and so on).", Traits = new List<string>() },
                    new() { Id = "rock-dwarf", Name = "Rock Dwarf", Description = "Your ancestors lived and worked among the great ancient stones of the mountains or the depths of the earth. This makes you solid as a rock when you plant your feet. You gain a +2 circumstance bonus to your Fortitude or Reflex DC against attempts to Shove or Trip you. This bonus also applies to saving throws against spells or effects that attempt to knock you prone.", Traits = new List<string>() },
                    new() { Id = "strong-blooded", Name = "Strong-Blooded Dwarf", Description = "Your blood runs hearty and strong, and you can shake off toxins. You gain poison resistance equal to half your level (minimum 1), and each of your successful saving throws against a poison affliction reduces its stage by 2, or by 1 for a virulent poison. Each critical success against an ongoing poison reduces its stage by 3, or by 2 for a virulent poison.", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "dwarven-lore", "dwarven-weapon-familiarity", "rock-runner", "stonecunning", "unburdened-iron", "vengeful-hatred" },
                    [5] = new List<string> { "boulder-roll", "dwarven-weapon-cunning", "mountain-stoutness" },
                    [9] = new List<string> { "dwarven-weapon-expertise", "stonewalker" },
                    [13] = new List<string> { "dwarven-weapon-specialization", "incredible-luck", "stone-bones" },
                    [17] = new List<string> { "dwarven-weapon-legend" }
                },
                Source = "Core Rulebook",
                Rarity = "Common"
            },

            ["halfling"] = new PfAncestry
            {
                Id = "halfling",
                Name = "Halfling",
                Description = "Claiming no place as their own, halflings control few settlements larger than villages. Instead, they frequently live among humans within the walls of larger cities, carving out small communities alongside taller folk. Many halflings lead perfectly fulfilling lives in the shadows of their larger neighbors, while others prefer a nomadic existence, traveling the world and taking advantage of opportunities and adventures as they come.",
                HitPoints = 6,
                Size = "Small",
                Speeds = new Dictionary<string, int> { ["land"] = 25 },
                AbilityBoosts = new List<string> { "Dexterity", "Wisdom", "Free" },
                AbilityFlaws = new List<string> { "Strength" },
                FreeAbilityBoosts = new List<string> { "Any" },
                Languages = new List<string> { "Common", "Halfling" },
                AdditionalLanguages = 0,
                Senses = new List<string>(),
                SenseRanges = new Dictionary<string, int>(),
                Traits = new List<string> { "Halfling", "Humanoid" },
                Features = new List<PfAncestryFeature>
                {
                    new()
                    {
                        Id = "keen-eyes",
                        Name = "Keen Eyes",
                        Description = "Your eyes are sharp, allowing you to make out small details about concealed or even invisible creatures that others might miss. You gain a +2 circumstance bonus when using the Seek action to find hidden or undetected creatures within 30 feet of you. When you target an opponent that is concealed from you or hidden from you, reduce the DC of the flat check to 3 for a concealed target or 9 for a hidden one.",
                        Traits = new List<string>()
                    }
                },
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "gutsy-halfling", Name = "Gutsy Halfling", Description = "Your family line is known for keeping a level head and staving off fear when the chips are down, making them wise leaders and sometimes even heroes. When you roll a success on a saving throw against an emotion effect, you get a critical success instead.", Traits = new List<string>() },
                    new() { Id = "hillock-halfling", Name = "Hillock Halfling", Description = "Accustomed to a calm life in the hills, your people find rest and relaxation especially replenishing, particularly when indulging in creature comforts. When you regain Hit Points overnight, add your level to the Hit Points regained. When anyone uses the Medicine skill to Treat your Wounds, you can eat a snack to add your level to the Hit Points you regain from their treatment.", Traits = new List<string>() },
                    new() { Id = "nomadic-halfling", Name = "Nomadic Halfling", Description = "Your ancestors have traveled from place to place for generations, never content to settle down. You gain two additional languages of your choice, chosen from among the common and uncommon languages available to you, and every time you take the Multilingual feat, you gain another language.", Traits = new List<string>() },
                    new() { Id = "twilight-halfling", Name = "Twilight Halfling", Description = "Your ancestors performed many secret acts under the concealing cover of dusk and darkness, and over time they developed the ability to see in twilight beyond even the usual keen sight of halflings. You gain low-light vision.", Traits = new List<string>() },
                    new() { Id = "wildwood-halfling", Name = "Wildwood Halfling", Description = "You hail from deep in a jungle or forest, and you've learned how to use your small size to wriggle through undergrowth, vines, and other obstacles. You ignore difficult terrain from trees, foliage, and undergrowth.", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "distracting-shadows", "halfling-lore", "halfling-luck", "halfling-weapon-familiarity", "sure-feet", "titan-slinger", "unfettered-halfling" },
                    [5] = new List<string> { "cultural-adaptability", "halfling-weapon-trickster", "pearl-diver", "stepwise-feet" },
                    [9] = new List<string> { "guiding-luck", "halfling-weapon-expertise", "irrepressible" },
                    [13] = new List<string> { "ceaseless-shadows", "halfling-weapon-specialization", "incredible-luck", "shadow-self" },
                    [17] = new List<string> { "halfling-weapon-legend", "legendary-luck" }
                },
                Source = "Core Rulebook",
                Rarity = "Common"
            },

            ["gnome"] = new PfAncestry
            {
                Id = "gnome",
                Name = "Gnome",
                Description = "Long ago, early gnome ancestors emigrated from the First World, realm of the fey. While it's unclear why the first gnomes wandered to Golarion, this lineage manifests in modern gnomes as bizarre reasoning, eccentricity, obsessive tendencies, and what some see as naivety. These qualities are further reflected in their physical characteristics, such as spindly limbs, brightly colored hair, and childlike and extremely expressive facial features that further reflect their emotions.",
                HitPoints = 8,
                Size = "Small",
                Speeds = new Dictionary<string, int> { ["land"] = 25 },
                AbilityBoosts = new List<string> { "Constitution", "Charisma", "Free" },
                AbilityFlaws = new List<string> { "Strength" },
                FreeAbilityBoosts = new List<string> { "Any" },
                Languages = new List<string> { "Common", "Gnomish", "Sylvan" },
                AdditionalLanguages = 0,
                Senses = new List<string> { "Low-Light Vision" },
                SenseRanges = new Dictionary<string, int>(),
                Traits = new List<string> { "Gnome", "Humanoid" },
                Features = new List<PfAncestryFeature>(),
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "chameleon-gnome", Name = "Chameleon Gnome", Description = "The color of your hair and skin is mutable, possibly due to latent magical energy. You can slowly change the vibrancy and the exact color, and the coloration can be different across your body, allowing for patterns such as bands or swirls. It takes a single action to make minor changes, and about 10 minutes to make major changes such as changing your hair from brown to bright green. This ability is always active to a degree, leading to your coloration slowly shifting to match your emotions.", Traits = new List<string>() },
                    new() { Id = "fey-touched", Name = "Fey-Touched Gnome", Description = "The blood of the fey is so strong within you that you're truly one of them. You gain the fey trait, in addition to the gnome and humanoid traits. Choose one cantrip from the primal spell list. You can cast this cantrip as a primal innate spell at will. A cantrip is heightened to a spell level equal to half your level rounded up.", Traits = new List<string>() },
                    new() { Id = "sensate-gnome", Name = "Sensate Gnome", Description = "You see all colors as brighter, hear all sounds as richer, and especially smell all scents with incredible detail. You gain a special sense: imprecise scent with a range of 30 feet. This means you can use your sense of smell to determine the exact location of a creature. The GM will usually double the range if you're downwind from the creature or halve the range if you're upwind.", Traits = new List<string>() },
                    new() { Id = "umbral-gnome", Name = "Umbral Gnome", Description = "Whether from a connection to dark or shadowy fey, from the underground deep gnomes also known as svirfneblin, or another source, you can see in complete darkness. You gain darkvision instead of low-light vision.", Traits = new List<string>() },
                    new() { Id = "wellspring-gnome", Name = "Wellspring Gnome", Description = "Some other source of magic has a greater hold on you than the primal magic of your fey lineage does. This connection might come from an aberrant planar experiment, a deity, a dragon, or even stranger sources. Choose arcane, divine, or occult. You gain one cantrip from that magical tradition's spell list. You can cast this cantrip as an innate spell at will, as a spell of your chosen tradition. A cantrip is heightened to a spell level equal to half your level rounded up.", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "animal-accomplice", "burrow-elocutionist", "fey-fellowship", "first-world-magic", "gnome-obsession", "gnome-weapon-familiarity", "illusion-sense" },
                    [5] = new List<string> { "energized-font", "gnome-weapon-innovator", "first-world-adept", "vivacious-conduit" },
                    [9] = new List<string> { "first-world-expert", "gnome-weapon-expertise", "homeward-bound" },
                    [13] = new List<string> { "eccentric-luck", "first-world-master", "gnome-weapon-specialization" },
                    [17] = new List<string> { "first-world-legend", "gnome-weapon-legend" }
                },
                Source = "Core Rulebook",
                Rarity = "Common"
            },

            ["goblin"] = new PfAncestry
            {
                Id = "goblin",
                Name = "Goblin",
                Description = "The convoluted histories other people cling to don't interest goblins. These small folk live in the moment, and they prefer tall tales over factual records. The wars of a few decades ago might as well be from the ancient past. Misunderstood by other people, goblins are happy how they are. Goblin virtues are about being present, creative, and honest. They strive to lead fulfilled lives, rather than worrying about how their journeys will end. To tell stories, not nitpick the facts. To be small, but dream big.",
                HitPoints = 6,
                Size = "Small",
                Speeds = new Dictionary<string, int> { ["land"] = 25 },
                AbilityBoosts = new List<string> { "Dexterity", "Charisma", "Free" },
                AbilityFlaws = new List<string> { "Wisdom" },
                FreeAbilityBoosts = new List<string> { "Any" },
                Languages = new List<string> { "Common", "Goblin" },
                AdditionalLanguages = 0,
                Senses = new List<string> { "Darkvision" },
                SenseRanges = new Dictionary<string, int> { ["darkvision"] = 60 },
                Traits = new List<string> { "Goblin", "Humanoid" },
                Features = new List<PfAncestryFeature>(),
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "charhide-goblin", Name = "Charhide Goblin", Description = "Your ancestors have always had a connection to fire and a thicker skin, which allows you to resist burning. You gain fire resistance equal to half your level (minimum 1). You can also recover from being on fire more easily. Your flat check to remove persistent fire damage is DC 10 instead of DC 15, which is reduced to DC 5 if another creature uses a particularly appropriate action to help.", Traits = new List<string>() },
                    new() { Id = "irongut-goblin", Name = "Irongut Goblin", Description = "You can subsist on food that most folks would consider spoiled. You can keep yourself fed with poor meals in a settlement as long as garbage is readily available, without paying for food. You can eat and drink things when it's not clear whether they're edible or poisonous. You gain a +2 circumstance bonus to saving throws against afflictions, against gaining the sickened condition, and to remove the sickened condition. When you roll a success on a Fortitude save affected by this bonus, you get a critical success instead. All these benefits apply only when the affliction or condition resulted from something you ingested.", Traits = new List<string>() },
                    new() { Id = "razortooth-goblin", Name = "Razortooth Goblin", Description = "Your family's teeth are formidable weapons. You gain a jaws unarmed attack that deals 1d6 piercing damage. Your jaws are in the brawling group and have the finesse and unarmed traits.", Traits = new List<string>() },
                    new() { Id = "snow-goblin", Name = "Snow Goblin", Description = "You are acclimated to living in frigid lands and have resistance to cold damage equal to half your level (minimum 1). You treat environmental cold effects as if they were one step less extreme (incredible cold becomes extreme, extreme cold becomes severe, and so on).", Traits = new List<string>() },
                    new() { Id = "unbreakable-goblin", Name = "Unbreakable Goblin", Description = "You're able to bounce back from injuries easily due to an exceptionally thick skull, cartilaginous bones, or some other mixed blessing. You gain 10 Hit Points from your ancestry instead of 6. When you fall, reduce the falling damage you take as though you had fallen half the distance (to a minimum of 0 damage).", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "city-scavenger", "goblin-lore", "goblin-scuttle", "goblin-song", "goblin-weapon-familiarity", "junk-tinker", "rough-rider", "very-sneaky" },
                    [5] = new List<string> { "goblin-weapon-frenzy", "cave-climber", "skittering-scuttle", "torch-goblin", "tree-climber" },
                    [9] = new List<string> { "bounce-back", "goblin-weapon-expertise", "hard-tail", "skull-hard" },
                    [13] = new List<string> { "goblin-weapon-specialization", "freeze-it", "very-very-sneaky" },
                    [17] = new List<string> { "goblin-weapon-legend" }
                },
                Source = "Core Rulebook",
                Rarity = "Common"
            },

            ["aasimar"] = new PfAncestry
            {
                Id = "aasimar",
                Name = "Aasimar",
                Description = "Aasimars are mortals born with a connection to celestial beings. They are not fully human, but bear angelic traits that mark them as touched by celestial power.",
                HitPoints = 8,
                Size = "Medium",
                Speeds = new Dictionary<string, int> { ["land"] = 25 },
                AbilityBoosts = new List<string> { "Charisma", "Free", "Free" },
                AbilityFlaws = new List<string>(),
                FreeAbilityBoosts = new List<string> { "Any", "Any" },
                Languages = new List<string> { "Common", "Celestial" },
                AdditionalLanguages = 0,
                Senses = new List<string> { "Darkvision" },
                SenseRanges = new Dictionary<string, int> { ["darkvision"] = 60 },
                Traits = new List<string> { "Aasimar", "Humanoid" },
                Features = new List<PfAncestryFeature>
                {
                    new()
                    {
                        Id = "celestial-resistance",
                        Name = "Celestial Resistance",
                        Description = "You have resistance 5 to acid, cold, and electricity.",
                        Traits = new List<string>()
                    }
                },
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "angelkin", Name = "Angelkin", Description = "Your celestial predecessor was a type of angel. You gain a +1 circumstance bonus to saving throws against effects with the death trait.", Traits = new List<string>() },
                    new() { Id = "emberkin", Name = "Emberkin", Description = "Your celestial predecessor was an ember of divine flame. You gain the Produce Flame cantrip as a divine innate spell.", Traits = new List<string>() },
                    new() { Id = "lawbringer", Name = "Lawbringer", Description = "Your celestial predecessor was a lawful celestial. When you roll initiative, you can choose to go last in the turn order.", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "celestial-eyes", "celestial-lore", "halo" },
                    [5] = new List<string> { "celestial-resistance", "heal-mount" },
                    [9] = new List<string> { "celestial-wings", "righteous-fire" },
                    [13] = new List<string> { "blessed-blood", "celestial-word" },
                    [17] = new List<string> { "redemption", "summon-celestial" }
                },
                Source = "Advanced Player's Guide",
                Rarity = "Uncommon"
            },

            ["anadi"] = new PfAncestry
            {
                Id = "anadi",
                Name = "Anadi",
                Description = "Anadi are a reclusive people from the Mwangi Expanse who resemble enormous spiders. Most anadi remain in the Mwangi Jungle, but some occasionally make their way into human society.",
                HitPoints = 8,
                Size = "Medium",
                Speeds = new Dictionary<string, int> { ["land"] = 25, ["climb"] = 25 },
                AbilityBoosts = new List<string> { "Dexterity", "Wisdom", "Free" },
                AbilityFlaws = new List<string> { "Constitution" },
                FreeAbilityBoosts = new List<string> { "Any" },
                Languages = new List<string> { "Anadi", "Mwangi" },
                AdditionalLanguages = 1,
                Senses = new List<string> { "Darkvision" },
                SenseRanges = new Dictionary<string, int> { ["darkvision"] = 60 },
                Traits = new List<string> { "Anadi", "Humanoid" },
                Features = new List<PfAncestryFeature>
                {
                    new()
                    {
                        Id = "change-shape",
                        Name = "Change Shape",
                        Description = "You can change between your human and spider forms. This transformation takes 1 minute.",
                        Traits = new List<string> { "concentrate", "polymorph", "primal", "transmutation" }
                    },
                    new()
                    {
                        Id = "fangs",
                        Name = "Fangs",
                        Description = "You gain a fangs unarmed attack that deals 1d6 piercing damage.",
                        Traits = new List<string>()
                    }
                },
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "huntsman", Name = "Huntsman Anadi", Description = "You're a skilled ambush hunter. When using a ranged weapon, you don't take a penalty for attacking at your second and third range increments.", Traits = new List<string>() },
                    new() { Id = "polychromatic", Name = "Polychromatic Anadi", Description = "Your coloration shifts to match your surroundings. You can Hide even when you don't have cover or aren't concealed.", Traits = new List<string>() },
                    new() { Id = "snaring", Name = "Snaring Anadi", Description = "You can produce a web to Trip creatures. You gain a web ranged unarmed attack with a range increment of 10 feet that deals no damage but can Trip opponents.", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "anadi-lore", "prey-sense", "web-weaver" },
                    [5] = new List<string> { "adaptive-colorations", "climbing-speed" },
                    [9] = new List<string> { "perfect-web-navigation", "web-trap" },
                    [13] = new List<string> { "web-impalement", "cocoon-master" },
                    [17] = new List<string> { "legendary-web-weaver" }
                },
                Source = "The Mwangi Expanse",
                Rarity = "Rare"
            },

            ["android"] = new PfAncestry
            {
                Id = "android",
                Name = "Android",
                Description = "Androids are artificial people created by technology. They have mechanical bodies with synthetic skin and living souls.",
                HitPoints = 8,
                Size = "Medium", 
                Speeds = new Dictionary<string, int> { ["land"] = 25 },
                AbilityBoosts = new List<string> { "Dexterity", "Intelligence", "Free" },
                AbilityFlaws = new List<string> { "Charisma" },
                FreeAbilityBoosts = new List<string> { "Any" },
                Languages = new List<string> { "Common", "Androffan" },
                AdditionalLanguages = 1,
                Senses = new List<string> { "Low-Light Vision" },
                SenseRanges = new Dictionary<string, int>(),
                Traits = new List<string> { "Android", "Humanoid" },
                Features = new List<PfAncestryFeature>
                {
                    new()
                    {
                        Id = "constructed",
                        Name = "Constructed",
                        Description = "Your synthetic body resists ailments better than those of purely biological organisms. You gain a +1 circumstance bonus to saving throws against diseases, poisons, and radiation.",
                        Traits = new List<string>()
                    },
                    new()
                    {
                        Id = "emotionally-unaware",
                        Name = "Emotionally Unaware",
                        Description = "You have difficulty processing emotions or emotive cues. You take a -1 circumstance penalty to Diplomacy and Performance checks, and on Perception checks to Sense Motive.",
                        Traits = new List<string>()
                    }
                },
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "companion", Name = "Companion Android", Description = "You were created to be a companion or servant. You gain the trained proficiency rank in Diplomacy (or another skill if already trained) and the Hobnobber skill feat.", Traits = new List<string>() },
                    new() { Id = "impersonator", Name = "Impersonator Android", Description = "You were designed to blend in. You don't have the emotionally unaware ability, and you gain the trained proficiency rank in Deception (or another skill if already trained).", Traits = new List<string>() },
                    new() { Id = "laborer", Name = "Laborer Android", Description = "You were created for heavy work. You gain the trained proficiency rank in Athletics (or another skill if already trained) and the Hefty Hauler skill feat.", Traits = new List<string>() },
                    new() { Id = "warrior", Name = "Warrior Android", Description = "Created for combat, you have enhanced physical capabilities. You gain the trained proficiency rank in Intimidation (or another skill if already trained) and the Intimidating Glare skill feat.", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "android-lore", "cleansing-subroutine", "nanite-surge" },
                    [5] = new List<string> { "advanced-sensors", "proximity-alert" },
                    [9] = new List<string> { "energy-conduit", "mechanical-symbiosis" },
                    [13] = new List<string> { "advanced-subroutines", "multitasking-subroutines" },
                    [17] = new List<string> { "system-reboot", "upgrade-chassis" }
                },
                Source = "Starfinder",
                Rarity = "Rare"
            },

            ["hobgoblin"] = new PfAncestry
            {
                Id = "hobgoblin",
                Name = "Hobgoblin",
                Description = "Hobgoblins are a militaristic people who organize their society along military lines. They value discipline, tactics, and achievement in battle above all else.",
                HitPoints = 8,
                Size = "Medium",
                Speeds = new Dictionary<string, int> { ["land"] = 25 },
                AbilityBoosts = new List<string> { "Constitution", "Intelligence", "Free" },
                AbilityFlaws = new List<string> { "Wisdom" },
                FreeAbilityBoosts = new List<string> { "Any" },
                Languages = new List<string> { "Common", "Goblin" },
                AdditionalLanguages = 1,
                Senses = new List<string> { "Darkvision" },
                SenseRanges = new Dictionary<string, int> { ["darkvision"] = 60 },
                Traits = new List<string> { "Goblin", "Humanoid" },
                Features = new List<PfAncestryFeature>(),
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "elfbane", Name = "Elfbane Hobgoblin", Description = "Hobgoblins were engineered long ago from the souls of elves, and some hobgoblins retain animosity toward their ancient enemies. You gain a +1 circumstance bonus to damage rolls against elves and creatures with the elf trait.", Traits = new List<string>() },
                    new() { Id = "runtboss", Name = "Runtboss Hobgoblin", Description = "You come from a long line of hobgoblins who commanded goblins. You gain the trained proficiency rank in Intimidation. If you're already trained in Intimidation, you instead become trained in a skill of your choice.", Traits = new List<string>() },
                    new() { Id = "smokeworker", Name = "Smokeworker Hobgoblin", Description = "Your family have been alchemists, engineers, and scientists for generations. You gain fire resistance equal to half your level (minimum 1). You also gain a +1 circumstance bonus to saving throws against inhaled toxins and poison gas.", Traits = new List<string>() },
                    new() { Id = "steelskin", Name = "Steelskin Hobgoblin", Description = "When you are unarmored or wearing light armor, your tough skin gives you a +1 circumstance bonus to AC instead of your armor's item bonus (use the higher bonus; they don't stack). This bonus increases to +2 against slashing damage.", Traits = new List<string>() },
                    new() { Id = "warmarch", Name = "Warmarch Hobgoblin", Description = "You come from a line of wandering mercenaries, constantly on the march and scavenging food on the trail. You gain the Forager skill feat, even if you don't meet the prerequisites.", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "hobgoblin-lore", "hobgoblin-weapon-familiarity", "leering-look", "pyroclastic-blast" },
                    [5] = new List<string> { "formation-training", "hobgoblin-weapon-discipline" },
                    [9] = new List<string> { "alchemical-Scholar", "hobgoblin-weapon-expertise" },
                    [13] = new List<string> { "formation-master", "hobgoblin-weapon-specialization" },
                    [17] = new List<string> { "hobgoblin-weapon-legend" }
                },
                Source = "Character Guide",
                Rarity = "Uncommon"
            },

            ["kobold"] = new PfAncestry
            {
                Id = "kobold",
                Name = "Kobold",
                Description = "Every kobold knows that their slight frame belies true, mighty draconic power. They are ingenious crafters and devoted allies within their warrens, but those who trespass into their territory find them to be inspired skirmishers, especially when they have the backing of a draconic sorcerer or true dragon overlord.",
                HitPoints = 6,
                Size = "Small",
                Speeds = new Dictionary<string, int> { ["land"] = 25 },
                AbilityBoosts = new List<string> { "Dexterity", "Charisma", "Free" },
                AbilityFlaws = new List<string> { "Constitution" },
                FreeAbilityBoosts = new List<string> { "Any" },
                Languages = new List<string> { "Common", "Draconic" },
                AdditionalLanguages = 1,
                Senses = new List<string> { "Darkvision" },
                SenseRanges = new Dictionary<string, int> { ["darkvision"] = 60 },
                Traits = new List<string> { "Kobold", "Humanoid" },
                Features = new List<PfAncestryFeature>(),
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "caveclimber", Name = "Caveclimber Kobold", Description = "You live in a vertically oriented home, and you're a consummate climber. You gain the Combat Climber skill feat and a +1 circumstance bonus to Athletics checks to climb.", Traits = new List<string>() },
                    new() { Id = "dragonscaled", Name = "Dragonscaled Kobold", Description = "Your scales are especially colorful, possessing some of the same resistance a dragon possesses. You gain resistance equal to half your level (minimum 1) to the damage type associated with your draconic exemplar.", Traits = new List<string>() },
                    new() { Id = "spellscale", Name = "Spellscale Kobold", Description = "A trace of draconic magic flows through you. Choose one cantrip from the arcane spell list. You can cast this cantrip as an arcane innate spell at will.", Traits = new List<string>() },
                    new() { Id = "strongjaw", Name = "Strongjaw Kobold", Description = "Your bloodline is noted for their powerful jaws and sharp teeth. You gain a jaws unarmed attack that deals 1d6 piercing damage.", Traits = new List<string>() },
                    new() { Id = "tunnelflood", Name = "Tunnelflood Kobold", Description = "You grew up in a warren crisscrossed by underwater passages, whether natural or excavated. You gain a 15-foot swim Speed.", Traits = new List<string>() },
                    new() { Id = "venomtail", Name = "Venomtail Kobold", Description = "A vestigial spur in your tail secretes one dose of lethal poison per day. You gain the Tail Toxin action.", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "kobold-lore", "kobold-weapon-familiarity", "dragon-speaker", "snare-crafter" },
                    [5] = new List<string> { "dragon-magic", "snare-specialist" },
                    [9] = new List<string> { "kobold-weapon-expertise", "trap-finder" },
                    [13] = new List<string> { "kobold-weapon-specialization", "greater-drag" },
                    [17] = new List<string> { "kobold-weapon-legend" }
                },
                Source = "Advanced Player's Guide",
                Rarity = "Uncommon"
            },

            ["leshy"] = new PfAncestry
            {
                Id = "leshy",
                Name = "Leshy",
                Description = "Leshys are living plants animated by primal magic. Originally created as guardians of natural locations, many leshys have since awakened to full consciousness and gained the freedom to determine their own paths in life.",
                HitPoints = 8,
                Size = "Small",
                Speeds = new Dictionary<string, int> { ["land"] = 25 },
                AbilityBoosts = new List<string> { "Constitution", "Wisdom", "Free" },
                AbilityFlaws = new List<string> { "Intelligence" },
                FreeAbilityBoosts = new List<string> { "Any" },
                Languages = new List<string> { "Common", "Sylvan" },
                AdditionalLanguages = 1,
                Senses = new List<string> { "Low-Light Vision" },
                SenseRanges = new Dictionary<string, int>(),
                Traits = new List<string> { "Leshy", "Plant" },
                Features = new List<PfAncestryFeature>
                {
                    new()
                    {
                        Id = "plant",
                        Name = "Plant",
                        Description = "You have the plant trait instead of the humanoid trait. Your body is composed of vegetable matter.",
                        Traits = new List<string>()
                    },
                    new()
                    {
                        Id = "verdant-metamorphosis",
                        Name = "Verdant Metamorphosis",
                        Description = "You know how to enter a restorative, vegetative state. When you take a long rest in an area with adequate sunlight and water, you regain twice as many Hit Points.",
                        Traits = new List<string>()
                    }
                },
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "fungus", Name = "Fungus Leshy", Description = "Your body was made from fungi that grows in the shade of caves and trees, and you are at home in dark caverns and shadowy forest floors. You gain darkvision.", Traits = new List<string>() },
                    new() { Id = "gourd", Name = "Gourd Leshy", Description = "You have a large gourd for a skull. Your knowledge comes from within your seeds, which rattle around inside your head. You gain the trained proficiency rank in Occultism.", Traits = new List<string>() },
                    new() { Id = "leaf", Name = "Leaf Leshy", Description = "Your body is made mostly from leaves. You gain gliding wings that allow you to fall slowly. As long as you can act, you take no damage from falls of any distance.", Traits = new List<string>() },
                    new() { Id = "lotus", Name = "Lotus Leshy", Description = "You effortlessly float on the surface of water. You can walk on the surface of still water and similar liquids, moving at half your normal Speed.", Traits = new List<string>() },
                    new() { Id = "pine", Name = "Pine Leshy", Description = "Your body is made from the hardy wood of a pine tree. You gain cold resistance equal to half your level (minimum 1), and your unarmed attacks deal 1 additional damage.", Traits = new List<string>() },
                    new() { Id = "vine", Name = "Vine Leshy", Description = "The prehensile vines woven into your body grant you unmatched skill at climbing. You gain a 10-foot climb Speed.", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "leshy-lore", "leshy-superstition", "seedpod" },
                    [5] = new List<string> { "bark-skin", "speak-with-kindred" },
                    [9] = new List<string> { "effortless-concentration", "wind-chime" },
                    [13] = new List<string> { "regrowth", "solar-rejuvenation" },
                    [17] = new List<string> { "verdant-weapon" }
                },
                Source = "Character Guide",
                Rarity = "Uncommon"
            },

            ["lizardfolk"] = new PfAncestry
            {
                Id = "lizardfolk",
                Name = "Lizardfolk",
                Description = "These reptilian humanoids are natural survivors with powerful jaws and tough scales. Lizardfolk move through the societies of other humanoids with an awareness of their alien perspective.",
                HitPoints = 8,
                Size = "Medium",
                Speeds = new Dictionary<string, int> { ["land"] = 25, ["swim"] = 15 },
                AbilityBoosts = new List<string> { "Strength", "Wisdom", "Free" },
                AbilityFlaws = new List<string> { "Intelligence" },
                FreeAbilityBoosts = new List<string> { "Any" },
                Languages = new List<string> { "Common", "Draconic" },
                AdditionalLanguages = 1,
                Senses = new List<string>(),
                SenseRanges = new Dictionary<string, int>(),
                Traits = new List<string> { "Humanoid", "Lizardfolk" },
                Features = new List<PfAncestryFeature>
                {
                    new()
                    {
                        Id = "claws",
                        Name = "Claws",
                        Description = "You have sharp claws that can be used as weapons. You gain claw unarmed attacks that deal 1d4 slashing damage and have the agile and finesse traits.",
                        Traits = new List<string>()
                    },
                    new()
                    {
                        Id = "aquatic-adaptation",
                        Name = "Aquatic Adaptation", 
                        Description = "Your reptilian biology allows you to hold your breath for a long time. You can hold your breath for 25 rounds (2.5 minutes) before you start suffocating.",
                        Traits = new List<string>()
                    }
                },
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "cliffscale", Name = "Cliffscale Lizardfolk", Description = "Your toes are adapted for gripping and climbing. You gain the Combat Climber skill feat and a +2 circumstance bonus to your Reflex DC against attempts to Shove or Trip you.", Traits = new List<string>() },
                    new() { Id = "frilled", Name = "Frilled Lizardfolk", Description = "You can flare your neck frill and flex your dorsal spines, Demoralizing your enemies. When you do, Demoralize loses the auditory trait and gains the visual trait, and you don't take a penalty when you attempt to Demoralize a creature that doesn't understand your language.", Traits = new List<string>() },
                    new() { Id = "sandstrider", Name = "Sandstrider Lizardfolk", Description = "Your thick scales help you retain water and energy, and your wide feet better distribute your weight for desert travel. You need only a single serving of food and water each week to avoid starvation or dehydration.", Traits = new List<string>() },
                    new() { Id = "unseen", Name = "Unseen Lizardfolk", Description = "Your skin shifts coloration rapidly when you're in danger. When you roll a critical failure on a Stealth check to Hide, you get a failure instead. When you roll a success, you get a critical success instead.", Traits = new List<string>() },
                    new() { Id = "wetlander", Name = "Wetlander Lizardfolk", Description = "Your family is from an aquatic community, continuing to swim in the rivers or lakes of your ancestors. You gain a 15-foot swim Speed.", Traits = new List<string>() },
                    new() { Id = "woodstalker", Name = "Woodstalker Lizardfolk", Description = "You move carefully through thick forest and jungle, leaving few traces. You can move through difficult terrain caused by plants at half your Speed (instead of one-quarter), and you leave fewer traces when moving through natural surroundings.", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "hunter-lizardfolk", "lizardfolk-lore", "razor-claws", "swim-speed" },
                    [5] = new List<string> { "gecko-grip", "hunting-group" },
                    [9] = new List<string> { "bone-magic", "terrain-advantage" },
                    [13] = new List<string> { "dragon-speaker", "envenom-fangs" },
                    [17] = new List<string> { "scion-transformation" }
                },
                Source = "Character Guide",
                Rarity = "Uncommon"
            },

            ["orc"] = new PfAncestry
            {
                Id = "orc",
                Name = "Orc",
                Description = "Orcs are forged in the fires of violence and conflict, often from the moment they're born. As they live lives that are frequently cut short, orcs revel in testing their strength against worthy foes, whether by challenging a higher-ranking member of their community or raiding a neighboring settlement.",
                HitPoints = 10,
                Size = "Medium",
                Speeds = new Dictionary<string, int> { ["land"] = 25 },
                AbilityBoosts = new List<string> { "Strength", "Free", "Free" },
                AbilityFlaws = new List<string>(),
                FreeAbilityBoosts = new List<string> { "Any", "Any" },
                Languages = new List<string> { "Common", "Orcish" },
                AdditionalLanguages = 1,
                Senses = new List<string> { "Darkvision" },
                SenseRanges = new Dictionary<string, int> { ["darkvision"] = 60 },
                Traits = new List<string> { "Humanoid", "Orc" },
                Features = new List<PfAncestryFeature>(),
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "badlands", Name = "Badlands Orc", Description = "You come from sun-scorched badlands, where long legs and an efficient stride were key to survival. You can Hustle twice as long while exploring before you have to stop.", Traits = new List<string>() },
                    new() { Id = "battle-ready", Name = "Battle-Ready Orc", Description = "You descend from a line of terrifying battlefield commanders. You become trained in Intimidation, and you can use Intimidation to Demoralize your foes even if you don't share a language with them.", Traits = new List<string>() },
                    new() { Id = "deep", Name = "Deep Orc", Description = "Your calloused hands and red eyes speak to a life spent in the deep darkness of mountain caverns. You gain darkvision with a range of 120 feet instead of 60 feet.", Traits = new List<string>() },
                    new() { Id = "hold-scarred", Name = "Hold-Scarred Orc", Description = "You or your ancestors have lived underground. You gain fire resistance equal to half your level (minimum 1), and you treat environmental heat effects as if they were one step less extreme.", Traits = new List<string>() },
                    new() { Id = "rainfall", Name = "Rainfall Orc", Description = "You were born in the aftermath of a great storm, or you have a supernatural connection to storms. You can go 10 minutes longer than normal before you have to attempt a Constitution check when holding your breath.", Traits = new List<string>() },
                    new() { Id = "winter", Name = "Winter Orc", Description = "Your ancestors survived in cold climates. You gain cold resistance equal to half your level (minimum 1), and you treat environmental cold effects as if they were one step less extreme.", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "bloody-blows", "icon-of-strength", "orc-lore", "orc-sight", "orc-superstition", "orc-weapon-familiarity" },
                    [5] = new List<string> { "orc-ferocity", "orc-weapon-carnage", "victorious-vigor" },
                    [9] = new List<string> { "instinctive-obstructions", "orc-weapon-expertise", "pervasive-superstition" },
                    [13] = new List<string> { "deathless-ferocity", "orc-weapon-specialization", "scar-thick-skin" },
                    [17] = new List<string> { "orc-weapon-legend", "unbreakable-ferocity" }
                },
                Source = "Advanced Player's Guide",
                Rarity = "Uncommon"
            },

            ["ratfolk"] = new PfAncestry
            {
                Id = "ratfolk",
                Name = "Ratfolk",
                Description = "Ysoki—as ratfolk call themselves—are a clever, adaptable, and fastidious ancestry who happily crowd their large families into the smallest living spaces they can find.",
                HitPoints = 6,
                Size = "Small",
                Speeds = new Dictionary<string, int> { ["land"] = 25 },
                AbilityBoosts = new List<string> { "Dexterity", "Intelligence", "Free" },
                AbilityFlaws = new List<string> { "Strength" },
                FreeAbilityBoosts = new List<string> { "Any" },
                Languages = new List<string> { "Common", "Ysoki" },
                AdditionalLanguages = 1,
                Senses = new List<string> { "Low-Light Vision" },
                SenseRanges = new Dictionary<string, int>(),
                Traits = new List<string> { "Humanoid", "Ratfolk" },
                Features = new List<PfAncestryFeature>
                {
                    new()
                    {
                        Id = "cheek-pouches",
                        Name = "Cheek Pouches",
                        Description = "You have cheek pouches that let you store up to four items of light Bulk inside your mouth, out of the hands of pickpockets and thieves. If you speak, any items fall out of your mouth.",
                        Traits = new List<string>()
                    }
                },
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "deep", Name = "Deep Rat", Description = "Your ancestors lived deeper underground than most ratfolk. You gain darkvision instead of low-light vision.", Traits = new List<string>() },
                    new() { Id = "desert", Name = "Desert Rat", Description = "You come from deep in the desert where water is scarce but dangerous creatures abound. You can go twice as long as normal before you're affected by starvation or thirst. Additionally, you gain a +2 circumstance bonus to Fortitude saves against diseases.", Traits = new List<string>() },
                    new() { Id = "longsnout", Name = "Longsnout Rat", Description = "The long snout common among your family gives you a keener sense of smell than most ratfolk. You gain imprecise scent with a range of 30 feet.", Traits = new List<string>() },
                    new() { Id = "sewer", Name = "Sewer Rat", Description = "You come from a long line of ysoki with a tolerance for filth and disease. You gain a +2 circumstance bonus to saving throws against diseases and the sickened condition.", Traits = new List<string>() },
                    new() { Id = "shadow", Name = "Shadow Rat", Description = "Your ancestors lived in dark spaces underground. You gain greater darkvision instead of low-light vision, allowing you to see in complete darkness.", Traits = new List<string>() },
                    new() { Id = "tunnel", Name = "Tunnel Rat", Description = "You can squeeze through tight spaces as if you were one size smaller, and you can move at full Speed while squeezing.", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "ratfolk-lore", "ratfolk-weapon-familiarity", "plague-sniffer", "tinkering" },
                    [5] = new List<string> { "cheek-pouches", "ratfolk-weapon-innovator" },
                    [9] = new List<string> { "ratfolk-weapon-expertise", "tunnel-rat-climber" },
                    [13] = new List<string> { "ratfolk-weapon-specialization", "unassuming-dedication" },
                    [17] = new List<string> { "ratfolk-weapon-legend" }
                },
                Source = "Advanced Player's Guide",
                Rarity = "Uncommon"
            },

            ["tengu"] = new PfAncestry
            {
                Id = "tengu",
                Name = "Tengu",
                Description = "Tengus are a gregarious and resourceful people that have spread far and wide from their ancestral home in Tian Xia, collecting and combining whatever innovations and traditions they happen across with those from their own long history.",
                HitPoints = 6,
                Size = "Medium",
                Speeds = new Dictionary<string, int> { ["land"] = 25 },
                AbilityBoosts = new List<string> { "Dexterity", "Free", "Free" },
                AbilityFlaws = new List<string>(),
                FreeAbilityBoosts = new List<string> { "Any", "Any" },
                Languages = new List<string> { "Common", "Tengu" },
                AdditionalLanguages = 2,
                Senses = new List<string> { "Low-Light Vision" },
                SenseRanges = new Dictionary<string, int>(),
                Traits = new List<string> { "Humanoid", "Tengu" },
                Features = new List<PfAncestryFeature>
                {
                    new()
                    {
                        Id = "sharp-beak",
                        Name = "Sharp Beak",
                        Description = "With your sharp beak, you are never without a weapon. You have a beak unarmed attack that deals 1d6 piercing damage. Your beak is in the brawling weapon group and has the finesse and unarmed traits.",
                        Traits = new List<string>()
                    }
                },
                Heritages = new List<PfHeritage>
                {
                    new() { Id = "dogtooth", Name = "Dogtooth Tengu", Description = "Your ancestors were known for their vicious, serrated beaks. Your beak unarmed attack gains the deadly d6 trait.", Traits = new List<string>() },
                    new() { Id = "jinxed", Name = "Jinxed Tengu", Description = "Your lineage has been exposed to curse energy or magics, granting you a twisted kind of luck. You gain the Dubious Knowledge skill feat. When you succeed at a Perception check or skill check, you can choose to critically fail instead. If you do, reroll the triggering check with a +4 circumstance bonus; this is a fortune effect.", Traits = new List<string>() },
                    new() { Id = "mountain", Name = "Mountain Tengu", Description = "You were born in high altitudes, such as mountain peaks or flying cities, and you can easily handle thin air. You don't need to attempt Fortitude saves from high altitudes, regardless of how quickly you ascended.", Traits = new List<string>() },
                    new() { Id = "stormtossed", Name = "Stormtossed Tengu", Description = "Whether due to a blessing from the kami of storms or simply a legacy of ancient magic in your bloodline, you can predict weather patterns. You can cast augury as a divine innate spell once per day, but only to predict weather-related events.", Traits = new List<string>() },
                    new() { Id = "wavediver", Name = "Wavediver Tengu", Description = "Your wings are especially large and adapted for soaring over water. You gain a 10-foot status bonus to your fly Speed, if you have one.", Traits = new List<string>() },
                    new() { Id = "windcaller", Name = "Windcaller Tengu", Description = "You have a connection to spirits of wind and sky. You can cast ghost sound as an occult innate cantrip at will.", Traits = new List<string>() }
                },
                AncestryFeatLevels = new Dictionary<int, List<string>>
                {
                    [1] = new List<string> { "eat-fortune", "long-nose", "storm-sight", "tengu-lore", "tengu-weapon-familiarity" },
                    [5] = new List<string> { "crowlike-dodge", "hurricane-swing", "tengu-weapon-study" },
                    [9] = new List<string> { "carrion-sight", "descending-winds", "tengu-weapon-expertise" },
                    [13] = new List<string> { "fortune-telling", "tengu-weapon-specialization", "wanderer-lore" },
                    [17] = new List<string> { "eclipse-eyes", "tengu-weapon-legend" }
                },
                Source = "Advanced Player's Guide", 
                Rarity = "Uncommon"
            }
        };
    }

    public static class Conditions
    {
        public static readonly Dictionary<string, PfCondition> CoreConditionsData = new()
        {
            ["blinded"] = new PfCondition
            {
                Id = "blinded",
                Name = "Blinded",
                Description = "You can't see. All normal terrain is difficult terrain to you. You can't detect anything using vision. You automatically critically fail Perception checks that require you to be able to see, and if vision is your only precise sense, you take a –4 status penalty to Perception checks. You are immune to visual effects. Blinded overrides dazzled.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = true,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["clumsy"] = new PfCondition
            {
                Id = "clumsy",
                Name = "Clumsy",
                Description = "Your movements become clumsy and inexact. Clumsy always includes a value. You take a status penalty equal to the condition value to Dexterity-based checks and DCs, including AC, Reflex saves, ranged attack rolls, and skill checks using Acrobatics, Stealth, and Thievery.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = true,
                MaxValue = 4,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["confused"] = new PfCondition
            {
                Id = "confused",
                Name = "Confused",
                Description = "You don't have your wits about you, and you attack wildly. You are flat-footed, you don't treat anyone as your ally (though they might still treat you as theirs), and you can't Delay, Ready, or use reactions. You use all your actions to Strike or cast offensive cantrips, though the GM can have you use other actions to facilitate attack, such as draw a weapon, move so that a target is in reach, and so forth. Your targets are determined randomly from among those you can see, as follows. Roll 1d4 to see who you attack: On a 1, you attack yourself, dealing damage with the same roll you would use to attack (including your multiple attack penalty). If you can't attack yourself, you attack an ally. On a 2–3, you attack a randomly determined creature within your reach. On a 4, you attack a randomly determined ally within your reach. If you have no allies in reach, you attack a randomly determined creature within reach. If no creature is within your reach, you move toward the closest creature as efficiently as possible. If you can't act, you babble incoherently, wasting your actions.",
                Type = ConditionType.Debuff,
                Traits = new List<string> { "mental" },
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string> { "mental" },
                Source = "Core Rulebook"
            },

            ["controlled"] = new PfCondition
            {
                Id = "controlled",
                Name = "Controlled",
                Description = "Someone else is making your decisions for you, usually because you're being commanded or magically dominated. The controller dictates how you act and can make you use any of your actions, including attacks, reactions, or even Delay. The controller usually does not have to spend their own actions when controlling you.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["dazzled"] = new PfCondition
            {
                Id = "dazzled",
                Name = "Dazzled",
                Description = "Your eyes are overstimulated. If vision is your only precise sense, all creatures and objects are concealed from you.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string> { "blinded" },
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["deafened"] = new PfCondition
            {
                Id = "deafened",
                Name = "Deafened",
                Description = "You can't hear. You automatically critically fail Perception checks that require you to be able to hear. You take a –2 status penalty to Perception checks for initiative and checks that involve sound but also rely on other senses. You are immune to auditory effects.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["doomed"] = new PfCondition
            {
                Id = "doomed",
                Name = "Doomed",
                Description = "Your soul is being torn away by supernatural forces or a terrible curse, and you are close to death. Doomed always includes a value. Doomed decreases by 1 each time you get a full night's rest. The dying value at which you die is reduced by your doomed value. If your maximum dying value is reduced to 0, you instantly die. When you die, you're no longer doomed.",
                Type = ConditionType.Death,
                Traits = new List<string>(),
                HasValue = true,
                MaxValue = 3,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["drained"] = new PfCondition
            {
                Id = "drained",
                Name = "Drained",
                Description = "You lose blood or life force. Drained always includes a value. You take a status penalty equal to your drained value on Constitution-based checks, such as Fortitude saves. You also lose a number of Hit Points equal to your level (minimum 1) × the drained value, and your maximum Hit Points are reduced by the same amount. You die if your maximum Hit Points are reduced to 0. Lost Hit Points and the maximum Hit Point reduction persist until the drained condition ends. When you're drained by blood loss or similar, you look noticeably pale and might feel cold. When you're drained by something other than blood loss, the effects vary but can include muscle spasms, lethargy, or fits of coughing.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = true,
                MaxValue = 4,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["dying"] = new PfCondition
            {
                Id = "dying",
                Name = "Dying",
                Description = "You are bleeding out or otherwise at death's door. While you have this condition, you are unconscious. Dying always includes a value, and if it ever reaches dying 4, you die. If you're dying, you must attempt a recovery check (page 459) at the start of your turn each round to determine whether you get better or worse. Your dying condition increases by 1 if you take damage while dying, or by 2 if you take damage from an enemy's critical hit or a critical failure on your save. If you lose the dying condition by succeeding at a recovery check and are still at 0 Hit Points, you remain unconscious, but you can wake up as described in that sidebar. You lose the dying condition automatically and wake up if you ever have 1 or more Hit Points. Anytime you lose the dying condition, you gain the wounded 1 condition, or increase your wounded condition by 1 if you already have that condition.",
                Type = ConditionType.Death,
                Traits = new List<string>(),
                HasValue = true,
                MaxValue = 4,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["enfeebled"] = new PfCondition
            {
                Id = "enfeebled",
                Name = "Enfeebled",
                Description = "You're physically weakened. Enfeebled always includes a value. You take a status penalty equal to the condition value to Strength-based checks and DCs, including Strength-based melee attack rolls, Strength-based damage rolls, and Athletics checks.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = true,
                MaxValue = 4,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["fascinated"] = new PfCondition
            {
                Id = "fascinated",
                Name = "Fascinated",
                Description = "You are compelled to focus your attention on something, distracting you from whatever else is going on around you. You take a –2 status penalty to Perception and skill checks, and you can't use actions with the concentrate trait unless they or their intended consequences are related to the subject of your fascination (as determined by the GM). For instance, you might be able to Seek and Recall Knowledge about the subject, but you likely can't cast a spell targeting a different creature. This condition ends if a creature uses hostile actions against you or any of your allies.",
                Type = ConditionType.Debuff,
                Traits = new List<string> { "mental" },
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string> { "mental" },
                Source = "Core Rulebook"
            },

            ["fatigued"] = new PfCondition
            {
                Id = "fatigued",
                Name = "Fatigued",
                Description = "You're tired and can't summon much energy. You take a –1 status penalty to AC and saving throws. You can't use exploration activities performed while traveling, such as Avoiding Notice, Detecting Magic, Following Tracks, or Searching. You recover from fatigue after a full night's rest.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["flat-footed"] = new PfCondition
            {
                Id = "flat-footed",
                Name = "Flat-Footed",
                Description = "You're distracted or otherwise unable to focus your full attention on defense. You take a –2 circumstance penalty to AC. Some effects give you the flat-footed condition only to certain creatures or against certain attacks. Others give you the flat-footed condition only if you fail a saving throw or other roll. The flat-footed condition on its own doesn't give you a penalty to Reflex saves, but other effects or conditions that make you flat-footed might also affect your Reflex saves.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["fleeing"] = new PfCondition
            {
                Id = "fleeing",
                Name = "Fleeing",
                Description = "You're forced to run away due to fear or some other compulsion. On your turn, you must spend each of your actions trying to escape the source of the fleeing condition as expediently as possible (such as by using move actions to flee, or opening doors barring your escape). The condition ends once you escape.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["frightened"] = new PfCondition
            {
                Id = "frightened",
                Name = "Frightened",
                Description = "You're gripped by fear and struggle to control your nerves. The frightened condition always includes a value. You take a status penalty equal to this value to all your checks and DCs. Unless specified otherwise, at the end of each of your turns, the value of your frightened condition decreases by 1.",
                Type = ConditionType.Debuff,
                Traits = new List<string> { "mental" },
                HasValue = true,
                MaxValue = 4,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string> { "mental", "fear" },
                Source = "Core Rulebook"
            },

            ["grabbed"] = new PfCondition
            {
                Id = "grabbed",
                Name = "Grabbed",
                Description = "A creature, object, or magic holds you in place. You can't move unless you successfully Escape or force the grabber to release you (such as by defeating them in combat). You are flat-footed and take a –2 circumstance penalty to attack rolls. When you attempt to Escape, you can use Athletics to attempt to break free, or you can use Acrobatics if you're not completely held down. The GM assigns the Escape DC, typically the same DC that was required to grab you. Escaping from being grabbed takes the same number of actions that was used to grab you, or a single action if unspecified.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["hidden"] = new PfCondition
            {
                Id = "hidden",
                Name = "Hidden",
                Description = "While you're hidden from a creature, that creature knows the space you're in but can't tell precisely where you are. You typically become hidden by using Stealth to Hide. When Seeking a creature using only imprecise senses, it remains hidden, rather than observed. A creature you're hidden from is flat-footed to you, and it must succeed at a DC 11 flat check when targeting you with an attack, spell, or other effect or it fails affect you. Area effects aren't subject to this flat check. You might be undetected by some creatures and hidden from others.",
                Type = ConditionType.Status,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["immobilized"] = new PfCondition
            {
                Id = "immobilized",
                Name = "Immobilized",
                Description = "You can't move. Being immobilized doesn't prevent you from using actions with the manipulate trait, though such actions might require a certain position or stability. If you're immobilized by being held in place (such as by the Grab action), moving forcibly out of your space (such as with the Shove action) doesn't free you automatically—the grappler can potentially move along with you.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["invisible"] = new PfCondition
            {
                Id = "invisible",
                Name = "Invisible",
                Description = "Creatures with this condition are concealed and can Hide even while being directly observed. However, they're still noticed by other senses.",
                Type = ConditionType.Buff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["paralyzed"] = new PfCondition
            {
                Id = "paralyzed",
                Name = "Paralyzed",
                Description = "Your body is frozen in place. You have the flat-footed and immobilized conditions, and you can't use any actions with the attack or manipulate traits except to Recall Knowledge and other purely mental actions. Your muscles are rigid and jerky. You can't act except to Recall Knowledge and use actions with the mental trait.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["persistent-damage"] = new PfCondition
            {
                Id = "persistent-damage",
                Name = "Persistent Damage",
                Description = "Instead of taking persistent damage immediately, you take it at the end of each of your turns as long as you have the condition, rolling any damage dice anew each time. After you take persistent damage, roll a DC 15 flat check to see if you recover from the persistent damage. If you succeed, the condition ends. If you fail, you continue to take persistent damage.",
                Type = ConditionType.Persistent,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["petrified"] = new PfCondition
            {
                Id = "petrified",
                Name = "Petrified",
                Description = "You have been turned to stone. You can't act, nor can you sense anything. You become an object with a Bulk equal to twice your normal Bulk (typically 12 for a petrified Medium creature or 6 for a petrified Small creature), AC 9, Hardness 8, and the same number of Hit Points you had when alive. You don't have a Broken Threshold. When you're petrified, the only action you can use is Recall Knowledge.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["prone"] = new PfCondition
            {
                Id = "prone",
                Name = "Prone",
                Description = "You're lying on the ground. You are flat-footed and take a –2 circumstance penalty to attack rolls. The only move actions you can use while you're prone are Crawl and Stand. Standing up ends the prone condition. You can Take Cover while prone to hunker down and gain greater cover against ranged attacks, though not against adjacent enemies.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["quickened"] = new PfCondition
            {
                Id = "quickened",
                Name = "Quickened",
                Description = "You gain 1 additional action at the start of your turn each round. Many effects that make you quickened specify the types of actions you can use with this additional action. If you become quickened from multiple sources, you can use the extra action you've been granted for any of the listed purposes.",
                Type = ConditionType.Buff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["restrained"] = new PfCondition
            {
                Id = "restrained",
                Name = "Restrained",
                Description = "You're tied up and can barely move, or a grappling creature has you pinned. You have the flat-footed and immobilized conditions, and you can't use any actions with the attack or manipulate traits except to attempt to Escape or Force Open your bonds. Restrained overrides grabbed.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = true,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["sickened"] = new PfCondition
            {
                Id = "sickened",
                Name = "Sickened",
                Description = "You feel ill. Sickened always includes a value. You take a status penalty equal to this value on all your checks and DCs. You can't willingly ingest anything—including elixirs and potions—while sickened. You can spend a single action retching in an attempt to recover, which lets you immediately attempt a Fortitude save against the DC of the effect that made you sickened. On a success, you reduce your sickened value by 1 (or by 2 on a critical success).",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = true,
                MaxValue = 4,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["slowed"] = new PfCondition
            {
                Id = "slowed",
                Name = "Slowed",
                Description = "You have fewer actions. Slowed always includes a value. When you regain your actions at the start of your turn, reduce the number of actions you regain by your slowed value. Because slowed has its effect at the start of your turn, you don't immediately lose actions if you become slowed during your turn.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = true,
                MaxValue = 3,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["stunned"] = new PfCondition
            {
                Id = "stunned",
                Name = "Stunned",
                Description = "You've become senseless. You can't act while stunned. Stunned usually includes a value, which indicates how many total actions you lose, possibly over multiple turns, from being stunned. Each time you regain actions (such as at the start of your turn), reduce the number you regain by your stunned value, then reduce your stunned value by the number of actions you lost. For example, if you were stunned 4, you would lose all 3 of your actions on your turn, then be stunned 1 on your following turn, causing you to lose 1 more action. Stunned might also have a duration instead of a value, such as 'stunned for 1 minute.' In this case, you lose all your actions for the listed duration. Stunned overrides slowed.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = true,
                MaxValue = null,
                Overrides = true,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["stupefied"] = new PfCondition
            {
                Id = "stupefied",
                Name = "Stupefied",
                Description = "Your thoughts and instincts are clouded. Stupefied always includes a value. You take a status penalty equal to this value on Intelligence-, Wisdom-, and Charisma-based checks and DCs, including Will saving throws, spell attack rolls, spell DCs, and skill checks that use these ability scores. Any time you attempt to Cast a Spell while stupefied, the spell is disrupted unless you succeed at a flat check with a DC equal to 5 + your stupefied value.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = true,
                MaxValue = 4,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["unconscious"] = new PfCondition
            {
                Id = "unconscious",
                Name = "Unconscious",
                Description = "You're sleeping or have been knocked out. You can't act. You take a –4 status penalty to AC, Perception, and Reflex saves, and you have the blinded and flat-footed conditions. When you gain this condition, you fall prone and drop items you are wielding or holding unless the effect states otherwise or the GM determines you're in a position in which you wouldn't.",
                Type = ConditionType.Debuff,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["undetected"] = new PfCondition
            {
                Id = "undetected",
                Name = "Undetected",
                Description = "When you are undetected by a creature, that creature cannot see you at all, has no idea what space you occupy, and can't target you, though you still can be affected by abilities that target an area. When you're undetected by a creature, that creature is flat-footed to you. A creature you're undetected by can guess which square you're in to try targeting you. Such an attack roll or spell attack roll has a DC 11 flat check failure chance.",
                Type = ConditionType.Status,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["unfriendly"] = new PfCondition
            {
                Id = "unfriendly",
                Name = "Unfriendly",
                Description = "This condition reflects a creature's disposition toward a particular character, and only matters for things like Diplomacy checks and social encounters. All characters begin with a disposition of indifferent toward most NPCs. An unfriendly creature dislikes you and doesn't want to help you. It might be convinced to go along with you if you can provide incentives or if doing so doesn't cause the creature any hardship. Unfriendly creatures are usually unwilling to risk anything for a character unless there's something in it for them.",
                Type = ConditionType.Status,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["unnoticed"] = new PfCondition
            {
                Id = "unnoticed",
                Name = "Unnoticed",
                Description = "A creature is unnoticed by you if you're totally unaware of its presence. A creature that is unnoticed is also undetected by you. You remain unaware of an unnoticed creature until you use the Seek action or until the creature becomes observed by you or performs an activity that would cause you to notice it.",
                Type = ConditionType.Status,
                Traits = new List<string>(),
                HasValue = false,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            },

            ["wounded"] = new PfCondition
            {
                Id = "wounded",
                Name = "Wounded",
                Description = "You have been seriously injured. If you lose the dying condition and are still at 0 Hit Points, you become wounded 1. If you already have the wounded condition when you lose the dying condition, your wounded condition value increases by 1. If you gain the dying condition while wounded, increase your dying condition value by your wounded value. The wounded condition ends if someone successfully restores Hit Points to you with Treat Wounds or you are restored to full Hit Points and rest for 10 minutes.",
                Type = ConditionType.Status,
                Traits = new List<string>(),
                HasValue = true,
                MaxValue = 3,
                Overrides = false,
                OverriddenBy = new List<string>(),
                ImmunityTraits = new List<string>(),
                Source = "Core Rulebook"
            }
        };
    }

    public static class Afflictions
    {
        public static readonly Dictionary<string, PfAffliction> CoreAfflictionsData = new()
        {
            ["cackle-fever"] = new PfDisease
            {
                Id = "cackle-fever",
                Name = "Cackle Fever",
                Description = "This disease targets humanoids, although gnomes are strangely immune. While in areas where the disease has spread, creatures can become infected through contact with contaminated objects, food, or water.",
                Level = 3,
                Traits = new List<string> { "disease" },
                SavingThrow = "Fortitude DC 17",
                Onset = "1 day",
                MaxDuration = "1 week",
                IsContagious = true,
                TransmissionMethod = "contact with contaminated objects",
                IncubationPeriod = "1 day",
                Stages = new List<PfAfflictionStage>
                {
                    new() { Stage = 1, Name = "Stage 1", Effect = "The creature can't use reactions", Duration = "1 day" },
                    new() { Stage = 2, Name = "Stage 2", Effect = "The creature is slowed 1 and can't use reactions", Duration = "1 day", Conditions = new List<string> { "slowed" } },
                    new() { Stage = 3, Name = "Stage 3", Effect = "The creature is slowed 1, can't use reactions, and must spend 1 action each turn to Interact to cackle madly", Duration = "1 day", Conditions = new List<string> { "slowed" } }
                },
                Source = "Core Rulebook",
                Rarity = "Common"
            },

            ["spider-venom"] = new PfPoison
            {
                Id = "spider-venom",
                Name = "Spider Venom",
                Description = "Spider venom is one of the most common poisons. It's often gathered by hunters and used to coat arrows and bolts.",
                Level = 1,
                Traits = new List<string> { "poison" },
                SavingThrow = "Fortitude DC 17",
                Onset = "",
                MaxDuration = "4 rounds",
                DeliveryMethod = "Injury",
                IsVirulent = false,
                PoisonType = "Natural toxin",
                Stages = new List<PfAfflictionStage>
                {
                    new() { Stage = 1, Name = "Stage 1", Effect = "1d4 poison damage and enfeebled 1", Duration = "1 round", DamageFormula = "1d4", DamageType = "poison", Conditions = new List<string> { "enfeebled" } },
                    new() { Stage = 2, Name = "Stage 2", Effect = "1d4 poison damage and enfeebled 2", Duration = "1 round", DamageFormula = "1d4", DamageType = "poison", Conditions = new List<string> { "enfeebled" } }
                },
                Source = "Core Rulebook",
                Rarity = "Common"
            },

            ["werewolf-curse"] = new PfCurse
            {
                Id = "werewolf-curse",
                Name = "Curse of the Werebeast",
                Description = "A werebeast's curse is passed on through bite wounds. This curse affects only humanoids.",
                Level = 2,
                Traits = new List<string> { "curse", "necromancy", "primal" },
                SavingThrow = "Fortitude DC 18",
                Onset = "on the next full moon",
                MaxDuration = "permanent",
                CurseType = "Transformation",
                RemovalMethods = new List<string> { "remove curse", "wolfsbane", "silver weapon ritual" },
                IsPermanent = true,
                TriggerCondition = "full moon",
                Stages = new List<PfAfflictionStage>
                {
                    new() { Stage = 1, Name = "Stage 1", Effect = "No ill effects", Duration = "1 month" },
                    new() { Stage = 2, Name = "Stage 2", Effect = "The creature gains weakness 5 to silver", Duration = "permanent" },
                    new() { Stage = 3, Name = "Stage 3", Effect = "The creature transforms into a lycanthrope on each full moon, with no memory of the time spent transformed", Duration = "permanent" }
                },
                Source = "Core Rulebook",
                Rarity = "Rare"
            },

            ["mindfire"] = new PfDisease
            {
                Id = "mindfire",
                Name = "Mindfire",
                Description = "Spread by mosquitoes in tropical swamps, this disease causes a dangerous fever that clouds the mind.",
                Level = 3,
                Traits = new List<string> { "disease" },
                SavingThrow = "Fortitude DC 17",
                Onset = "1 day",
                MaxDuration = "1 week",
                IsContagious = false,
                TransmissionMethod = "insect bite",
                IncubationPeriod = "1 day",
                Stages = new List<PfAfflictionStage>
                {
                    new() { Stage = 1, Name = "Stage 1", Effect = "Enfeebled 1", Duration = "1 day", Conditions = new List<string> { "enfeebled" } },
                    new() { Stage = 2, Name = "Stage 2", Effect = "Enfeebled 2 and stupefied 2", Duration = "1 day", Conditions = new List<string> { "enfeebled", "stupefied" } },
                    new() { Stage = 3, Name = "Stage 3", Effect = "Enfeebled 3, stupefied 3, and confused", Duration = "1 day", Conditions = new List<string> { "enfeebled", "stupefied", "confused" } }
                },
                Source = "Core Rulebook",
                Rarity = "Common"
            },

            ["purple-worm-venom"] = new PfPoison
            {
                Id = "purple-worm-venom",
                Name = "Purple Worm Venom",
                Description = "Few survive a dose of a purple worm's venom.",
                Level = 13,
                Traits = new List<string> { "poison" },
                SavingThrow = "Fortitude DC 32",
                Onset = "",
                MaxDuration = "6 rounds",
                DeliveryMethod = "Injury",
                IsVirulent = true,
                PoisonType = "Monster venom",
                Stages = new List<PfAfflictionStage>
                {
                    new() { Stage = 1, Name = "Stage 1", Effect = "5d6 poison damage and enfeebled 2", Duration = "1 round", DamageFormula = "5d6", DamageType = "poison", Conditions = new List<string> { "enfeebled" } },
                    new() { Stage = 2, Name = "Stage 2", Effect = "6d6 poison damage and enfeebled 3", Duration = "1 round", DamageFormula = "6d6", DamageType = "poison", Conditions = new List<string> { "enfeebled" } },
                    new() { Stage = 3, Name = "Stage 3", Effect = "8d6 poison damage and enfeebled 4", Duration = "1 round", DamageFormula = "8d6", DamageType = "poison", Conditions = new List<string> { "enfeebled" } }
                },
                Source = "Core Rulebook",
                Rarity = "Rare"
            },

            ["blinding-sickness"] = new PfDisease
            {
                Id = "blinding-sickness",
                Name = "Blinding Sickness",
                Description = "This painful infection causes bleeding from the eyes and eventual blindness.",
                Level = 6,
                Traits = new List<string> { "disease" },
                SavingThrow = "Fortitude DC 22",
                Onset = "1 day",
                MaxDuration = "indefinite",
                IsContagious = true,
                TransmissionMethod = "contact",
                IncubationPeriod = "1 day",
                Stages = new List<PfAfflictionStage>
                {
                    new() { Stage = 1, Name = "Stage 1", Effect = "Dazzled", Duration = "1 day", Conditions = new List<string> { "dazzled" } },
                    new() { Stage = 2, Name = "Stage 2", Effect = "Dazzled and sickened 1", Duration = "1 day", Conditions = new List<string> { "dazzled", "sickened" } },
                    new() { Stage = 3, Name = "Stage 3", Effect = "Blinded", Duration = "1 day", Conditions = new List<string> { "blinded" } },
                    new() { Stage = 4, Name = "Stage 4", Effect = "Blinded (this stage doesn't end and can't be cured until the disease is cured)", Duration = "indefinite", Conditions = new List<string> { "blinded" } }
                },
                Source = "Core Rulebook",
                Rarity = "Uncommon"
            }
        };
    }
}