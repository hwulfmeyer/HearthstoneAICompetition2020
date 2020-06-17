using SabberStoneCore.Model;
using System.Collections.Generic;

namespace SabberStoneBasicAI.AIAgents.PredatorMCTS
{
	/// <summary>
	/// Contains the top tier card decks based on the meta-report from the 16.03.2018.
	/// </summary>
    public static class TopTierDecks
    {
		/// <summary>
		/// Reconstruction of the Cubelock deck (Control) based on 
		/// https://www.hearthstoneheroes.de/decks/cubelock-meta/
		/// and the reversed history of changes till 01.03.2018.
		/// </summary>
		/// <returns>the Cubelock deck</returns>
		public static List<Card> Cubelock()
		{
			return new List<Card>()
			{
				//Dark Pact
				Cards.FromId ("LOOT_017"),
				Cards.FromId ("LOOT_017"),
				// Kobold Librarian
				Cards.FromId ("LOOT_014"),
				Cards.FromId ("LOOT_014"),
				// Defile
				Cards.FromId ("ICC_041"),
				Cards.FromId ("ICC_041"),
				// Lesser Amethyst Spellstone
				Cards.FromId ("LOOT_043"),
				Cards.FromId ("LOOT_043"),
				// Hellfire
				Cards.FromId ("CS2_062"),
				Cards.FromId ("CS2_062"),
				// Skull of the Man'ari
				Cards.FromId ("LOOT_420"),
				// Carnivorous Cube
				Cards.FromId ("LOOT_161"),
				Cards.FromId ("LOOT_161"),
				// Faceless Manipulator
				Cards.FromId ("EX1_564"),
				Cards.FromId ("EX1_564"),
				// Doomguard
				Cards.FromId ("EX1_310"),
				Cards.FromId ("EX1_310"),
				// Possessed Lackey
				Cards.FromId ("LOOT_306"),
				Cards.FromId ("LOOT_306"),
				// Voidlord
				Cards.FromId ("LOOT_368"),
				Cards.FromId ("LOOT_368"),
				// Bloodreaver Gul'dan
				Cards.FromId ("ICC_831"),
				// Mountain Giant
				Cards.FromId ("EX1_105"),
				Cards.FromId ("EX1_105"),
				// Doomsayer
				Cards.FromId ("NEW1_021"),
				Cards.FromId ("NEW1_021"),
				// Prince Taldaram
				Cards.FromId ("ICC_852"),
				// Mistress of Mixtures
				Cards.FromId ("CFM_120"),
				Cards.FromId ("CFM_120"),
				// N'Zoth, the Corruptor
				Cards.FromId ("OG_133"),
				// Stonehill Defender
				//Cards.FromId ("UNG_072"),
				//Cards.FromId ("UNG_072"),
				// Spiritsinger Umbra
				//Cards.FromId ("UNG_900"),
				// Lich King
				//Cards.FromId ("ICC_314"),
				// Lord Godfrey
				//Cards.FromId ("GIL_825")
			};
		}

		/// <summary>
		/// Reconstruction of the Murloc Paladin deck (Midrange) based on
		/// https://www.hearthstoneheroes.de/decks/murloc-paladin-meta/
		/// and the reversed history of changes till 01.03.2018.
		/// </summary>
		/// <returns>the Murloc Paladin deck</returns>
		public static List<Card> MurlocPaladin()
		{
			return new List<Card>()
			{
				// Righteous Protector
				Cards.FromId ("ICC_038"),
				Cards.FromId ("ICC_038"),
				// Murloc Tidecaller
				Cards.FromId ("EX1_509"),
				Cards.FromId ("EX1_509"),
				// Hydrologist
				Cards.FromId ("UNG_011"),
				Cards.FromId ("UNG_011"),
				// Knife Juggler
				Cards.FromId ("NEW1_019"),
				Cards.FromId ("NEW1_019"),
				// Rockpool Hunter
				Cards.FromId ("UNG_073"),
				Cards.FromId ("UNG_073"),
				// Unidentified Maul
				Cards.FromId ("LOOT_286"),
				// Divine Favor
				Cards.FromId ("EX1_349"),
				Cards.FromId ("EX1_349"),
				// Murloc Warleader
				Cards.FromId ("EX1_507"),
				Cards.FromId ("EX1_507"),
				// Coldlight Seer
				Cards.FromId ("EX1_103"),
				// Gentle Megasaur
				Cards.FromId ("UNG_089"),
				Cards.FromId ("UNG_089"),
				// Spellbreaker
				Cards.FromId ("EX1_048"),
				// Call to Arms
				Cards.FromId ("LOOT_093"),
				Cards.FromId ("LOOT_093"),
				// Fungalmancer
				Cards.FromId ("LOOT_167"),
				// Sunkeeper Tarim
				Cards.FromId ("UNG_015"),
				// Vinecleaver
				Cards.FromId ("UNG_950"),
				// Grimscale Chum
				Cards.FromId ("CFM_650"),
				Cards.FromId ("CFM_650"),
				// Vilefin Inquisitor
				Cards.FromId ("OG_006"),
				Cards.FromId ("OG_006"),
				// Rallying Blade
				Cards.FromId ("OG_222"),
				Cards.FromId ("OG_222")
			};
		}

		/// <summary>
		/// Reconstruction of the Dude Paladin deck (Midrange) based on
		/// https://www.hearthstoneheroes.de/decks/dude-paladin-meta/
		/// </summary>
		/// <returns>the Dude Paladin deck</returns>
		public static List<Card> DudePaladin()
		{
			return new List<Card>()
			{
				// Lost in the Jungle
				Cards.FromId ("UNG_960"),
				Cards.FromId ("UNG_960"),
				// Argent Squire
				Cards.FromId ("EX1_008"),
				Cards.FromId ("EX1_008"),
				// Righteous Protector
				Cards.FromId ("ICC_038"),
				Cards.FromId ("ICC_038"),
				// Equality
				Cards.FromId ("EX1_619"),
				// Knife Juggler
				Cards.FromId ("NEW1_019"),
				Cards.FromId ("NEW1_019"),
				// Drygulch Jailor
				Cards.FromId ("LOOT_363"),
				// Dire Wolf Alpha
				Cards.FromId ("EX1_162"),
				Cards.FromId ("EX1_162"),
				// Rallying Blade
				Cards.FromId ("OG_222"),
				Cards.FromId ("OG_222"),
				// Unidentified Maul
				Cards.FromId ("LOOT_286"),
				// Divine Favor
				Cards.FromId ("EX1_349"),
				Cards.FromId ("EX1_349"),
				// Steward of Darkshire
				Cards.FromId ("OG_310"),
				Cards.FromId ("OG_310"),
				// Lightfused Stegodon
				Cards.FromId ("UNG_962"),
				Cards.FromId ("UNG_962"),
				// Call to Arms
				Cards.FromId ("LOOT_093"),
				Cards.FromId ("LOOT_093"),
				// Level Up!
				Cards.FromId ("LOOT_333"),
				// Stand Against Darkness
				Cards.FromId ("OG_273"),
				Cards.FromId ("OG_273"),
				// Crystal Lion
				Cards.FromId ("LOOT_313"),
				Cards.FromId ("LOOT_313"),
				// Sunkeeper Tarim
				Cards.FromId ("UNG_015"),
				// Vinecleaver
				Cards.FromId ("UNG_950")
			};
		}

		/// <summary>
		/// Reconstruction of the Dragon Priest deck (Midrange) based on
		/// https://www.hearthstoneheroes.de/decks/dragon-priest-meta/
		/// and the reversed history of changes till 01.03.2018.
		/// </summary>
		/// <returns>the Dragon Priest deck</returns>
		public static List<Card> DragonPriest()
		{
			return new List<Card>()
			{
				// Inner Fire
				Cards.FromId ("CS1_129"),
				Cards.FromId ("CS1_129"),
				// Power Word: Shield
				Cards.FromId ("CS2_004"),
				Cards.FromId ("CS2_004"),
				// Potion of Madness
				Cards.FromId ("CFM_603"),
				Cards.FromId ("CFM_603"),
				// Northshire Cleric
				Cards.FromId ("CS2_235"),
				Cards.FromId ("CS2_235"),
				// Divine Spirit
				Cards.FromId ("CS2_236"),
				Cards.FromId ("CS2_236"),
				// Shadow Visions
				Cards.FromId ("UNG_029"),
				Cards.FromId ("UNG_029"),
				// Netherspite Historian
				Cards.FromId ("KAR_062"),
				Cards.FromId ("KAR_062"),
				// Radiant Elemental
				Cards.FromId ("UNG_034"),
				Cards.FromId ("UNG_034"),
				// Kabal Talonpriest
				Cards.FromId ("CFM_626"),
				Cards.FromId ("CFM_626"),
				// Twilight Acolyte
				Cards.FromId ("LOOT_528"),
				Cards.FromId ("LOOT_528"),
				// Duskbreaker
				Cards.FromId ("LOOT_410"),
				Cards.FromId ("LOOT_410"),
				// Twilight Drake
				Cards.FromId ("EX1_043"),
				Cards.FromId ("EX1_043"),
				// Drakonid Operative
				Cards.FromId ("CFM_605"),
				Cards.FromId ("CFM_605"),
				//Silence
				Cards.FromId ("EX1_332"),
				// Shadow Ascendant
				Cards.FromId ("ICC_210"),
				Cards.FromId ("ICC_210"),
				// Cabal Shadow Priest
				Cards.FromId ("EX1_091")
			};
		}

		/// <summary>
		/// Reconstruction of the Control Warlock deck (Control) based on
		/// https://www.hearthstoneheroes.de/decks/control-warlock-meta/
		/// and the reversed history of changes till 01.03.2018.
		/// </summary>
		/// <returns>the Control Warlock deck</returns>
		public static List<Card> ControlWarlock()
		{
			return new List<Card>()
			{
				//Dark Pact
				Cards.FromId ("LOOT_017"),
				Cards.FromId ("LOOT_017"),
				// Kobold Librarian
				Cards.FromId ("LOOT_014"),
				Cards.FromId ("LOOT_014"),
				// Defile
				Cards.FromId ("ICC_041"),
				Cards.FromId ("ICC_041"),
				// Stonehill Defender
				Cards.FromId ("UNG_072"),
				Cards.FromId ("UNG_072"),
				// Lesser Amethyst Spellstone
				Cards.FromId ("LOOT_043"),
				Cards.FromId ("LOOT_043"),
				// Hellfire
				Cards.FromId ("CS2_062"),
				Cards.FromId ("CS2_062"),
				// Possessed Lackey
				Cards.FromId ("LOOT_306"),
				Cards.FromId ("LOOT_306"),
				// Rin, the First Disciple
				Cards.FromId ("LOOT_415"),
				// Twisting Nether
				Cards.FromId ("EX1_312"),
				Cards.FromId ("EX1_312"),
				// Voidlord
				Cards.FromId ("LOOT_368"),
				Cards.FromId ("LOOT_368"),
				// Bloodreaver Gul'dan
				Cards.FromId ("ICC_831"),
				// Mistress of Mixtures
				Cards.FromId ("CFM_120"),
				Cards.FromId ("CFM_120"),
				// Doomsayer
				Cards.FromId ("NEW1_021"),
				Cards.FromId ("NEW1_021"),
				// N'Zoth, the Corruptor
				Cards.FromId ("OG_133"),
				// Siphon Soul
				Cards.FromId ("EX1_309"),
				// Skulking Geist
				Cards.FromId ("ICC_701"),
				//Mortal Coil
				Cards.FromId ("EX1_302"),
				// Gnomeferatu
				Cards.FromId ("ICC_407"),
				Cards.FromId ("ICC_407")
			};
		}	
	}
}
