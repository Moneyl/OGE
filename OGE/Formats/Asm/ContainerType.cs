﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OGE.Formats.Asm
{
    public enum ContainerType : byte
    {
        None = 0,
        Glass = 1,
        EffectsEnv = 2,
        EffectsPreload = 3,
        EffectsDlc = 4,
        MpEffects = 5,
        LayerSmall = 6,
        LayerLarge = 7,
        Audio = 8,
        ClothSim = 9,
        Decals = 10,
        DecalsPreload = 11,
        Fsm = 12,
        Ui = 13,
        Env = 14,
        Chunk = 15,
        ChunkPreload = 16,
        Stitch = 17,
        World = 18,
        HumanHead = 19,
        Human = 20,
        Player = 21,
        Items = 22,
        ItemsPreload = 23,
        ItemsMpPreload = 24,
        ItemsDlc = 25,
        WeaponLarge = 26,
        WeaponSmall = 27,
        Skybox = 28,
        Vehicle = 29,
        VoicePersona = 30,
        AlwaysLoadedVoicePersona = 31,
        Foliage = 32,
        UiPeg = 33,
        MaterialEffect = 34,
        MaterialPreload = 35,
        SharedBackpack = 36,
        LandmarkLod = 37,
        GpsPreload = 38,
        NumContainerTypes = 39
    }
}
