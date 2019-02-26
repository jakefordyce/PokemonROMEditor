﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PokemonROMEditor.Models
{
    public enum PokeType
    {
        NORMAL = 0,
        FIGHTING = 1,
        FLYING = 2,
        POISON = 3,
        GROUND = 4,
        ROCK = 5,
        BUG = 7,
        GHOST = 8,
        FIRE = 20,
        WATER = 21,
        GRASS = 22,
        ELECTRIC = 23,
        PSYCHIC = 24,
        ICE = 25,
        DRAGON = 26
    };

    public enum EvolveType
    {
        NONE = 0,
        LEVEL = 1,
        STONE = 2,
        TRADE = 3
    }

    public enum DamageModifier
    {
        No_Effect = 0,
        Half_Damage = 5,
        Double_Damage = 20
    }

    public enum MoveEffect
    {
        No_Additional_Effect = 0,
        Sleep_No_Damage = 1,
        High_Chance_Of_Poison = 2,
        Absorb_Half = 3,
        Low_Chance_Of_Burn = 4,
        Low_Chance_Of_Freeze = 5,
        Low_Chance_Of_Paralyze = 6,
        User_Feints = 7,
        Eat_Dream = 8,
        Mirror_Last_Move = 9,
        Raise_Attack = 10,
        Raise_Defense = 11,
        Raise_Speed = 12,
        Raise_Special = 13,
        Raise_Accuracy = 14,
        Raise_Evasion = 15,
        Money_After_Battle = 16,
        Always_Hits = 17,
        Lower_Enemy_Attack = 18,
        Lower_Enemy_Defense = 19,
        Lower_Enemy_Speed = 20,
        Lower_Enemy_Special = 21,
        Lower_Enemy_Accuracy = 22,
        Lower_Enemy_Evasion = 23,
        Copy_Enemy_Type = 24,
        Remove_All_Stat_Changes = 25,
        Bide_Effect = 26,
        Attack_2_3_Turns_Then_Confused = 27,
        Leave_Battle = 28,
        Attack_2_5_Times = 29,
        Attack_2_5_Turns = 30,
        Low_Chance_Of_Flinch = 31,
        Enemy_Sleeps = 32,
        Very_High_Chance_Of_Poison = 33,
        High_Chance_Of_Burn = 34,
        High_Chance_Of_Freeze = 35,
        High_Chance_Of_Paralyze = 36,
        High_Chance_Of_Flinch = 37,
        One_Hit_KO = 38,
        Charge_1_Turn_Attack_Next = 39,
        Deal_Half_HP = 40,
        Deal_Set_Damage = 41,
        Attack_2_5_Times_Trap = 42,
        Fly_Effect = 43,
        Attack_2_Times = 44,
        User_Damaged_If_Miss = 45,
        Ignore_Stat_Changes = 46,
        Zero_Crit_Chance_Broken = 47,
        User_Takes_Recoil_Damage = 48,
        Enemy_Confused = 49,
        Raise_Attack_Sharply = 50,
        Raise_Defense_Sharply = 51,
        Raise_Speed_Sharply = 52,
        Raise_Special_Sharply = 53,
        Raise_Accuracy_Sharply = 54,
        Raise_Evasion_Sharply = 55,
        Recover_HP = 56,
        Transform_Into_Enemy = 57,
        Lower_Enemy_Attack_Sharply = 58,
        Lower_Enemy_Defense_Sharply = 59,
        Lower_Enemy_Speed_Sharply = 60,
        Lower_Enemy_Special_Sharply = 61,
        Lower_Enemy_Accuracy_Sharply = 62,
        Lower_Enemy_Evasion_Sharply = 63,
        Half_Damage_From_Special = 64,
        Half_Damage_From_Attack = 65,
        Enemy_Poisoned = 66,
        Enemy_Paralyzed = 67,
        Low_Chance_Lower_Enemy_Attack = 68,
        Low_Chance_Lower_Enemy_Defense = 69,
        Low_Chance_Lower_Enemy_Speed = 70,
        High_Chance_Lower_Enemy_Special = 71,
        Low_Chance_Lower_Enemy_Accuracy = 72,
        Low_Chance_Lower_Enemy_Evasion = 73,
        Low_Chance_Enemy_Confused = 76,
        Attack_2_Times_Chance_Enemy_Poison = 77,
        Create_Substitute = 79,
        Recharge_After_Attack = 80,
        Attack_Raised_When_Hit_Lose_Control = 81,
        Mime_Opponents_Move = 82,
        Use_Random_Move = 83,
        Steal_HP_Each_Turn = 84,
        Move_Has_No_Effect = 85,
        Disable_Random_Enemy_Move = 86
    }

    public enum EvolveStone
    {
        Nothing = 0,
        MOON_STONE = 10,
        FIRE_STONE = 32,
        THUNDER_STONE = 33,
        WATER_STONE = 34,
        LEAF_STONE = 47
    }

    public enum ItemType
    {
        MASTER_BALL = 1,
        ULTRA_BALL = 2,
        GREAT_BALL = 3,
        POKE_BALL = 4,
        TOWN_MAP = 5,
        BICYCLE = 6,
        SURFBOAD = 7,
        SAFARI_BALL = 8,
        POKEDEX = 9,
        MOON_STONE = 10,
        ANTIDOTE = 11,
        BURN_HEAL = 12,
        ICE_HEAL = 13,
        AWAKENING = 14,
        PARLYZ_HEAL = 15,
        FULL_RESTORE = 16,
        MAX_POTION = 17,
        HYPER_POTION = 18,
        SUPER_POTION = 19,
        POTION = 20,
        BOULDER_BADGE = 21,
        CASCADE_BADGE = 22,
        THUNDER_BADGE = 23,
        RAINBOW_BADGE = 24,
        SOUL_BADGE = 25,
        MARSH_BADGE = 26,
        VOLCANO_BADGE = 27,
        EARTH_BADGE = 28,
        ESCAPE_ROPE = 29,
        REPEL = 30,
        OLD_AMBER = 31,
        FIRE_STONE = 32,
        THUNDER_STONE = 33,
        WATER_STONE = 34,
        HP_UP = 35,
        PROTEIN = 36,
        IRON = 37,
        CARBOS = 38,
        CALCIUM = 39,
        RARE_CANDY = 40,
        DOME_FOSSIL = 41,
        HELIX_FOSSIL = 42,
        SECRET_KEY = 43,
        UNUSED_ITEM = 44,
        BIKE_VOUCHER = 45,
        X_ACCURACY = 46,
        LEAF_STONE = 47,
        CARD_KEY = 48,
        NUGGET = 49,
        PP_UP_2 = 50,
        POKE_DOLL = 51,
        FULL_HEAL = 52,
        REVIVE = 53,
        MAX_REVIVE = 54,
        GUARD_SPEC = 55,
        SUPER_REPEL = 56,
        MAX_REPEL = 57,
        DIRE_HIT = 58,
        COIN = 59,
        FRESH_WATER = 60,
        SODA_POP = 61,
        LEMONADE = 62,
        SS_TICKET = 63,
        GOLD_TEETH = 64,
        X_ATTACK = 65,
        X_DEFEND = 66,
        X_SPEED = 67,
        X_SPECIAL = 68,
        COIN_CASE = 69,
        OAKS_PARCEL = 70,
        ITEMFINDER = 71,
        SILPH_SCOPE = 72,
        POKE_FLUTE = 73,
        LIFT_KEY = 74,
        EXP_ALL = 75,
        OLD_ROD = 76,
        GOOD_ROD = 77,
        SUPER_ROD = 78,
        PP_UP = 79,
        ETHER = 80,
        MAX_ETHER = 81,
        ELIXER = 82,
        MAX_ELIXER = 83,
        TM_01 = 201,
        TM_02 = 202,
        TM_03 = 203,
        TM_04 = 204,
        TM_05 = 205,
        TM_06 = 206,
        TM_07 = 207,
        TM_08 = 208,
        TM_09 = 209,
        TM_10 = 210,
        TM_11 = 211,
        TM_12 = 212,
        TM_13 = 213,
        TM_14 = 214,
        TM_15 = 215,
        TM_16 = 216,
        TM_17 = 217,
        TM_18 = 218,
        TM_19 = 219,
        TM_20 = 220,
        TM_21 = 221,
        TM_22 = 222,
        TM_23 = 223,
        TM_24 = 224,
        TM_25 = 225,
        TM_26 = 226,
        TM_27 = 227,
        TM_28 = 228,
        TM_29 = 229,
        TM_30 = 230,
        TM_31 = 231,
        TM_32 = 232,
        TM_33 = 233,
        TM_34 = 234,
        TM_35 = 235,
        TM_36 = 236,
        TM_37 = 237,
        TM_38 = 238,
        TM_39 = 239,
        TM_40 = 240,
        TM_41 = 241,
        TM_42 = 242,
        TM_43 = 243,
        TM_44 = 244,
        TM_45 = 245,
        TM_46 = 246,
        TM_47 = 247,
        TM_48 = 248,
        TM_49 = 249,
        TM_50 = 250,

    }

    public enum GrowthType
    {
        Medium_Fast = 0,
        Medium_Slow = 3,
        Fast = 4,
        Slow = 5
    }

    public enum TileSet
    {
        Overworld = 0,
        Reds_House_1 = 1,
        Mart = 2,
        Forest = 3,
        Reds_House_2 = 4,
        Dojo = 5,
        Pokecenter = 6,
        Gym = 7,
        House = 8,
        Forest_Gate = 9,
        Museum = 10,
        Underground = 11,
        Gate = 12,
        Ship = 13,
        Ship_Port = 14,
        Cemetery = 15,
        Interior = 16,
        Cavern = 17,
        Lobby = 18,
        Mansion = 19,
        Lab = 20,
        Club = 21,
        Facility = 22,
        Plateau = 23
    }

    public enum MapObjectType
    {
        Person = 0,
        Trainer = 1,
        Pokemon = 2,
        Item = 3
    }

    public enum MapObjectMovementType
    {
        Walk = 254,
        Stay = 255
    }

    public enum MapObjectFacing
    {
        Zero = 0,
        One = 1,
        Two = 2,
        Three = 3,
        Down = 208,
        Up = 209,
        Left = 210,
        Right = 211,
        None = 255
    }    

}
